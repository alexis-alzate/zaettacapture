# Contexto de Trabajo - Zaetta Capture

## 1. Que es Zaetta Capture

Zaetta Capture es una aplicacion de escritorio independiente para capturar pantalla, editar la evidencia y copiarla rapido al portapapeles.

La idea principal es tener una alternativa interna, liviana y controlada al flujo de herramientas tipo Lightshot o Greenshot, evitando depender de instaladores externos que puedan venir con componentes no deseados o alertas de seguridad.

Zaetta Capture no hace parte de PlayOps Suite. Es otro producto y debe mantenerse en un repositorio separado.

## 2. Objetivo del producto

Permitir que el equipo capture evidencia visual de forma rapida, clara y segura.

El flujo esperado es:

1. El usuario activa la captura desde el icono de bandeja o con un atajo.
2. Selecciona un area de la pantalla.
3. La captura queda visible en un editor flotante.
4. Puede dibujar, marcar, escribir texto, pixelar informacion sensible o mover elementos.
5. Puede copiar la imagen al portapapeles.
6. Al copiar, la captura se cierra y el usuario puede pegarla en Teams, WhatsApp, correo o cualquier sistema.

## 3. Estado actual

La version funcional principal es la nativa en C# WinForms:

- Carpeta: `ZAETTA_CAPTURE_NATIVE/`
- Archivo principal: `ZaettaCapture.cs`
- Instalador: `InstallerZaettaFinal.cs`

Tambien existe una version inicial en Python:

- Carpeta: `ZAETTA_CAPTURE/`
- Archivo principal: `main.py`

La version Python fue util para prototipar, pero la version nativa se siente mas rapida para seleccionar pantalla y debe ser la base principal si se busca experiencia tipo Lightshot.

## 4. Funcionalidades implementadas

- Icono en bandeja del sistema.
- Click sobre el icono de bandeja para iniciar captura.
- Menu de opciones desde el icono de bandeja.
- Captura de pantalla con seleccion de area.
- Soporte para multiples pantallas.
- Captura sobre todo el escritorio virtual de Windows usando `SystemInformation.VirtualScreen`, no solo la pantalla donde esta el mouse.
- Selector visual tipo Lightshot, con borde punteado y fondo atenuado.
- Editor inmediato sobre la seleccion.
- Barra compacta de herramientas.
- Herramientas visibles principales.
- Menu de mas herramientas con tres puntos.
- Botones compactos con iconos dibujados para herramientas principales, estilo oscuro/minimal y estados hover/activo mas pulidos.
- Tooltips descriptivos en botones.
- Copiar con boton.
- Copiar con clic derecho sobre la captura.
- Copiar con `Ctrl + C`.
- Cierre automatico despues de copiar.
- Cancelar seleccion al hacer clic fuera o con `Esc`.
- Guardar imagen localmente.
- Hook nativo para `Impr Pant`, consumiendo la tecla para evitar que Windows o teclados pequenos ejecuten zoom u otra accion del sistema.
- Bloqueo de capturas simultaneas para evitar overlays infinitos o capturas cada vez mas oscuras.
- Herramienta de texto.
- Herramienta para mover elementos.
- Herramientas de dibujo.
- Flechas con cabeza agrandada mediante `AdjustableArrowCap` para que se vean mas claras en evidencias.
- Ajuste de color para figuras y trazos.
- Icono y marca visual Zaetta.
- Ventana "Acerca de" con desarrollador, version y descripcion.
- Instalador `.exe` con barra de progreso.
- Acceso directo en escritorio.
- Instalacion local en `%LOCALAPPDATA%`.
- Reemplazo/limpieza de versiones anteriores durante instalacion.

## 5. Herramientas de edicion esperadas

La aplicacion debe mantenerse cercana al flujo de Lightshot:

- Flecha.
- Marco o rectangulo.
- Linea.
- Lapiz.
- Resaltador.
- Pixelar.
- Numero.
- Texto.
- Mover elementos.
- Deshacer.
- Copiar.
- Guardar.
- Cerrar.

Las herramientas mas usadas deben estar visibles o cerca del area seleccionada. Las herramientas secundarias pueden estar en un menu compacto.

## 6. Reglas importantes de comportamiento

