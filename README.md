# BotW VFX — efectos estilo *Breath of the Wild* en Unity 6 (URP)

Recreación de cuatro efectos de Zelda: Breath of the Wild a partir de dos referencias:

- **Bomba remota**: vídeo de Daniel Ilett, *Creating Zelda: Breath of the Wild's Remote Bomb in Unity Shader Graph & VFX Graph* (youtube.com/watch?v=bjZr7gzyvt8).
- **Explosión, rayo Guardián y flecha ancestral**: artículo *Making Zelda-like VFX with Unity* (80.lv).

![Explosión](Captures/2_Explosión.png)

## Abrir y probar

1. Unity Hub → **Add project from disk** → selecciona esta carpeta (`Documents/vfx`). Versión: **Unity 6000.6.4f1**.
2. Abre `Assets/BotwVFX/Scenes/BotwVFX_Demo.unity` (o **Tools ▸ BotW VFX ▸ Open Demo Scene**) y pulsa **Play**.

| Tecla | Acción |
|---|---|
| `1` – `4` | Bomba remota · Explosión · Rayo Guardián · Flecha ancestral |
| `Espacio` / `R` | Repetir el efecto actual |
| `←` `→` | Efecto anterior / siguiente |
| `T` | Cámara lenta (x0.25) para estudiar el timing |
| `A` | Modo automático (va pasando por todos) |
| Clic derecho + arrastrar / rueda | Orbitar / zoom |

**Sin dar a Play**: selecciona un efecto en la jerarquía (`Effects/...`) y mueve el slider **Tiempo** del inspector para recorrerlo fotograma a fotograma.

## Estructura

```
Assets/BotwVFX/
  Shaders/          ToonParticle, ToonSmoke, EnergySphere (+ ToonLit y GradientSky para el escenario)
  Scripts/Runtime/  VfxTimeline (base), RemoteBombVfx, GuardianBeamVfx, AncientArrowVfx,
                    VfxDirector (sacudida + cámara lenta), FaceCamera, DemoController
  Scripts/Editor/   Generadores: texturas, mallas, materiales, prefabs, escena; capturas y test
  Prefabs/          VFX_RemoteBomb, VFX_Explosion, VFX_GuardianBeam, VFX_AncientArrow
  Textures/ Meshes/ Materials/   (generados por código)
Captures/           Hojas de contacto renderizadas (6 instantes por efecto)
```

Todo lo de `Textures/`, `Meshes/`, `Materials/`, `Prefabs/` y la escena se genera con **Tools ▸ BotW VFX ▸ Rebuild Everything**.
⚠️ Esa opción **sobrescribe** prefabs y escena: si retocas un efecto a mano, duplícalo antes (o edita los valores en `Scripts/Editor/BotwEffects.cs`).

## Las técnicas (y de dónde salen)

**1. Erosión toon en vez de fade** (`ToonParticle.shader`). La clave del look BotW: las partículas no se desvanecen, se *comen* con un corte duro.
`valor = forma × ruido` → `alpha = valor > erosión`, y una banda justo en el borde del corte se pinta con `_EdgeColor` (núcleo claro, borde saturado).
La erosión la controla el alpha de *Color over Lifetime* (`_AlphaErosion = 1`) o `Custom1.x`, así que se anima con las curvas normales del Particle System.

**2. Esfera de la bomba remota** (`EnergySphere.shader`, vídeo de Ilett). `paso(umbral, (fresnel + intersección con el suelo) × ruido)`.
El fresnel solo en caras delanteras, la intersección con `Scene Depth − Screen Position.w`, y el umbral animado: empieza en negativo (todo blanco = destello) y sube hasta dejar solo el borde.
Añadidos: ruido 3D sin costuras, dos pasadas (caras traseras y luego delanteras) y disolución final.

**3. Humo toon de 4 canales** (`ToonSmoke.shader`, 80.lv). R = sombreado pintado, G = ruido, B = erosión, A = máscara.
`Custom1` = (erosión del fuego, corte de sombra, erosión del alpha, rotación del ruido). El fuego se retira hacia el centro y deja humo con dos tonos.

