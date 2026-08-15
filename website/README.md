# Zaetta Software Website

Sitio estatico inicial para `zaettasoftware.com`.

Arquitectura actual: DreamHost conserva el dominio/DNS, Vercel aloja esta carpeta como sitio estatico y GitHub Releases aloja los instaladores `.exe`.

## Archivos

- `index.html`: pagina principal de descarga.
- `privacidad.html`: política pública de tratamiento de datos personales.
- `legal.css`: estilos de las páginas legales.
- `styles.css`: estilos visuales.
- `assets/logo_oficial.png`: logo oficial usado en la pagina.
- `latest.json`: manifiesto que lee el actualizador interno de Zaetta Capture.
- `.htaccess`: reglas utiles si alguna vez se publica en Apache/DreamHost.

## Publicacion

Vercel debe apuntar al root directory:

```text
website
```

La estructura publica esperada queda:

```text
https://zaettasoftware.com/
https://zaettasoftware.com/latest.json
```

`latest.json` apunta al asset versionado en GitHub Releases para conservar las
actualizaciones automaticas de la aplicacion.

Los botones publicos usan un flujo de descarga protegido:

1. La persona escribe su correo en el formulario del sitio.
2. La persona acepta la política de privacidad; las novedades siguen siendo opcionales.
3. La funcion `download-counter` guarda el registro privado en Supabase.
4. La funcion entrega un enlace firmado que vence en 10 minutos.
5. Ese enlace registra la descarga y redirige al instalador de GitHub Releases.

El consentimiento para recibir novedades es opcional y se guarda separado del
registro necesario para gestionar la descarga y futuras licencias.

La versión actual se ofrece como acceso anticipado gratuito. El sitio no debe
presentar un botón de compra ni prometer una licencia de pago hasta que existan
el cobro y la activación correspondientes.

## DNS

Si DreamHost tambien administra el hosting, normalmente basta con apuntar el dominio al hosting desde el panel de DreamHost.

Registros usados para Vercel:

```text
A      @      216.198.79.1
CNAME  www    b6f74b7a12af6643.vercel-dns-017.com
```

## Releases y upgrades

El instalador versionado debe vivir en GitHub Releases.

Release actual:

```text
Tag: v1.0.29
Title: Zaetta Capture v1.0.29
Asset: ZaettaCaptureSetup.exe
SHA256: 7a9d6ded73757dd041bb6c57a6689ea3c709b398cc0b37ad61d058e17511a903
Size: 1762304 bytes
```

URL esperada del asset:

```text
https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.29/ZaettaCaptureSetup.exe
```

`latest.json` debe apuntar a esa URL para que el updater descargue desde GitHub Releases y no desde Vercel.
