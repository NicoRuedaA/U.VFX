using UnityEngine;
using static BotwVfx.EditorTools.EmeraldLayers;
using static BotwVfx.EditorTools.EmeraldMoveBuilder;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Lote 6 (movimientos 101-120). Mismo criterio que los lotes 1-5: referencias del anime y del juego resumidas en el
    /// comentario de cada receta, colores de las referencias y volumen en el pico escalado a lo que cubren en pantalla.
    /// Casi todo el lote son movimientos de estado (sin estrella de impacto, cámara lenta ni sacudida fuerte): 102-116, 118 y 119.
    /// Solo hacen daño 101 Tinieblas, 117 Venganza y 120 Autodestrucción.
    /// Presupuesto: como mucho 10 sistemas Shuriken por movimiento (≤ 600 KB por prefab); paneles, líneas y mallas son baratos.
    /// </summary>
    public static partial class EmeraldRecipes
    {
        // Tinieblas (anime): sombra negra-violeta con borde magenta. Los canales bajos tienen que ser muy bajos: en la captura
        // un verde lineal de 0,06 ya se ve como 143/255 y el magenta se lava a lavanda.
        static readonly Pal NightShade = new Pal("NightShade", new Color(0.05f, 0.0f, 0.07f), new Color(1.0f, 0.004f, 0.75f));
        // Orbes violeta del juego (Tinieblas).
        static readonly Pal ShadeOrb = new Pal("ShadeOrb", new Color(0.6f, 0.25f, 1.3f), new Color(0.35f, 0.05f, 1.0f));
        // Fortaleza (anime): brillo verde-amarillo del cuerpo.
        static readonly Pal HardenGreen = new Pal("HardenGreen", new Color(1.3f, 1.3f, 0.4f), new Color(0.5f, 1.1f, 0.1f));
        // Reducción (anime): el usuario brilla rosa intenso (canales ≤ 1,3 para que no se lave a blanco).
        static readonly Pal MinPink = new Pal("MinPink", new Color(1.3f, 0.3f, 0.9f), new Color(0.8f, 0.1f, 0.5f));
        // Paneles: núcleo = relleno translúcido, borde = marco brillante (material Pane/Frame).
        static readonly Pal BarrierPane = new Pal("BarrierPane", new Color(0.12f, 0.45f, 1.3f), new Color(1.8f, 2.2f, 2.6f));
        static readonly Pal LightPane = new Pal("LightPane", new Color(1.3f, 0.85f, 0.1f), new Color(2.2f, 1.6f, 0.3f));
        static readonly Pal MirrorPane = new Pal("MirrorPane", new Color(0.02f, 0.06f, 0.5f), new Color(2.0f, 2.3f, 2.6f));
        // Reflejo (juego): escudo verde azulado translúcido (borde del casco brillante).
        static readonly Pal ReflectTeal = new Pal("ReflectTeal", new Color(0.6f, 2.0f, 1.9f), new Color(0.15f, 0.7f, 0.8f));
        // Barrera (anime): cúpula violeta (canales ≤ 1,3).
        static readonly Pal BarrierViolet = new Pal("BarrierViolet", new Color(0.7f, 0.35f, 1.3f), new Color(0.35f, 0.12f, 0.8f));
        // Mimético (juego): motas rosa-magenta intensas.
        static readonly Pal MimicPink = new Pal("MimicPink", new Color(1.3f, 0.25f, 0.8f), new Color(1.0f, 0.05f, 0.5f));
        // Rojo profundo (Rayo Confuso anime, Venganza): RageRed se lava a rosa en los brillos grandes.
        static readonly Pal DeepRed = new Pal("DeepRed", new Color(0.45f, 0.002f, 0.004f), new Color(0.25f, 0.0005f, 0.002f));
        // Haces de luz de Recuperación (juego): amarillo suave, no blanco.
        static readonly Pal ShaftYellow = new Pal("ShaftYellow", new Color(1.3f, 1.2f, 0.5f), new Color(1.0f, 0.75f, 0.1f));
        // Dedo de Metrónomo (juego): rojo plano (FootRed se ve salmón; con (0,8, 0,07, 0,015) la captura da sRGB 254,171,110).
        static readonly Pal FingerRed = new Pal("FingerRed", new Color(0.55f, 0.004f, 0.001f), new Color(0.3f, 0.001f, 0f));
        // Rizo Defensa (juego): bola cian claro.
        static readonly Pal CurlCyan = new Pal("CurlCyan", new Color(0.7f, 1.4f, 1.5f), new Color(0.15f, 0.7f, 0.9f));

        // Punto medio del campo (nubes que cubren el espacio entre los dos).
        static readonly Vector3 MidField = new Vector3(0f, 0.9f, 0f);
        // Panel delante del usuario (pantallas y barreras).
        static readonly Vector3 FrontPanel = new Vector3(-1.75f, 1.25f, -0.15f);

        // ---------------------------------------------------------------- 101 Tinieblas
        // Refs: anime = sombra gigante negra-violeta con borde magenta que se alza junto al usuario; juego = trazos negros quebrados
        // que cruzan la pantalla y orbes violeta alrededor del rival.
        static void M101(EmeraldMoveBuilder b)
        {
            b.Keys(0.55f, 1.3f, 1.6f, 2.6f, Hit, 0.8f);
            // El clip pinta la silueta y el rayo con borde rojo: las referencias tienen borde magenta.
            b.HideBaked();
            // Sombra que se alza detrás del usuario: lenguas oscuras con borde magenta (silueta dentada) y humo violeta.
            Aura(b, Attacker + new Vector3(0.5f, 0f, 0.9f), NightShade, 0.25f, 1.75f, 2.6f, 70f, false);
            Tint(Fog(b, Attacker + new Vector3(0.4f, 0f, 0.7f), BotwMaterials.Get("EX_DarkSmoke"), 0.3f, 1.3f, 1.1f, 14f, 1.3f, 0.9f, 1.3f), new Color(0.45f, 0.2f, 0.55f));
            // Rayo quebrado negro con borde magenta.
            var from = AttackerChest + new Vector3(0.4f, 0.35f, 0f);
            Bolt(b, from, Hit, NightShade, 0.85f, 1.7f, 0.8f, 12, 0.6f, 3, 61, 0.05f);
            Bolt(b, from + new Vector3(0f, 0.3f, 0f), Hit + new Vector3(0f, 0.6f, 0.3f), NightShade, 0.9f, 1.6f, 0.32f, 10, 0.7f, 2, 62);
            // Fragmentos de sombra al impactar.
            ImpactStar(b, Hit, NightShade, 1.3f, 4.8f, 12, 0);
            RadialBolts(b, Hit, NightShade, 1.3f, 1.9f, 5, 1.7f, 63, 0.3f);
            Sparks(b, Hit, NightShade, 1.3f, 26, 5f, 11f, 0.8f, 1.6f);
            // Orbes violeta alrededor del rival (juego).
            b.Cue(b.Ps("ShadeOrbs", Mat(ShadeOrb, "Glow"), TargetChest + new Vector3(0f, 0.4f, 0f))
                .Sphere(1.5f).Velocity(new Vector3(0f, 0.3f, 0f)).Life(1.2f, 1.6f).Size(0.5f, 1.0f).Burst(7)
                .SizeOverLife(C(0f, 0.3f, 0.2f, 1f, 1f, 0.8f)).AlphaOverLife(0f, 0f, 0.15f, 1f, 0.7f, 1f, 1f, 0f).Order(4), 1.25f);
            HitShell(b, Target, Magenta, 1.3f, 0.6f);
            b.Light(Hit + new Vector3(-0.8f, 0.5f, 0f), Lc(1f, 0.35f, 0.9f), 9f, 0f, 0f, 0.8f, 1f, 1.32f, 3.5f, 1.8f, 1.2f, 2.6f, 0f);
            b.Shake(1.3f, 0.35f, 0.35f);
        }

        // ---------------------------------------------------------------- 102 Mimético
        // Refs: anime = el usuario copia el ataque y brilla blanco (alas blancas luminosas); juego = motas rosa-magenta alrededor
        // del rival. Movimiento de estado: sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M102(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.5f, 1.9f, 2.9f, AttackerChest, 0.1f);
            // El clip pinta una estrella magenta en el usuario y un destello blanco grande en el rival.
            b.HideBaked();
            Orbit(b, TargetChest + new Vector3(0f, 0.1f, 0f), MimicPink, 0.3f, 1.3f, 12, 1.0f, 3f, 0.6f);
            Glints(b, TargetChest, Pink, 0.4f, 1.0f, 1.0f, 14f);
            // El símbolo copiado viaja del rival al usuario.
            var user = AttackerChest + new Vector3(0.2f, 0.2f, 0f);
            DrainMotes(b, TargetChest, user, MimicPink, 0.95f, 0.6f, 40f, 0.55f, 1.6f, "Glow", 0.6f);
            Projectile(b, TargetChest, user, White, 1.0f, 0.5f, 1.0f, 0.5f, "Star");
            // El usuario se ilumina con la copia.
            Aura(b, Attacker, White, 1.4f, 2.8f, 1.25f, 30f);
            RingPulses(b, AttackerChest, Pink, 1.5f, 3, 0.12f, 3.0f);
            Sparkles(b, AttackerChest, 1.5f, 1.0f, 1.2f, 30f, 0.9f, new Color(1f, 0.55f, 0.85f), Color.white);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(1f, 0.75f, 0.95f), 8f, 0f, 0f, 0.4f, 0.6f, 1.5f, 2f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 103 Chirrido
        // Refs: anime = resplandor blanco-azul en la boca, aros azules concéntricos y garabatos eléctricos blanco-azules; juego = túnel
        // de aros cian finos desde la boca hacia el rival. Movimiento de estado: sin estrella, sin cámara lenta, sin sacudida.
        static void M103(EmeraldMoveBuilder b)
        {
            b.Keys(0.45f, 1.15f, 1.5f, 2.6f, TargetHead, 0.15f);
            // El clip termina en un destello grande sobre el rival (no hace daño).
            b.HideBaked();
            var mouth = Mouth + new Vector3(0.25f, -0.05f, 0f);
            var head = TargetHead + new Vector3(-0.4f, 0f, 0f);
            b.Cue(b.Ps("MouthGlow", Mat(Cyan, "Glow"), mouth)
                .Life(1.5f).Size(1.2f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.15f, 1f, 0.5f, 0.85f, 0.8f, 1.05f, 1f, 0.4f))
                .AlphaOverLife(0f, 0f, 0.08f, 1f, 0.85f, 1f, 1f, 0f).Order(6), 0.4f);
            RingTunnel(b, mouth, head, Cyan, 0.5f, 1.2f, 16f, 0.5f, 0.6f, 1.3f);
            RingTunnel(b, mouth, head, White, 0.55f, 1.1f, 7f, 0.45f, 0.4f, 1.0f);
            RingPulses(b, mouth, Cyan, 0.45f, 6, 0.18f, 2.2f);
            Crackle(b, mouth, Cyan, 0.5f, 1.2f, 0.8f, 50f);
            Crackle(b, TargetHead, Cyan, 1.1f, 1.0f, 0.8f, 40f);
            Glints(b, TargetHead, White, 1.1f, 1.0f, 0.9f, 14f);
            HitShell(b, Target, Cyan, 1.15f, 0.9f);
            b.Light(mouth + new Vector3(0.4f, 0.4f, -0.4f), Lc(0.7f, 0.9f, 1f), 8f, 0f, 0f, 0.4f, 1.5f, 1.2f, 1.8f, 2.6f, 0f);
        }

        // ---------------------------------------------------------------- 104 Doble Equipo
        // Refs: anime = fila de copias del usuario y una silueta blanca brillante; juego = imágenes residuales con rayas horizontales
        // blanco-grises. Movimiento de estado (sube Evasión): sin estrella, sin cámara lenta, sin sacudida.
        static void M104(EmeraldMoveBuilder b)
        {
            b.Keys(0.4f, 1.0f, 1.6f, 2.8f, AttackerChest, 0f);
            // El clip pinta una estrella blanca enorme: las copias propias lo sustituyen.
            b.HideBaked();
            Afterimages(b, Attacker, ScreenRight * 0.85f, White, 0.45f, 3, 0.1f, 1.8f);
            Afterimages(b, Attacker, -ScreenRight * 0.8f, White, 0.5f, 2, 0.12f, 1.7f);
            Afterimages(b, Attacker, new Vector3(0.9f, 0f, 0.9f), Wind, 0.6f, 2, 0.15f, 1.5f);
            Streaks(b, AttackerChest - ScreenRight * 2.8f, AttackerChest + ScreenRight * 2.8f, White, 0.4f, 1.4f, 70f, 1.1f, 1.2f, 0.3f);
            Dust(b, Attacker, 0.45f, 0.9f, 6, 0.7f);
            Glints(b, AttackerChest, White, 0.6f, 1.6f, 2.2f, 16f);
            RisingArrows(b, Attacker, Wind, 1.1f, 1.2f, 12f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(0.85f, 0.9f, 1f), 7f, 0f, 0f, 0.4f, 1.2f, 1.6f, 1.2f, 2.8f, 0f);
        }

        // ---------------------------------------------------------------- 105 Recuperación
        // Refs: anime = fondo verde-amarillo con destellos blancos de cuatro puntas; juego = haces de luz amarillo-blancos desde arriba
        // y orbes amarillo-verdes alrededor del usuario. Movimiento de estado (cura): sin estrella, sin cámara lenta, sin sacudida.
        static void M105(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.3f, 1.8f, 2.9f, AttackerChest, 0f);
            b.Baked(1.3f); // la columna de luz amarilla y las cruces verdes del clip encajan con el juego
            Beam(b, Attacker + new Vector3(-1.2f, 7f, 1.0f), Attacker + new Vector3(0.1f, 0f, 0f), ShaftYellow, 0.35f, 2.4f, 0.7f, 0.3f);
            Beam(b, Attacker + new Vector3(0.6f, 7f, 1.8f), Attacker + new Vector3(0.6f, 0f, -0.2f), ShaftYellow, 0.45f, 2.2f, 0.4f, 0.3f);
            Gather(b, AttackerChest, Pollen, 0.5f, 1.3f, 2.2f, 30f, 2.4f, "Glow");
            Sparkles(b, AttackerChest, 0.6f, 1.8f, 1.6f, 26f, 1.0f, Color.white, new Color(0.8f, 1f, 0.45f));
            Aura(b, Attacker, Mint, 1.1f, 2.7f, 1.2f, 0f);
            GroundGlow(b, Attacker, Pollen, 0.5f, 2.8f, 1.6f);
            b.Light(AttackerChest + new Vector3(0.6f, 1.2f, -0.5f), Lc(1f, 1f, 0.7f), 9f, 0f, 0f, 0.4f, 1.5f, 1.3f, 2.2f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 106 Fortaleza
        // Refs: anime = el cuerpo brilla verde-amarillo con destellos y esquirlas azul pálido en abanico; juego = casco blanco
        // translúcido sobre el usuario, motas negras y rayos blancos. Movimiento de estado: sin estrella, sin cámara lenta, sin sacudida.
        static void M106(EmeraldMoveBuilder b)
        {
            b.Keys(0.4f, 1.0f, 1.6f, 2.8f, AttackerChest, 0f);
            // El clip pinta una estrella de impacto blanca grande (no es un golpe).
            b.HideBaked();
            b.Cue(b.Ps("HardenGlow", Mat(HardenGreen, "Glow"), AttackerChest + new Vector3(0f, 0.1f, 0f))
                .Life(1.1f).Size(2.1f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.4f, 1f, 1f, 0.9f))
                .AlphaOverLife(0f, 0f, 0.2f, 1f, 0.6f, 1f, 1f, 0f).Order(3), 0.4f);
            // Casco claro que se endurece (se encoge de golpe y se queda).
            var shellScale = new Vector3(0.78f, 1.12f, 0.78f);
            var shell = b.MeshPart("HardShell", BotwMeshes.Sphere, Mat(Steel, "Shell"), Attacker + new Vector3(0f, 0.85f, 0f), Vector3.zero, shellScale, order: 2);
            var st = b.Track(shell.transform, shell, 0.85f, 2.8f);
            st.scaleFrom = shellScale * 1.35f;
            st.scaleTo = shellScale;
            st.scaleCurve = C(0.85f, 0f, 1.0f, 1.03f, 1.1f, 1f, 2.8f, 1f);
            st.property = "_Dissolve";
            st.propertyCurve = C(0.85f, 0.7f, 1.0f, 0f, 2.4f, 0f, 2.8f, 1f);
            Rays(b, AttackerChest, Ice, 0.95f, 14, 4.5f, 0.45f);
            Glints(b, AttackerChest, White, 1.0f, 1.4f, 1.0f, 18f);
            Debris(b, AttackerChest, 1.0f, 12, 4f, 0.6f, Mat(CrackDark, "Solid"));
            CutLine(b, AttackerChest + new Vector3(0f, 0.2f, 0f), White, 1.35f, 35f, 2.6f, 0.25f, 0.3f);
            RingPulses(b, AttackerChest, Steel, 1.0f, 2, 0.15f, 2.8f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(0.9f, 1f, 0.8f), 8f, 0f, 0f, 0.4f, 1.2f, 1.0f, 2f, 2.8f, 0f);
        }

        // ---------------------------------------------------------------- 107 Reducción
        // Refs: anime = el usuario brilla rosa intenso con rayas amarillo-naranjas de fondo; juego = rayas azules verticales que suben
        // alrededor del usuario. Movimiento de estado (sube Evasión): sin estrella, sin cámara lenta, sin sacudida.
        static void M107(EmeraldMoveBuilder b)
        {
            b.Keys(0.4f, 1.2f, 1.7f, 2.9f, AttackerChest, 0f);
            // El clip pinta un destello blanco grande (no es un golpe).
            b.HideBaked();
            // Silueta rosa que se encoge hasta el suelo y luego recupera su tamaño.
            var body = new Vector3(0.66f, 1.05f, 0.66f);
            var shell = b.MeshPart("ShrinkShell", BotwMeshes.Sphere, Mat(Pink, "Shell"), Attacker + new Vector3(0f, 0.85f, 0f), Vector3.zero, body, order: 2);
            var st = b.Track(shell.transform, shell, 0.35f, 2.9f);
            var k = C(0.35f, 0f, 0.6f, 0f, 1.1f, 1f, 2.1f, 1f, 2.55f, 0f);
            st.scaleFrom = body * 1.08f;
            st.scaleTo = body * 0.3f;
            st.scaleCurve = k;
            st.posFrom = Attacker + new Vector3(0f, 0.85f, 0f);
            st.posTo = Attacker + new Vector3(0f, 0.28f, 0f);
            st.posCurve = k;
            st.property = "_Dissolve";
            st.propertyCurve = C(0.35f, 0.8f, 0.5f, 0f, 2.6f, 0f, 2.9f, 1f);
            // Brillo rosa del cuerpo (anime) que se encoge con la silueta y vuelve a crecer.
            b.Cue(b.Ps("PinkGlow", Mat(MinPink, "Glow"), Attacker + new Vector3(0f, 0.6f, 0f))
                .Life(2.4f).Size(2.8f).Burst(1).SizeOverLife(C(0f, 0.6f, 0.08f, 1f, 0.33f, 0.22f, 0.75f, 0.22f, 0.9f, 0.9f, 1f, 0.2f))
                .AlphaOverLife(0f, 0f, 0.05f, 1f, 0.92f, 1f, 1f, 0f).Order(3), 0.25f);
            Flash(b, Attacker + new Vector3(0f, 0.3f, 0f), Pink, 1.15f, 1.2f);
            RisingArrows(b, Attacker, DragonBlue, 1.0f, 1.4f, 30f);
            Streaks(b, AttackerChest + new Vector3(-0.5f, 2.6f, 0.8f), AttackerChest + new Vector3(0.6f, -1.4f, -0.6f), Gold, 0.4f, 1.2f, 40f, 1.4f, 1.1f, 0.3f);
            Glints(b, Attacker + new Vector3(0f, 0.35f, 0f), Pink, 1.2f, 1.3f, 0.6f, 14f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(1f, 0.6f, 0.9f), 8f, 0f, 0f, 0.35f, 1.6f, 1.2f, 1.2f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 108 Pantalla de Humo
        // Refs: anime = chorro de humo verde azulado oscuro que sale de la boca; juego = bocanadas de humo gris que ruedan del
        // usuario al rival. Movimiento de estado (baja Precisión): sin estrella, sin cámara lenta, sin sacudida.
        static void M108(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.2f, 1.9f, 3.0f, MidField, 0.1f);
            // El clip pinta manchas negras opacas y un destello en el rival.
            b.HideBaked();
            var smoke = BotwMaterials.Get("EX_DarkSmoke");
            var tint = new Color(0.85f, 1f, 1f);
            var mouth = Mouth + new Vector3(0.25f, -0.1f, 0f);
            var dir = TargetChest + new Vector3(-0.8f, 0.2f, 0f) - mouth;
            b.Cue(Tint(b.Ps("SmokeJet", smoke, mouth)
                .Sphere(0.3f).Velocity(dir / 0.9f).Speed(0.3f, 1.2f).Drag(0.6f).Life(0.9f, 1.3f).Size(0.7f, 1.1f).Rate(40f).Duration(0.9f)
                .SizeOverLife(C(0f, 0.4f, 0.4f, 1.4f, 1f, 2.0f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.6f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(2), tint), 0.45f);
            // Banco de humo que cubre el espacio entre los dos.
            b.Cue(Tint(b.Ps("SmokeBank", smoke, MidField + new Vector3(0.6f, -0.2f, 0f))
                .Box(new Vector3(4.5f, 0.6f, 1.6f)).Speed(0.2f, 0.6f).Velocity(new Vector3(0f, 0.25f, 0f)).Life(1.4f, 2.0f).Size(1.4f, 2.4f)
                .Rate(30f).Duration(1.1f).SizeOverLife(C(0f, 0.4f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.55f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1), tint), 0.9f);
            Tint(Fog(b, Target, smoke, 1.0f, 1.6f, 1.2f, 12f, 1.3f, 0.6f, 1.8f), tint);
            Smoke(b, mouth, smoke, 0.42f, 6, 0.6f, 0.3f, 0.3f, 0.9f);
            b.Light(MidField + new Vector3(-1f, 1f, -0.5f), Lc(0.75f, 0.85f, 0.9f), 7f, 0f, 0f, 0.5f, 0.6f, 3.0f, 0f);
        }

        // ---------------------------------------------------------------- 109 Rayo Confuso
        // Refs: anime = el usuario es una silueta roja brillante moteada de oscuro; juego = motas rosa, violeta y azul que giran
        // alrededor del rival bajo un arco verde-blanco. Movimiento de estado (confunde): sin estrella, sin cámara lenta, sin sacudida.
        static void M109(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.45f, 1.9f, 3.0f, TargetHead, 0.1f);
            // El clip mezcla un aro amarillo y orbes sueltos: la luz viajera y las órbitas propias lo sustituyen.
            b.HideBaked();
            Aura(b, Attacker, DeepRed, 0.25f, 1.4f, 1.35f, 35f);
            b.Cue(b.Ps("RedBody", Mat(DeepRed, "Glow"), AttackerChest + new Vector3(0f, 0.2f, 0f))
                .Life(1.1f).Size(2.7f).Burst(1).SizeOverLife(C(0f, 0.4f, 0.25f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 0f, 0.15f, 0.9f, 0.7f, 0.9f, 1f, 0f).Order(3), 0.3f);
            // Luz cian que viaja al rival.
            Projectile(b, AttackerChest + new Vector3(0.5f, 0.5f, 0f), TargetHead, Cyan, 0.9f, 0.55f, 1.0f, 0.5f, "Glow", new Vector3(0f, 0.8f, 0f));
            var colors = new[] { new Color(1f, 0.35f, 0.85f), new Color(0.6f, 0.3f, 1f), new Color(0.3f, 0.5f, 1f) };
            Flash(b, TargetHead, Cyan, 1.45f, 1.6f);
            Orbit(b, TargetHead + new Vector3(0f, -0.1f, 0f), White, 1.4f, 1.6f, 10, 0.8f, 4f, 0.7f, "Glow", 12f, colors);
            SpinArc(b, TargetHead + new Vector3(0f, 0.25f, 0f), Mint, 1.4f, 2.9f, 0.9f, 0.6f, 500f, 15f);
            Glints(b, TargetHead, White, 1.45f, 1.3f, 0.8f, 12f);
            HitShell(b, Target, Magenta, 1.45f, 0.8f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(1f, 0.4f, 0.4f), 8f, 0f, 0f, 0.3f, 1.5f, 1.3f, 1f, 3.0f, 0f);
        }

        // ---------------------------------------------------------------- 110 Refugio
        // Refs: anime = el usuario se mete en su caparazón naranja con una estela de estrellas amarillas; juego = concha enorme que
        // envuelve al usuario. Movimiento de estado (sube Defensa): sin estrella, sin cámara lenta, sin sacudida.
        static void M110(EmeraldMoveBuilder b)
        {
            b.Keys(0.4f, 1.1f, 1.7f, 2.9f, AttackerChest, 0f);
            // El clip pinta una esfera azul y estrellas naranjas sueltas: la cúpula y el aro propios lo sustituyen.
            b.HideBaked();
            // Cúpula de agua que se cierra sobre el usuario (se retrae).
            var domeFrom = new Vector3(1.25f, 1.55f, 1.25f);
            var domeTo = new Vector3(0.95f, 0.8f, 0.95f);
            var dome = b.MeshPart("ShellDome", BotwMeshes.Sphere, Mat(BubbleBlue, "Shell"), Attacker + new Vector3(0f, 0.5f, 0f), Vector3.zero, domeTo, order: 2);
            var dt = b.Track(dome.transform, dome, 0.45f, 2.9f);
            dt.scaleFrom = domeFrom;
            dt.scaleTo = domeTo;
            dt.scaleCurve = C(0.45f, 0f, 1.05f, 1.04f, 1.15f, 1f, 2.9f, 1f);
            dt.property = "_Dissolve";
            dt.propertyCurve = C(0.45f, 0.8f, 0.6f, 0f, 2.5f, 0f, 2.9f, 1f);
            GroundGlow(b, Attacker, Water, 1.0f, 2.9f, 1.8f);
            SpinArc(b, Attacker + new Vector3(0f, 0.6f, 0f), Water, 1.05f, 2.6f, 1.4f, 0.8f, -500f, 8f);
            Splash(b, Attacker + new Vector3(0f, 0.3f, 0f), Water, 1.05f, 0.7f, 16);
            // Estela de estrellas amarillas (anime).
            Orbit(b, AttackerChest + new Vector3(0f, 0.2f, 0f), Gold, 0.4f, 1.6f, 6, 1.2f, 3.5f, 0.85f, "Star", 18f);
            Glints(b, AttackerChest, Gold, 1.1f, 1.3f, 1.2f, 14f, 1.5f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(0.6f, 0.85f, 1f), 8f, 0f, 0f, 0.4f, 1.3f, 1.1f, 1.8f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 111 Rizo Defensa
        // Refs: anime = el usuario se enrosca en una bola con rayas azules de velocidad; juego = bola cian claro enorme y brillante que
        // envuelve al usuario. Movimiento de estado (sube Defensa): sin estrella, sin cámara lenta, sin sacudida.
        static void M111(EmeraldMoveBuilder b)
        {
            b.Keys(0.4f, 1.1f, 1.7f, 2.9f, AttackerChest, 0f);
            // El clip pinta un portal azul y flechas: la bola propia lo sustituye.
            b.HideBaked();
            SpeedLines(b, AttackerChest, Cyan, 0.35f, 0.7f, 3.0f, 120f, 1.3f);
            // Bola que se comprime alrededor del usuario.
            var ball = Vector3.one * 1.2f;
            var r = b.MeshPart("CurlBall", BotwMeshes.Sphere, Mat(CurlCyan, "Shell"), Attacker + new Vector3(0f, 0.85f, 0f), Vector3.zero, ball, order: 2);
            var tr = b.Track(r.transform, r, 0.4f, 2.9f);
            tr.scaleFrom = ball * 1.8f;
            tr.scaleTo = ball;
            tr.scaleCurve = C(0.4f, 0f, 1.05f, 1.04f, 1.15f, 1f, 2.9f, 1f);
            tr.property = "_Dissolve";
            tr.propertyCurve = C(0.4f, 0.8f, 0.55f, 0f, 2.5f, 0f, 2.9f, 1f);
            b.Cue(b.Ps("BallGlow", Mat(Cyan, "Glow"), AttackerChest)
                .Life(1.4f).Size(2.8f).Burst(1).SizeOverLife(C(0f, 0.6f, 0.12f, 1f, 1f, 0.9f))
                .AlphaOverLife(0f, 0f, 0.1f, 0.9f, 0.4f, 0.6f, 0.85f, 0.55f, 1f, 0f).Order(3), 1.0f);
            RingPulses(b, AttackerChest, Cyan, 1.05f, 3, 0.12f, 3.2f);
            Streaks(b, Attacker + new Vector3(0f, -0.3f, 0f), Attacker + new Vector3(0f, 3.2f, 0f), White, 0.5f, 1.4f, 30f, 1.0f, 1.2f, 0.4f);
            GroundGlow(b, Attacker, Cyan, 1.0f, 2.9f, 1.5f);
            Glints(b, AttackerChest, White, 1.1f, 1.4f, 1.1f, 14f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(0.6f, 0.9f, 1f), 8f, 0f, 0f, 0.4f, 1f, 1.1f, 2.2f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 112 Barrera
        // Refs: anime = cúpula translúcida violeta-lila enorme con bordes blanco-grises; juego = panel azul claro brillante con facetas
        // blancas delante del usuario. Movimiento de estado (sube Defensa): sin estrella, sin cámara lenta, sin sacudida.
        static void M112(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.15f, 1.8f, 3.0f, FrontPanel, 0f);
            // El clip mezcla un portal violeta, cristales azules y un brillo rosa: el panel propio lo sustituye.
            b.HideBaked();
            Panel(b, FrontPanel, new Vector2(2.4f, 2.7f), 65f, BarrierPane, 0.55f, 3.0f, 0.3f);
            // Doble borde que se abre un poco después.
            Panel(b, FrontPanel + new Vector3(0.3f, 0f, 0.1f), new Vector2(2.8f, 3.1f), 65f, BarrierPane, 0.7f, 3.0f, 0.3f, false, false);
            // Cúpula violeta tenue alrededor del usuario (anime).
            var domeScale = Vector3.one * 2.0f;
            var dome = b.MeshPart("BarrierDome", BotwMeshes.Sphere, Mat(BarrierViolet, "Shell"), AttackerChest + new Vector3(0.3f, 0f, 0f), Vector3.zero, domeScale, order: 1);
            var dt = b.Track(dome.transform, dome, 0.45f, 1.7f);
            dt.scaleFrom = domeScale * 0.5f;
            dt.scaleTo = domeScale;
            dt.scaleCurve = C(0.45f, 0f, 0.7f, 1f, 1.7f, 1.05f);
            dt.property = "_Dissolve";
            dt.propertyCurve = C(0.45f, 0.8f, 0.6f, 0f, 1.3f, 0.2f, 1.7f, 1f);
            Flash(b, FrontPanel, White, 1.15f, 2.2f);
            RingPulses(b, FrontPanel, Lilac, 1.1f, 3, 0.18f, 3.2f);
            Glints(b, FrontPanel, White, 1.0f, 1.6f, 1.2f, 16f);
            b.Light(FrontPanel + new Vector3(-0.6f, 0.5f, -0.6f), Lc(0.7f, 0.8f, 1f), 8f, 0f, 0f, 0.5f, 1.5f, 1.15f, 2.2f, 3.0f, 0f);
        }

        // ---------------------------------------------------------------- 113 Pantalla de Luz
        // Refs: anime = masa de luz amarillo-dorada y cúpula blanca con destellos; juego = paneles blancos luminosos delante del
        // usuario. Movimiento de estado: sin estrella, sin cámara lenta, sin sacudida.
        static void M113(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.15f, 1.8f, 3.0f, FrontPanel, 0f);
            // El clip pinta un panel dorado con retícula pero también un brillo rosa en el usuario: el panel propio lo sustituye.
            b.HideBaked();
            Panel(b, FrontPanel, new Vector2(2.6f, 2.9f), 65f, LightPane, 0.5f, 3.0f, 0.3f);
            b.Cue(b.Ps("ScreenBloom", Mat(Sun, "Glow"), FrontPanel)
                .Life(0.7f).Size(3.6f).Burst(1).SizeOverLife(C(0f, 0.4f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 0f, 0.15f, 0.9f, 1f, 0f).Order(1), 0.8f);
            CutLine(b, FrontPanel, White, 1.2f, 20f, 3.4f, 0.4f, 0.35f);
            Sparkles(b, FrontPanel, 0.7f, 2.0f, 1.4f, 30f, 1.0f, new Color(1f, 0.85f, 0.4f), Color.white);
            Glints(b, FrontPanel, Gold, 1.1f, 1.6f, 1.3f, 14f);
            b.Light(FrontPanel + new Vector3(-0.6f, 0.5f, -0.6f), Lc(1f, 0.9f, 0.55f), 9f, 0f, 0f, 0.5f, 1.5f, 1.15f, 2.5f, 3.0f, 0f);
        }

        // ---------------------------------------------------------------- 114 Niebla
        // Refs: anime = bocanadas de humo gris oscuro sobre fondo de rayas violeta; juego = bruma negra-gris que envuelve al usuario.
        // Movimiento de estado (anula cambios): sin estrella, sin cámara lenta, sin sacudida.
        static void M114(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.2f, 2.0f, 3.2f, MidField, 0f);
            // El clip pinta manchas negras sólidas que tapan al usuario.
            b.HideBaked();
            var dark = BotwMaterials.Get("EX_DarkSmoke");
            var tint = new Color(1.15f, 1.12f, 1.25f);
            b.Cue(Tint(b.Ps("HazeBurst", dark, AttackerChest + new Vector3(0.3f, 0.2f, 0f))
                .Sphere(0.8f).Speed(0.8f, 2.0f).Drag(2f).Life(1.4f, 2.0f).Size(1.3f, 2.2f).Burst(8).Velocity(new Vector3(0f, 0.2f, 0f))
                .SizeOverLife(C(0f, 0.4f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.5f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(2), tint), 0.45f);
            // La nube avanza por el campo hasta el rival.
            b.Cue(Tint(b.Ps("HazeBank", dark, Attacker + new Vector3(0.8f, 0.6f, 0f))
                .Box(new Vector3(1.0f, 0.4f, 1.4f)).Velocity(new Vector3(3.0f, 0.15f, 0f)).Speed(0.1f, 0.4f).Life(1.6f, 2.2f).Size(1.8f, 2.8f)
                .Rate(22f).Duration(1.2f).SizeOverLife(C(0f, 0.4f, 0.3f, 1f, 1f, 1.25f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.55f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1), tint), 0.7f);
            Tint(Fog(b, Attacker, dark, 0.6f, 2.0f, 1.4f, 10f, 1.4f, 0.7f, 2.0f), tint);
            Tint(Fog(b, Target, dark, 1.3f, 1.6f, 1.4f, 10f, 1.4f, 0.7f, 2.0f), tint);
            // Bruma fría a ras de suelo (Hielo).
            b.Cue(b.Ps("ColdMist", BotwMaterials.Get("EM_Mist"), MidField + new Vector3(0f, -0.7f, 0f))
                .Box(new Vector3(6f, 0.1f, 2.5f)).Speed(0.1f, 0.3f).Life(1.2f, 1.8f).Size(1.0f, 1.8f).Rate(14f).Duration(1.4f)
                .SizeOverLife(C(0f, 0.4f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.55f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(0), 0.9f);
            b.Light(MidField + new Vector3(-1f, 1f, -0.5f), Lc(0.75f, 0.75f, 0.9f), 7f, 0f, 0f, 0.5f, 0.6f, 3.2f, 0f);
        }

        // ---------------------------------------------------------------- 115 Reflejo
        // Refs: anime = resplandor arcoíris pastel con destellos blancos; juego = escudo elíptico verde azulado translúcido con
        // destellos blancos y borde irisado delante del usuario. Movimiento de estado: sin estrella, sin cámara lenta, sin sacudida.
        static void M115(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.15f, 1.8f, 3.0f, FrontPanel, 0f);
            // El clip pinta un panel hexagonal cian y un brillo rosa en el usuario: el escudo propio lo sustituye.
            b.HideBaked();
            var shieldScale = new Vector3(0.55f, 1.45f, 1.25f);
            var shield = b.MeshPart("ReflectShield", BotwMeshes.Sphere, Mat(ReflectTeal, "Shell"), FrontPanel, new Vector3(0f, -30f, 0f), shieldScale, order: 2);
            var st = b.Track(shield.transform, shield, 0.5f, 3.0f);
            st.scaleFrom = new Vector3(0.1f, 0.3f, 0.3f);
            st.scaleTo = shieldScale;
            st.scaleCurve = C(0.5f, 0f, 0.8f, 1.05f, 0.9f, 1f, 3.0f, 1f);
            st.property = "_Dissolve";
            st.propertyCurve = C(0.5f, 0.6f, 0.7f, 0f, 2.6f, 0f, 3.0f, 1f);
            // Borde irisado: aros de colores que se abren uno tras otro.
            RingPulses(b, FrontPanel, Pink, 0.9f, 3, 0.3f, 2.9f);
            RingPulses(b, FrontPanel, Cyan, 1.0f, 3, 0.3f, 2.9f);
            RingPulses(b, FrontPanel, AuroraYellow, 1.1f, 3, 0.3f, 2.9f);
            Sparkles(b, FrontPanel, 0.6f, 2.0f, 1.2f, 30f, 1.0f, new Color(1f, 0.6f, 0.9f), new Color(0.6f, 1f, 1f), new Color(1f, 1f, 0.5f), new Color(0.7f, 1f, 0.5f));
            ArcWaves(b, FrontPanel, FrontPanel + new Vector3(1.6f, 0f, 0f), Cyan, 1.2f, 0.6f, 4f, 0.5f, 1.4f, 2.4f);
            b.Light(FrontPanel + new Vector3(-0.6f, 0.5f, -0.6f), Lc(0.7f, 1f, 0.95f), 8f, 0f, 0f, 0.5f, 1.4f, 1.15f, 2f, 3.0f, 0f);
        }

        // ---------------------------------------------------------------- 116 Foco Energía
        // Refs: anime = contorno cian-blanco brillante alrededor del usuario; juego = rayas y energía amarillo-naranjas que se
        // concentran en el usuario. Movimiento de estado (sube críticos): sin estrella, sin cámara lenta, sin sacudida.
        static void M116(EmeraldMoveBuilder b)
        {
            b.Keys(0.4f, 1.2f, 1.7f, 2.9f, AttackerChest, 0f);
            // El clip termina en un destello blanco grande sobre el usuario (no es un golpe).
            b.HideBaked();
            Aura(b, Attacker, Cyan, 0.3f, 2.6f, 1.2f, 0f);
            RisingArrows(b, Attacker, Gold, 0.5f, 1.8f, 26f);
            SpeedLines(b, AttackerChest, Gold, 0.4f, 0.9f, 3.4f, 120f, 1.3f);
            Streaks(b, Attacker + new Vector3(0f, -0.2f, 0f), Attacker + new Vector3(0f, 3.5f, 0f), White, 0.9f, 1.4f, 26f, 1.1f, 1.0f, 0.4f);
            Gather(b, AttackerChest, Sun, 0.4f, 0.8f, 2.0f, 40f, 1.3f, "Glow");
            Glints(b, AttackerChest, Gold, 0.9f, 1.6f, 1.2f, 18f);
            Flash(b, AttackerChest + new Vector3(0f, 0.5f, 0f), Gold, 1.2f, 1.6f);
            GroundGlow(b, Attacker, Gold, 0.5f, 2.8f, 1.5f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(1f, 0.85f, 0.5f), 8f, 0f, 0f, 0.4f, 1.2f, 1.2f, 2.2f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 117 Venganza
        // Refs: anime = el usuario aguanta sin efecto visible; juego = marcas rojas de enfado sobre el usuario y bocanadas de humo
        // blanco al soltar el golpe.
        static void M117(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 2.0f, 2.25f, 3.2f, Hit, 1f);
            // El clip carga y golpea antes (impacto a 1,1 s): la carga larga y la descarga propias lo sustituyen.
            b.HideBaked();
            Aura(b, Attacker, DeepRed, 0.2f, 2.0f, 1.3f, 40f);
            b.Cue(Bursts(b.Ps("BideGlow", Mat(DeepRed, "Glow"), AttackerChest + new Vector3(0.1f, 0.2f, 0f))
                .Life(0.35f).Size(2.8f).SizeOverLife(C(0f, 0.6f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 0f, 0.25f, 0.9f, 1f, 0f).Order(3), 1, 0.5f, 0.95f, 1.4f, 1.75f), 0.5f);
            // Marcas de enfado (cruces rojas cortas) sobre la cabeza.
            b.Cue(Bursts(b.Ps("AngerMarks", Mat(Red, "Spark"), AttackerHead + new Vector3(0.3f, 0.35f, -0.3f))
                .Sphere(0.15f).Speed(0.01f).Life(0.3f).Size(0.05f).Stretch(14f)
                .AlphaOverLife(0f, 1f, 0.6f, 1f, 1f, 0f).Order(6), 4, 0.6f, 1.05f, 1.5f), 0.6f);
            // Descarga: haz blanco y golpe en el rival.
            var from = AttackerChest + new Vector3(0.4f, 0.2f, 0f);
            Beam(b, from, Hit, Cream, 1.95f, 2.4f, 0.55f, 0.08f);
            Streaks(b, from, Hit, Cream, 1.9f, 0.25f, 80f, 0.6f, 1.6f, 0.2f);
            ImpactStar(b, Hit, Cream, 2.0f, 5.4f, 14, 0);
            Smoke(b, AttackerChest + new Vector3(0f, 0.6f, 0f), BotwMaterials.Get("EM_Dust"), 1.95f, 8, 1.1f, 1.0f, 0.8f, 1.2f);
            Shockwave(b, Target, Red, 2.02f, 4.5f);
            HitShell(b, Target, Red, 2.0f, 0.5f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(1f, 0.45f, 0.4f), 10f, 0f, 0f, 0.3f, 1.2f, 1.9f, 2f, 2.02f, 4f, 2.4f, 1.2f, 3.2f, 0f);
            b.Shake(0.5f, 0.08f, 1.4f);
            b.Shake(2.0f, 0.5f, 0.4f);
            b.SlowMo(2.01f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 118 Metrónomo
        // Refs: anime = las manos del usuario brillan blanco-azul; juego = dedo rojo-naranja gigante que se balancea sobre el usuario,
        // arco cian, rayos amarillos y destellos. Movimiento de estado (invoca otro): sin estrella, sin cámara lenta, sin sacudida.
        static void M118(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.6f, 2.1f, 3.0f, TargetChest, 0.3f);
            // El clip termina en un destello blanco grande sobre el usuario: el dedo y la descarga de ejemplo propios lo sustituyen.
            b.HideBaked();
            var wrist = AttackerHead + new Vector3(0.35f, 0.45f, -0.25f);
            WagFinger(b, wrist, FingerRed, 0.3f, 1.75f, 1.7f, 37f, 28f, 0.45f);
            var top = wrist + new Vector3(0f, 0.95f, 0f);
            b.Cue(b.Ps("CyanArc", Mat(Cyan, "Ring"), top)
                .Life(1.4f).Size(3.0f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.12f, 1f, 1f, 1.05f))
                .AlphaOverLife(0f, 0f, 0.1f, 1f, 0.85f, 1f, 1f, 0f).Order(2), 0.35f);
            Rays(b, top, Gold, 0.4f, 6, 4.5f, 0.6f);
            Sparkles(b, top, 0.35f, 1.4f, 1.6f, 22f, 0.9f, Color.white, new Color(1f, 0.95f, 0.5f), new Color(1f, 0.7f, 0.9f));
            // Descarga de ejemplo hacia el rival.
            RingPulses(b, top, White, 1.55f, 2, 0.1f, 2.2f);
            Projectile(b, AttackerChest + new Vector3(0.5f, 0.6f, 0f), TargetChest, Gold, 1.6f, 0.45f, 1.2f, 0.6f, "Glow");
            Flash(b, TargetChest, Gold, 2.05f, 2.4f);
            RingPulses(b, TargetChest, Gold, 2.05f, 2, 0.12f, 2.6f);
            b.Light(top + new Vector3(0.3f, 0f, -0.5f), Lc(1f, 0.8f, 0.55f), 9f, 0f, 0f, 0.3f, 1.5f, 1.6f, 1.2f, 2.07f, 2.2f, 3.0f, 0f);
        }

        // ---------------------------------------------------------------- 119 Espejo
        // Refs: anime = espejo azul oscuro de borde blanco brillante que recibe un haz amarillo; juego = sin efecto (solo el texto).
        // Movimiento de estado (imita): sin estrella, sin cámara lenta, sin sacudida.
        static void M119(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.0f, 1.5f, 2.6f, FrontPanel, 0.3f);
            // El clip pinta un óvalo azul y un destello grande en el rival: el espejo y las ondas propios lo sustituyen.
            b.HideBaked();
            var mirror = FrontPanel + new Vector3(-0.05f, -0.05f, 0f);
            Panel(b, mirror, new Vector2(2.0f, 2.4f), 60f, MirrorPane, 0.3f, 2.6f, 0.25f, true, false);
            CutLine(b, mirror, White, 0.62f, 30f, 2.6f, 0.3f, 0.3f);
            // Onda que llega del rival, destello en el espejo y onda devuelta.
            var target = TargetChest + new Vector3(-0.4f, 0.2f, 0f);
            Beam(b, target, mirror + new Vector3(0.15f, 0f, 0f), Sun, 0.55f, 1.0f, 0.45f, 0.3f);
            ArcWaves(b, target, mirror + new Vector3(0.2f, 0f, 0f), Sun, 0.55f, 0.35f, 8f, 0.45f, 1.6f, 1.0f, 180f);
            Flash(b, mirror, White, 1.0f, 2.4f);
            Beam(b, mirror + new Vector3(0.15f, 0f, 0f), target, Sun, 1.05f, 1.6f, 0.9f, 0.3f);
            ArcWaves(b, mirror + new Vector3(0.3f, 0f, 0f), target, Sun, 1.0f, 0.4f, 9f, 0.45f, 1.4f, 2.6f);
            Flash(b, TargetChest, Sun, 1.5f, 2.2f);
            HitShell(b, Target, Sun, 1.5f, 0.5f);
            Glints(b, mirror, White, 0.9f, 1.4f, 1.1f, 14f);
            b.Light(mirror + new Vector3(-0.6f, 0.5f, -0.6f), Lc(1f, 0.95f, 0.7f), 9f, 0f, 0f, 0.5f, 0.8f, 1.02f, 3f, 1.5f, 1.8f, 2.6f, 0f);
        }

        // ---------------------------------------------------------------- 120 Autodestrucción
        // Refs: anime = destello blanco enorme que llena la pantalla con rayas radiales blancas y amarillas; juego = el usuario
        // brilla y estalla con rayas naranja-rojas horizontales y chispas, después humo oscuro.
        static void M120(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.3f, 1.55f, 3.0f, AttackerChest, 1f);
            // El clip pinta una bola amarilla de contorno rojo: las referencias son blancas (anime) y naranjas (juego).
            b.HideBaked();
            var core = AttackerChest + new Vector3(0.2f, 0.2f, 0f);
            Charge(b, core, White, 0.3f, 1.0f, 1.8f, 2.2f, false);
            Aura(b, Attacker, White, 0.4f, 1.32f, 1.3f, 0f);
            ImpactStar(b, core, White, 1.3f, 9f, 0, 0, 0.4f);
            Rays(b, core, Gold, 1.3f, 18, 11f, 0.45f);
            // Rayas naranja-rojas horizontales y chispas (juego); la bola de fuego llenaba la pantalla de lenguas planas.
            Streaks(b, core - ScreenRight * 3f, core + ScreenRight * 3f, Fire, 1.3f, 0.35f, 90f, 1.2f, 1.6f, 0.25f);
            Sparks(b, core, Fire, 1.32f, 40, 8f, 16f, 0.8f, 1.4f);
            Shockwave(b, Attacker, Cream, 1.32f, 7f);
            Smoke(b, core, BotwMaterials.Get("EX_DarkSmoke"), 1.5f, 12, 2.0f, 0.8f, 1.2f, 1.8f);
            b.Light(core + new Vector3(0.4f, 0.6f, -0.6f), Lc(1f, 0.85f, 0.6f), 16f, 0f, 0f, 0.3f, 1f, 1.25f, 3f, 1.33f, 9f, 1.9f, 3f, 3.0f, 0f);
            b.Shake(0.5f, 0.06f, 0.8f);
            b.Shake(1.3f, 0.7f, 0.6f);
            b.SlowMo(1.31f, 0.25f, 0.1f);
        }
    }
}
