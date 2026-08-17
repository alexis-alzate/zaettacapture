create table if not exists public.download_counters (
  counter_key text primary key,
  value bigint not null check (value >= 0),
  updated_at timestamptz not null default now()
);

alter table public.download_counters enable row level security;

do $$
begin
  if not exists (
    select 1
    from pg_policies
    where schemaname = 'public'
      and tablename = 'download_counters'
      and policyname = 'Deny public access to download counters'
  ) then
    create policy "Deny public access to download counters"
    on public.download_counters
    for all
    to anon, authenticated
    using (false)
    with check (false);
  end if;
end;
$$;

revoke all on table public.download_counters from public, anon, authenticated;
grant select, insert, update on table public.download_counters to service_role;

insert into public.download_counters (counter_key, value)
values ('zaetta-capture', 102)
on conflict (counter_key) do update
set value = greatest(public.download_counters.value, excluded.value),
    updated_at = now();

create or replace function public.get_download_counter(p_key text)
returns bigint
language sql
stable
security invoker
set search_path = ''
as $$
  select value
  from public.download_counters
  where counter_key = p_key;
$$;

create or replace function public.increment_download_counter(p_key text)
returns bigint
language plpgsql
volatile
security invoker
set search_path = ''
as $$
declare
  next_value bigint;
begin
  insert into public.download_counters (counter_key, value)
  values (p_key, 1)
  on conflict (counter_key) do update
  set value = public.download_counters.value + 1,
      updated_at = now()
  returning value into next_value;

  return next_value;
end;
$$;

revoke all on function public.get_download_counter(text) from public, anon, authenticated;
revoke all on function public.increment_download_counter(text) from public, anon, authenticated;
grant execute on function public.get_download_counter(text) to service_role;
grant execute on function public.increment_download_counter(text) to service_role;

create table if not exists public.product_feedback (
  id bigint generated always as identity primary key,
  product text not null default 'zaetta-capture',
  name text,
  email text,
  category text not null,
  message text not null,
  source text not null default 'website',
  status text not null default 'new',
  created_at timestamptz not null default now(),
  constraint product_feedback_name_length check (name is null or char_length(name) <= 80),
  constraint product_feedback_email_length check (email is null or char_length(email) <= 254),
  constraint product_feedback_category_allowed check (category in ('feature', 'experience', 'bug', 'integration', 'other')),
  constraint product_feedback_message_length check (char_length(message) between 20 and 1200),
  constraint product_feedback_source_allowed check (source = 'website'),
  constraint product_feedback_status_allowed check (status in ('new', 'reviewed', 'planned', 'completed', 'declined'))
);

alter table public.product_feedback enable row level security;

do $$
begin
  if not exists (
    select 1
    from pg_policies
    where schemaname = 'public'
      and tablename = 'product_feedback'
      and policyname = 'Deny public access to product feedback'
  ) then
    create policy "Deny public access to product feedback"
    on public.product_feedback
    for all
    to anon, authenticated
    using (false)
    with check (false);
  end if;
end;
$$;

revoke all on table public.product_feedback from public, anon, authenticated;
grant insert on table public.product_feedback to service_role;
grant usage, select on sequence public.product_feedback_id_seq to service_role;

create table if not exists public.download_leads (
  id uuid primary key default gen_random_uuid(),
  product text not null default 'zaetta-capture',
  email text not null,
  version text not null,
  source text not null default 'website',
  marketing_consent boolean not null default false,
  marketing_consent_at timestamptz,
  request_count integer not null default 1,
  download_count integer not null default 0,
  first_requested_at timestamptz not null default now(),
  last_requested_at timestamptz not null default now(),
  last_downloaded_at timestamptz,
  updated_at timestamptz not null default now(),
  constraint download_leads_product_email_unique unique (product, email),
  constraint download_leads_product_allowed check (product = 'zaetta-capture'),
  constraint download_leads_email_length check (char_length(email) between 5 and 254),
  constraint download_leads_email_normalized check (email = lower(btrim(email))),
  constraint download_leads_email_format check (email ~ '^[^[:space:]@]+@[^[:space:]@]+[.][^[:space:]@]+$'),
  constraint download_leads_version_length check (char_length(version) between 1 and 32),
  constraint download_leads_source_allowed check (source = 'website'),
  constraint download_leads_request_count_positive check (request_count > 0),
  constraint download_leads_download_count_nonnegative check (download_count >= 0)
);

