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
