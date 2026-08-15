const ALLOWED_ORIGINS = new Set([
  "https://www.zaettasoftware.com",
  "https://zaettasoftware.com",
  "https://zaettacapture.vercel.app",
]);

const ALLOWED_CATEGORIES = new Set([
  "feature",
  "experience",
  "bug",
  "integration",
  "other",
]);

function corsHeaders(origin: string) {
  return {
    "Access-Control-Allow-Origin": origin,
    "Access-Control-Allow-Headers": "content-type",
    "Access-Control-Allow-Methods": "POST, OPTIONS",
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
    },
  });
}

function getDatabaseKey() {
  const modernKeys = Deno.env.get("SUPABASE_SECRET_KEYS");

  if (modernKeys) {
    const parsed = JSON.parse(modernKeys);
    if (parsed.default) return String(parsed.default);
  }

  return Deno.env.get("SUPABASE_SERVICE_ROLE_KEY") ?? "";
}

function optionalText(value: unknown, maxLength: number) {
  if (typeof value !== "string") return null;
  const cleaned = value.trim().replace(/\s+/g, " ");
  return cleaned ? cleaned.slice(0, maxLength) : null;
}

Deno.serve(async (request: Request) => {
  const origin = request.headers.get("Origin") ?? "";

  if (!ALLOWED_ORIGINS.has(origin)) {
    return jsonResponse({ error: "Origin not allowed" }, 403);
  }

  if (request.method === "OPTIONS") {
    return new Response(null, { status: 204, headers: corsHeaders(origin) });
  }

  if (request.method !== "POST") {
    return jsonResponse({ error: "Method not allowed" }, 405, origin);
  }

  const contentLength = Number(request.headers.get("Content-Length") || 0);
  if (contentLength > 10000) {
    return jsonResponse({ error: "Request too large" }, 413, origin);
  }

  let payload: Record<string, unknown>;

  try {
    payload = await request.json();
  } catch {
    return jsonResponse({ error: "Invalid request" }, 400, origin);
  }

  if (typeof payload.website === "string" && payload.website.trim()) {
    return jsonResponse({ ok: true }, 201, origin);
  }

  if (payload.privacyAccepted !== true) {
    return jsonResponse({
      error: "Debes aceptar la política de privacidad para enviar tu idea.",
    }, 400, origin);
  }

  const name = optionalText(payload.name, 80);
  const email = optionalText(payload.email, 254)?.toLowerCase() ?? null;
  const category = typeof payload.category === "string" ? payload.category : "";
  const message = typeof payload.message === "string" ? payload.message.trim() : "";

  if (!ALLOWED_CATEGORIES.has(category)) {
    return jsonResponse({ error: "Selecciona un tipo de propuesta válido." }, 400, origin);
  }

  if (message.length < 20 || message.length > 1200) {
    return jsonResponse({ error: "La propuesta debe tener entre 20 y 1.200 caracteres." }, 400, origin);
  }

  if (email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    return jsonResponse({ error: "Ingresa un correo válido." }, 400, origin);
  }

  const projectUrl = Deno.env.get("SUPABASE_URL") ?? "";
  const databaseKey = getDatabaseKey();

  if (!projectUrl || !databaseKey) {
    return jsonResponse({ error: "El buzón de ideas no está disponible." }, 503, origin);
  }

  const headers: Record<string, string> = {
    "apikey": databaseKey,
    "Content-Type": "application/json",
    "Prefer": "return=minimal",
  };

  if (!databaseKey.startsWith("sb_secret_")) {
    headers.Authorization = `Bearer ${databaseKey}`;
  }

  try {
    const response = await fetch(`${projectUrl}/rest/v1/product_feedback`, {
      method: "POST",
      headers,
      body: JSON.stringify({
        product: "zaetta-capture",
        name,
        email,
        category,
        message,
        source: "website",
        status: "new",
      }),
    });

    if (!response.ok) {
      console.error("Feedback insert failed", response.status, await response.text());
      return jsonResponse({ error: "No pudimos guardar tu propuesta." }, 503, origin);
    }

    return jsonResponse({ ok: true }, 201, origin);
  } catch (error) {
    console.error("Feedback request failed", error);
    return jsonResponse({ error: "No pudimos guardar tu propuesta." }, 503, origin);
  }
});