create index if not exists download_leads_last_requested_idx
on public.download_leads (last_requested_at desc);

alter table public.download_leads enable row level security;

do $$
begin
  if not exists (
    select 1
    from pg_policies
    where schemaname = 'public'
      and tablename = 'download_leads'
      and policyname = 'Deny public access to download leads'
  ) then
    create policy "Deny public access to download leads"
    on public.download_leads
    for all
    to anon, authenticated
    using (false)
    with check (false);
  end if;
end;
$$;

revoke all on table public.download_leads from public, anon, authenticated;
grant select, insert, update on table public.download_leads to service_role;

create or replace function public.register_download_lead(
  p_email text,
  p_version text,
  p_marketing_consent boolean default false,
  p_source text default 'website'
)
returns table (lead_id uuid)
language plpgsql
volatile
security invoker
set search_path = ''
as $$
declare
  normalized_email text := lower(btrim(p_email));
  registered_id uuid;
begin
  if normalized_email is null
     or char_length(normalized_email) < 5
     or char_length(normalized_email) > 254
     or normalized_email !~ '^[^[:space:]@]+@[^[:space:]@]+[.][^[:space:]@]+$' then
    raise exception 'Invalid email';
  end if;

  if p_version is null or char_length(btrim(p_version)) not between 1 and 32 then
    raise exception 'Invalid version';
  end if;

  if p_source <> 'website' then
    raise exception 'Invalid source';
  end if;

  insert into public.download_leads (
    product,
    email,
    version,
    source,
    marketing_consent,
    marketing_consent_at
  )
  values (
    'zaetta-capture',
    normalized_email,
    btrim(p_version),
    p_source,
    coalesce(p_marketing_consent, false),
    case when coalesce(p_marketing_consent, false) then now() else null end
  )
  on conflict (product, email) do update
  set version = excluded.version,
      source = excluded.source,
      marketing_consent = public.download_leads.marketing_consent or excluded.marketing_consent,
      marketing_consent_at = case
        when public.download_leads.marketing_consent_at is null and excluded.marketing_consent then now()
        else public.download_leads.marketing_consent_at
      end,
      request_count = public.download_leads.request_count + 1,
      last_requested_at = now(),
      updated_at = now()
  returning id into registered_id;

  return query select registered_id;
end;
$$;

create or replace function public.record_download_start(
  p_lead_id uuid,
  p_counter_key text
)
returns bigint
language plpgsql
volatile
security invoker
set search_path = ''
as $$
declare
  next_value bigint;
begin
  update public.download_leads
  set download_count = download_count + 1,
      last_downloaded_at = now(),
      updated_at = now()
  where id = p_lead_id;

  if not found then
    raise exception 'Download lead not found';
  end if;

  insert into public.download_counters (counter_key, value)
  values (p_counter_key, 1)
  on conflict (counter_key) do update
  set value = public.download_counters.value + 1,
      updated_at = now()
  returning value into next_value;

  return next_value;
end;
$$;

revoke all on function public.register_download_lead(text, text, boolean, text) from public, anon, authenticated;
revoke all on function public.record_download_start(uuid, text) from public, anon, authenticated;
grant execute on function public.register_download_lead(text, text, boolean, text) to service_role;
grant execute on function public.record_download_start(uuid, text) to service_role;

