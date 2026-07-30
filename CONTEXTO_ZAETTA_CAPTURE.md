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
- Entrada: `App/Program.cs`
- Overlay/editor principal: `Capture/CaptureOverlay.cs`
- Instalador: `InstallerZaettaFinal.cs`
- Build actual probado en Linux con Mono mediante `tools/build-native.sh`.
- Ejecutables generados actuales:
  - `ZAETTA_CAPTURE_NATIVE/Zaetta Capture Final.exe`
  - `INSTALADOR_ZAETTA_CAPTURE_FINAL.exe`

Tambien existe una version inicial en Python:

- Carpeta: `ZAETTA_CAPTURE/`
- Archivo principal: `main.py`

La version Python fue util para prototipar, pero la version nativa se siente mas rapida para seleccionar pantalla y debe ser la base principal si se busca experiencia tipo Lightshot.

## 4. Funcionalidades implementadas

- Icono en bandeja del sistema.
- Click sobre el icono de bandeja para iniciar captura.
- Menu de opciones desde el icono de bandeja.
- Opcion chuleable `Mantener posicion del area seleccionada`, inspirada en Lightshot, para que cada captura nueva arranque usando el ultimo rectangulo si existe.
- Opcion `Repetir ultima area` desde el menu de bandeja para forzar el ultimo rectangulo usado.
- Captura de pantalla con seleccion de area.
- Soporte para multiples pantallas.
- Captura sobre todo el escritorio virtual de Windows usando `SystemInformation.VirtualScreen`, no solo la pantalla donde esta el mouse.
- Proceso marcado como DPI-aware al iniciar para evitar recortes desplazados o a medias en equipos con multiples monitores y escalas distintas.
- Selector visual tipo Lightshot, con borde punteado y fondo atenuado.
- Editor inmediato sobre la seleccion.
- Barra compacta de herramientas.
- Herramientas visibles principales.
- Menu de mas herramientas con tres puntos.
- Botones compactos con iconos dibujados para herramientas principales, estilo oscuro/minimal y estados hover/activo mas pulidos.
- Render de botones ajustado con bordes alineados y sin doble reduccion de rectangulo para evitar desniveles visuales en iconos compactos.
- Tooltips descriptivos en botones.
- Boton de candado en la barra inferior para bloquear temporalmente el cierre al hacer clic fuera de la seleccion.
- Copiar con boton.
- Copiar con clic derecho sobre la captura.
- Copiar con `Ctrl + C`.
- Cierre automatico despues de copiar.
- Cancelar seleccion al hacer clic fuera o con `Esc`.
- Guardar imagen localmente.
- Atajos globales con `RegisterHotKey`. Se retiro el hook global de bajo nivel para reducir falsos positivos de antivirus/Teams.
- Bloqueo de capturas simultaneas para evitar overlays infinitos o capturas cada vez mas oscuras.
- Persistencia local de la ultima area usada y de la preferencia `Mantener posicion del area seleccionada`; cuando esta chuleada, incluso el atajo normal abre el overlay con ese rectangulo marcado sobre una captura nueva.
- Herramienta de texto.
- Herramienta para mover elementos.
- La herramienta `Mover` tambien puede mover la seleccion completa: clic en una anotacion mueve la anotacion; clic en espacio vacio dentro de la seleccion mueve todo el rectangulo y arrastra sus anotaciones.
- Herramientas de dibujo.
- Flechas con cabeza agrandada mediante `AdjustableArrowCap` para que se vean mas claras en evidencias.
- Ajuste de color para figuras y trazos.
- Ajuste rapido de grosor mientras se dibuja: clic izquierdo sostenido + rueda del mouse arriba/abajo. Se valida con `Control.MouseButtons` y bandera interna porque `MouseWheel` puede llegar con `e.Button = None`.
- Ajuste posterior: al pasar el mouse sobre una anotacion ya creada, la rueda permite cambiar tamano o grosor segun la herramienta; en pixelado ajusta la intensidad del mosaico.
- Pixelado real por mosaico, con intensidad ajustable por operacion.
- Icono y marca visual Zaetta con el logo oficial.
- Ventana "Acerca de" con desarrollador, version y descripcion.
- Instalador `.exe` con barra de progreso.
- Instalador con logo oficial embebido como recurso `ZaettaLogo`.
- Acceso directo en escritorio.
- Instalacion local en `%LOCALAPPDATA%`.
- Reemplazo/limpieza de versiones anteriores durante instalacion.
- Diagnostico de arranque: si la app falla al iniciar, muestra un mensaje y escribe `Pictures\Zaetta Capture\startup-error.log`.

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
- Si el candado esta activo, clic izquierdo fuera de la seleccion no debe cerrar el capturador.
- Aunque el candado este activo, clic derecho, boton Copiar y `Ctrl + C` deben copiar y cerrar.
- El candado es un estado temporal del overlay activo, no una preferencia global de bandeja. Cada nueva captura inicia desbloqueada.
- El objetivo del candado es permitir que el usuario mantenga la seleccion visible mientras interactua accidentalmente por fuera del rectangulo, sin perder el recorte que ya tenia listo.
- El programa debe sentirse inmediato; la seleccion no puede tener lag perceptible.
- El instalador debe sobreescribir versiones anteriores y evitar que queden varias copias con nombres distintos.
- Al activar captura, el usuario debe poder seleccionar cualquier monitor conectado, incluso si el mouse estaba inicialmente en otro monitor. Por eso `StartCapture` debe usar el escritorio virtual completo.
- En equipos con monitores a 125%, 150% o escalas mixtas, la app debe activar DPI awareness antes de crear ventanas; de lo contrario Windows puede virtualizar coordenadas y copiar una zona incorrecta.
- No se deben abrir multiples overlays al mantener presionado `Impr Pant` o al disparar varias veces el atajo. `TrayContext.captureActive` bloquea una nueva captura hasta que el overlay actual cierre.
- Si `Mantener posicion del area seleccionada` esta chuleado, cualquier captura nueva debe usar la ultima area guardada si existe, incluso cuando se active con atajo normal.
- Si `Mantener posicion del area seleccionada` esta deschuleado, una captura normal debe iniciar desde cero.
- `Repetir ultima area` fuerza el uso de la ultima area aunque la opcion automatica este desactivada.
- La ultima area se guarda en `Pictures\Zaetta Capture\last-selection.txt` como `x,y,width,height`.
- La preferencia se guarda en `Pictures\Zaetta Capture\capture-preferences.txt` como `1` o `0`.

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

