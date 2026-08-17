export const PRODUCT = "zaetta-capture";
export const LICENSE_PRICE_COP = 10_000;
export const LICENSE_CURRENCY = "COP";
export const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
export const UUID_PATTERN = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;

const LICENSE_ALPHABET = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

export function normalizeEmail(value: unknown) {
  if (typeof value !== "string") return "";
  return value.trim().toLowerCase();
}

export function validEmail(email: string) {
  return email.length >= 5 && email.length <= 254 && EMAIL_PATTERN.test(email);
}

export function createLicenseKey() {
  const random = new Uint8Array(15);
  crypto.getRandomValues(random);
  const characters = Array.from(
    random,
    (byte) => LICENSE_ALPHABET[byte % LICENSE_ALPHABET.length],
  ).join("");
  return `ZAE-${characters.slice(0, 5)}-${characters.slice(5, 10)}-${characters.slice(10, 15)}`;
}

export function maskEmail(email: string) {
  const [local, domain] = email.split("@");
  if (!local || !domain) return "";
  const visible = local.slice(0, Math.min(2, local.length));
  return `${visible}${"*".repeat(Math.max(2, local.length - visible.length))}@${domain}`;
}

export function parseExpectedLiveMode() {
  return (Deno.env.get("MERCADOPAGO_EXPECT_LIVE_MODE") ?? "false").toLowerCase() === "true";
}