-- Licencias solidarias de Zaetta Capture.
-- Estas tablas no se consultan directamente desde el navegador. Las Edge
-- Functions usan una clave secreta y exponen solamente respuestas mínimas.
create table if not exists public.license_orders (
  id uuid primary key default gen_random_uuid(),
  checkout_token uuid not null default gen_random_uuid() unique,
  product text not null default 'zaetta-capture',
  buyer_email text not null,
  total_amount integer not null default 10000,
  currency text not null default 'COP',
  source text not null default 'website',
  privacy_accepted_at timestamptz not null,
  terms_accepted_at timestamptz not null,
  privacy_version text not null default '2026-08-15',
  terms_version text not null default 'draft-2026-08-15',
  offer_version text not null default 'solidarity-v1',
  mp_preference_id text,
  mp_payment_id text,
  mp_status_detail text,
  live_mode boolean,
  status text not null default 'pending',
  checkout_error text,
  email_delivery_status text not null default 'not_required',
  email_delivery_attempts integer not null default 0,
  email_delivery_error text,
  email_claimed_at timestamptz,
  email_sent_at timestamptz,
  approved_at timestamptz,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  constraint license_orders_product_allowed check (product = 'zaetta-capture'),
  constraint license_orders_email_length check (char_length(buyer_email) between 5 and 254),
  constraint license_orders_email_normalized check (buyer_email = lower(btrim(buyer_email))),
  constraint license_orders_email_format check (buyer_email ~ '^[^[:space:]@]+@[^[:space:]@]+[.][^[:space:]@]+$'),
  constraint license_orders_amount_fixed check (total_amount = 10000),
  constraint license_orders_currency_fixed check (currency = 'COP'),
  constraint license_orders_source_allowed check (source = 'website'),
  constraint license_orders_privacy_version_allowed check (privacy_version = '2026-08-15'),
  constraint license_orders_terms_version_allowed check (terms_version = 'draft-2026-08-15'),
  constraint license_orders_offer_version_allowed check (offer_version = 'solidarity-v1'),
  constraint license_orders_status_allowed check (
    status in ('pending', 'approved', 'rejected', 'checkout_error', 'refunded', 'charged_back')
  ),
  constraint license_orders_email_delivery_allowed check (
    email_delivery_status in ('not_required', 'pending', 'sending', 'sent', 'failed')
  ),
  constraint license_orders_email_attempts_nonnegative check (email_delivery_attempts >= 0)
);

create unique index if not exists license_orders_mp_preference_unique
on public.license_orders (mp_preference_id)
where mp_preference_id is not null;

create unique index if not exists license_orders_mp_payment_unique
on public.license_orders (mp_payment_id)
where mp_payment_id is not null;

create index if not exists license_orders_email_created_idx
on public.license_orders (buyer_email, created_at desc);

create index if not exists license_orders_status_created_idx
on public.license_orders (status, created_at desc);

create table if not exists public.licenses (
  id uuid primary key default gen_random_uuid(),
  order_id uuid not null unique references public.license_orders(id) on delete restrict,
  license_key text not null unique,
  buyer_email text not null,
  product text not null default 'zaetta-capture',
  status text not null default 'active',
  max_devices integer not null default 2,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  revoked_at timestamptz,
  constraint licenses_product_allowed check (product = 'zaetta-capture'),
  constraint licenses_email_normalized check (buyer_email = lower(btrim(buyer_email))),
  constraint licenses_status_allowed check (status in ('active', 'revoked', 'refunded')),
  constraint licenses_max_devices_positive check (max_devices between 1 and 10),
  constraint licenses_key_format check (license_key ~ '^ZAE-[A-Z0-9]{5}-[A-Z0-9]{5}-[A-Z0-9]{5}$')
);

create table if not exists public.solidarity_ledger (
  id uuid primary key default gen_random_uuid(),
  order_id uuid not null unique references public.license_orders(id) on delete restrict,
  gross_amount integer not null,
  gateway_fee_amount integer,
  zaetta_covered_fee_amount integer,
  committed_amount integer not null,
  currency text not null default 'COP',
  status text not null default 'reserved',
  cause_name text,
  evidence_url text,
  reserved_at timestamptz not null default now(),
  donated_at timestamptz,
  updated_at timestamptz not null default now(),
  constraint solidarity_amounts_valid check (
    gross_amount = 10000
    and committed_amount = 10000
    and (gateway_fee_amount is null or gateway_fee_amount >= 0)
    and (zaetta_covered_fee_amount is null or zaetta_covered_fee_amount >= 0)
  ),
  constraint solidarity_currency_fixed check (currency = 'COP'),
  constraint solidarity_status_allowed check (
    status in ('reserved', 'donated', 'cancelled', 'refund_review')
  )
);

