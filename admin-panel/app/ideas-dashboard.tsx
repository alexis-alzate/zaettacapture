"use client";

import { useCallback, useEffect, useMemo, useState } from "react";

type Status = "new" | "reviewed" | "planned" | "completed" | "declined";
type Priority = "low" | "normal" | "high";
type View = "ideas" | "downloads";
type DownloadState = "all" | "downloaded" | "requested";
type ConsentState = "all" | "accepted" | "not-accepted";

type Idea = {
  id: number;
  product: string;
  name: string | null;
  email: string | null;
  category: string;
  message: string;
  source: string | null;
  status: Status;
  priority: Priority;
  admin_note: string | null;
  reviewed_at: string | null;
  updated_at: string;
  created_at: string;
};

type DownloadLead = {
  id: string;
  product: string;
  email: string;
  version: string;
  source: string;
  marketing_consent: boolean;
  marketing_consent_at: string | null;
  request_count: number;
  download_count: number;
  first_requested_at: string;
  last_requested_at: string;
  last_downloaded_at: string | null;
  updated_at: string;
};

const STATUS: Record<Status, string> = {
  new: "Nueva",
  reviewed: "Revisada",
  planned: "Planeada",
  completed: "Completada",
  declined: "Descartada",
};

const PRIORITY: Record<Priority, string> = { low: "Baja", normal: "Normal", high: "Alta" };
const CATEGORY: Record<string, string> = {
  feature: "Nueva función",
  experience: "Experiencia",
  bug: "Error",
  integration: "Integración",
  other: "Otro",
};

const PREVIEW_IDEAS: Idea[] = [
  {
    id: 2, product: "Zaetta Capture", name: "Laura Pérez", email: "laura@ejemplo.com",
    category: "feature", message: "Me gustaría que la aplicación tuviera una mejor selección de color para organizar cada captura.",
    source: "website", status: "new", priority: "normal", admin_note: "", reviewed_at: null,
    updated_at: "2026-08-14T22:01:00.000Z", created_at: "2026-08-14T22:01:00.000Z",
  },
  {
    id: 3, product: "Zaetta Capture", name: "Mariana López", email: "mariana@ejemplo.com",
    category: "experience", message: "Sería buenísimo poder guardar mis perfiles favoritos y recuperarlos con un clic cuando voy a grabar.",
    source: "website", status: "planned", priority: "high", admin_note: "Revisar para la próxima versión mayor.",
    reviewed_at: "2026-08-15T08:30:00.000Z", updated_at: "2026-08-15T08:30:00.000Z", created_at: "2026-08-13T15:42:00.000Z",
  },
  {
    id: 4, product: "Zaetta Capture", name: "Carlos Méndez", email: "carlos@ejemplo.com",
    category: "integration", message: "¿Pueden añadir una integración directa con Google Drive para subir los archivos al terminar la captura?",
    source: "website", status: "reviewed", priority: "low", admin_note: "", reviewed_at: "2026-08-14T12:00:00.000Z",
    updated_at: "2026-08-14T12:00:00.000Z", created_at: "2026-08-12T19:18:00.000Z",
  },
];

const PREVIEW_DOWNLOADS: DownloadLead[] = [
  {
    id: "c15dd0c6-5842-4a62-8c8a-13219688aa01", product: "zaetta-capture", email: "andres@ejemplo.com",
    version: "1.0.29", source: "website", marketing_consent: true, marketing_consent_at: "2026-08-15T15:33:00.000Z",
    request_count: 2, download_count: 1, first_requested_at: "2026-08-15T15:31:00.000Z",
    last_requested_at: "2026-08-15T15:33:00.000Z", last_downloaded_at: "2026-08-15T15:34:00.000Z", updated_at: "2026-08-15T15:34:00.000Z",
  },
  {
    id: "c15dd0c6-5842-4a62-8c8a-13219688aa02", product: "zaetta-capture", email: "valentina@ejemplo.com",
    version: "1.0.29", source: "website", marketing_consent: false, marketing_consent_at: null,
    request_count: 1, download_count: 0, first_requested_at: "2026-08-15T14:18:00.000Z",
    last_requested_at: "2026-08-15T14:18:00.000Z", last_downloaded_at: null, updated_at: "2026-08-15T14:18:00.000Z",
  },
  {
    id: "c15dd0c6-5842-4a62-8c8a-13219688aa03", product: "zaetta-capture", email: "camilo@ejemplo.com",
    version: "1.0.29", source: "website", marketing_consent: true, marketing_consent_at: "2026-08-14T20:02:00.000Z",
    request_count: 1, download_count: 1, first_requested_at: "2026-08-14T20:02:00.000Z",
    last_requested_at: "2026-08-14T20:02:00.000Z", last_downloaded_at: "2026-08-14T20:03:00.000Z", updated_at: "2026-08-14T20:03:00.000Z",
  },
];