**4. Explosión en 5 fases** (80.lv): bola de pinchos → onda → escombros/ascuas → humo caliente → disipación lenta. Las tres primeras ocurren en ~0,2 s y la última dura ~3 s (curva de clímax, sensación *ease-out*).
Las lenguas de fuego salen de **medios toroides que miran a cámara** (`FaceCamera`), como en el artículo.

**5. Rayo Guardián** (80.lv): láser que se fija y parpadea cada vez más rápido → carga (chispas que convergen, orbe, anillos) → rayo con **esferas escaladas** + **tiras curvas** con rotación/escala alternas → dos lens flares (uno pequeño y uno alargado) **empujados hacia la cámara** (`_CameraOffset`, el "negated view direction") → explosión anidada + **cámara lenta** y sacudida.

**6. Flecha ancestral** (80.lv): lente gigante con marco Sheikah y cáusticas, **portal con UV polares** (el ruido gira y cae al centro), todo es absorbido (velocidad radial negativa) y colapso final.

**Color**: los colores HDR están en lineal y **sin tonemapping** cada canal se recorta a 1, así que `(2.2, 0.25, 0.02)` se ve naranja saturado y lo que pasa de 1 alimenta al bloom. Paleta en `BotwMaterials.cs`.

## ¿Por qué HLSL + Particle System y no Shader Graph + VFX Graph?

Los shaders escritos a mano y los sistemas creados por código se pueden regenerar y probar automáticamente, y los de partículas (`ToonParticle`, `ToonSmoke`, `EnergySphere`) solo usan `UnityCG.cginc`, así que están pensados para funcionar **también en el Built-in Render Pipeline**.
La lógica es la misma que en el vídeo; si quieres rehacerlo en Shader Graph, cada línea del shader corresponde a un nodo (`step`/`smoothstep` → *Step/Smoothstep*, `fresnel` → *Fresnel Effect*, `_CameraDepthTexture` → *Scene Depth*, `VFACE` → *Is Front Face*...).

## Usarlos en otro proyecto

Copia `Shaders/`, `Scripts/Runtime/`, `Textures/`, `Meshes/`, `Materials/` y `Prefabs/`. Coloca un prefab y llama a `GetComponent<VfxTimeline>().Play()`.
Para que la sacudida funcione sin `DemoController`, añade un `VfxDirector` con *Apply Shake To Main Camera*.

- **URP**: activa *Depth Texture* y *HDR* en el asset de URP y pon un Volume con Bloom (umbral ~1).
- **Built-in** (p. ej. un proyecto con shaders de superficie): los shaders de partículas deberían funcionar tal cual (no probado); la esfera necesita `camera.depthTextureMode |= DepthTextureMode.Depth` y el bloom requiere el paquete de post-procesado. `ToonLit` es solo para URP (es el del escenario).

## Herramientas extra

- **Tools ▸ BotW VFX ▸ Capture Contact Sheets**: renderiza 6 instantes de cada efecto en `Captures/`.
- Línea de comandos (sin abrir el editor):
  - `-executeMethod BotwVfx.EditorTools.BotwVfxBuilder.BuildAllBatch -quit`: regenerar todo.
  - `-executeMethod BotwVfx.EditorTools.BotwCapture.CaptureBatch -captureDir RUTA`: capturas.
  - `-executeMethod BotwVfx.EditorTools.BotwPlayTest.RunBatch -captureDir RUTA`: entra en Play, lanza los 4 efectos y falla si hay errores.
- Depuración de *vertex streams*: `Shader.SetGlobalFloat("_BotwDebugMode", 1..5)` muestra color de vértice, `Custom1`, UV, normales o color sin alpha.

## Limitaciones

- El Guardián de la escena es atrezo hecho con primitivas, solo para dar contexto al rayo.
- Sin sonido. El remolino de la disipación se hace rotando el ruido, sin flow map.
- Probado en Windows con el nivel de calidad *PC* de URP. La compatibilidad con Built-in está pensada en el código pero no se ha probado.
