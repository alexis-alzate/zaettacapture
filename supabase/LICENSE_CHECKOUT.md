# Flujo de licencia solidaria

Este módulo adapta a Zaetta Capture el patrón de compra probado en Lujo Urban:
orden pendiente, Checkout Pro, Webhook firmado, confirmación directa con Mercado
Pago, aprobación atómica y entrega por correo.

El precio no llega desde el navegador. El servidor siempre crea una licencia
personal por `10.000 COP` y el Webhook rechaza cualquier pago cuyo valor, moneda,
referencia o modo no coincida.

## Estado actual

El código está preparado localmente, pero el cobro público permanece apagado:

```html
<body data-license-checkout-enabled="false">
```

No cambiarlo a `true` hasta completar toda la lista de lanzamiento.

## Aplicación de Mercado Pago

Usar la misma cuenta comercial de Lujo Urban, pero crear una aplicación separada
llamada `Zaetta Capture`. No reutilizar la aplicación ni el secreto de Webhooks
de la tienda de beats.

En la aplicación de Zaetta:

1. Activar Checkout Pro.
2. Configurar Webhooks en modo pruebas y producción.
3. Seleccionar el evento `Payments`.
4. Utilizar esta URL:

```text
https://ocnoiraaqosfmbluccba.supabase.co/functions/v1/mercadopago-webhook
```

5. Guardar por separado el Access Token y el secreto generado para Webhooks.

El código exige la firma `x-signature`. Un Webhook sin secreto o con firma
incorrecta nunca aprueba una licencia.

## Secretos de Supabase

Configurar en Edge Function Secrets. Nunca escribir los valores en Git:

```text
MERCADOPAGO_ACCESS_TOKEN
MERCADOPAGO_WEBHOOK_SECRET
MERCADOPAGO_EXPECT_LIVE_MODE=false
ZAETTA_SITE_URL=https://zaettasoftware.com
RESEND_API_KEY
ZAETTA_LICENSE_FROM_EMAIL="Zaetta Capture <licencias@zaettasoftware.com>"
ZAETTA_SUPPORT_EMAIL=soporte@zaettasoftware.com
```

Para producción, cambiar `MERCADOPAGO_EXPECT_LIVE_MODE` a `true` y usar las
credenciales productivas de la aplicación Zaetta Capture.

## Componentes

- `license-checkout`: valida correo y consentimientos, crea la orden privada y
  solicita una preferencia de `10.000 COP` a Mercado Pago.
- `mercadopago-webhook`: valida la firma, vuelve a consultar el pago en Mercado
  Pago y aprueba o revoca la licencia según el estado real.
- `license-status`: recibe el ID y el token privado guardado en `sessionStorage`
  y devuelve solamente el estado mínimo para la pantalla de resultado.
- `license_orders`: conserva trazabilidad comercial y de entrega.
- `licenses`: guarda las claves privadas y su estado.
- `solidarity_ledger`: diferencia valores reservados, donados, cancelados y en
  revisión por reembolso.
- `license_payment_events`: conserva un registro mínimo de procesamiento sin
  almacenar el cuerpo completo del Webhook.
- `trial_registrations`: fija la fecha de inicio de los 30 días de prueba por
  equipo (`device_fingerprint`), para que reinstalar la app no la reinicie.
- `license_devices`: dispositivos activados por licencia, con tope de
  `max_devices`. Permite desactivar un equipo para liberar cupo en otro.
- `trial-start`: la llama la app de Windows al primer uso. Registra o
  devuelve la fecha de inicio/fin de la prueba de 30 días para ese equipo.
- `license-activate`: la llama la app de Windows para activar una clave en
  el equipo, o para desactivar un equipo y liberar cupo (`action: "activate"`
  o `action: "deactivate"`). Se llama también en cada arranque para
  revalidar el estado (si se revoca o reembolsa una licencia, esto la
  vuelve a bloquear).

## Huella de equipo (`device_fingerprint`)

La app de Windows identifica el equipo con prioridad de fuentes, para que
reinstalar el sistema operativo no reinicie la prueba gratuita ni libere
cupos de licencia:

1. Serial de BIOS/placa base + disco físico (`fingerprint_source = hardware`).
   Sobrevive una reinstalación de Windows porque vive en el firmware.
2. Si el equipo no expone esos valores (comunes en equipos económicos o
   máquinas virtuales), se usa el `MachineGuid` de Windows como respaldo
   (`fingerprint_source = machine_guid_fallback`). Ese sí se regenera al
   reinstalar Windows, por lo que la protección es menor en ese caso.

Ninguna huella de software es infalible: alguien técnico con una máquina
virtual podría falsificar estos valores. La huella cierra el caso común
(reinstalar Windows para resetear el contador), no el caso adversarial.

## Lista obligatoria antes del lanzamiento

1. Completar nombre o razón social, identificación tributaria y domicilio que
   deban mostrarse en las condiciones de comercio electrónico.
2. Convertir `terminos-licencia.html` de borrador a versión vigente y actualizar
   `terms_version` en `schema.sql` antes de recibir aceptaciones reales.
3. Verificar `licencias@zaettasoftware.com` en Resend.
4. ~~Aplicar `schema.sql` y ejecutar los asesores de seguridad de Supabase.~~
   Hecho el 2026-08-17 (migración `zaetta_capture_full_schema`).
5. Desplegar `license-checkout`, `license-status` y `mercadopago-webhook`
   (`trial-start` y `license-activate` ya están desplegadas y probadas desde
   el 2026-08-17) y confirmar que `verify_jwt = false` solo aplica a estos
   endpoints públicos controlados.
6. Probar una compra completa con credenciales y usuarios de prueba.
7. Simular reintentos del mismo Webhook y comprobar que existe una sola licencia.
8. Probar pago rechazado, pendiente, reembolsado y contracargo.
9. Revisar los límites por correo, monitorear abuso y añadir un desafío anti-bot
   si el tráfico real supera la protección inicial.
10. Implementar y probar la activación dentro de la aplicación de Windows.
11. Revisar los términos comerciales y tributarios con un profesional en Colombia.
12. Cambiar `data-license-checkout-enabled` a `true` solamente al terminar todo.

## Transparencia solidaria

Después de un pago aprobado, los `10.000 COP` quedan en estado `reserved`. Solo
un proceso administrativo puede cambiar el registro a `donated`, indicando la
causa, la fecha y el comprobante. Si hay reembolso antes de la entrega, el valor
se cancela. Si la entrega ya ocurrió, el caso pasa a `refund_review` para que
Zaetta asuma y documente la diferencia.