function Icon({ name, size = 18 }: { name: string; size?: number }) {
  const paths: Record<string, React.ReactNode> = {
    inbox: <><path d="M4 5h16v14H4z"/><path d="m4 13 4-8h8l4 8"/><path d="M4 13h5l1.5 2h3L15 13h5"/></>,
    search: <><circle cx="11" cy="11" r="7"/><path d="m20 20-4-4"/></>,
    refresh: <><path d="M20 11a8 8 0 1 0-2.3 5.7"/><path d="M20 5v6h-6"/></>,
    mail: <><rect x="3" y="5" width="18" height="14" rx="2"/><path d="m3 7 9 6 9-6"/></>,
    external: <><path d="M14 4h6v6"/><path d="m20 4-9 9"/><path d="M18 13v6a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V7a1 1 0 0 1 1-1h6"/></>,
    spark: <><path d="m12 3 1.2 4.2L17 9l-3.8 1.8L12 15l-1.2-4.2L7 9l3.8-1.8z"/><path d="m5 15 .7 2.3L8 18l-2.3.7L5 21l-.7-2.3L2 18l2.3-.7z"/></>,
    clock: <><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/></>,
    copy: <><rect x="8" y="8" width="11" height="11" rx="2"/><path d="M16 8V6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v8a2 2 0 0 0 2 2h2"/></>,
    download: <><path d="M12 3v12"/><path d="m7 10 5 5 5-5"/><path d="M4 19h16"/></>,
    check: <path d="m5 12 4 4L19 6"/>,
    chevron: <path d="m9 18 6-6-6-6"/>,
  };
  return <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">{paths[name]}</svg>;
}

function formatDate(value: string, compact = false) {
  return new Intl.DateTimeFormat("es-CO", compact
    ? { day: "numeric", month: "short" }
    : { day: "numeric", month: "long", year: "numeric", hour: "numeric", minute: "2-digit" }
  ).format(new Date(value));
}

function localPreview() {
  if (typeof window === "undefined") return false;
  if (new URLSearchParams(window.location.search).get("live") === "1") return false;
  return window.location.hostname === "localhost" || window.location.hostname === "terminal.local";
}

