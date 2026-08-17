import { callRpc } from "../_shared/database.ts";
import { jsonResponse, readJsonBody } from "../_shared/http.ts";
import { normalizeEmail, validEmail } from "../_shared/license.ts";

type TrialResult = {
  started_at: string;
  expires_at: string;
};

function normalizedFingerprint(value: unknown) {
  return typeof value === "string" ? value.trim() : "";
}

function validFingerprint(value: string) {
  return value.length >= 32 && value.length <= 128;
}

Deno.serve(async (request: Request) => {
  if (request.method !== "POST") {
    return jsonResponse({ error: "Method not allowed" }, 405);
  }

  let body: Record<string, unknown>;
  try {
    body = await readJsonBody(request, 2000);
  } catch {
    return jsonResponse({ error: "Solicitud no válida." }, 400);
  }

  const deviceFingerprint = normalizedFingerprint(body.deviceFingerprint);
  const fingerprintSource = typeof body.fingerprintSource === "string" ? body.fingerprintSource : "";
  const email = normalizeEmail(body.email);

  if (!validFingerprint(deviceFingerprint)) {
    return jsonResponse({ error: "No pudimos identificar este equipo." }, 400);
  }
  if (fingerprintSource !== "hardware" && fingerprintSource !== "machine_guid_fallback") {
    return jsonResponse({ error: "No pudimos identificar este equipo." }, 400);
  }
  if (!validEmail(email)) {
    return jsonResponse({ error: "Ingresa un correo válido." }, 400);
  }

  try {
    const result = await callRpc<TrialResult[]>("register_trial_device", {
      p_device_fingerprint: deviceFingerprint,
      p_fingerprint_source: fingerprintSource,
      p_email: email,
    });
    const trial = Array.isArray(result) ? result[0] ?? null : null;
    if (!trial) throw new Error("Trial registration returned no data");

    return jsonResponse({
      startedAt: trial.started_at,
      expiresAt: trial.expires_at,
    }, 200);
  } catch (error) {
    console.error("Trial start failed", error);
    return jsonResponse({ error: "No pudimos iniciar la prueba gratuita." }, 503);
  }
});
