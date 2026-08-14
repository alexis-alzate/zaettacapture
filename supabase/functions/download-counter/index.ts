const ALLOWED_ORIGINS = new Set([
  "https://www.zaettasoftware.com",
  "https://zaettasoftware.com",
  "https://zaettacapture.vercel.app",
]);

const COUNTER_KEY = "zaetta-capture";
const INSTALLER_URL = "https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.29/ZaettaCaptureSetup.exe";

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

async function runCounterRpc(rpcName: string) {
  const projectUrl = Deno.env.get("SUPABASE_URL") ?? "";
  const databaseKey = getDatabaseKey();

  if (!projectUrl || !databaseKey) {
    throw new Error("Counter service unavailable");
  }

  const headers: Record<string, string> = {
    "apikey": databaseKey,
    "Content-Type": "application/json",
  };

  if (!databaseKey.startsWith("sb_secret_")) {
    headers.Authorization = `Bearer ${databaseKey}`;
  }

  const response = await fetch(`${projectUrl}/rest/v1/rpc/${rpcName}`, {
    method: "POST",
    headers,
    body: JSON.stringify({ p_key: COUNTER_KEY }),
  });

  if (!response.ok) {
    throw new Error(`Counter RPC failed with ${response.status}: ${await response.text()}`);
  }

  const count = Number(await response.json());

  if (!Number.isSafeInteger(count) || count < 0) {
    throw new Error("Invalid counter value");
  }

  return count;
}

Deno.serve(async (request: Request) => {
  const requestUrl = new URL(request.url);
  const isDownloadRequest = request.method === "GET" && requestUrl.searchParams.get("download") === "1";

  if (isDownloadRequest) {
    try {
      await runCounterRpc("increment_download_counter");
    } catch (error) {
      console.error("Download counter increment failed", error);
    }

    return new Response(null, {
      status: 302,
      headers: {
        "Location": INSTALLER_URL,
        "Cache-Control": "no-store, max-age=0",
      },
    });
  }

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

  const rpcName = request.method === "POST"
    ? "increment_download_counter"
    : "get_download_counter";

  try {
    const count = await runCounterRpc(rpcName);
    return jsonResponse({ count }, 200, origin);
  } catch (error) {
    console.error("Counter request failed", error);
    return jsonResponse({ error: "Counter service unavailable" }, 503, origin);
  }
});
