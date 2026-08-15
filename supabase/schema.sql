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
