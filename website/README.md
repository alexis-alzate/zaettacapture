# Zaetta Software Website

Sitio estatico inicial para `zaettasoftware.com`.

Nota importante: por ahora `zaettasoftware.com` es un dominio comprado en DreamHost. Un dominio no aloja archivos por si solo; falta definir hosting para publicar esta carpeta.

## Archivos

- `index.html`: pagina principal de descarga.
- `styles.css`: estilos visuales.
- `assets/logo_oficial.png`: logo oficial usado en la pagina.
- `downloads/ZaettaCaptureSetup.exe`: instalador publico.
- `latest.json`: manifiesto para una futura funcion de autoactualizacion.
- `.htaccess`: reglas para servir `latest.json` sin cache largo y tratar `.exe` como descarga.

## Publicacion

Si se contrata hosting en DreamHost, subir el contenido de esta carpeta al directorio web del dominio, normalmente:

```text
zaettasoftware.com/
```

La estructura publica esperada queda:

```text
https://zaettasoftware.com/
https://zaettasoftware.com/downloads/ZaettaCaptureSetup.exe
https://zaettasoftware.com/latest.json
```

Sube tambien `.htaccess`; algunos clientes FTP lo ocultan por empezar con punto.

Si se usa otro hosting estatico, subir esta misma carpeta a ese proveedor y luego apuntar el DNS del dominio desde DreamHost.

## DNS

Si DreamHost tambien administra el hosting, normalmente basta con apuntar el dominio al hosting desde el panel de DreamHost.

Si el sitio se aloja en otro servicio, el DNS de DreamHost debe apuntar al proveedor externo con registros `A`, `AAAA` o `CNAME`, segun indique ese proveedor.

## Releases y upgrades

El instalador versionado debe vivir en GitHub Releases.

Release inicial:

```text
Tag: v1.0
Title: Zaetta Capture v1.0
Asset: ZaettaCaptureSetup.exe
SHA256: 8b571227d196cd58f90a04b9a34862602b908b670ed7e4566fe12ca25539a570
Size: 1723904 bytes
```

URL esperada del asset:

```text
https://github.com/alexis-alzate/zaettacapture/releases/download/v1.0/ZaettaCaptureSetup.exe
```

`latest.json` debe apuntar a esa URL para que la futura app updater descargue desde GitHub Releases y no desde Vercel.
