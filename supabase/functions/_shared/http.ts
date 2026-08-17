export const ALLOWED_WEBSITE_ORIGINS = new Set([
  "https://www.zaettasoftware.com",
  "https://zaettasoftware.com",
  "https://zaettacapture.vercel.app",
  "http://localhost:4173",
  "http://127.0.0.1:4173",
  "http://localhost:8080",
  "http://127.0.0.1:8080",
  "http://localhost:8132",
  "http://127.0.0.1:8132",
]);

export function corsHeaders(origin: string) {
  return {
    "Access-Control-Allow-Origin": origin,
    "Access-Control-Allow-Headers": "content-type",
    "Access-Control-Allow-Methods": "POST, OPTIONS",
    "Access-Control-Max-Age": "86400",
    "Vary": "Origin",
  };
}

export function jsonResponse(body: unknown, status: number, origin = "") {
  return new Response(JSON.stringify(body), {
    status,
    headers: {
      ...(origin ? corsHeaders(origin) : {}),
      "Content-Type": "application/json; charset=utf-8",
      "Cache-Control": "no-store",
      "Referrer-Policy": "no-referrer",
      "X-Content-Type-Options": "nosniff",
      "X-Frame-Options": "DENY",
    },
  });
}

export function websiteOrigin(request: Request) {
  const origin = request.headers.get("Origin") ?? "";
  return ALLOWED_WEBSITE_ORIGINS.has(origin) ? origin : null;
}

export async function readJsonBody(request: Request, maxBytes = 5000) {
  const contentLength = Number(request.headers.get("Content-Length") || 0);
  if (contentLength > maxBytes) throw new Error("REQUEST_TOO_LARGE");

  const rawBody = await request.text();
  if (rawBody.length > maxBytes) throw new Error("REQUEST_TOO_LARGE");

  const parsed = JSON.parse(rawBody);
  if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) {
    throw new Error("INVALID_BODY");
  }
  return parsed as Record<string, unknown>;
}
