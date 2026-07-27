# Zaetta Capture

Capturador de pantalla corporativo local, inspirado en el flujo de Lightshot.

## Objetivo

Capturar, editar, copiar y guardar evidencias sin subir informacion a servicios externos.

## Funciones

- Captura por boton.
- Atajo global `Ctrl + Shift + S` si la libreria `keyboard` esta disponible.
- Seleccion de area con pantalla oscurecida.
- Editor inmediato sobre la captura.
- Herramientas: lapiz, resaltador, linea, flecha, rectangulo y texto.
- Color y grosor configurables.
- Deshacer y limpiar.
- Copiar imagen editada al portapapeles.
- Guardar PNG local.

## Instalacion para desarrollo

```powershell
cd C:\Automatizaciones\procesoAM\ZAETTA_CAPTURE
python -m pip install -r requirements.txt
python main.py
```

## Seguridad

- No sube capturas a internet.
- No usa servicios externos.
- No crea tareas programadas.
- No modifica registro.
- No inyecta procesos.
- Solo captura cuando el usuario ejecuta la accion.
