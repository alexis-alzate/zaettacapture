import { callRpc } from "../_shared/database.ts";
import { jsonResponse, readJsonBody } from "../_shared/http.ts";
import { UUID_PATTERN } from "../_shared/license.ts";

type ActivationResult = {
  activated: boolean;
  license_status: string;
  max_devices: number;
  active_device_count: number;
};

type DeviceRow = {
  device_id: string;
  device_name: string | null;
  activated_at: string;
};

function trimmed(value: unknown, maxLength: number) {
  return typeof value === "string" ? value.trim().slice(0, maxLength) : "";
}

function validFingerprint(value: string) {
  return value.length >= 32 && value.length <= 128;
}

async function handleActivate(body: Record<string, unknown>) {
  const licenseKey = trimmed(body.licenseKey, 40);
  const deviceFingerprint = trimmed(body.deviceFingerprint, 128);
  const fingerprintSource = trimmed(body.fingerprintSource, 40);
  const deviceName = trimmed(body.deviceName, 80) || null;

  if (!licenseKey) {
    return jsonResponse({ error: "Ingresa una clave de licencia." }, 400);
  }
  if (!validFingerprint(deviceFingerprint)) {
    return jsonResponse({ error: "No pudimos identificar este equipo." }, 400);
  }
  if (fingerprintSource !== "hardware" && fingerprintSource !== "machine_guid_fallback") {
    return jsonResponse({ error: "No pudimos identificar este equipo." }, 400);
  }

  try {
    const result = await callRpc<ActivationResult[]>("activate_license_device", {
      p_license_key: licenseKey,
      p_device_fingerprint: deviceFingerprint,
      p_fingerprint_source: fingerprintSource,
      p_device_name: deviceName,
    });
    const activation = Array.isArray(result) ? result[0] ?? null : null;
    if (!activation) throw new Error("Activation returned no data");

    if (activation.activated) {
      return jsonResponse({
        activated: true,
        licenseStatus: activation.license_status,
        maxDevices: activation.max_devices,
        activeDeviceCount: activation.active_device_count,
      }, 200);
    }

    const devices = await callRpc<DeviceRow[]>("list_license_devices", {
      p_license_key: licenseKey,
    });

    return jsonResponse({
      activated: false,
      licenseStatus: activation.license_status,
      maxDevices: activation.max_devices,
      activeDeviceCount: activation.active_device_count,
      devices: (Array.isArray(devices) ? devices : []).map((device) => ({
        deviceId: device.device_id,
        deviceName: device.device_name,
        activatedAt: device.activated_at,
      })),
    }, 200);
  } catch (error) {
    const message = error instanceof Error ? error.message : "";
    if (message.includes("License not found")) {
      return jsonResponse({ error: "No encontramos esa licencia." }, 404);
    }
    console.error("License activation failed", error);
    return jsonResponse({ error: "No pudimos activar la licencia." }, 503);
  }
}

async function handleDeactivate(body: Record<string, unknown>) {
  const licenseKey = trimmed(body.licenseKey, 40);
  const deviceId = trimmed(body.deviceId, 36);

  if (!licenseKey) {
    return jsonResponse({ error: "Ingresa una clave de licencia." }, 400);
  }
  if (!UUID_PATTERN.test(deviceId)) {
    return jsonResponse({ error: "No pudimos identificar ese dispositivo." }, 400);
  }

  try {
    const deactivated = await callRpc<boolean>("deactivate_license_device", {
      p_license_key: licenseKey,
      p_device_id: deviceId,
    });
    return jsonResponse({ deactivated: deactivated === true }, 200);
  } catch (error) {
    console.error("License device deactivation failed", error);
    return jsonResponse({ error: "No pudimos desactivar ese dispositivo." }, 503);
  }
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

  const action = typeof body.action === "string" ? body.action : "activate";

  if (action === "deactivate") {
    return handleDeactivate(body);
  }
  if (action === "activate") {
    return handleActivate(body);
  }
  return jsonResponse({ error: "Acción no reconocida." }, 400);
});