export default function IdeasDashboard() {
  const [view, setView] = useState<View>("ideas");
  const [ideas, setIdeas] = useState<Idea[]>([]);
  const [downloads, setDownloads] = useState<DownloadLead[]>([]);
  const [selectedId, setSelectedId] = useState<number | null>(null);
  const [selectedDownloadId, setSelectedDownloadId] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [downloadQuery, setDownloadQuery] = useState("");
  const [status, setStatus] = useState<Status | "all">("all");
  const [category, setCategory] = useState("all");
  const [downloadState, setDownloadState] = useState<DownloadState>("all");
  const [consentState, setConsentState] = useState<ConsentState>("all");
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [isPreview, setIsPreview] = useState(false);
  const [lastSync, setLastSync] = useState<Date | null>(null);
  const [copied, setCopied] = useState(false);

  const loadDashboard = useCallback(async (quiet = false) => {
    if (localPreview()) {
      setIsPreview(true);
      setIdeas(PREVIEW_IDEAS);
      setDownloads(PREVIEW_DOWNLOADS);
      setSelectedId((current) => current ?? PREVIEW_IDEAS[0].id);
      setSelectedDownloadId((current) => current ?? PREVIEW_DOWNLOADS[0].id);
      setLastSync(new Date());
      setLoading(false);
      setRefreshing(false);
      return;
    }
    if (!quiet) setLoading(true);
    setRefreshing(quiet);
    setError("");
    try {
      const response = await fetch("/api/ideas", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ action: "dashboard" }) });
      const data = await response.json() as { ideas?: Idea[]; downloads?: DownloadLead[]; error?: string };
      if (!response.ok || !Array.isArray(data.ideas) || !Array.isArray(data.downloads)) throw new Error(data.error ?? "No pudimos cargar el panel.");
      setIdeas(data.ideas);
      setDownloads(data.downloads);
      setSelectedId((current) => data.ideas!.some((idea) => idea.id === current) ? current : (data.ideas![0]?.id ?? null));
      setSelectedDownloadId((current) => data.downloads!.some((lead) => lead.id === current) ? current : (data.downloads![0]?.id ?? null));
      setLastSync(new Date());
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "No pudimos cargar el panel.");
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  }, []);

  useEffect(() => {
    const timer = window.setTimeout(() => void loadDashboard(), 0);
    return () => window.clearTimeout(timer);
  }, [loadDashboard]);

  const filtered = useMemo(() => {
    const needle = query.trim().toLowerCase();
    return ideas.filter((idea) => {
      const textMatch = !needle || [idea.name, idea.email, idea.message, CATEGORY[idea.category]].filter(Boolean).some((value) => String(value).toLowerCase().includes(needle));
      return textMatch && (status === "all" || idea.status === status) && (category === "all" || idea.category === category);
    });
  }, [ideas, query, status, category]);

  const filteredDownloads = useMemo(() => {
    const needle = downloadQuery.trim().toLowerCase();
    return downloads.filter((lead) => {
      const textMatch = !needle || [lead.email, lead.version, lead.source].some((value) => value.toLowerCase().includes(needle));
      const stateMatch = downloadState === "all" || (downloadState === "downloaded" ? lead.download_count > 0 : lead.download_count === 0);
      const consentMatch = consentState === "all" || (consentState === "accepted" ? lead.marketing_consent : !lead.marketing_consent);
      return textMatch && stateMatch && consentMatch;
    });
  }, [downloads, downloadQuery, downloadState, consentState]);

  const selected = ideas.find((idea) => idea.id === selectedId) ?? null;
  const selectedDownload = downloads.find((lead) => lead.id === selectedDownloadId) ?? null;
  const counts = useMemo(() => ({
    total: ideas.length,
    new: ideas.filter((idea) => idea.status === "new").length,
    high: ideas.filter((idea) => idea.priority === "high").length,
    planned: ideas.filter((idea) => idea.status === "planned").length,
  }), [ideas]);
  const downloadCounts = useMemo(() => ({
    unique: downloads.length,
    requests: downloads.reduce((total, lead) => total + lead.request_count, 0),
    downloads: downloads.reduce((total, lead) => total + lead.download_count, 0),
    consented: downloads.filter((lead) => lead.marketing_consent).length,
  }), [downloads]);

  function patchSelected(patch: Partial<Idea>) {
    if (!selected) return;
    setIdeas((current) => current.map((idea) => idea.id === selected.id ? { ...idea, ...patch } : idea));
  }

  async function saveIdea(next: Idea) {
    setSaving(true);
    setError("");
    if (isPreview) {
      await new Promise((resolve) => window.setTimeout(resolve, 420));
      setIdeas((current) => current.map((idea) => idea.id === next.id ? { ...next, updated_at: new Date().toISOString() } : idea));
      setSaving(false);
      return;
    }
    try {
      const response = await fetch("/api/ideas", {
        method: "POST", headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ action: "update", id: next.id, status: next.status, priority: next.priority, adminNote: next.admin_note ?? "" }),
      });
      const data = await response.json() as { idea?: Idea; error?: string };
      if (!response.ok || !data.idea) throw new Error(data.error ?? "No pudimos guardar los cambios.");
      setIdeas((current) => current.map((idea) => idea.id === next.id ? data.idea! : idea));
      setLastSync(new Date());
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "No pudimos guardar los cambios.");
    } finally {
      setSaving(false);
    }
  }

  async function copyEmail(email: string | null | undefined) {
    if (!email) return;
    await navigator.clipboard.writeText(email);
    setCopied(true);
    window.setTimeout(() => setCopied(false), 1400);
  }

  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div className="brand"><div className="brand-mark"><span>Z</span></div><div><strong>ZAETTA</strong><small>CAPTURE</small></div></div>
        <div className="workspace-label">ADMIN STUDIO</div>
        <nav className="sidebar-nav" aria-label="Navegación principal">
          <button className={`nav-item ${view === "ideas" ? "active" : ""}`} type="button" onClick={() => setView("ideas")} aria-pressed={view === "ideas"}><Icon name="inbox"/><span>Solicitudes</span><em>{counts.new}</em></button>
          <button className={`nav-item ${view === "downloads" ? "active" : ""}`} type="button" onClick={() => setView("downloads")} aria-pressed={view === "downloads"}><Icon name="download"/><span>Descargas</span><em>{downloadCounts.unique}</em></button>
        </nav>
        <div className="sidebar-spacer"/>
        <a className="site-link" href="https://zaettasoftware.com" target="_blank" rel="noreferrer"><span><Icon name="external"/> Ver sitio web</span><Icon name="chevron" size={15}/></a>
        <div className="sidebar-footer"><span className="online-dot"/><div><strong>Panel privado</strong><small>Conexión protegida</small></div></div>
      </aside>

      <section className="main-area">
        <header className="topbar">
          <div><p className="eyebrow">{view === "ideas" ? "ZAETTA CAPTURE · FEEDBACK" : "ZAETTA CAPTURE · DISTRIBUCIÓN"}</p><h1>{view === "ideas" ? "Solicitudes" : "Descargas"}</h1><p className="subtitle">{view === "ideas" ? "Escucha, organiza y convierte ideas en producto." : "Conoce quién descarga y acompaña cada futura licencia."}</p></div>
          <div className="sync-state"><span className={refreshing ? "sync-dot spinning" : "sync-dot"}/><div><strong>{refreshing ? "Actualizando…" : "Todo al día"}</strong><small>{lastSync ? `Sincronizado ${new Intl.DateTimeFormat("es-CO", { hour: "numeric", minute: "2-digit" }).format(lastSync)}` : "Conectando…"}</small></div></div>
        </header>

        <div className="content-wrap">
          {isPreview && <div className="preview-banner"><Icon name="spark" size={16}/> Vista de diseño · Los datos de producción aparecerán aquí</div>}
          {error && <div className="error-banner"><span>{error}</span><button type="button" onClick={() => void loadDashboard()}>Reintentar</button></div>}

          {view === "ideas" ? <>
            <section className="metrics" aria-label="Resumen de solicitudes">
              <Metric label="Total" value={counts.total} hint="Solicitudes recibidas"/>
              <Metric label="Nuevas" value={counts.new} hint="Pendientes por revisar" accent/>
              <Metric label="Prioridad alta" value={counts.high} hint="Requieren atención"/>
              <Metric label="Planeadas" value={counts.planned} hint="En hoja de ruta"/>
            </section>

            <section className="inbox-panel">
              <div className="toolbar">
                <label className="search-box"><Icon name="search"/><input value={query} onChange={(event) => setQuery(event.target.value)} placeholder="Buscar por nombre, correo o idea…" aria-label="Buscar solicitudes"/></label>
                <div className="filter-group">
                  <select value={status} onChange={(event) => setStatus(event.target.value as Status | "all")} aria-label="Filtrar por estado"><option value="all">Todos los estados</option>{Object.entries(STATUS).map(([value, label]) => <option value={value} key={value}>{label}</option>)}</select>
                  <select value={category} onChange={(event) => setCategory(event.target.value)} aria-label="Filtrar por categoría"><option value="all">Todas las categorías</option>{Object.entries(CATEGORY).map(([value, label]) => <option value={value} key={value}>{label}</option>)}</select>
                  <button className="icon-button" type="button" onClick={() => void loadDashboard(true)} aria-label="Actualizar solicitudes"><Icon name="refresh"/></button>
                </div>
              </div>

              <div className="inbox-grid">
                <div className="request-list">
                  <div className="list-heading"><span>{filtered.length} {filtered.length === 1 ? "solicitud" : "solicitudes"}</span><small>Más recientes primero</small></div>
                  {loading ? <LoadingCards/> : filtered.length === 0 ? <Empty/> : filtered.map((idea) => (
                    <button className={`request-card ${selectedId === idea.id ? "selected" : ""}`} key={idea.id} type="button" onClick={() => setSelectedId(idea.id)}>
                      <div className="card-topline"><span className={`status-badge status-${idea.status}`}><i/>{STATUS[idea.status]}</span><time dateTime={idea.created_at}>{formatDate(idea.created_at, true)}</time></div>
                      <strong className="request-title">{CATEGORY[idea.category] ?? "Solicitud"}</strong>
                      <p>{idea.message}</p>
                      <div className="card-footer"><span className="avatar">{(idea.name ?? idea.email ?? "A").charAt(0).toUpperCase()}</span><span className="person"><strong>{idea.name || "Anónimo"}</strong><small>{idea.email || "Sin correo"}</small></span><span className={`priority priority-${idea.priority}`}>{PRIORITY[idea.priority]}</span></div>
                    </button>
                  ))}
                </div>

                <aside className={`detail-panel ${selected ? "has-selection" : ""}`} aria-label="Detalle de solicitud">
                  {selected ? <IdeaDetail idea={selected} saving={saving} copied={copied} onPatch={patchSelected} onSave={() => void saveIdea(selected)} onCopy={() => void copyEmail(selected.email)}/> : <div className="detail-empty"><div className="empty-icon"><Icon name="spark" size={24}/></div><strong>Elige una solicitud</strong><span>Aquí verás el mensaje completo y podrás organizarlo.</span></div>}
                </aside>
              </div>
            </section>
          </> : <>
            <section className="metrics" aria-label="Resumen de descargas">
              <Metric label="Correos únicos" value={downloadCounts.unique} hint="Personas registradas"/>
              <Metric label="Solicitudes" value={downloadCounts.requests} hint="Enlaces preparados"/>
              <Metric label="Descargas" value={downloadCounts.downloads} hint="Instaladores iniciados" accent/>
              <Metric label="Novedades" value={downloadCounts.consented} hint="Aceptaron recibir noticias"/>
            </section>

            <section className="inbox-panel download-panel">
              <div className="toolbar">
                <label className="search-box"><Icon name="search"/><input value={downloadQuery} onChange={(event) => setDownloadQuery(event.target.value)} placeholder="Buscar por correo o versión…" aria-label="Buscar descargas"/></label>
                <div className="filter-group">
                  <select value={downloadState} onChange={(event) => setDownloadState(event.target.value as DownloadState)} aria-label="Filtrar por descarga"><option value="all">Todas las actividades</option><option value="downloaded">Descargó el instalador</option><option value="requested">Solo pidió el enlace</option></select>
                  <select value={consentState} onChange={(event) => setConsentState(event.target.value as ConsentState)} aria-label="Filtrar por consentimiento"><option value="all">Cualquier consentimiento</option><option value="accepted">Aceptó novedades</option><option value="not-accepted">Sin novedades</option></select>
                  <button className="icon-button" type="button" onClick={() => void loadDashboard(true)} aria-label="Actualizar descargas"><Icon name="refresh"/></button>
                </div>
              </div>

              <div className="inbox-grid">
                <div className="request-list">
                  <div className="list-heading"><span>{filteredDownloads.length} {filteredDownloads.length === 1 ? "correo" : "correos"}</span><small>Actividad más reciente</small></div>
                  {loading ? <LoadingCards/> : filteredDownloads.length === 0 ? <DownloadEmpty/> : filteredDownloads.map((lead) => (
                    <button className={`request-card download-card ${selectedDownloadId === lead.id ? "selected" : ""}`} key={lead.id} type="button" onClick={() => setSelectedDownloadId(lead.id)}>
                      <div className="card-topline"><span className={`status-badge ${lead.download_count > 0 ? "status-completed" : "status-new"}`}><i/>{lead.download_count > 0 ? "Descargó" : "Solicitó enlace"}</span><time dateTime={lead.last_requested_at}>{formatDate(lead.last_requested_at, true)}</time></div>
                      <strong className="request-title download-email">{lead.email}</strong>
                      <p>Versión {lead.version} · {lead.source === "website" ? "Sitio web" : lead.source}</p>
                      <div className="card-footer"><span className="avatar">{lead.email.charAt(0).toUpperCase()}</span><span className="person"><strong>{lead.request_count} {lead.request_count === 1 ? "solicitud" : "solicitudes"}</strong><small>{lead.marketing_consent ? "Aceptó novedades" : "Solo gestión de descarga"}</small></span><span className="priority download-count-pill">{lead.download_count} {lead.download_count === 1 ? "descarga" : "descargas"}</span></div>
                    </button>
                  ))}
                </div>

                <aside className={`detail-panel ${selectedDownload ? "has-selection" : ""}`} aria-label="Detalle de descarga">
                  {selectedDownload ? <DownloadDetail lead={selectedDownload} copied={copied} onCopy={() => void copyEmail(selectedDownload.email)}/> : <div className="detail-empty"><div className="empty-icon"><Icon name="download" size={24}/></div><strong>Elige un correo</strong><span>Aquí verás su historial de solicitudes y descargas.</span></div>}
                </aside>
              </div>
            </section>
          </>}
        </div>
      </section>
    </main>
  );
}