- El selector de captura no debe cambiar de color segun el color de dibujo.
- El selector debe verse como una seleccion limpia y profesional: borde punteado, fondo atenuado y manejadores discretos.
- El color elegido solo aplica a flechas, texto, marcos, lineas, lapiz y resaltador.
- Las flechas deben tener cabeza visible y profesional. Actualmente se usa `AdjustableArrowCap(5.8f, 7.2f, true)`.
- Al copiar, no debe abrir ventana de guardar.
- Al copiar, no debe quedar ninguna ventana flotante activa.
- Al hacer clic derecho sobre la captura, debe copiar sin abrir el menu contextual de Windows.
- Si el usuario cancela, debe cerrar todo y devolver el control normal del mouse.
- El programa debe sentirse inmediato; la seleccion no puede tener lag perceptible.
- El instalador debe sobreescribir versiones anteriores y evitar que queden varias copias con nombres distintos.
- Al activar captura, el usuario debe poder seleccionar cualquier monitor conectado, incluso si el mouse estaba inicialmente en otro monitor. Por eso `StartCapture` debe usar el escritorio virtual completo.
- No se deben abrir multiples overlays al mantener presionado `Impr Pant` o al disparar varias veces el atajo. `TrayContext.captureActive` bloquea una nueva captura hasta que el overlay actual cierre.

## 7. Atajos actuales y deseados

Atajos deseados:

- `Print Screen`: iniciar captura.
- `Delete` o `Supr`: opcion configurable para iniciar captura si el usuario lo prefiere.
- `Ctrl + Shift + S`: atajo alternativo.
- `Ctrl + C`: copiar captura editada.
- `Esc`: cancelar captura o cerrar editor.
- `R`: seleccionar rectangulo.
- `T`: seleccionar texto.
- `F`: seleccionar flecha.
- `L`: seleccionar linea.
- `P`: seleccionar lapiz o pixelar segun configuracion.

Pendiente importante: dejar un panel simple para cambiar el atajo sin tocar codigo.

Nota actual: `Print Screen` se maneja con un hook de bajo nivel cuando no tiene modificadores. Esto se hizo porque en pantallas pequenas o portatiles algunos equipos ejecutaban zoom en vez de abrir la captura. El hook consume la tecla y lanza Zaetta, evitando que el evento continue hacia Windows u otra app.

El hook debe disparar una sola captura por pulsacion. Actualmente marca `shortcutDown` en `KeyDown` y lo libera en `KeyUp`, evitando auto-repeticiones cuando la tecla queda sostenida.

## 8. Arquitectura actual

### Version nativa

`ZAETTA_CAPTURE_NATIVE/ZaettaCapture.cs`

Contiene:

- Arranque de la app.
- Bandeja del sistema.
- Captura de pantalla.
- Seleccion de area.
- Editor flotante.
- Herramientas de dibujo.
- Copia al portapapeles.
- Guardado de imagen.
- Ventana Acerca de.
- `KeyboardShortcutHook`, usado para capturar `Impr Pant` con mayor control que `RegisterHotKey`.
- `DrawingStyle.ConfigureLineCap`, helper que centraliza el estilo de lineas/flechas.
- `StartCapture`, actualmente captura `SystemInformation.VirtualScreen` para permitir seleccion libre en cualquier pantalla.
- `TrayContext.captureActive`, bandera que impide abrir mas de una captura al mismo tiempo.

`ZAETTA_CAPTURE_NATIVE/InstallerZaettaFinal.cs`

Contiene:

- Instalacion local.
- Copia del ejecutable final.
- Creacion de acceso directo.
- Limpieza/reemplazo de versiones anteriores.
- Barra de progreso.
- Mensajes de exito o error.

### Version Python

`ZAETTA_CAPTURE/main.py`

Fue la primera version. Sirve como referencia funcional y visual, pero no debe ser la prioridad para rendimiento.

## 9. Comandos de compilacion

Desde:

```powershell
cd C:\Automatizaciones\zaettacapture_repo
```

