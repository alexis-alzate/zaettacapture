import { callRpc, savePaymentEvent } from "../_shared/database.ts";
import { createLicenseKey, LICENSE_CURRENCY, LICENSE_PRICE_COP, parseExpectedLiveMode, UUID_PATTERN } from "../_shared/license.ts";

type MercadoPagoNotification = {
  id?: unknown;
  type?: unknown;
  action?: unknown;
  data?: { id?: unknown };
};

type MercadoPagoPayment = {
  id?: unknown;
  status?: unknown;
  status_detail?: unknown;
  external_reference?: unknown;
  transaction_amount?: unknown;
  currency_id?: unknown;
  live_mode?: unknown;
  fee_details?: Array<{ amount?: unknown }>;
};

type ApprovedOrder = {
  order_id: string;
  checkout_token: string;
  license_key: string;
};

type EmailClaim = {
  order_id: string;
  buyer_email: string;
  license_key: string;
};

function jsonResponse(body: unknown, status: number) {
  return new Response(JSON.stringify(body), {
    status,
    headers: {
      "Content-Type": "application/json; charset=utf-8",
      "Cache-Control": "no-store",
      "X-Content-Type-Options": "nosniff",
    },
  });
}

function signatureParts(value: string) {
  const parts = new Map<string, string>();
  for (const segment of value.split(",")) {
    const separator = segment.indexOf("=");
    if (separator <= 0) continue;
    parts.set(segment.slice(0, separator).trim(), segment.slice(separator + 1).trim());
  }
  return { timestamp: parts.get("ts") ?? "", signature: parts.get("v1") ?? "" };
}

function hexBytes(value: string) {
  if (!/^[0-9a-f]{64}$/i.test(value)) return null;
  const bytes = new Uint8Array(value.length / 2);
  for (let index = 0; index < value.length; index += 2) {
    bytes[index / 2] = Number.parseInt(value.slice(index, index + 2), 16);
  }
  return bytes;
}

async function validWebhookSignature(request: Request, dataId: string) {
  const secret = Deno.env.get("MERCADOPAGO_WEBHOOK_SECRET") ?? "";
  const requestId = request.headers.get("x-request-id") ?? "";
  const rawSignature = request.headers.get("x-signature") ?? "";
  const { timestamp, signature } = signatureParts(rawSignature);
  const signatureBytes = hexBytes(signature);

  if (!secret || !requestId || !timestamp || !/^\d{10,16}$/.test(timestamp) || !signatureBytes) {
    return false;
  }

  const manifest = `id:${dataId.toLowerCase()};request-id:${requestId};ts:${timestamp};`;
  const key = await crypto.subtle.importKey(
    "raw",
    new TextEncoder().encode(secret),
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["verify"],
  );

  return crypto.subtle.verify(
    "HMAC",
    key,
    signatureBytes,
    new TextEncoder().encode(manifest),
  );
}

function textValue(value: unknown, maxLength: number) {
  return typeof value === "string" ? value.slice(0, maxLength) : "";
}

function paymentFee(payment: MercadoPagoPayment) {
  if (!Array.isArray(payment.fee_details)) return null;
  const total = payment.fee_details.reduce((sum, fee) => {
    const amount = Number(fee?.amount);
    return Number.isFinite(amount) && amount > 0 ? sum + amount : sum;
  }, 0);
  return Math.round(total);
}

function escapeHtml(value: string) {
  return value.replace(/[&<>'"]/g, (character) => {
    if (character === "&") return "&amp;";
    if (character === "<") return "&lt;";
    if (character === ">") return "&gt;";
    if (character === "'") return "&#39;";
    return "&quot;";
  });
}

async function sendLicenseEmail(claim: EmailClaim) {
  const apiKey = Deno.env.get("RESEND_API_KEY") ?? "";
  const from = Deno.env.get("ZAETTA_LICENSE_FROM_EMAIL") ?? "";
  const supportEmail = Deno.env.get("ZAETTA_SUPPORT_EMAIL") ?? "soporte@zaettasoftware.com";
  if (!apiKey || !from) throw new Error("Transactional email is not configured");

  const safeKey = escapeHtml(claim.license_key);
  const safeOrder = escapeHtml(claim.order_id);
  const safeSupport = escapeHtml(supportEmail);
  const response = await fetch("https://api.resend.com/emails", {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${apiKey}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      from,
      to: [claim.buyer_email],
      subject: "Tu licencia solidaria de Zaetta Capture",
      html: `
        <div style="background:#080909;color:#f7f7f2;font-family:Arial,sans-serif;padding:32px">
          <div style="max-width:620px;margin:auto;border:1px solid #4f4215;border-radius:18px;padding:30px;background:#111311">
            <p style="color:#f4c430;font-weight:700;letter-spacing:.08em">ZAETTA CAPTURE</p>
            <h1 style="font-size:28px;margin:12px 0">Tu licencia está activa</h1>
            <p>Gracias por adquirir la licencia solidaria de Zaetta Capture.</p>
            <div style="margin:24px 0;padding:18px;border-radius:12px;background:#070807;border:1px solid #685619;text-align:center">
              <span style="display:block;color:#a7aaa4;font-size:13px;margin-bottom:8px">CLAVE DE LICENCIA</span>
              <strong style="font-size:22px;letter-spacing:.08em;color:#ffd43b">${safeKey}</strong>
            </div>
            <p><strong>$10.000 COP</strong> quedaron reservados para el compromiso solidario de Zaetta. Cuando se realice la entrega a la causa seleccionada, publicaremos el resultado y el comprobante correspondiente.</p>
            <p style="color:#a7aaa4;font-size:13px">Orden: ${safeOrder}</p>
            <p style="color:#a7aaa4;font-size:13px">Si necesitas ayuda, escribe a ${safeSupport}.</p>
          </div>
        </div>`,
    }),
  });

  if (!response.ok) {
    throw new Error(`Resend failed with ${response.status}: ${await response.text()}`);
  }
}