function Metric({ label, value, hint, accent = false }: { label: string; value: number; hint: string; accent?: boolean }) {
  return <article className={`metric-card ${accent ? "accent" : ""}`}><div><span>{label}</span><strong>{String(value).padStart(2, "0")}</strong></div><small>{hint}</small></article>;
}

function LoadingCards() { return <div className="loading-stack" aria-label="Cargando"><div/><div/><div/></div>; }
function Empty() { return <div className="empty-state"><div className="empty-icon"><Icon name="inbox" size={24}/></div><strong>No encontramos solicitudes</strong><span>Prueba con otro filtro o término de búsqueda.</span></div>; }
function DownloadEmpty() { return <div className="empty-state"><div className="empty-icon"><Icon name="download" size={24}/></div><strong>No encontramos descargas</strong><span>Prueba con otro filtro o espera el próximo registro.</span></div>; }

function DownloadDetail({ lead, copied, onCopy }: { lead: DownloadLead; copied: boolean; onCopy: () => void }) {
  return <div className="detail-content download-detail" key={lead.id}>
    <div className="detail-kicker"><span className={`status-badge ${lead.download_count > 0 ? "status-completed" : "status-new"}`}><i/>{lead.download_count > 0 ? "Descarga iniciada" : "Enlace solicitado"}</span><span>#{lead.id.slice(0, 8).toUpperCase()}</span></div>
    <h2>Historial de descarga</h2>
    <div className="detail-author"><span className="avatar large">{lead.email.charAt(0).toUpperCase()}</span><div><strong>{lead.email}</strong><span>Versión {lead.version}</span></div><button type="button" className="copy-button" onClick={onCopy}><Icon name="copy" size={15}/>{copied ? "Copiado" : "Copiar"}</button></div>
    <section className="download-summary" aria-label="Actividad de descarga">
      <article><span>Solicitudes de enlace</span><strong>{lead.request_count}</strong><small>La misma persona puede solicitarlo nuevamente</small></article>
      <article className="accent"><span>Descargas iniciadas</span><strong>{lead.download_count}</strong><small>Redirecciones confirmadas al instalador</small></article>
    </section>
    <div className="meta-grid download-meta-grid">
      <div><span>Primera solicitud</span><strong><Icon name="clock" size={15}/> {formatDate(lead.first_requested_at)}</strong></div>
      <div><span>Última solicitud</span><strong><Icon name="clock" size={15}/> {formatDate(lead.last_requested_at)}</strong></div>
      <div><span>Última descarga</span><strong>{lead.last_downloaded_at ? formatDate(lead.last_downloaded_at) : "Aún no la inició"}</strong></div>
      <div><span>Origen</span><strong>{lead.source === "website" ? "Sitio web" : lead.source}</strong></div>
    </div>
    <section className={`consent-card ${lead.marketing_consent ? "accepted" : ""}`}>
      <span className="consent-icon"><Icon name={lead.marketing_consent ? "check" : "mail"} size={17}/></span>
      <div><strong>{lead.marketing_consent ? "Aceptó recibir novedades" : "No aceptó comunicaciones opcionales"}</strong><p>{lead.marketing_consent ? `Consentimiento registrado${lead.marketing_consent_at ? ` el ${formatDate(lead.marketing_consent_at)}` : ""}` : "Su correo se conserva únicamente para gestionar la descarga y las futuras licencias."}</p></div>
    </section>
    <div className="download-actions"><a href={`mailto:${lead.email}`}><Icon name="mail" size={16}/> Escribir correo</a><span>Registro privado y protegido</span></div>
  </div>;
}

