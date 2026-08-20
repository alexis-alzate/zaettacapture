import { NextRequest, NextResponse } from "next/server";

const ENDPOINT = "https://ocnoiraaqosfmbluccba.supabase.co/functions/v1/ideas-site-admin";

type Payload = {
  action?: "dashboard" | "list" | "update";
  id?: number;
  status?: string;
  priority?: string;
  adminNote?: string;
};

function fromBase64(value: string) {
  return Uint8Array.from(atob(value), (char) => char.charCodeAt(0));
}

function toBase64Url(value: ArrayBuffer) {
  const bytes = new Uint8Array(value);
  let raw = "";
  for (const byte of bytes) raw += String.fromCharCode(byte);
  return btoa(raw).replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "");
}

async function getPrivateKey() {
  let encoded = process.env.ZAETTA_SIGNING_PRIVATE_KEY ?? "";
  if (!encoded) {
    try {
      const runtime = await import("cloudflare:workers");
      const workerEnv = runtime.env as unknown as { ZAETTA_SIGNING_PRIVATE_KEY?: string };
      encoded = workerEnv.ZAETTA_SIGNING_PRIVATE_KEY ?? "";
    } catch {
      encoded = "";
    }
  }
  if (!encoded) return null;
  return crypto.subtle.importKey("pkcs8", fromBase64(encoded), { name: "Ed25519" }, false, ["sign"]);
}

function respond(body: Record<string, unknown>, status: number) {
  const response = NextResponse.json(body, { status });
  response.headers.set("Cache-Control", "no-store");
  return response;
}

export async function POST(request: NextRequest) {
  const requestOrigin = request.headers.get("origin");
  if (requestOrigin && requestOrigin !== request.nextUrl.origin) {
    return respond({ error: "Origen no permitido." }, 403);
  }

  const contentLength = Number(request.headers.get("content-length") ?? 0);
  if (contentLength > 16_000) return respond({ error: "Solicitud demasiado grande." }, 413);

  let input: Payload;
  try {
    input = (await request.json()) as Payload;
  } catch {
    return respond({ error: "Solicitud no válida." }, 400);
  }

  if (input.action !== "dashboard" && input.action !== "list" && input.action !== "update") {
    return respond({ error: "Acción no permitida." }, 400);
  }

  const payload: Record<string, unknown> = { action: input.action };
  if (input.action === "update") {
    payload.id = input.id;
    payload.status = input.status;
    payload.priority = input.priority;
    payload.adminNote = input.adminNote;
  }

  const privateKey = await getPrivateKey();
  if (!privateKey) return respond({ error: "La conexión privada aún no está configurada." }, 503);

  const body = JSON.stringify(payload);
  const timestamp = String(Date.now());
  const nonce = crypto.randomUUID();
  const message = new TextEncoder().encode(`${timestamp}.${nonce}.${body}`);
  const signature = toBase64Url(await crypto.subtle.sign("Ed25519", privateKey, message));

  let result: Response;
  try {
    result = await fetch(ENDPOINT, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "x-zaetta-timestamp": timestamp,
        "x-zaetta-nonce": nonce,
        "x-zaetta-signature": signature,
      },
      body,
      cache: "no-store",
    });
  } catch {
    return respond({ error: "No pudimos conectar con la bandeja de ideas." }, 503);
  }

  let data: Record<string, unknown>;
  try {
    data = (await result.json()) as Record<string, unknown>;
  } catch {
    data = { error: "La respuesta del servidor no es válida." };
  }
  return respond(data, result.status);
}
