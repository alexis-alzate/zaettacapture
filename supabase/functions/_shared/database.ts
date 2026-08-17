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

  return Deno.env.get("SUPABASE_SECRET_KEY") ??
    Deno.env.get("SUPABASE_SERVICE_ROLE_KEY") ?? "";
}

export function databaseConfig() {
  const projectUrl = Deno.env.get("SUPABASE_URL") ?? "";
  const databaseKey = getDatabaseKey();

  if (!projectUrl || !databaseKey) {
    throw new Error("Database configuration is unavailable");
  }

  const headers: Record<string, string> = {
    "apikey": databaseKey,
    "Content-Type": "application/json",
  };

  if (!databaseKey.startsWith("sb_secret_")) {
    headers.Authorization = `Bearer ${databaseKey}`;
  }

  return { projectUrl, headers };
}

export async function callRpc<T>(rpcName: string, body: Record<string, unknown>) {
  const { projectUrl, headers } = databaseConfig();
  const response = await fetch(`${projectUrl}/rest/v1/rpc/${rpcName}`, {
    method: "POST",
    headers,
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    const detail = await response.text();
    throw new Error(`${rpcName} failed with ${response.status}: ${detail}`);
  }

  const text = await response.text();
  return (text ? JSON.parse(text) : null) as T;
}

type PaymentEvent = {
  providerEventId?: string | null;
  paymentId?: string | null;
  eventType: string;
  action?: string | null;
  outcome: string;
  detail?: string | null;
  processed?: boolean;
};

export async function savePaymentEvent(event: PaymentEvent) {
  const { projectUrl, headers } = databaseConfig();
  const response = await fetch(
    `${projectUrl}/rest/v1/license_payment_events?on_conflict=provider_event_id`,
    {
      method: "POST",
      headers: {
        ...headers,
        "Prefer": "resolution=merge-duplicates,return=minimal",
      },
      body: JSON.stringify({
        provider_event_id: event.providerEventId ?? null,
        payment_id: event.paymentId ?? null,
        event_type: event.eventType,
        action: event.action ?? null,
        outcome: event.outcome,
        detail: event.detail ? event.detail.slice(0, 500) : null,
        processed_at: event.processed ? new Date().toISOString() : null,
      }),
    },
  );

  if (!response.ok) {
    console.error("Payment event could not be saved", response.status, await response.text());
  }
}