function IdeaDetail({ idea, saving, copied, onPatch, onSave, onCopy }: {
  idea: Idea; saving: boolean; copied: boolean; onPatch: (patch: Partial<Idea>) => void; onSave: () => void; onCopy: () => void;
}) {
  return <div className="detail-content" key={idea.id}>
    <div className="detail-kicker"><span className={`status-badge status-${idea.status}`}><i/>{STATUS[idea.status]}</span><span>#{String(idea.id).padStart(4, "0")}</span></div>
    <h2>{CATEGORY[idea.category] ?? "Solicitud"}</h2>
    <div className="detail-author"><span className="avatar large">{(idea.name ?? idea.email ?? "A").charAt(0).toUpperCase()}</span><div><strong>{idea.name || "Usuario anónimo"}</strong><span>{idea.email || "No dejó correo"}</span></div>{idea.email && <button type="button" className="copy-button" onClick={onCopy}><Icon name="copy" size={15}/>{copied ? "Copiado" : "Copiar"}</button>}</div>
    <section className="message-section"><label>Mensaje completo</label><blockquote>“{idea.message}”</blockquote></section>
    <div className="meta-grid"><div><span>Recibida</span><strong><Icon name="clock" size={15}/> {formatDate(idea.created_at)}</strong></div><div><span>Origen</span><strong>{idea.source === "website" ? "Sitio web" : (idea.source || "Formulario")}</strong></div></div>
    <div className="editor-section">
      <label htmlFor="idea-status">Estado</label>
      <select id="idea-status" value={idea.status} onChange={(event) => onPatch({ status: event.target.value as Status })}>{Object.entries(STATUS).map(([value, label]) => <option value={value} key={value}>{label}</option>)}</select>
      <label>Prioridad</label>
      <div className="priority-picker">{(Object.keys(PRIORITY) as Priority[]).map((value) => <button key={value} className={idea.priority === value ? "active" : ""} type="button" onClick={() => onPatch({ priority: value })}>{PRIORITY[value]}</button>)}</div>
      <label htmlFor="admin-note">Nota interna</label>
      <textarea id="admin-note" value={idea.admin_note ?? ""} onChange={(event) => onPatch({ admin_note: event.target.value })} maxLength={2000} placeholder="Agrega contexto, decisiones o próximos pasos…"/>
      <div className="save-row">{idea.email ? <a href={`mailto:${idea.email}`}><Icon name="mail" size={16}/> Responder por correo</a> : <span/>}<button className="save-button" type="button" disabled={saving} onClick={onSave}>{saving ? "Guardando…" : "Guardar cambios"}</button></div>
    </div>
  </div>;
}
