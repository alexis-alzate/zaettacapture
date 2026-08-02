# Zaetta Software Website

Sitio estatico inicial para `zaettasoftware.com`.

Arquitectura actual: DreamHost conserva el dominio/DNS, Vercel aloja esta carpeta como sitio estatico y GitHub Releases aloja los instaladores `.exe`.

## Archivos

- `index.html`: pagina principal de descarga.
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

El boton de descarga y `latest.json` apuntan al asset versionado en GitHub Releases.

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
Tag: v1.0.24
Title: Zaetta Capture v1.0.24
Asset: ZaettaCaptureSetup.exe
SHA256: be6a350fbf416f8261d818074e2a0ae04c6cc56b54d8de19bf7bf39ca981270c
Size: 1755136 bytes
```

URL esperada del asset:

```text
https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0.24/ZaettaCaptureSetup.exe
```

`latest.json` debe apuntar a esa URL para que el updater descargue desde GitHub Releases y no desde Vercel.