Nota actual: `Print Screen` se maneja con `RegisterHotKey`, no con hook global de bajo nivel. Esto reduce alertas de seguridad al compartir el instalador por canales corporativos.

## 8. Arquitectura actual

### Version nativa

`ZAETTA_CAPTURE_NATIVE/`

Contiene:

- `App/`: arranque, metadatos de producto, bandeja del sistema, dialogo de atajo y flujo inicial de captura.
- `SystemIntegration/`: DPI awareness, portapapeles y hotkeys globales mediante `RegisterHotKey`.
- `Editing/`: herramientas, operaciones de dibujo, pixelado y estilos de dibujo.
- `Storage/`: rutas locales, historial, ultima area y preferencias de captura.
- `UI/`: helpers visuales compartidos.
- `Capture/CaptureOverlay.cs`: estado principal, constructor y helpers comunes del overlay.
- `Capture/CaptureOverlay.Keyboard.cs`: foco, atajos del overlay y supresion de menu contextual.
- `Capture/CaptureOverlay.Input.cs`: eventos de mouse, rueda, seleccion y drag.
- `Capture/CaptureOverlay.Toolbar.cs`: barra de herramientas, menus, colores, grosor e iconos de herramientas.
- `Capture/CaptureOverlay.Tools.cs`: seleccion de herramientas, menus, colores, grosor, iconos y alternancia del candado.
- `Capture/CaptureOverlay.Rendering.cs`: pintado de seleccion, anotaciones, handles y render final.
- `Capture/CaptureOverlay.Interaction.cs`: hit testing, mover, escalar y redimensionar anotaciones/seleccion.
- `Capture/CaptureOverlay.Adjustments.cs`: cambios de tamano/grosor/intensidad por rueda o botones.
- `Capture/CaptureOverlay.Text.cs`: edicion de texto sobre la captura.
- `Capture/CaptureOverlay.Commands.cs`: copiar, guardar y deshacer.
- `Capture/ScreenshotService.cs`: captura del escritorio virtual de Windows.
- `Capture/ScreenshotCapture.cs`: resultado de captura con bounds e imagen.
- `Storage/HistoryService.cs`: guardado y apertura del historial local.
- `Storage/LastSelectionStore.cs`: persistencia de la ultima area seleccionada.
- `Storage/CapturePreferencesStore.cs`: persistencia de `Mantener posicion del area seleccionada`.

### Candado de captura

El candado vive dentro de `CaptureOverlay` porque aplica solo a la captura abierta en ese momento. No debe guardarse como preferencia global.

Archivos relacionados:

- `Capture/CaptureOverlay.cs`: campo `selectionLocked`.
- `Capture/CaptureOverlay.Input.cs`: evita cerrar con clic izquierdo fuera de la seleccion cuando el candado esta activo.
- `Capture/CaptureOverlay.Toolbar.cs`: agrega el boton `Lock` en la barra inferior.
- `Capture/CaptureOverlay.Tools.cs`: metodo `ToggleSelectionLock`.
- `UI/ZaettaButton.cs`: dibujo del icono de candado.

