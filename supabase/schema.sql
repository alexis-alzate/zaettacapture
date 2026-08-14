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
