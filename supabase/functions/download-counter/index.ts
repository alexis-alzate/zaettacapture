const ALLOWED_ORIGINS = new Set([
  "https://www.zaettasoftware.com",
  "https://zaettasoftware.com",
  "https://zaettacapture.vercel.app",
]);

const COUNTER_KEY = "zaetta-capture";

function corsHeaders(origin: string) {
  return {
    "Access-Control-Allow-Origin": origin,
    "Access-Control-Allow-Headers": "content-type",
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

Deno.serve(async (request: Request) => {
  const origin = request.headers.get("Origin") ?? "";

  if (!ALLOWED_ORIGINS.has(origin)) {
    return jsonResponse({ error: "Origin not allowed" }, 403);
  }

  if (request.method === "OPTIONS") {
    return new Response(null, { status: 204, headers: corsHeaders(origin) });
  }

  if (request.method !== "GET" && request.method !== "POST") {
    return jsonResponse({ error: "Method not allowed" }, 405, origin);
  }

  const projectUrl = Deno.env.get("SUPABASE_URL") ?? "";
  const databaseKey = getDatabaseKey();

  if (!projectUrl || !databaseKey) {
    return jsonResponse({ error: "Counter service unavailable" }, 503, origin);
  }

  const rpcName = request.method === "POST"
    ? "increment_download_counter"
    : "get_download_counter";
  const headers: Record<string, string> = {
    "apikey": databaseKey,
    "Content-Type": "application/json",
  };

  if (!databaseKey.startsWith("sb_secret_")) {
    headers.Authorization = `Bearer ${databaseKey}`;
  }

  try {
    const response = await fetch(`${projectUrl}/rest/v1/rpc/${rpcName}`, {
      method: "POST",
      headers,
      body: JSON.stringify({ p_key: COUNTER_KEY }),
    });

    if (!response.ok) {
      console.error("Counter RPC failed", response.status, await response.text());
      return jsonResponse({ error: "Counter service unavailable" }, 503, origin);
    }

    const count = Number(await response.json());

    if (!Number.isSafeInteger(count) || count < 0) {
      return jsonResponse({ error: "Invalid counter value" }, 503, origin);
    }

    return jsonResponse({ count }, 200, origin);
  } catch (error) {
    console.error("Counter request failed", error);
    return jsonResponse({ error: "Counter service unavailable" }, 503, origin);
  }
});