Comportamiento esperado:

- Desbloqueado: clic izquierdo fuera de la seleccion cierra/cancela la captura.
- Bloqueado: clic izquierdo fuera de la seleccion no cierra la captura.
- Bloqueado o desbloqueado: clic derecho dentro de la seleccion copia y cierra.
- Bloqueado o desbloqueado: boton Copiar copia y cierra.
- Bloqueado o desbloqueado: `Ctrl + C` copia y cierra.
- `Esc` y boton `X` siguen funcionando como cancelacion manual.
- `SystemIntegration/StartupDiagnostics.cs`: log de errores de arranque.
- `Legacy/EditorForm.cs`: editor anterior conservado como referencia.
- `ZaettaCapture.cs`: archivo historico minimo que apunta a la nueva separacion.
- `TrayContext.StartCapture`, actualmente captura `SystemInformation.VirtualScreen` para permitir seleccion libre en cualquier pantalla.
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
.\tools\build-native.ps1
```

En Linux con Mono instalado:

```bash
./tools/build-native.sh
```

Requisito Linux:

```bash
sudo apt-get install -y mono-devel
```

Compilar instalador final:

```powershell
.\tools\build-native.ps1
```

En Linux con Mono instalado:

```bash
./tools/build-native.sh
```

El build actual embebe en el instalador:

- `ZaettaApp`: ejecutable final de la aplicacion.
- `ZaettaLogo`: `ZAETTA_CAPTURE/logo_oficial.png`.

Tamanos recientes de referencia:

- `ZAETTA_CAPTURE_NATIVE/Zaetta Capture Final.exe`: ~172 KB.
- `INSTALADOR_ZAETTA_CAPTURE_FINAL.exe`: ~1.7 MB cuando incluye `ZaettaLogo`.
- `ZAETTA_CAPTURE/logo_oficial.png`: ~1.4 MB.

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

- Evaluar presets visuales para pixelado suave/medio/fuerte si el equipo los necesita.
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

1. `ZAETTA_CAPTURE_NATIVE/App/Program.cs`
2. `ZAETTA_CAPTURE_NATIVE/App/TrayContext.cs`
3. `ZAETTA_CAPTURE_NATIVE/Capture/CaptureOverlay.cs`
4. `ZAETTA_CAPTURE_NATIVE/Editing/`
5. `ZAETTA_CAPTURE_NATIVE/UI/`
6. `ZAETTA_CAPTURE_NATIVE/InstallerZaettaFinal.cs`
7. Este archivo `CONTEXTO_ZAETTA_CAPTURE.md`
8. `README.md`

Si hay que corregir funcionalidad de captura, hacerlo primero en la version nativa.

Si hay que corregir el instalador, tocar solo `InstallerZaettaFinal.cs`.

Si hay que cambiar branding/icono, el logo fuente oficial esta en `ZAETTA_CAPTURE/logo_oficial.png` y el icono que se embebe en los ejecutables es `ZAETTA_CAPTURE/zaetta_icon.ico`. Para regenerar el ICO desde el PNG se puede usar `tools/make-ico.ps1`; el script recorta el canvas alrededor de la placa y la Z para que el icono se lea mejor en bandeja y accesos directos pequenos.

Si la app no aparece en bandeja al ejecutarse, revisar primero:

1. La flecha de iconos ocultos de Windows.
2. `Pictures\Zaetta Capture\startup-error.log`.
3. Compatibilidad de metodos .NET Framework si el build fue generado con Mono.

## 16. Ultimos cambios registrados

### 2026-07-28

- Se refactorizo la app nativa para salir del monolito original `ZaettaCapture.cs`.
- Se separaron responsabilidades en `App/`, `Capture/`, `Editing/`, `Storage/`, `SystemIntegration/`, `UI/` y `Legacy/`.
- `ZaettaCapture.cs` quedo como archivo historico minimo; el overlay real vive en `Capture/`.
- `CaptureOverlay` se dividio como `partial class` en archivos por responsabilidad: estado/base, teclado, mouse/input, toolbar, tools, rendering, interaction, adjustments, text y commands.
- Se agregaron servicios internos:
  - `AppInfo`.
  - `ScreenshotService`.
  - `ScreenshotCapture`.
  - `HistoryService`.
  - `LastSelectionStore`.
  - `CapturePreferencesStore`.
  - `StartupDiagnostics`.
- Se agrego build multiplataforma:
  - `tools/build-native.ps1` para Windows/.NET Framework.
  - `tools/build-native.sh` para Linux con Mono.
- Se compilo exitosamente con Mono 6.8 en Ubuntu.
- Se agrego fallback de icono de bandeja con `SystemIcons.Application` si falla `Icon.ExtractAssociatedIcon`.
- Se agrego manejo global de error de arranque en `Program.Main`; los errores se muestran y se guardan en `Pictures\Zaetta Capture\startup-error.log`.
- Se corrigio compatibilidad .NET de `String.Split` en `LastSelectionStore` usando `Split(new[] { ',' }, StringSplitOptions.None)`.
- Se implemento `Repetir ultima area`.
- Se implemento la opcion chuleable `Mantener posicion del area seleccionada`, inspirada en Lightshot.
- Se persistio la ultima area seleccionada en `Pictures\Zaetta Capture\last-selection.txt`.
- Se persistio la preferencia de mantener area en `Pictures\Zaetta Capture\capture-preferences.txt`.
- Se implemento movimiento de la seleccion completa con la herramienta `Mover`; las anotaciones se desplazan junto con el rectangulo.
- Se implemento boton de candado en el overlay para impedir que un clic izquierdo fuera de la seleccion cierre accidentalmente la captura.
- Se mantuvo el cierre normal al copiar con clic derecho, boton Copiar o `Ctrl + C`, incluso cuando el candado esta activo.
- Se recompilo la app y el instalador.
- Se incrusto `ZAETTA_CAPTURE/logo_oficial.png` como recurso `ZaettaLogo` en el instalador.
- El instalador quedo nuevamente alrededor de 1.7 MB por incluir el logo oficial nitido.

### 2026-07-27

- Se agrego `CONTEXTO_ZAETTA_CAPTURE.md` para que cualquier agente o desarrollador pueda retomar el proyecto sin mezclarlo con PlayOps Suite.
- Se corrigio `README.md` para apuntar a este archivo de contexto.
- Se retiro el hook nativo de teclado para reducir falsos positivos de antivirus/Teams.
- `Impr Pant` queda gestionado por `RegisterHotKey`; si algun equipo intercepta esa tecla, se recomienda configurar un atajo alternativo desde la app.
- Se recompilo la aplicacion nativa y el instalador final.
- Se aumento el tamano de la cabeza de las flechas usando `AdjustableArrowCap`.
- Se cambio la captura para cubrir el escritorio virtual completo y permitir seleccionar cualquier monitor conectado.
- Se corrigio el bug de overlays/capturas infinitas que oscurecian progresivamente la pantalla al dispararse varias capturas seguidas.
- Se pulio el render de botones e iconos compactos con coordenadas `RectangleF`, `PixelOffsetMode.Half` y centrado real para evitar fondos descuadrados o bordes asimetricos.
- Se agrego ajuste rapido por hover + scroll: al pasar el mouse sobre una anotacion y mover la rueda, flechas/lineas/trazos cambian grosor, texto/numeros cambian tamano, rectangulos escalan su area y pixelado ajusta intensidad.
- Se separo el comportamiento de rueda: scroll normal agranda o achica el objeto bajo el cursor; `Ctrl + scroll` ajusta el grosor de flechas, lineas, marcos, lapiz y resaltador.
- Se agrego intensidad ajustable de pixelado: `+`/`-` cambian la intensidad cuando Pixelar esta activo y cada operacion guarda su propio nivel.
- Se agrego el logo oficial como fuente en `ZAETTA_CAPTURE/logo_oficial.png`, se regenero `ZAETTA_CAPTURE/zaetta_icon.ico` multi-tamano y se recompilaron app e instalador con ese icono.
- Se reemplazo la Z azul hardcodeada del instalador por el logo/icono oficial embebido.
- Se ajusto el encabezado del instalador para usar una Z dorada recortada desde el logo oficial y se cambio la paleta de boton/progreso de cyan a dorado.
- Se dejo una sola Z visual en el instalador: el icono oficial funciona como la Z del titulo y el texto queda como "aetta Capture", evitando una Z blanca redundante.
- Se movio el boton principal del instalador hacia adentro para que "Finalizar" no quede pegado/cortado contra el borde derecho.
- Se reintrodujo el glow/difuminado de la Z como fondo pintado directamente en el formulario y ubicado fuera del texto, para dar profundidad sin tapar titulo/subtitulo.
- Se acerco la Z/icono del titulo al texto, dibujandola por encima del label para que funcione mejor como la Z de "Zaetta", y se elimino el blur por pixel calculado al abrir el instalador para mejorar el tiempo de arranque.
- Se cambio la paleta visual de la app principal de cyan/azul a negro/dorado: acentos globales, Z de la barra, botones activos, hover y menus.
- Se corrigio `Ctrl + Z` para que al deshacer una anotacion tambien se limpien seleccion y estados de mover/redimensionar, evitando contornos fantasma.
- Se subieron estos cambios a GitHub.

Commits relevantes:

- `afc10cf` - Agregar contexto de trabajo de Zaetta Capture.
- `74dac16` - Corregir captura con Impr Pant en pantallas pequenas.
- `39806c4` - Agrandar cabeza de flechas.