create index if not exists solidarity_ledger_status_reserved_idx
on public.solidarity_ledger (status, reserved_at desc);

create table if not exists public.license_payment_events (
  id bigint generated always as identity primary key,
  provider_event_id text,
  payment_id text,
  event_type text not null,
  action text,
  outcome text not null,
  detail text,
  received_at timestamptz not null default now(),
  processed_at timestamptz
);

create unique index if not exists license_payment_events_provider_event_unique
on public.license_payment_events (provider_event_id);

create index if not exists license_payment_events_payment_idx
on public.license_payment_events (payment_id, received_at desc);

alter table public.license_orders enable row level security;
alter table public.licenses enable row level security;
alter table public.solidarity_ledger enable row level security;
alter table public.license_payment_events enable row level security;

do $$
declare
  protected_table text;
  policy_name text;
begin
  foreach protected_table in array array[
    'license_orders',
    'licenses',
    'solidarity_ledger',
    'license_payment_events'
  ] loop
    policy_name := 'Deny public access to ' || protected_table;
    if not exists (
      select 1
      from pg_catalog.pg_policies
      where schemaname = 'public'
        and tablename = protected_table
        and policyname = policy_name
    ) then
      execute format(
        'create policy %I on public.%I for all to anon, authenticated using (false) with check (false)',
        policy_name,
        protected_table
      );
    end if;
  end loop;
end;
$$;

revoke all on table public.license_orders from public, anon, authenticated;
revoke all on table public.licenses from public, anon, authenticated;
revoke all on table public.solidarity_ledger from public, anon, authenticated;
revoke all on table public.license_payment_events from public, anon, authenticated;
revoke all on sequence public.license_payment_events_id_seq from public, anon, authenticated;

grant select, insert, update on table public.license_orders to service_role;
grant select, insert, update on table public.licenses to service_role;
grant select, insert, update on table public.solidarity_ledger to service_role;
grant select, insert, update on table public.license_payment_events to service_role;
grant usage, select on sequence public.license_payment_events_id_seq to service_role;

create or replace function public.create_license_order(
  p_email text,
  p_privacy_accepted boolean,
  p_terms_accepted boolean,
  p_source text default 'website'
)
returns table (order_id uuid, checkout_token uuid)
language plpgsql
volatile
security invoker
set search_path = ''
as $$
declare
  normalized_email text := lower(btrim(p_email));
  recent_attempts integer;
begin
  if p_privacy_accepted is not true or p_terms_accepted is not true then
    raise exception 'Required consent missing';
  end if;

  if normalized_email is null
     or char_length(normalized_email) < 5
     or char_length(normalized_email) > 254
     or normalized_email !~ '^[^[:space:]@]+@[^[:space:]@]+[.][^[:space:]@]+$' then
    raise exception 'Invalid email';
  end if;

  if p_source <> 'website' then
    raise exception 'Invalid source';
  end if;

  select count(*)
  into recent_attempts
  from public.license_orders
  where buyer_email = normalized_email
    and created_at >= now() - interval '15 minutes';

  if recent_attempts >= 3 then
    raise exception 'Checkout rate limit exceeded';
  end if;

  return query
  insert into public.license_orders (
    buyer_email,
    privacy_accepted_at,
    terms_accepted_at,
    source
  )
  values (
    normalized_email,
    now(),
    now(),
    p_source
  )
  returning id, public.license_orders.checkout_token;
end;
$$;

create or replace function public.attach_license_preference(
  p_order_id uuid,
  p_checkout_token uuid,
  p_preference_id text
)
returns boolean
language plpgsql
volatile
security invoker
set search_path = ''
as $$
begin
  update public.license_orders
  set mp_preference_id = p_preference_id,
      checkout_error = null,
      status = 'pending',
      updated_at = now()
  where id = p_order_id
    and checkout_token = p_checkout_token
    and status in ('pending', 'checkout_error')
    and p_preference_id is not null
    and char_length(p_preference_id) between 1 and 255;

  return found;
end;
$$;

