using UnityEngine;
using static BotwVfx.EditorTools.EmeraldLayers;
using static BotwVfx.EditorTools.EmeraldMoveBuilder;
using static BotwVfx.EditorTools.Fx;
using MinMaxCurve = UnityEngine.ParticleSystem.MinMaxCurve;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Lote 7 (movimientos 121-140). Mismo criterio que los lotes 1-6: referencias del anime y del juego resumidas en el
    /// comentario de cada receta, colores de las referencias y volumen en el pico escalado a lo que cubren en pantalla.
    /// Movimientos de estado (sin estrella de impacto, cámara lenta ni sacudida fuerte): 133, 134, 135, 137 y 139.
    /// Presupuesto: como mucho 10 sistemas Shuriken por movimiento (≤ 600 KB por prefab); mallas y líneas son baratas.
    /// Colores sólidos/planos: en la captura el valor lineal v se ve como v^(1/6) aprox. (0,07 → 171/255), así que los canales
    /// bajos de los colores saturados van casi a 0.
    /// </summary>
    public static partial class EmeraldRecipes
    {
        // Huevo (Bomba Huevo, Ovocuración): blanco cálido sólido.
        static readonly Pal EggWhite = new Pal("EggWhite", new Color(1.15f, 1.1f, 0.95f), new Color(0.7f, 0.66f, 0.58f));
        // Lengua de Lengüetazo (juego): rosa intenso; borde magenta oscuro.
        static readonly Pal TonguePink = new Pal("TonguePink", new Color(1.0f, 0.004f, 0.11f), new Color(0.3f, 0.002f, 0.012f));
        // Lodo violeta de Residuos (anime): resplandor violeta y charco oscuro.
        static readonly Pal SludgeViolet = new Pal("SludgeViolet", new Color(0.32f, 0.004f, 1.3f), new Color(0.1f, 0.002f, 0.6f));
        static readonly Pal SludgeDark = new Pal("SludgeDark", new Color(0.12f, 0.004f, 0.25f), new Color(0.04f, 0.001f, 0.1f));
        // Hueso (juego): blanco hueso sólido.
        static readonly Pal BoneCream = new Pal("BoneCream", new Color(0.5f, 0.45f, 0.32f), new Color(0.3f, 0.26f, 0.17f));
        // Estrellas planas de Meteoros (juego): amarillo limón.
        static readonly Pal StarYellow = new Pal("StarYellow", new Color(1.0f, 0.6f, 0.0005f), new Color(1.0f, 0.35f, 0.0003f));
        // Púas de Clavo Cañón: núcleo crema y borde naranja (el disparo del juego).
        static readonly Pal SpikeCream = new Pal("SpikeCream", new Color(2.6f, 2.45f, 2.0f), new Color(2.2f, 0.9f, 0.15f));
        // Valvas de agua de Tenaza: cian claro del catálogo (#a0d6df) en sólido plano.
        static readonly Pal ClampWater = new Pal("ClampWater", new Color(0.06f, 0.35f, 0.5f), new Color(0.01f, 0.12f, 0.3f));
        // Espirales verdes de Restricción (juego).
        static readonly Pal VineSolid = new Pal("VineSolid", new Color(0.02f, 0.3f, 0.002f), new Color(0.005f, 0.08f, 0.001f));
        // Interrogantes de Amnesia (anime): azul claro con borde azul.
        static readonly Pal QuestionBlue = new Pal("QuestionBlue", new Color(0.04f, 0.3f, 0.75f), new Color(0.003f, 0.004f, 0.3f));
        // Ojos de Deslumbrar (juego): destello blanco con halo rojo puro.
        static readonly Pal GlareStar = new Pal("GlareStar", new Color(2.6f, 2.4f, 2.4f), new Color(1.3f, 0.004f, 0.01f));
        static readonly Pal GlareRed = new Pal("GlareRed", new Color(1.5f, 0.05f, 0.06f), new Color(1.0f, 0.003f, 0.006f));
        // Motas violetas de Comesueños (descripción).
        static readonly Pal DreamViolet = new Pal("DreamViolet", new Color(0.45f, 0.15f, 1.3f), new Color(0.2f, 0.02f, 0.9f));
        // Cuchara de Kinético (juego): gris plata plano (Metal se ve blanco).
        static readonly Pal SpoonSilver = new Pal("SpoonSilver", new Color(0.09f, 0.09f, 0.11f), new Color(0.035f, 0.035f, 0.045f));
        // Bolas de Bombardeo (juego): gris claro sólido.
        static readonly Pal BallGrey = new Pal("BallGrey", new Color(0.32f, 0.32f, 0.35f), new Color(0.13f, 0.13f, 0.15f));

        // ---------------------------------------------------------------- 121 Bomba Huevo
        // Refs: anime = Exeggutor lanza bajo un sol blanco (sin huevo a la vista); juego = huevo blanco en arco con rayas blancas
        // radiales de manga. Descripción: el huevo estalla al llegar.
        static void M121(EmeraldMoveBuilder b)
        {
            b.Keys(0.95f, 1.3f, 1.45f, 2.4f, Hit, 0.9f);
            // El clip pinta un huevo pequeño y una explosión: el huevo y el estallido propios lo sustituyen.
            b.HideBaked();
            var from = AttackerChest + new Vector3(0.4f, 0.35f, 0f);
            Rays(b, from + new Vector3(0.3f, 0.2f, 0f), White, 0.55f, 12, 5.5f, 0.4f);
            Lob(b, BotwMeshes.Sphere, Mat(EggWhite, "Solid"), from, Hit + new Vector3(0f, 0.1f, 0f), 0.6f, 0.7f, new Vector3(0.5f, 0.65f, 0.5f), 1.6f, 300f, White, 0.45f);
            // Estallido: estrella, bola de fuego, cáscaras y humo.
            ImpactStar(b, Hit, Cream, 1.3f, 5.6f, 0, 0);
            FireBurst(b, Hit, 1.32f, 1.25f, false);
            Debris(b, Hit, 1.3f, 16, 7f, 1.3f, Mat(EggWhite, "Solid"));
            Smoke(b, Hit, BotwMaterials.Get("EX_Smoke"), 1.4f, 8, 1.2f, 0.7f, 0.6f, 1.6f);
            Shockwave(b, Target, Orange, 1.32f, 4f);
            HitShell(b, Target, Gold, 1.3f, 0.45f);
            b.Light(Hit + new Vector3(-0.8f, 0.6f, 0f), Lc(1f, 0.8f, 0.5f), 11f, 0f, 0f, 1.28f, 0f, 1.32f, 6f, 1.7f, 2f, 2.4f, 0f);
            b.Shake(1.3f, 0.45f, 0.4f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 122 Lengüetazo
        // Refs: anime = sin captura; juego = lengua rosa-magenta enorme que llega al rival y un resplandor cian-azul donde lo toca.
        static void M122(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.0f, 1.2f, 2.2f, TargetChest, 0.5f);
            // El clip pinta esquirlas violeta y una lengua pequeña: la lengua propia lo sustituye.
            b.HideBaked();
            var mouth = Mouth + new Vector3(0.2f, -0.1f, 0f);
            var lick = TargetChest + new Vector3(-0.25f, 0.1f, -0.05f);
            Vine(b, mouth, lick, TonguePink, 0.45f, 1.0f, 1.8f, 0.8f, 0.14f);
            // Contacto: resplandor cian-azul (juego), saliva y chispas de parálisis.
            b.Cue(b.Ps("LickGlow", Mat(Cyan, "Glow"), lick)
                .Life(0.7f).Size(2.2f).Burst(1).SizeOverLife(C(0f, 0.4f, 0.2f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(3), 0.98f);
            Flash(b, lick, Pink, 1.0f, 1.6f);
            RingPulses(b, lick, Cyan, 1.0f, 2, 0.12f, 2.2f);
            Sparks(b, lick, Pink, 1.0f, 14, 3f, 6f, 1.2f, 1.2f);
            Crackle(b, TargetChest, Electric, 1.05f, 0.9f, 0.8f, 45f);
            HitShell(b, Target, Cyan, 1.0f, 0.5f);
            b.Light(lick + new Vector3(-0.7f, 0.5f, 0f), Lc(0.7f, 0.85f, 1f), 8f, 0f, 0f, 0.95f, 0f, 1.0f, 3f, 1.4f, 1f, 2.2f, 0f);
            b.Shake(1.0f, 0.2f, 0.25f);
        }

        // ---------------------------------------------------------------- 123 Polución
        // Refs: anime = nube de humo gris espesa que sale hacia arriba; juego = bocanada de humo magenta-violeta que llega al rival.
        static void M123(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.25f, 1.6f, 2.8f, TargetChest, 0.5f);
            // El clip pinta manchas negras y un destello rosa: el humo propio lo sustituye.
            b.HideBaked();
            var mouth = Mouth + new Vector3(0.2f, -0.05f, 0f);
            var smoke = BotwMaterials.Get("EX_Smoke");
            var poison = BotwMaterials.Get("EM_PoisonSmoke");
            var grey = Grad(new[] { 0f, 1f, 0.97f, 0.95f, 0.6f, 0.95f, 0.9f, 0.9f, 1f, 0.95f, 0.75f, 0.95f }, new[] { 0f, 0.6f, 0.1f, 1f, 0.8f, 1f, 1f, 0f });
            ConeSpray(b, "SmogJet", mouth, TargetChest + new Vector3(-0.3f, 0.2f, 0f), smoke, 0.5f, 0.85f, 34f, 11f, 0.55f, 2.0f, 0.75f, grey, 0f, 30f, 2)
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.6f, 0.05f, 1f, 1f), Rand(0f, 1f));
            Tint(b.Cue(b.Ps("SmogPuff", smoke, mouth + new Vector3(0.2f, 0.2f, 0f))
                .Sphere(0.4f).Speed(0.6f, 1.6f).Drag(2f).Life(1.0f, 1.5f).Size(0.8f, 1.4f).Burst(7).Velocity(new Vector3(0.3f, 0.8f, 0f))
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.5f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1), 0.45f), new Color(0.95f, 0.92f, 0.9f));
            // En el rival la nube se vuelve violeta (juego).
            var violet = new Color(0.95f, 0.45f, 1f);
            Tint(b.Cue(b.Ps("PoisonPuff", poison, TargetChest)
                .Sphere(0.6f).Speed(1.0f, 2.4f).Drag(2.2f).Life(1.2f, 1.7f).Size(1.2f, 2.0f).Burst(10).Velocity(new Vector3(0f, 0.4f, 0f))
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.45f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(2), 1.2f), violet);
            Tint(Fog(b, Target, poison, 1.25f, 1.3f, 1.0f, 9f, 1.2f, 0.6f, 1.7f), violet);
            b.Cue(b.Ps("PoisonBubbles", BotwMaterials.Get("EM_Bubble"), Target + new Vector3(0f, 0.4f, 0f))
                .Sphere(0.6f).Velocity(new Vector3(0f, 0.9f, 0f)).Speed(0.1f, 0.5f).Life(0.6f, 1f).Size(0.14f, 0.3f)
                .Rate(16f).Duration(1.0f).SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 1.4f);
            HitShell(b, Target, Violet, 1.25f, 0.6f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.85f, 0.6f, 1f), 8f, 0f, 0f, 1.2f, 0f, 1.3f, 1.8f, 2.8f, 0f);
            b.Shake(1.25f, 0.15f, 0.3f);
        }

        // ---------------------------------------------------------------- 124 Residuos
        // Refs: anime = chorros alargados de lodo violeta con motas oscuras en diagonal; juego = masa violeta-magenta que salpica
        // al rival y gotea. Descripción: arcos de lodo, salpicadura y charco temporal.
        static void M124(EmeraldMoveBuilder b)
        {
            b.Keys(0.95f, 1.3f, 1.5f, 2.8f, TargetChest, 0.7f);
            // El clip pinta una mancha negra opaca sobre el usuario: los arcos de lodo propios lo sustituyen.
            b.HideBaked();
            var from = Mouth + new Vector3(0.2f, -0.05f, 0f);
            Projectile(b, from, TargetChest + new Vector3(0f, 0.1f, 0f), SludgeViolet, 0.75f, 0.52f, 1.1f, 0.6f, "Glow", new Vector3(0f, 1.0f, 0f));
            Projectile(b, from + new Vector3(0f, 0.1f, 0.2f), TargetChest + new Vector3(0f, 0.4f, 0.2f), SludgeViolet, 0.85f, 0.5f, 0.9f, 0.5f, "Glow", new Vector3(0f, 1.5f, 0f));
            Projectile(b, from + new Vector3(0f, -0.1f, -0.2f), TargetChest + new Vector3(0f, -0.2f, -0.2f), SludgeViolet, 0.95f, 0.48f, 0.95f, 0.5f, "Glow", new Vector3(0f, 0.7f, 0f));
            Splash(b, TargetChest, SludgeViolet, 1.27f, 1.3f, 26, BotwMaterials.Get("EM_PoisonSmoke"));
            Flash(b, TargetChest, SludgeViolet, 1.27f, 2.4f);
            // Charco oscuro que se extiende bajo el rival y se seca.
            var puddle = new Vector3(2.6f, 0.4f, 2.0f);
            var pr = b.MeshPart("Puddle", Coin, Mat(SludgeDark, "Solid"), Target + new Vector3(0f, 0.03f, 0f), Vector3.zero, puddle, order: 0);
            var pt = b.Track(pr.transform, pr, 1.3f, 2.8f);
            pt.scaleFrom = new Vector3(0.3f, 0.4f, 0.3f);
            pt.scaleTo = puddle;
            pt.scaleCurve = C(1.3f, 0f, 1.5f, 1.05f, 1.6f, 1f, 2.4f, 1f, 2.8f, 0.3f);
            pt.property = "_Erosion";
            pt.propertyCurve = C(1.3f, 0f, 2.3f, 0f, 2.8f, 1f);
            b.Cue(b.Ps("SludgeBubbles", BotwMaterials.Get("EM_Bubble"), Target + new Vector3(0f, 0.15f, 0f))
                .GroundCircle(0.9f, 1f).Velocity(new Vector3(0f, 0.6f, 0f)).Life(0.5f, 0.9f).Size(0.15f, 0.32f)
                .Rate(14f).Duration(1.0f).SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 1.5f);
            HitShell(b, Target, Violet, 1.27f, 0.6f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.8f, 0.45f, 1f), 9f, 0f, 0f, 1.25f, 0f, 1.29f, 3.5f, 1.7f, 1.4f, 2.8f, 0f);
            b.Shake(1.27f, 0.3f, 0.3f);
        }

        // ---------------------------------------------------------------- 125 Hueso Palo
        // Refs: anime = sin captura; juego = hueso blanco que golpea de arriba abajo y un arco blanco con destellos alrededor del rival.
        static void M125(EmeraldMoveBuilder b)
        {
            b.Keys(0.85f, 1.25f, 1.4f, 2.3f, TargetHead, 0.8f);
            // El clip pinta un hueso pequeño junto al usuario: el hueso propio golpea en el rival.
            b.HideBaked();
            var grip = TargetChest - ScreenRight * 1.25f + new Vector3(0f, 0.15f, 0f);
            SwingProp(b, Bone, Mat(BoneCream, "Solid"), grip, new Vector3(1.9f, 1.5f, 1.9f), 0.5f, 1.25f, 2.1f, 35f, -85f);
            // Estela del golpe: media luna blanca centrada en la empuñadura.
            Slash(b, grip + new Vector3(0f, 1.45f, 0f), White, 1.13f, -90f, 1.45f, 0.3f, 120f);
            var head = TargetHead + new Vector3(-0.2f, 0.05f, -0.1f);
            ImpactStar(b, head, Cream, 1.25f, 3.8f, 10, 24);
            RingPulses(b, TargetChest + new Vector3(0f, 0.2f, 0f), White, 1.25f, 2, 0.1f, 3.6f);
            Dust(b, Target, 1.28f, 1f, 10);
            Shockwave(b, Target, Sand, 1.28f, 3.2f);
            HitShell(b, Target, Cream, 1.25f, 0.4f);
            b.Light(head + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.9f, 0.7f), 9f, 0f, 0f, 1.23f, 0f, 1.27f, 4f, 1.6f, 1.2f, 2.3f, 0f);
            b.Shake(1.25f, 0.35f, 0.35f);
            b.SlowMo(1.26f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 126 Llamarada
        // Refs: anime = llamarada enorme en forma de 大 (estrella de cinco brazos) naranja con núcleo amarillo-blanco; juego = bola
        // de fuego amarillo-blanca con borde naranja sobre el rival.
        static void M126(EmeraldMoveBuilder b)
        {
            b.Keys(0.95f, 1.3f, 1.5f, 2.6f, Hit, 1f);
            // El clip pinta una explosión de fuego sin la figura 大: la figura y el estallido propios lo sustituyen.
            b.HideBaked();
            var mouth = Mouth + new Vector3(0.25f, -0.05f, 0f);
            b.Cue(b.Ps("MouthFire", Mat(Fire, "Glow"), mouth)
                .Life(0.6f).Size(1.4f).Burst(1).SizeOverLife(C(0f, 0.2f, 0.8f, 1f, 1f, 1.3f))
                .AlphaOverLife(0f, 0f, 0.2f, 1f, 0.8f, 1f, 1f, 0f).Order(5), 0.3f);
            // Figura 大: sale delante de la boca, viaja al rival creciendo y se abre al llegar.
            var start = mouth + new Vector3(0.6f, 0f, 0f);
            var end = Hit + new Vector3(-0.35f, 0.3f, 0f);
            const float travel = 0.7f, life = 1.0f;
            var v = (end - start) / travel;
            float k = travel / life;
            foreach (var layer in new[] { (Fire, 2.6f, 9), (FireSpin, 4.2f, 8), (Red, 5.6f, 7) })
            {
                var f = b.Ps("FlameStar", Mat(layer.Item1, "Blade"), start)
                    .Mesh(FlameStar, ParticleSystemRenderSpace.View).Life(life).Size(layer.Item2).Burst(1).Rotation(0f, 0f).Spin(25f, 25f)
                    .SizeOverLife(C(0f, 0.3f, k, 1f, 1f, 1.25f)).AlphaOverLife(0f, 1f, 0.75f, 1f, 1f, 0f).Order(layer.Item3);
                var vel = f.ps.velocityOverLifetime;
                vel.enabled = true;
                vel.space = ParticleSystemSimulationSpace.World;
                vel.x = new MinMaxCurve(1f, C(0f, v.x, k - 0.02f, v.x, k + 0.02f, 0f, 1f, 0f));
                vel.y = new MinMaxCurve(1f, C(0f, v.y, k - 0.02f, v.y, k + 0.02f, 0f, 1f, 0f));
                vel.z = new MinMaxCurve(1f, C(0f, v.z, k - 0.02f, v.z, k + 0.02f, 0f, 1f, 0f));
                b.Cue(f, 0.6f);
            }
            // Estallido amarillo-blanco (juego) que se deshace en lenguas, brasas y humo.
            ImpactStar(b, Hit, Fire, 1.3f, 5f, 0, 0);
            FireBurst(b, Hit, 1.32f, 1.35f, false);
            Embers(b, Hit, 1.45f, 1.0f, 1.2f, 40f);
            b.Light(Hit + new Vector3(-0.8f, 0.6f, 0f), Lc(1f, 0.6f, 0.25f), 14f, 0f, 0f, 0.6f, 1.2f, 1.28f, 2f, 1.32f, 8f, 1.9f, 3f, 2.6f, 0f);
            b.Shake(1.3f, 0.6f, 0.5f);
            b.SlowMo(1.31f, 0.25f, 0.1f);
        }

        // ---------------------------------------------------------------- 127 Cascada
        // Refs: anime = columna cónica de agua azul con bordes blancos y rayas blancas verticales de fondo; juego = oleada de agua
        // cian brillante con rayas blancas hacia el rival.
        static void M127(EmeraldMoveBuilder b)
        {
            b.Keys(0.9f, 1.3f, 1.45f, 2.5f, Hit, 0.9f);
            b.Baked(1.3f); // el cono de agua azul con rayas blancas del clip es el del anime
            Streaks(b, Attacker + new Vector3(0f, -0.2f, 0f), Attacker + new Vector3(0f, 4.2f, 0f), White, 0.45f, 0.9f, 50f, 1.0f, 1.3f, 0.35f);
            RisingArrows(b, Attacker, Water, 0.5f, 0.8f, 26f);
            Stream(b, AttackerChest + new Vector3(0.4f, 0f, 0f), Hit, Mat(Water, "Glow"), 1.0f, 0.3f, 60f, 0.3f, 0.5f, 1.4f, 0f, 0.3f);
            // Golpe hacia arriba: géiser en el rival.
            ImpactStar(b, Hit, Water, 1.3f, 5.2f, 0, 0);
            Splash(b, Hit, Water, 1.3f, 1.5f, 34);
            Streaks(b, Target + new Vector3(0f, 0f, 0f), Target + new Vector3(0f, 4.5f, 0f), Water, 1.3f, 0.45f, 70f, 0.6f, 1.6f, 0.3f);
            Shockwave(b, Target, Water, 1.32f, 4.2f);
            HitShell(b, Target, Cyan, 1.3f, 0.45f);
            b.Light(Hit + new Vector3(-0.8f, 0.6f, 0f), Lc(0.55f, 0.85f, 1f), 10f, 0f, 0f, 0.5f, 1f, 1.28f, 1.5f, 1.32f, 5f, 1.8f, 1.6f, 2.5f, 0f);
            b.Shake(1.3f, 0.45f, 0.4f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 128 Tenaza
        // Refs: anime = sin captura; juego = dos valvas de concha malva que se cierran sobre el rival. Descripción: con Squirtle,
        // dos arcos de agua que se cierran alrededor del objetivo (sin pinza).
        static void M128(EmeraldMoveBuilder b)
        {
            b.Keys(0.85f, 1.2f, 1.6f, 2.6f, TargetChest, 0.5f);
            // El clip pinta un aro de agua delante del usuario: las valvas propias lo sustituyen.
            b.HideBaked();
            var pivot = b.Node("ClampPivot", TargetChest + new Vector3(0f, 0.05f, 0f));
            pivot.localRotation = Quaternion.Euler(0f, 37f, 0f);
            foreach (var layer in new[] { (Mat(ClampWater, "Flat"), 1.3f, 1.0f, 4), (Mat(White, "Blade"), 1.05f, 0.85f, 5) })
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    var scale = new Vector3(layer.Item2 * 1.3f, layer.Item2, 1f);
                    var r = b.MeshPart("ClampArc", Crescent, layer.Item1, new Vector3(s * 2.2f, 0f, 0f),
                        new Vector3(0f, 0f, s < 0 ? 180f : 0f), scale, pivot, layer.Item4);
                    var tr = b.Track(r.transform, r, 0.55f, 2.6f);
                    tr.posFrom = new Vector3(s * 2.3f, 0f, 0f);
                    tr.posTo = new Vector3(s * layer.Item3, 0f, 0f);
                    tr.posCurve = C(0.55f, 0f, 1.0f, 0.55f, 1.2f, 1f, 1.6f, 1f, 1.68f, 0.85f, 1.76f, 1f, 2.0f, 1f, 2.08f, 0.85f, 2.16f, 1f, 2.6f, 1f);
                    tr.scaleFrom = scale * 0.5f;
                    tr.scaleTo = scale;
                    tr.scaleCurve = C(0.55f, 0f, 0.8f, 1f, 2.6f, 1f);
                    tr.property = "_Erosion";
                    tr.propertyCurve = C(0.55f, 0.5f, 0.7f, 0f, 2.25f, 0f, 2.6f, 1f);
                }
            }
            Splash(b, TargetChest, Water, 1.2f, 1.0f, 22);
            Flash(b, TargetChest, Water, 1.2f, 1.8f);
            b.Cue(Bursts(b.Ps("ClampPulse", Mat(Water, "Ring"), TargetChest).Life(0.3f).Size(3.0f)
                .SizeOverLife(C(0f, 0.25f, 0.4f, 0.9f, 1f, 1.15f)).AlphaOverLife(0f, 1f, 0.4f, 0.85f, 1f, 0f).Order(4), 1, 1.2f, 1.68f, 2.08f), 1.2f);
            Bubbles(b, Target + new Vector3(0f, 0.3f, 0f), TargetHead + new Vector3(0f, 1.0f, 0f), Water, 1.2f, 1.2f, 10f, 0.8f, 0.2f, 0.35f);
            HitShell(b, Target, Water, 1.2f, 0.5f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.55f, 0.85f, 1f), 8f, 0f, 0f, 0.8f, 0.6f, 1.2f, 2.2f, 1.6f, 1.2f, 2.6f, 0f);
            b.Shake(1.2f, 0.25f, 0.3f);
            b.Shake(1.68f, 0.12f, 0.15f);
            b.Shake(2.08f, 0.12f, 0.15f);
        }

        // ---------------------------------------------------------------- 129 Meteoros
        // Refs: anime = estrellas doradas de cinco puntas y una estela dorada con purpurina; juego = estrellas amarillas planas
        // alrededor del rival y destellos blancos de cuatro puntas.
        static void M129(EmeraldMoveBuilder b)
        {
            b.Keys(0.85f, 1.15f, 1.5f, 2.6f, TargetChest, 0.6f);
            // El clip pinta estrellas naranjas de contorno rojo: las referencias son amarillas.
            b.HideBaked();
            var from = AttackerChest + new Vector3(0.5f, 0.5f, 0f);
            var star = Mat(StarYellow, "Flat");
            Slash(b, AttackerChest + new Vector3(0.3f, 0.6f, 0f), Gold, 0.45f, -60f, 2.2f, 0.5f, -300f);
            StarStream(b, from, TargetChest + new Vector3(0f, 0.2f, 0f), Star5, star, Gold, 0.55f, 1.2f, 18f, 0.55f, 1.15f, 0.9f, 0.22f);
            Streaks(b, from, TargetChest, Gold, 0.55f, 1.2f, 60f, 0.9f, 0.6f, 0.5f);
            HitStars(b, TargetChest, Gold, new[] { 1.1f, 1.3f, 1.5f, 1.7f }, 2.6f, 8, 0.4f);
            b.Cue(b.Ps("StarBurst", star, TargetChest)
                .Mesh(Star5, ParticleSystemRenderSpace.View).Sphere(0.6f).Speed(4f, 8f).Drag(3f).Life(0.8f, 1.1f).Size(0.6f, 1.0f).Burst(12)
                .Rotation(0f, 72f).Spin(-200f, 200f).SizeOverLife(C(0f, 0.5f, 0.2f, 1f, 1f, 0.6f)).AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(6), 1.15f);
            Glints(b, TargetChest, White, 1.1f, 1.2f, 1.3f, 16f, 1.4f);
            HitShell(b, Target, Gold, 1.15f, 0.5f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(1f, 0.9f, 0.5f), 9f, 0f, 0f, 0.5f, 0.8f, 1.15f, 2.5f, 1.8f, 1.2f, 2.6f, 0f);
            b.Shake(1.15f, 0.25f, 0.5f);
        }

        // ---------------------------------------------------------------- 130 Cabezazo
        // Refs: anime = el usuario embiste con rayas de velocidad blanco-cian; juego = vórtice de aros azul oscuro con núcleo
        // blanco-azul en la cabeza del usuario. El primer turno sube la Defensa.
        static void M130(EmeraldMoveBuilder b)
        {
            b.Keys(0.8f, 1.4f, 1.55f, 2.4f, Hit, 1f);
            // El clip golpea antes (impacto a ~0,9 s): la carga larga y la embestida propias lo sustituyen.
            b.HideBaked();
            var head = AttackerHead + new Vector3(0.35f, 0f, 0f);
            Charge(b, head, DragonBlue, 0.3f, 0.9f, 1.8f, 1.1f, true);
            RisingArrows(b, Attacker, Steel, 0.35f, 0.8f, 16f);
            var hitHead = TargetHead + new Vector3(-0.4f, -0.15f, -0.1f);
            Projectile(b, head, hitHead, Cyan, 1.15f, 0.25f, 1.6f, 1.1f, "Glow");
            Streaks(b, AttackerChest + new Vector3(0.2f, 0.2f, 0f), Hit, White, 1.1f, 0.35f, 90f, 0.9f, 1.4f, 0.22f);
            ImpactStar(b, hitHead, White, 1.4f, 4.8f, 14, 0);
            RingPulses(b, hitHead, DragonBlue, 1.4f, 3, 0.08f, 4.2f);
            HitShell(b, Target, Cyan, 1.4f, 0.45f);
            b.Light(head + new Vector3(0.3f, 0.5f, -0.5f), Lc(0.6f, 0.8f, 1f), 11f, 0f, 0f, 0.3f, 1.2f, 1.2f, 2f, 1.42f, 6f, 1.8f, 1.5f, 2.4f, 0f);
            b.Shake(1.4f, 0.55f, 0.45f);
            b.SlowMo(1.41f, 0.25f, 0.1f);
        }

        // ---------------------------------------------------------------- 131 Clavo Cañón
        // Refs: anime = sin captura; juego = disparo pequeño y brillante blanco-naranja hacia el rival. Descripción: ráfaga de púas
        // claras y estrechas (de 2 a 5 impactos).
        static void M131(EmeraldMoveBuilder b)
        {
            var hits = new[] { 1.15f, 1.4f, 1.65f, 1.9f };
            const float travel = 0.3f;
            var launches = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
                launches[i] = hits[i] - travel;
            b.Keys(1.0f, 1.15f, 1.65f, 2.5f, Hit, 0.6f);
            // El clip pinta un proyectil negro con contorno rojo: las púas claras propias lo sustituyen.
            b.HideBaked();
            var from = Mouth + new Vector3(0.25f, -0.1f, 0f);
            Volley(b, from, Hit, SpikeCream, launches, 3, travel, 2.2f, 0.25f, 0.22f);
            b.Cue(Bursts(b.Ps("Muzzle", Mat(Orange, "Glow"), from).Life(0.14f).Size(0.9f)
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 0.8f)).AlphaOverLife(0f, 1f, 1f, 0f).Order(6), 1, launches), launches[0]);
            HitStars(b, Hit, Cream, hits, 2.9f, 8, 0.35f);
            RingPulses(b, Hit, Orange, hits[0], 4, 0.25f, 2.2f);
            HitShell(b, Target, Cream, hits[0], 0.9f);
            b.Light(Hit + new Vector3(-0.8f, 0.6f, 0f), Lc(1f, 0.85f, 0.55f), 8f, 0f, 0f, 1.13f, 0f, 1.16f, 2.5f, 1.4f, 2.5f, 1.9f, 2.5f, 2.5f, 0f);
            foreach (float t in hits)
                b.Shake(t, 0.18f, 0.15f);
        }

        // ---------------------------------------------------------------- 132 Restricción
        // Refs: anime = lianas azul-verdosas borrosas que envuelven al rival; juego = espirales verdes enormes que se enroscan
        // alrededor del rival. Descripción: tentáculos cortos que envuelven y comprimen.
        static void M132(EmeraldMoveBuilder b)
        {
            b.Keys(0.85f, 1.3f, 1.75f, 2.8f, TargetChest, 0.4f);
            // El clip apenas pinta nada propio (destello y polvo): las lianas y espirales propias lo sustituyen.
            b.HideBaked();
            Vine(b, AttackerChest + new Vector3(0.3f, 0f, 0f), TargetChest + new Vector3(-0.3f, 0f, 0f), VineGreen, 0.4f, 0.95f, 1.35f, 0.18f, 0.5f, 0f);
            Vine(b, AttackerChest + new Vector3(0.3f, -0.3f, 0.1f), TargetChest + new Vector3(-0.3f, -0.35f, 0.1f), VineGreen, 0.45f, 1.0f, 1.35f, 0.15f, 0.45f, 2f);
            HelixWrap(b, Target, VineSolid, 0.9f, 1.6f, 2.8f, 3.6f, 1.15f, 2.0f, 260f, 0f);
            HelixWrap(b, Target, VineSolid, 0.95f, 1.65f, 2.8f, 4.4f, 1.5f, 2.6f, -200f, 60f);
            var squeezes = new[] { 1.65f, 2.0f, 2.35f };
            HitStars(b, TargetChest, Leaf, squeezes, 1.8f, 6, 0.45f);
            SwirlLeaves(b, Target, 1.0f, 1.4f, 1.4f, 1.6f, 18f);
            Dust(b, Target, 1.25f, 0.9f, 8, 0.8f);
            HitShell(b, Target, Leaf, 1.25f, 0.5f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.7f, 1f, 0.5f), 8f, 0f, 0f, 0.9f, 0.6f, 1.3f, 1.8f, 2.2f, 1.2f, 2.8f, 0f);
            foreach (float t in squeezes)
                b.Shake(t, 0.15f, 0.15f);
        }

        // ---------------------------------------------------------------- 133 Amnesia
        // Refs: anime = signos de interrogación azul claro de borde oscuro alrededor de la cabeza; juego = nube de pensamiento blanca
        // con un interrogante azul sobre el usuario. Movimiento de estado (sube Def. Esp.): sin estrella, sin cámara lenta, sin sacudida.
        static void M133(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.8f, 2.8f, AttackerHead, 0f);
            b.Baked(1.3f); // la nube de pensamiento blanca con el interrogante azul del clip es la del juego
            var head = AttackerHead + new Vector3(0f, 0.2f, 0f);
            QuestionMark(b, head - ScreenRight * 0.85f + new Vector3(0f, 0.15f, 0f), QuestionBlue, 0.5f, 2.6f, 0.9f, -15f, 0.17f);
            QuestionMark(b, head + ScreenRight * 0.85f + new Vector3(0f, -0.1f, 0f), QuestionBlue, 0.72f, 2.7f, 0.8f, 18f, 0.17f);
            QuestionMark(b, head - ScreenRight * 0.35f + new Vector3(0f, 0.75f, 0f), QuestionBlue, 0.95f, 2.8f, 1.05f, -5f, 0.17f);
            Glints(b, head, White, 0.6f, 1.8f, 1.1f, 10f);
            GroundGlow(b, Attacker, Cyan, 1.2f, 2.8f, 1.0f);
            b.Light(head + new Vector3(0.5f, 0.5f, -0.5f), Lc(0.75f, 0.85f, 1f), 7f, 0f, 0f, 0.5f, 0.8f, 1.3f, 1.4f, 2.8f, 0f);
        }

        // ---------------------------------------------------------------- 134 Kinético
        // Refs: anime = sin captura; juego = cucharas plateadas sobre fondo violeta con rayas radiales blanco-violeta. Descripción:
        // ondas de la cuchara y distorsión alrededor del rival. Movimiento de estado (baja Precisión): sin estrella ni sacudida.
        static void M134(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.2f, 1.7f, 2.8f, TargetChest, 0.1f);
            // El clip pinta un aro azul grande y una cuchara azul transparente: la cuchara plateada propia lo sustituye.
            b.HideBaked();
            var bend = AttackerChest + ScreenRight * 0.75f + new Vector3(0f, 0.55f, 0f);
            BendSpoon(b, bend, SpoonSilver, 0.35f, 0.7f, 1.15f, 2.6f, 2.2f, -55f);
            RingPulses(b, bend + new Vector3(0f, 0.6f, 0f), Violet, 0.75f, 4, 0.12f, 2.4f);
            SpeedLines(b, AttackerChest + new Vector3(0f, 0.3f, 0f), Violet, 0.4f, 1.3f, 3.8f, 110f, 1.3f);
            ArcWaves(b, bend + new Vector3(0.3f, 0.3f, 0f), TargetChest, Lilac, 0.8f, 0.9f, 5f, 0.6f, 1.0f, 2.8f);
            Orbit(b, TargetHead + new Vector3(0f, -0.1f, 0f), Violet, 1.2f, 1.5f, 8, 0.8f, 4f, 0.6f, "Glow", 12f);
            RingPulses(b, TargetChest, Lilac, 1.2f, 3, 0.2f, 3.0f);
            Glints(b, bend + new Vector3(0f, 0.4f, 0f), White, 0.6f, 1.6f, 0.9f, 12f);
            HitShell(b, Target, Lilac, 1.2f, 0.8f);
            b.Light(bend + new Vector3(0.3f, 0.3f, -0.5f), Lc(0.85f, 0.6f, 1f), 8f, 0f, 0f, 0.4f, 1.2f, 1.2f, 1.6f, 2.8f, 0f);
        }

        // ---------------------------------------------------------------- 135 Ovocuración
        // Refs: anime = resplandor amarillo cálido junto a la boca; juego = cáscaras de huevo blancas abiertas, orbe blanco y
        // destellos cian-blancos en espiral alrededor del usuario. Movimiento de estado (cura): sin estrella ni sacudida.
        static void M135(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.2f, 1.7f, 2.9f, AttackerChest, 0f);
            // El clip pinta un huevo y luego una explosión naranja enorme: el huevo que se abre propio lo sustituye.
            b.HideBaked();
            var egg = AttackerChest + ScreenRight * 0.8f + new Vector3(0f, 0.25f, 0f);
            EggCrack(b, egg, Mat(EggWhite, "Solid"), 0.35f, 1.15f, 2.3f, 0.75f);
            b.Cue(b.Ps("EggGlow", Mat(Sun, "Glow"), egg)
                .Life(1.0f).Size(1.1f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.7f, 1f, 0.85f, 1.2f, 1f, 0.6f))
                .AlphaOverLife(0f, 0f, 0.2f, 0.8f, 0.85f, 1f, 1f, 0f).Order(2), 0.4f);
            Flash(b, egg + new Vector3(0f, 0.2f, 0f), White, 1.15f, 2.0f);
            Projectile(b, egg + new Vector3(0f, 0.3f, 0f), AttackerChest + new Vector3(0f, 0.15f, 0f), White, 1.2f, 0.45f, 1.0f, 0.3f, "Glow");
            Sparkles(b, AttackerChest, 1.2f, 1.5f, 1.2f, 30f, 0.9f, new Color(0.5f, 1f, 1f), Color.white);
            Orbit(b, AttackerChest + new Vector3(0f, 0.1f, 0f), Cyan, 1.2f, 1.4f, 10, 1.0f, 4f, 0.5f, "Star", 15f);
            Aura(b, Attacker, Mint, 1.45f, 2.8f, 1.2f, 0f);
            GroundGlow(b, Attacker, Mint, 1.2f, 2.9f, 1.1f);
            Glints(b, AttackerChest, White, 1.3f, 1.4f, 1.2f, 14f);
            b.Light(egg + new Vector3(0.3f, 0.5f, -0.5f), Lc(1f, 0.95f, 0.75f), 8f, 0f, 0f, 0.4f, 1f, 1.15f, 2.4f, 1.8f, 1.4f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 136 Patada Salto Alta
        // Refs: anime = pierna con un resplandor blanco alargado; juego = estallido de púas blanco-cian desde el suelo bajo el rival.
        static void M136(EmeraldMoveBuilder b)
        {
            b.Keys(1.05f, 1.35f, 1.5f, 2.5f, KickHit, 1f);
            // El clip pinta un arco azul corto y golpea antes: el salto alto propio lo sustituye.
            b.HideBaked();
            Dust(b, Attacker, 0.55f, 0.7f, 6, 0.6f);
            Streaks(b, Attacker, Attacker + new Vector3(0f, 3.5f, 0f), White, 0.5f, 0.3f, 50f, 0.6f, 1.2f, 0.3f);
            Projectile(b, AttackerChest + new Vector3(0.3f, 0.2f, 0f), KickHit + new Vector3(-0.1f, 0.1f, 0f), White, 0.75f, 0.6f, 1.0f, 0.6f, "Glow", new Vector3(0f, 2.6f, 0f));
            ImpactStar(b, KickHit, White, 1.35f, 5.0f, 10, 34);
            Petals(b, Target, Wind, 1.36f, 18, 0.7f, 9f, 1.4f);
            Shockwave(b, Target, Cream, 1.37f, 4.5f, true);
            HitShell(b, Target, White, 1.35f, 0.45f);
            b.Light(KickHit + new Vector3(-0.6f, 0.6f, 0f), Lc(0.9f, 0.95f, 1f), 11f, 0f, 0f, 0.75f, 0.8f, 1.33f, 1.5f, 1.37f, 6f, 1.8f, 1.6f, 2.5f, 0f);
            b.Shake(1.35f, 0.6f, 0.5f);
            b.SlowMo(1.36f, 0.25f, 0.1f);
        }

        // ---------------------------------------------------------------- 137 Deslumbrar
        // Refs: anime = ojos naranja-rojos brillantes; juego = dos ojos con destellos blancos de cuatro puntas y halo rojo.
        // Descripción: dos focos rojizos y anillos que inmovilizan. Movimiento de estado (paraliza): sin estrella ni sacudida.
        static void M137(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.05f, 1.5f, 2.6f, TargetChest, 0.1f);
            // El clip pinta pinchos negro-rojos grandes alrededor del usuario: los focos propios lo sustituyen.
            b.HideBaked();
            foreach (int s in new[] { -1, 1 })
            {
                var eye = Eyes + ScreenRight * (0.45f * s) + new Vector3(0f, 0.05f, 0f);
                b.Cue(b.Ps("EyeGlow", Mat(GlareRed, "Glow"), eye)
                    .Life(1.7f).Size(0.85f).Burst(1).SizeOverLife(C(0f, 0.2f, 0.12f, 1.1f, 0.25f, 1f, 0.85f, 1f, 1f, 0.5f))
                    .AlphaOverLife(0f, 0f, 0.08f, 1f, 0.85f, 1f, 1f, 0f).Order(7), 0.4f);
                b.Cue(b.Ps("EyeFlare", Mat(GlareStar, "Star"), eye)
                    .Life(1.5f).Size(1.7f).Burst(1).Rotation(0f, 0f).Spin(30f * s, 30f * s)
                    .SizeOverLife(C(0f, 0.1f, 0.1f, 1.15f, 0.2f, 0.9f, 0.85f, 1f, 1f, 0.3f))
                    .AlphaOverLife(0f, 0f, 0.08f, 1f, 0.85f, 1f, 1f, 0f).Order(8), 0.5f);
            }
            CutLine(b, Eyes + new Vector3(0f, 0.02f, 0f), White, 0.55f, 90f, 3.6f, 0.14f, 0.7f);
            RingStream(b, Eyes + ScreenRight * 0.3f, TargetHead, GlareRed, 0.75f, 0.6f, 7f, 0.4f, 0.5f, 1.6f);
            SpinArc(b, TargetChest + new Vector3(0f, 0.15f, 0f), GlareRed, 1.05f, 2.5f, 0.95f, 0.5f, 420f, 10f);
            SpinArc(b, TargetChest + new Vector3(0f, -0.35f, 0f), GlareRed, 1.1f, 2.5f, 1.05f, 0.5f, -360f, -8f);
            Crackle(b, TargetChest, Electric, 1.1f, 1.3f, 0.8f, 40f);
            HitShell(b, Target, Electric, 1.05f, 0.7f);
            b.Light(Eyes + new Vector3(0.4f, 0.3f, -0.5f), Lc(1f, 0.25f, 0.2f), 8f, 0f, 0f, 0.45f, 1.6f, 1.6f, 1.6f, 2.2f, 0f);
        }

        // ---------------------------------------------------------------- 138 Comesueños
        // Refs: anime = remolinos de luz cian pálida que barren la pantalla; juego = estallido de jirones rosa-blancos sobre el rival
        // dormido con pompas encima. Descripción: motas violetas vuelven del rival al usuario.
        static void M138(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.0f, 1.5f, 2.7f, TargetChest, 0.6f);
            // El clip pinta un arco magenta del rival al usuario: el drenaje propio lo sustituye.
            b.HideBaked();
            Bubbles(b, TargetHead + new Vector3(0f, 0.25f, 0f), TargetHead + new Vector3(0.3f, 1.6f, 0f), White, 0.2f, 1.0f, 6f, 1.0f, 0.15f, 0.35f, 0.2f);
            b.Cue(b.Ps("DreamBurst", Mat(Pink, "Glow"), TargetChest)
                .Life(0.7f).Size(3.0f).Burst(1).SizeOverLife(C(0f, 0.4f, 0.2f, 1f, 1f, 1.15f))
                .AlphaOverLife(0f, 1f, 0.5f, 0.8f, 1f, 0f).Order(3), 0.95f);
            Rays(b, TargetChest, Pink, 1.0f, 16, 4.5f, 0.45f);
            Sparkles(b, TargetChest, 1.0f, 0.9f, 1.0f, 30f, 1.0f, new Color(1f, 0.6f, 0.85f), Color.white);
            DrainMotes(b, TargetChest, AttackerChest + new Vector3(0.2f, 0.2f, 0f), DreamViolet, 1.1f, 1.1f, 40f, 0.65f, 2.3f, "Glow", 0.6f);
            DrainMotes(b, TargetChest, AttackerChest + new Vector3(0.2f, 0.2f, 0f), Pink, 1.15f, 1.0f, 20f, 0.6f, 1.2f, "Star", 0.5f);
            Aura(b, Attacker, Lilac, 1.6f, 2.7f, 1.2f, 0f);
            Glints(b, AttackerChest, Pink, 1.6f, 1.0f, 1.0f, 14f);
            HitShell(b, Target, Pink, 1.0f, 0.5f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(1f, 0.7f, 0.95f), 9f, 0f, 0f, 0.95f, 0f, 1.0f, 3f, 1.6f, 1.5f, 2.7f, 0f);
            b.Shake(1.0f, 0.25f, 0.3f);
        }

        // ---------------------------------------------------------------- 139 Gas Venenoso
        // Refs: anime = nubes verde oliva oscuras alrededor del usuario; juego = bocanadas violeta-magenta que cruzan hasta el rival.
        // Descripción: gas oliva que se expande por el campo. Movimiento de estado (envenena): sin estrella ni sacudida.
        static void M139(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.8f, 3.0f, TargetChest, 0.1f);
            // El clip pinta calaveras verdes y un destello rosa: el gas propio lo sustituye.
            b.HideBaked();
            var dust = BotwMaterials.Get("EM_Dust");
            var poison = BotwMaterials.Get("EM_PoisonSmoke");
            var olive = new Color(0.62f, 0.62f, 0.36f);
            // Oliva en la boca que vira a violeta al llegar al rival.
            var gas = Grad(new[] { 0f, 0.62f, 0.62f, 0.36f, 0.5f, 0.62f, 0.55f, 0.42f, 1f, 0.8f, 0.35f, 0.95f }, new[] { 0f, 0.6f, 0.1f, 1f, 0.8f, 1f, 1f, 0f });
            ConeSpray(b, "GasCone", Mouth + new Vector3(0.2f, -0.1f, 0f), TargetChest + new Vector3(0f, 0.2f, 0f), dust, 0.5f, 1.1f, 28f, 16f, 0.6f, 2.4f, 1.0f, gas, 0f, 25f, 2);
            Tint(b.Cue(b.Ps("GasPuff", dust, Mouth + new Vector3(0.3f, 0f, 0f))
                .Sphere(0.5f).Speed(0.8f, 2.0f).Drag(2f).Life(1.2f, 1.8f).Size(0.9f, 1.6f).Burst(8).Velocity(new Vector3(0.4f, 0.3f, 0f))
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.25f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.5f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1), 0.45f), olive);
            Tint(Fog(b, Attacker + new Vector3(0.6f, 0f, 0.5f), dust, 0.4f, 1.1f, 1.2f, 6f, 1.1f, 0.8f, 1.6f), olive);
            Tint(Fog(b, Target, poison, 1.3f, 1.5f, 1.2f, 12f, 1.3f, 0.6f, 1.8f), new Color(0.95f, 0.5f, 1f));
            b.Cue(b.Ps("GasBubbles", BotwMaterials.Get("EM_Bubble"), Target + new Vector3(0f, 0.4f, 0f))
                .Sphere(0.6f).Velocity(new Vector3(0f, 0.9f, 0f)).Speed(0.1f, 0.5f).Life(0.6f, 1f).Size(0.14f, 0.3f)
                .Rate(16f).Duration(1.2f).SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 1.4f);
            HitShell(b, Target, Violet, 1.3f, 0.6f);
            b.Light(MidField + new Vector3(-0.5f, 1f, -0.5f), Lc(0.8f, 0.75f, 0.85f), 8f, 0f, 0f, 0.5f, 0.6f, 1.3f, 1.2f, 3.0f, 0f);
        }

        // ---------------------------------------------------------------- 140 Bombardeo
        // Refs: anime = sin captura; juego = bola gris pequeña que vuela en arco corto hacia el rival. Descripción: varias esferas
        // tipo semilla en arcos cortos (de 2 a 5 impactos).
        static void M140(EmeraldMoveBuilder b)
        {
            var hits = new[] { 1.15f, 1.4f, 1.65f, 1.9f };
            const float travel = 0.4f;
            var launches = new float[hits.Length];
            for (int i = 0; i < hits.Length; i++)
                launches[i] = hits[i] - travel;
            b.Keys(0.95f, 1.15f, 1.65f, 2.6f, Hit, 0.6f);
            // El clip pinta bolas blanco-negras: las bolas grises propias lo sustituyen.
            b.HideBaked();
            var from = AttackerChest + new Vector3(0.45f, 0.35f, 0f);
            var balls = Lob(b, BotwMeshes.Sphere, Mat(BallGrey, "Solid"), from, Hit, launches[0], travel, Vector3.one * 0.42f, 0.7f, 200f, White, 0.3f);
            balls.Sphere(0.15f);
            Bursts(balls, 1, launches);
            b.Cue(Bursts(b.Ps("Muzzle", Mat(Cream, "Glow"), from).Life(0.14f).Size(0.9f)
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 0.8f)).AlphaOverLife(0f, 1f, 1f, 0f).Order(6), 1, launches), launches[0]);
            HitStars(b, Hit, Cream, hits, 2.7f, 8, 0.35f);
            b.Cue(Bursts(b.Ps("HitDust", BotwMaterials.Get("EM_Dust"), Hit)
                .Sphere(0.3f).Speed(1.5f, 3f).Drag(3f).Life(0.5f, 0.8f).Size(0.5f, 0.8f)
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.1f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.4f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1), 5, hits), hits[0]);
            HitShell(b, Target, Cream, hits[0], 0.9f);
            b.Light(Hit + new Vector3(-0.8f, 0.6f, 0f), Lc(1f, 0.9f, 0.7f), 8f, 0f, 0f, 1.13f, 0f, 1.16f, 2.5f, 1.4f, 2.5f, 1.9f, 2.5f, 2.5f, 0f);
            foreach (float t in hits)
                b.Shake(t, 0.18f, 0.15f);
        }
    }
}