Compilar aplicacion nativa:

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /out:'.\ZAETTA_CAPTURE_NATIVE\Zaetta Capture Final.exe' /win32icon:'.\ZAETTA_CAPTURE\zaetta_icon.ico' /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll '.\ZAETTA_CAPTURE_NATIVE\ZaettaCapture.cs'
```

Compilar instalador final:

```powershell
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /out:'.\INSTALADOR_ZAETTA_CAPTURE_FINAL.exe' /win32icon:'.\ZAETTA_CAPTURE\zaetta_icon.ico' "/resource:.\ZAETTA_CAPTURE_NATIVE\Zaetta Capture Final.exe,ZaettaApp" /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll '.\ZAETTA_CAPTURE_NATIVE\InstallerZaettaFinal.cs'
```

## 10. Como ejecutar en desarrollo

Version nativa:

```powershell
cd C:\Automatizaciones\zaettacapture_repo
& '.\ZAETTA_CAPTURE_NATIVE\Zaetta Capture Final.exe'
```

Version Python:

```powershell
cd C:\Automatizaciones\zaettacapture_repo\ZAETTA_CAPTURE
python -m pip install -r requirements.txt
python main.py
```

## 11. Repositorio

Repositorio GitHub:

```text
https://github.com/alexis-alzate/zaettacapture.git
```

Este repositorio debe contener codigo fuente, README y contexto. No debe versionar instaladores pesados, archivos temporales ni capturas personales.

## 12. Pendientes recomendados

Prioridad alta:

- Mejorar completamente el movimiento de elementos dibujados para que no se queden pegados.
- Permitir redimensionar flechas, rectangulos y textos con manejadores.
- Implementar configuracion persistente de atajos.
- Confirmar en varios equipos que `Print Screen` y `Supr` se puedan capturar de forma estable.
- Asegurar que copiar por boton, clic derecho y `Ctrl + C` compartan exactamente la misma logica.
- Validar que al copiar/cancelar no quede ningun overlay activo.

Prioridad media:

- Mejorar pixelado para que sea real y no solo un rectangulo visual.
- Agregar menu compacto con iconos y descripcion.
- Guardar ultima carpeta usada.
- Agregar historial local de ultimas capturas guardadas.
- Agregar opcion de abrir carpeta de evidencias.
- Agregar modo de color claro/oscuro para la barra.

Prioridad baja:

- Exportar a PDF.
- Subida opcional a SharePoint o OneDrive.
- Firma visual automatica en evidencias.
- Modo corporativo con marca de agua.

## 13. Decisiones tomadas

- Zaetta Capture debe ser independiente de PlayOps Suite.
- La version nativa es preferible por rendimiento.
- El instalador puede pesar mas que Lightshot porque incluye runtime/logica propia y no depende de un instalador externo comercial.
- El peso del instalador no es un problema grave si la app sera usada constantemente y evita instalar herramientas externas con alertas.
- La prioridad no es competir con todas las funciones de Lightshot, sino cubrir el flujo operativo seguro: capturar, marcar, copiar y cerrar rapido.

## 14. Criterio de calidad visual

La app debe verse:

- Minimalista.
- Compacta.
- Rapida.
- No corporativa pesada.
- Con barra de herramientas fina.
- Con botones limpios.
- Con iconos claros.
- Con selector parecido a Lightshot.
- Con color principal sobrio.

Evitar:

- Botones muy grandes.
- Bordes gruesos.
- Colores chillones por defecto.
- Barras invasivas.
- Ventanas innecesarias.
- Menus gigantes.

## 15. Nota para retomar trabajo

Cuando se vuelva a trabajar en esta app, empezar revisando:

1. `ZAETTA_CAPTURE_NATIVE/ZaettaCapture.cs`
2. `ZAETTA_CAPTURE_NATIVE/InstallerZaettaFinal.cs`
3. Este archivo `CONTEXTO_ZAETTA_CAPTURE.md`
4. `README.md`

Si hay que corregir funcionalidad de captura, hacerlo primero en la version nativa.

Si hay que corregir el instalador, tocar solo `InstallerZaettaFinal.cs`.

Si hay que cambiar branding/icono, revisar `ZAETTA_CAPTURE/zaetta_icon.ico`.

## 16. Ultimos cambios registrados

### 2026-07-27

- Se agrego `CONTEXTO_ZAETTA_CAPTURE.md` para que cualquier agente o desarrollador pueda retomar el proyecto sin mezclarlo con PlayOps Suite.
- Se corrigio `README.md` para apuntar a este archivo de contexto.
- Se cambio la captura de `Impr Pant` para usar un hook nativo de teclado cuando no hay modificadores.
- Se corrigio el caso donde en equipos pequenos `Impr Pant` podia activar zoom en vez de Zaetta Capture.
- Se recompilo la aplicacion nativa y el instalador final.
- Se aumento el tamano de la cabeza de las flechas usando `AdjustableArrowCap`.
- Se cambio la captura para cubrir el escritorio virtual completo y permitir seleccionar cualquier monitor conectado.
- Se corrigio el bug de overlays/capturas infinitas que oscurecian progresivamente la pantalla al dispararse varias capturas seguidas.
- Se subieron estos cambios a GitHub.

Commits relevantes:

- `afc10cf` - Agregar contexto de trabajo de Zaetta Capture.
- `74dac16` - Corregir captura con Impr Pant en pantallas pequenas.
- `39806c4` - Agrandar cabeza de flechas.