create or replace function public.mark_license_checkout_error(
  p_order_id uuid,
  p_checkout_token uuid,
  p_error text
)
returns void
language plpgsql
volatile
security invoker
set search_path = ''
as $$
begin
  update public.license_orders
  set status = 'checkout_error',
      checkout_error = left(coalesce(p_error, 'Unknown checkout error'), 500),
      updated_at = now()
  where id = p_order_id
    and checkout_token = p_checkout_token
    and status in ('pending', 'checkout_error');
end;
$$;

create or replace function public.approve_license_order(
  p_order_id uuid,
  p_payment_id text,
  p_status_detail text,
  p_amount integer,
  p_currency text,
  p_live_mode boolean,
  p_gateway_fee integer,
  p_license_key text
)
returns table (order_id uuid, checkout_token uuid, license_key text)
language plpgsql
volatile
security invoker
set search_path = ''
as $$
declare
  locked_order public.license_orders%rowtype;
begin
  select *
  into locked_order
  from public.license_orders
  where id = p_order_id
  for update;

  if not found then
    raise exception 'Order not found';
  end if;

  if p_amount <> locked_order.total_amount or p_currency <> locked_order.currency then
    raise exception 'Payment amount or currency mismatch';
  end if;

  if p_payment_id is null or char_length(p_payment_id) > 100 then
    raise exception 'Invalid payment id';
  end if;

  if locked_order.status in ('refunded', 'charged_back') then
    raise exception 'Order can no longer be approved';
  end if;

  if locked_order.mp_payment_id is not null and locked_order.mp_payment_id <> p_payment_id then
    raise exception 'Order already linked to another payment';
  end if;

  insert into public.licenses (
    order_id,
    license_key,
    buyer_email
  )
  values (
    locked_order.id,
    p_license_key,
    locked_order.buyer_email
  )
  on conflict (order_id) do nothing;

  insert into public.solidarity_ledger (
    order_id,
    gross_amount,
    gateway_fee_amount,
    zaetta_covered_fee_amount,
    committed_amount
  )
  values (
    locked_order.id,
    locked_order.total_amount,
    case when p_gateway_fee is null then null else greatest(p_gateway_fee, 0) end,
    case when p_gateway_fee is null then null else greatest(p_gateway_fee, 0) end,
    locked_order.total_amount
  )
  on conflict (order_id) do nothing;

  update public.license_orders
  set status = 'approved',
      mp_payment_id = p_payment_id,
      mp_status_detail = left(coalesce(p_status_detail, ''), 120),
      live_mode = p_live_mode,
      checkout_error = null,
      email_delivery_status = case
        when email_delivery_status in ('sent', 'sending') then email_delivery_status
        else 'pending'
      end,
      approved_at = coalesce(approved_at, now()),
      updated_at = now()
  where id = locked_order.id;

  return query
  select o.id, o.checkout_token, l.license_key
  from public.license_orders o
  join public.licenses l on l.order_id = o.id
  where o.id = locked_order.id;
end;
$$;

create or replace function public.record_license_payment_state(
  p_order_id uuid,
  p_payment_id text,
  p_status text,
  p_status_detail text,
  p_live_mode boolean
)
returns void
language plpgsql
volatile
security invoker
set search_path = ''
as $$
declare
  current_status text;
begin
  if p_status not in ('rejected', 'cancelled', 'refunded', 'charged_back') then
    raise exception 'Unsupported payment state';
  end if;

  select status
  into current_status
  from public.license_orders
  where id = p_order_id
  for update;

  if not found then
    return;
  end if;

  if p_status in ('rejected', 'cancelled') and current_status <> 'approved' then
    update public.license_orders
    set status = 'rejected',
        mp_status_detail = left(coalesce(p_status_detail, ''), 120),
        live_mode = p_live_mode,
        updated_at = now()
    where id = p_order_id;
    return;
  end if;

  if p_status in ('refunded', 'charged_back') then
    update public.license_orders
    set status = case when p_status = 'refunded' then 'refunded' else 'charged_back' end,
        mp_payment_id = coalesce(mp_payment_id, p_payment_id),
        mp_status_detail = left(coalesce(p_status_detail, ''), 120),
        live_mode = p_live_mode,
        updated_at = now()
    where id = p_order_id;

    update public.licenses
    set status = case when p_status = 'refunded' then 'refunded' else 'revoked' end,
        revoked_at = coalesce(revoked_at, now()),
        updated_at = now()
    where order_id = p_order_id;

    update public.solidarity_ledger
    set status = case when status = 'donated' then 'refund_review' else 'cancelled' end,
        updated_at = now()
    where order_id = p_order_id;
  end if;