async function deliverLicenseEmail(orderId: string) {
  const claims = await callRpc<EmailClaim[]>("claim_license_email", { p_order_id: orderId });
  const claim = Array.isArray(claims) ? claims[0] ?? null : null;
  if (!claim) return;

  try {
    await sendLicenseEmail(claim);
    await callRpc("complete_license_email", {
      p_order_id: orderId,
      p_success: true,
      p_error: null,
    });
  } catch (error) {
    const message = error instanceof Error ? error.message : "Email delivery failed";
    await callRpc("complete_license_email", {
      p_order_id: orderId,
      p_success: false,
      p_error: message,
    });
    throw error;
  }
}

Deno.serve(async (request: Request) => {
  if (request.method !== "POST") return jsonResponse({ error: "Method not allowed" }, 405);

  let body: MercadoPagoNotification;
  try {
    body = JSON.parse(await request.text()) as MercadoPagoNotification;
  } catch {
    return jsonResponse({ error: "Invalid JSON" }, 400);
  }

  const requestUrl = new URL(request.url);
  const dataId = String(requestUrl.searchParams.get("data.id") ?? body.data?.id ?? "").trim();
  const eventType = String(requestUrl.searchParams.get("type") ?? body.type ?? "").trim();
  const providerEventId = body.id == null ? null : String(body.id);
  const action = body.action == null ? null : String(body.action);

  if (!dataId || eventType !== "payment") {
    return jsonResponse({ received: true, ignored: true }, 200);
  }

  if (!await validWebhookSignature(request, dataId)) {
    await savePaymentEvent({
      providerEventId,
      paymentId: dataId,
      eventType,
      action,
      outcome: "invalid_signature",
      processed: true,
    });
    return jsonResponse({ error: "Invalid signature" }, 401);
  }

  const accessToken = Deno.env.get("MERCADOPAGO_ACCESS_TOKEN") ?? "";
  if (!accessToken) return jsonResponse({ error: "Webhook is not configured" }, 503);

  try {
    const paymentResponse = await fetch(
      `https://api.mercadopago.com/v1/payments/${encodeURIComponent(dataId)}`,
      { headers: { "Authorization": `Bearer ${accessToken}` } },
    );

    if (paymentResponse.status === 404) {
      await savePaymentEvent({
        providerEventId,
        paymentId: dataId,
        eventType,
        action,
        outcome: "payment_not_found",
        processed: true,
      });
      return jsonResponse({ received: true }, 200);
    }
    if (!paymentResponse.ok) {
      throw new Error(`Mercado Pago payment lookup failed with ${paymentResponse.status}`);
    }

    const payment = await paymentResponse.json() as MercadoPagoPayment;
    const paymentId = String(payment.id ?? dataId);
    const orderId = textValue(payment.external_reference, 100);
    const status = textValue(payment.status, 40);
    const statusDetail = textValue(payment.status_detail, 120);
    const amount = Number(payment.transaction_amount);
    const currency = textValue(payment.currency_id, 10);
    const liveMode = payment.live_mode === true;

    if (!UUID_PATTERN.test(orderId)) throw new Error("Invalid external reference");
    if (!Number.isSafeInteger(amount) || amount !== LICENSE_PRICE_COP || currency !== LICENSE_CURRENCY) {
      throw new Error("Payment amount or currency mismatch");
    }
    if (liveMode !== parseExpectedLiveMode()) {
      throw new Error("Payment live mode mismatch");
    }

    if (status === "approved") {
      const approved = await callRpc<ApprovedOrder[]>("approve_license_order", {
        p_order_id: orderId,
        p_payment_id: paymentId,
        p_status_detail: statusDetail,
        p_amount: amount,
        p_currency: currency,
        p_live_mode: liveMode,
        p_gateway_fee: paymentFee(payment),
        p_license_key: createLicenseKey(),
      });
      if (!Array.isArray(approved) || !approved[0]) throw new Error("Order approval returned no data");

      await deliverLicenseEmail(orderId);
    } else if (["rejected", "cancelled", "refunded", "charged_back"].includes(status)) {
      await callRpc("record_license_payment_state", {
        p_order_id: orderId,
        p_payment_id: paymentId,
        p_status: status,
        p_status_detail: statusDetail,
        p_live_mode: liveMode,
      });
    }

    await savePaymentEvent({
      providerEventId,
      paymentId,
      eventType,
      action,
      outcome: status || "unknown",
      detail: statusDetail,
      processed: true,
    });
    return jsonResponse({ received: true }, 200);
  } catch (error) {
    const message = error instanceof Error ? error.message : "Webhook processing failed";
    console.error("Mercado Pago webhook failed", error);
    await savePaymentEvent({
      providerEventId,
      paymentId: dataId,
      eventType,
      action,
      outcome: "processing_error",
      detail: message,
      processed: false,
    });
    return jsonResponse({ error: "Webhook processing failed" }, 500);
  }
});
