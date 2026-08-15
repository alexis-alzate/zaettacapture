const ALLOWED_ORIGINS = new Set([
  "https://www.zaettasoftware.com",
  "https://zaettasoftware.com",
  "https://zaettacapture.vercel.app",
  "http://localhost:4173",
  "http://127.0.0.1:4173",
  "http://localhost:8080",
  "http://127.0.0.1:8080",
]);

const COUNTER_KEY = "zaetta-capture";
const PRODUCT = "zaetta-capture";
const CURRENT_VERSION = "1.0.29";
const INSTALLER_URL = "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.29/ZaettaCaptureSetup.exe";
const WEBSITE_DOWNLOAD_URL = "https://zaettasoftware.com/?descargar=1";
const PUBLIC_FUNCTION_URL = "https://ocnoiraaqosfmbluccba.supabase.co/functions/v1/download-counter";
const TOKEN_LIFETIME_MS = 10 * 60 * 1000;
const ENFORCE_EMAIL_GATE = true;
const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

type DownloadTokenPayload = {
  leadId: string;
  expiresAt: number;
  nonce: string;
};

function corsHeaders(origin: string) {
  return {
    "Access-Control-Allow-Origin": origin,
    "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type",
    "Access-Control-Allow-Methods": "GET, POST, OPTIONS",
    "Access-Control-Max-Age": "86400",
    "Vary": "Origin",
  };
}

function jsonResponse(body: unknown, status: number, origin = "") {
  return new Response(JSON.stringify(body), {
    status,
    headers: {
      ...(origin ? corsHeaders(origin) : {}),
      "Content-Type": "application/json; charset=utf-8",
      "Cache-Control": "no-store",
      "X-Content-Type-Options": "nosniff",
    },
  });
}

function redirectResponse(location: string, status = 302) {
  return new Response(null, {
    status,
    headers: {
      "Location": location,
      "Cache-Control": "no-store, max-age=0",
      "Referrer-Policy": "no-referrer",
      "X-Content-Type-Options": "nosniff",
    },
  });
}

function getDatabaseKey() {
  const modernKeys = Deno.env.get("SUPABASE_SECRET_KEYS");

  if (modernKeys) {
    try {
      const parsed = JSON.parse(modernKeys);
      if (parsed.default) return String(parsed.default);
    } catch {
      console.error("SUPABASE_SECRET_KEYS is not valid JSON");
    }
  }

  return Deno.env.get("SUPABASE_SERVICE_ROLE_KEY") ?? "";
}

function databaseConfig() {
  const projectUrl = Deno.env.get("SUPABASE_URL") ?? "";
  const databaseKey = getDatabaseKey();

  if (!projectUrl || !databaseKey) {
    throw new Error("Download service unavailable");
  }

  const headers: Record<string, string> = {
    "apikey": databaseKey,
    "Content-Type": "application/json",
  };

  if (!databaseKey.startsWith("sb_secret_")) {
    headers.Authorization = `Bearer ${databaseKey}`;
  }

  return { projectUrl, databaseKey, headers };
}

async function callRpc(rpcName: string, body: Record<string, unknown>) {
  const { projectUrl, headers } = databaseConfig();
  const response = await fetch(`${projectUrl}/rest/v1/rpc/${rpcName}`, {
    method: "POST",
    headers,
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    throw new Error(`${rpcName} failed with ${response.status}: ${await response.text()}`);
  }

  return response.json();
}

async function getCounter() {
  const value = Number(await callRpc("get_download_counter", { p_key: COUNTER_KEY }));

  if (!Number.isSafeInteger(value) || value < 0) {
    throw new Error("Invalid counter value");
  }

  return value;
}

async function incrementLegacyCounter() {
  const value = Number(await callRpc("increment_download_counter", { p_key: COUNTER_KEY }));

  if (!Number.isSafeInteger(value) || value < 0) {
    throw new Error("Invalid counter value");
  }
}

async function registerLead(email: string, marketingConsent: boolean) {
  const result = await callRpc("register_download_lead", {
    p_email: email,
    p_version: CURRENT_VERSION,
    p_marketing_consent: marketingConsent,
    p_source: "website",
  });

  const leadId = Array.isArray(result) ? result[0]?.lead_id : null;
  if (typeof leadId !== "string" || !/^[0-9a-f-]{36}$/i.test(leadId)) {
    throw new Error("Invalid download lead response");
  }

  return leadId;
}

async function recordDownloadStart(leadId: string) {
  await callRpc("record_download_start", {
    p_lead_id: leadId,
    p_counter_key: COUNTER_KEY,
  });
}

function bytesToBase64Url(bytes: Uint8Array) {
  let binary = "";
  for (const byte of bytes) binary += String.fromCharCode(byte);
  return btoa(binary).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "");
}

function base64UrlToBytes(value: string) {
  const normalized = value.replace(/-/g, "+").replace(/_/g, "/");
  const padded = normalized + "=".repeat((4 - normalized.length % 4) % 4);
  const binary = atob(padded);
  return Uint8Array.from(binary, (character) => character.charCodeAt(0));
}