end;
$$;

create or replace function public.claim_license_email(p_order_id uuid)
returns table (order_id uuid, buyer_email text, license_key text)
language plpgsql
volatile
security invoker
set search_path = ''
as $$
begin
  return query
  with claimed as (
    update public.license_orders
    set email_delivery_status = 'sending',
        email_delivery_attempts = email_delivery_attempts + 1,
        email_delivery_error = null,
        email_claimed_at = now(),
        updated_at = now()
    where id = p_order_id
      and status = 'approved'
      and (
        email_delivery_status in ('pending', 'failed')
        or (email_delivery_status = 'sending' and email_claimed_at < now() - interval '15 minutes')
      )
    returning id, public.license_orders.buyer_email
  )
  select c.id, c.buyer_email, l.license_key
  from claimed c
  join public.licenses l on l.order_id = c.id;
end;
$$;

create or replace function public.complete_license_email(
  p_order_id uuid,
  p_success boolean,
  p_error text default null
)
returns void
language plpgsql
volatile
security invoker
set search_path = ''
as $$
begin
  update public.license_orders
  set email_delivery_status = case when p_success then 'sent' else 'failed' end,
      email_delivery_error = case when p_success then null else left(coalesce(p_error, 'Email delivery failed'), 500) end,
      email_sent_at = case when p_success then coalesce(email_sent_at, now()) else email_sent_at end,
      updated_at = now()
  where id = p_order_id
    and email_delivery_status = 'sending';
end;
$$;

create or replace function public.get_license_order_status(
  p_order_id uuid,
  p_checkout_token uuid
)
returns table (
  status text,
  buyer_email text,
  total_amount integer,
  currency text,
  license_key text,
  license_status text,
  solidarity_status text,
  committed_amount integer
)
language sql
stable
security invoker
set search_path = ''
as $$
  select
    o.status,
    o.buyer_email,
    o.total_amount,
    o.currency,
    l.license_key,
    l.status,
    s.status,
    s.committed_amount
  from public.license_orders o
  left join public.licenses l on l.order_id = o.id
  left join public.solidarity_ledger s on s.order_id = o.id
  where o.id = p_order_id
    and o.checkout_token = p_checkout_token;
$$;

revoke all on function public.create_license_order(text, boolean, boolean, text) from public, anon, authenticated;
revoke all on function public.attach_license_preference(uuid, uuid, text) from public, anon, authenticated;
revoke all on function public.mark_license_checkout_error(uuid, uuid, text) from public, anon, authenticated;
revoke all on function public.approve_license_order(uuid, text, text, integer, text, boolean, integer, text) from public, anon, authenticated;
revoke all on function public.record_license_payment_state(uuid, text, text, text, boolean) from public, anon, authenticated;
revoke all on function public.claim_license_email(uuid) from public, anon, authenticated;
revoke all on function public.complete_license_email(uuid, boolean, text) from public, anon, authenticated;
revoke all on function public.get_license_order_status(uuid, uuid) from public, anon, authenticated;

grant execute on function public.create_license_order(text, boolean, boolean, text) to service_role;
grant execute on function public.attach_license_preference(uuid, uuid, text) to service_role;
grant execute on function public.mark_license_checkout_error(uuid, uuid, text) to service_role;
grant execute on function public.approve_license_order(uuid, text, text, integer, text, boolean, integer, text) to service_role;
grant execute on function public.record_license_payment_state(uuid, text, text, text, boolean) to service_role;
grant execute on function public.claim_license_email(uuid) to service_role;
grant execute on function public.complete_license_email(uuid, boolean, text) to service_role;
grant execute on function public.get_license_order_status(uuid, uuid) to service_role;
