import { callRpc } from "../_shared/database.ts";
import { corsHeaders, jsonResponse, readJsonBody, websiteOrigin } from "../_shared/http.ts";
import {
  LICENSE_CURRENCY,
  LICENSE_PRICE_COP,
  normalizeEmail,
  parseExpectedLiveMode,
  PRODUCT,
  validEmail,
  UUID_PATTERN,
} from "../_shared/license.ts";

type CreatedOrder = {
  order_id: string;
  checkout_token: string;
};

type MercadoPagoPreference = {
  id?: unknown;
  init_point?: unknown;
  sandbox_init_point?: unknown;
};

function publicSiteUrl() {
  const configured = (Deno.env.get("ZAETTA_SITE_URL") ?? "https://zaettasoftware.com").trim();
  const url = new URL(configured);
  if (url.protocol !== "https:" && url.hostname !== "localhost" && url.hostname !== "127.0.0.1") {
    throw new Error("ZAETTA_SITE_URL must use HTTPS");
  }
  return url.origin;
}

Deno.serve(async (request: Request) => {
  const origin = websiteOrigin(request);
  if (!origin) return jsonResponse({ error: "Origin not allowed" }, 403);

  if (request.method === "OPTIONS") {
    return new Response(null, { status: 204, headers: corsHeaders(origin) });
  }
  if (request.method !== "POST") {
    return jsonResponse({ error: "Method not allowed" }, 405, origin);
  }

  let body: Record<string, unknown>;
  try {
    body = await readJsonBody(request);
  } catch (error) {
    const tooLarge = error instanceof Error && error.message === "REQUEST_TOO_LARGE";
    return jsonResponse(
      { error: tooLarge ? "La solicitud es demasiado grande." : "Solicitud no válida." },
      tooLarge ? 413 : 400,
      origin,
    );
  }

  if (typeof body.website === "string" && body.website.trim()) {
    return jsonResponse({ ok: true }, 201, origin);
  }

  const email = normalizeEmail(body.email);
  if (!validEmail(email)) {
    return jsonResponse({ error: "Ingresa un correo válido." }, 400, origin);
  }
  if (body.privacyAccepted !== true || body.termsAccepted !== true) {
    return jsonResponse({ error: "Debes aceptar la privacidad y los términos de compra." }, 400, origin);
  }

  const accessToken = Deno.env.get("MERCADOPAGO_ACCESS_TOKEN") ?? "";
  if (!accessToken) {
    return jsonResponse({ error: "El pago todavía no está disponible." }, 503, origin);
  }

  let order: CreatedOrder | null = null;
  try {
    const created = await callRpc<CreatedOrder[]>("create_license_order", {
      p_email: email,
      p_privacy_accepted: true,
      p_terms_accepted: true,
      p_source: "website",
    });
    order = Array.isArray(created) ? created[0] ?? null : null;

    if (
      !order ||
      !UUID_PATTERN.test(order.order_id) ||
      !UUID_PATTERN.test(order.checkout_token)
    ) {
      throw new Error("Invalid order response");
    }

    const siteUrl = publicSiteUrl();
    const resultUrl = `${siteUrl}/?licencia=resultado&order=${encodeURIComponent(order.order_id)}`;
    const preferenceResponse = await fetch("https://api.mercadopago.com/checkout/preferences", {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${accessToken}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        items: [{
          id: "zaetta-capture-personal",
          title: "Zaetta Capture - Licencia solidaria",
          description: "Licencia personal de pago único para Windows",
          quantity: 1,
          currency_id: LICENSE_CURRENCY,
          unit_price: LICENSE_PRICE_COP,
        }],
        payer: { email },
        external_reference: order.order_id,
        back_urls: {
          success: `${resultUrl}&resultado=approved`,
          pending: `${resultUrl}&resultado=pending`,
          failure: `${resultUrl}&resultado=failure`,
        },
        auto_return: "approved",
        statement_descriptor: "ZAETTA CAPTURE",
        metadata: {
          product: PRODUCT,
          order_id: order.order_id,
        },
      }),
    });

    const preference = await preferenceResponse.json() as MercadoPagoPreference;
    if (!preferenceResponse.ok) {
      throw new Error(`Mercado Pago preference failed with ${preferenceResponse.status}`);
    }

    const preferenceId = typeof preference.id === "string" ? preference.id : "";
    const initPointValue = parseExpectedLiveMode()
      ? preference.init_point
      : preference.sandbox_init_point ?? preference.init_point;
    const initPoint = typeof initPointValue === "string" ? initPointValue : "";
    const parsedInitPoint = new URL(initPoint);

    if (
      !preferenceId ||
      parsedInitPoint.protocol !== "https:" ||
      !(
        parsedInitPoint.hostname === "mercadopago.com" ||
        parsedInitPoint.hostname.endsWith(".mercadopago.com") ||
        parsedInitPoint.hostname === "mercadopago.com.co" ||
        parsedInitPoint.hostname.endsWith(".mercadopago.com.co")
      )
    ) {
      throw new Error("Mercado Pago returned an invalid preference");
    }

    const attached = await callRpc<boolean>("attach_license_preference", {
      p_order_id: order.order_id,
      p_checkout_token: order.checkout_token,
      p_preference_id: preferenceId,
    });
    if (attached !== true) throw new Error("Preference could not be attached to order");

    return jsonResponse({
      ok: true,
      orderId: order.order_id,
      checkoutToken: order.checkout_token,
      initPoint,
      amount: LICENSE_PRICE_COP,
      currency: LICENSE_CURRENCY,
    }, 201, origin);
  } catch (error) {
    console.error("License checkout failed", error);

    if (order) {
      try {
        await callRpc("mark_license_checkout_error", {
          p_order_id: order.order_id,
          p_checkout_token: order.checkout_token,
          p_error: error instanceof Error ? error.message : "Unknown checkout error",
        });
      } catch (markError) {
        console.error("Checkout error could not be recorded", markError);
      }
    }

    const rateLimited = error instanceof Error && error.message.includes("rate limit");
    return jsonResponse({
      error: rateLimited
        ? "Realizaste varios intentos. Espera unos minutos antes de continuar."
        : "No pudimos iniciar el pago. Inténtalo nuevamente.",
    }, rateLimited ? 429 : 503, origin);
  }
});
