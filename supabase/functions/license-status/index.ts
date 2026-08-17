import { callRpc } from "../_shared/database.ts";
import { corsHeaders, jsonResponse, readJsonBody, websiteOrigin } from "../_shared/http.ts";
import { maskEmail, UUID_PATTERN } from "../_shared/license.ts";

type LicenseStatus = {
  status: string;
  buyer_email: string;
  total_amount: number;
  currency: string;
  license_key: string | null;
  license_status: string | null;
  solidarity_status: string | null;
  committed_amount: number | null;
};

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
    body = await readJsonBody(request, 3000);
  } catch {
    return jsonResponse({ error: "Solicitud no válida." }, 400, origin);
  }

  const orderId = typeof body.orderId === "string" ? body.orderId : "";
  const checkoutToken = typeof body.checkoutToken === "string" ? body.checkoutToken : "";
  if (!UUID_PATTERN.test(orderId) || !UUID_PATTERN.test(checkoutToken)) {
    return jsonResponse({ error: "No pudimos identificar la compra." }, 400, origin);
  }

  try {
    const result = await callRpc<LicenseStatus[]>("get_license_order_status", {
      p_order_id: orderId,
      p_checkout_token: checkoutToken,
    });
    const order = Array.isArray(result) ? result[0] ?? null : null;
    if (!order) return jsonResponse({ error: "Compra no encontrada." }, 404, origin);

    return jsonResponse({
      status: order.status,
      buyerEmail: maskEmail(order.buyer_email),
      amount: order.total_amount,
      currency: order.currency,
      licenseKey: order.status === "approved" ? order.license_key : null,
      licenseStatus: order.license_status,
      solidarityStatus: order.solidarity_status,
      committedAmount: order.committed_amount,
    }, 200, origin);
  } catch (error) {
    console.error("License status failed", error);
    return jsonResponse({ error: "No pudimos consultar la compra." }, 503, origin);
  }
});