async function getSigningKey() {
  const { databaseKey } = databaseConfig();
  const material = new TextEncoder().encode(`zaetta-download-token-v1:${databaseKey}`);
  const digest = await crypto.subtle.digest("SHA-256", material);
  return crypto.subtle.importKey(
    "raw",
    digest,
    { name: "HMAC", hash: "SHA-256" },
    false,
    ["sign", "verify"],
  );
}

async function createDownloadToken(leadId: string) {
  const payload: DownloadTokenPayload = {
    leadId,
    expiresAt: Date.now() + TOKEN_LIFETIME_MS,
    nonce: crypto.randomUUID(),
  };
  const encodedPayload = bytesToBase64Url(
    new TextEncoder().encode(JSON.stringify(payload)),
  );
  const signature = await crypto.subtle.sign(
    "HMAC",
    await getSigningKey(),
    new TextEncoder().encode(encodedPayload),
  );
  return `${encodedPayload}.${bytesToBase64Url(new Uint8Array(signature))}`;
}

async function verifyDownloadToken(token: string) {
  const parts = token.split(".");
  if (parts.length !== 2 || token.length > 1200) return null;

  try {
    const valid = await crypto.subtle.verify(
      "HMAC",
      await getSigningKey(),
      base64UrlToBytes(parts[1]),
      new TextEncoder().encode(parts[0]),
    );
    if (!valid) return null;

    const payload = JSON.parse(
      new TextDecoder().decode(base64UrlToBytes(parts[0])),
    ) as Partial<DownloadTokenPayload>;
    const now = Date.now();

    if (
      typeof payload.leadId !== "string" ||
      !/^[0-9a-f-]{36}$/i.test(payload.leadId) ||
      typeof payload.expiresAt !== "number" ||
      payload.expiresAt < now ||
      payload.expiresAt > now + TOKEN_LIFETIME_MS + 60_000 ||
      typeof payload.nonce !== "string"
    ) {
      return null;
    }

    return payload as DownloadTokenPayload;
  } catch {
    return null;
  }
}

Deno.serve(async (request: Request) => {
  const requestUrl = new URL(request.url);
  const token = requestUrl.searchParams.get("token");

  if (request.method === "GET" && token) {
    const payload = await verifyDownloadToken(token);
    if (!payload) {
      return redirectResponse(WEBSITE_DOWNLOAD_URL, 303);
    }

    try {
      await recordDownloadStart(payload.leadId);
    } catch (error) {
      console.error("Download start could not be recorded", error);
    }

    return redirectResponse(INSTALLER_URL);
  }

  if (request.method === "GET" && requestUrl.searchParams.get("download") === "1") {
    if (ENFORCE_EMAIL_GATE) {
      return redirectResponse(WEBSITE_DOWNLOAD_URL, 303);
    }

    try {
      await incrementLegacyCounter();
    } catch (error) {
      console.error("Legacy download counter increment failed", error);
    }
    return redirectResponse(INSTALLER_URL);
  }

  const origin = request.headers.get("Origin") ?? "";
  if (!ALLOWED_ORIGINS.has(origin)) {
    return jsonResponse({ error: "Origin not allowed" }, 403);
  }

  if (request.method === "OPTIONS") {
    return new Response(null, { status: 204, headers: corsHeaders(origin) });
  }

  if (request.method === "GET") {
    try {
      return jsonResponse({ count: await getCounter() }, 200, origin);
    } catch (error) {
      console.error("Counter request failed", error);
      return jsonResponse({ error: "Counter service unavailable" }, 503, origin);
    }
  }

  if (request.method !== "POST") {
    return jsonResponse({ error: "Method not allowed" }, 405, origin);
  }

  const contentLength = Number(request.headers.get("Content-Length") || 0);
  if (contentLength > 5000) {
    return jsonResponse({ error: "Request too large" }, 413, origin);
  }

  let body: Record<string, unknown>;
  try {
    const rawBody = await request.text();
    if (rawBody.length > 5000) {
      return jsonResponse({ error: "Request too large" }, 413, origin);
    }
    body = JSON.parse(rawBody);
  } catch {
    return jsonResponse({ error: "Solicitud no válida." }, 400, origin);
  }

  if (typeof body.website === "string" && body.website.trim()) {
    return jsonResponse({ ok: true }, 201, origin);
  }

  if (body.privacyAccepted !== true) {
    return jsonResponse({
      error: "Debes aceptar la política de privacidad para continuar.",
    }, 400, origin);
  }

  const email = typeof body.email === "string" ? body.email.trim().toLowerCase() : "";
  if (email.length < 5 || email.length > 254 || !EMAIL_PATTERN.test(email)) {
    return jsonResponse({ error: "Ingresa un correo válido." }, 400, origin);
  }

  try {
    const leadId = await registerLead(email, body.marketingConsent === true);
    const downloadToken = await createDownloadToken(leadId);
    const downloadUrl = new URL(PUBLIC_FUNCTION_URL);
    downloadUrl.searchParams.set("token", downloadToken);

    return jsonResponse({
      ok: true,
      product: PRODUCT,
      version: CURRENT_VERSION,
      downloadUrl: downloadUrl.toString(),
    }, 201, origin);
  } catch (error) {
    console.error("Download registration failed", error);
    return jsonResponse({
      error: "No pudimos preparar tu descarga. Inténtalo nuevamente.",
    }, 503, origin);
  }
});
