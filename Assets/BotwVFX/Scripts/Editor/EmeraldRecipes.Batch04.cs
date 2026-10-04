using UnityEngine;
using static BotwVfx.EditorTools.EmeraldLayers;
using static BotwVfx.EditorTools.EmeraldMoveBuilder;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Lote 4 (movimientos 061-080). Mismo criterio que los lotes 1-3 (referencias del anime y del juego, resumidas
    /// en el comentario de cada receta) y, además, prioridad al volumen: la masa del efecto en su pico se escala
    /// para cubrir en pantalla lo que cubren las referencias (vista de combate en tres cuartos).
    /// Movimientos de estado sin estrella de impacto, cámara lenta ni sacudida fuerte: 073, 074, 077, 078, 079.
    /// Presupuesto: como mucho ~11 sistemas Shuriken por movimiento (≤ 600 KB por prefab).
    /// </summary>
    public static partial class EmeraldRecipes
    {
        // Barrido bajo (pies del rival).
        static readonly Vector3 LowHit = new Vector3(2.62f, 0.3f, -0.12f);
        // Semilla de Drenadoras (anime): centro naranja-marrón y borde amarillo brillante.
        static readonly Pal Seed = new Pal("Seed", new Color(0.75f, 0.3f, 0.06f), new Color(2.7f, 2.1f, 0.35f));
        // Pompas de Rayo Burbuja: interior azul claro y borde azul (no blancas).
        static readonly Pal BubbleBlue = new Pal("BubbleBlue", new Color(0.8f, 1.7f, 2.6f), new Color(0.1f, 0.55f, 1.9f));
        // Cintas de Rayo Aurora: núcleos con color (no blancos) para que se distingan las tres bandas.
        static readonly Pal AuroraCyan = new Pal("AuroraCyan", new Color(0.9f, 2.4f, 2.8f), new Color(0.05f, 1.0f, 2.2f));
        static readonly Pal AuroraPink = new Pal("AuroraPink", new Color(2.6f, 0.9f, 1.9f), new Color(1.8f, 0.15f, 1.1f));
        static readonly Pal AuroraYellow = new Pal("AuroraYellow", new Color(2.7f, 2.5f, 0.7f), new Color(1.6f, 1.3f, 0.05f));

        // ---------------------------------------------------------------- 061 Rayo Burbuja
        // Refs: anime = torrente de pompas grandes translúcidas con brillo y rayas de velocidad blancas;
        // juego = sarta de pompas cian a lo largo de un haz fino blanco-cian.
        static void M061(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.3f, 1.6f, 2.4f, Target, 0.7f);
            b.Baked(1.3f); // la sarta de pompas azules del clip es el juego
            var from = Mouth + new Vector3(0.25f, -0.05f, 0f);
            var to = Hit + new Vector3(0.3f, 0.1f, 0f);
            Charge(b, from, Water, 0.45f, 0.45f, 0.9f, 0.7f, false);
            Beam(b, from, Hit, Water, 0.9f, 1.75f, 0.16f, 0.2f);
            // Pompas medianas densas y pompas grandes del anime (crecen hacia el rival).
            Bubbles(b, from, to, BubbleBlue, 0.9f, 0.85f, 50f, 0.5f, 0.4f, 1.4f, 0.5f);
            Bubbles(b, from + new Vector3(0.3f, 0.1f, 0f), to, BubbleBlue, 0.95f, 0.75f, 16f, 0.55f, 0.9f, 2.8f, 1.0f);
            Streaks(b, from + new Vector3(0.4f, 0f, 0f), Hit, White, 0.9f, 0.8f, 55f, 0.9f, 1.1f, 0.3f);
            BubblePops(b, Hit, BubbleBlue, new[] { 1.3f, 1.45f, 1.6f }, 1.6f, 12);
            Splash(b, Hit, Water, 1.3f, 1.3f, 26);
            HitShell(b, Target, Water, 1.3f, 0.5f);
            b.Light(Hit + new Vector3(-0.8f, 0.4f, 0f), Lc(0.6f, 0.85f, 1f), 10f, 0f, 0f, 0.9f, 1.2f, 1.3f, 3.5f, 1.7f, 1.4f, 2.4f, 0f);
            b.Shake(1.3f, 0.22f, 0.3f);
        }

        // ---------------------------------------------------------------- 062 Rayo Aurora
        // Refs: anime = cintas pastel (blanco, cian, rosa, amarillo) que se entrelazan en arco;
        // juego = haz blanco con flecos arcoíris rosa-violeta-cian y chispas amarillo-verdes en el usuario.
        static void M062(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.6f, 2.5f, Target, 0.8f);
            // El clip pinta un tubo arcoíris fino y muy saturado: las tres cintas pastel propias lo sustituyen.
            b.HideBaked();
            var from = Mouth + new Vector3(0.25f, -0.05f, 0f);
            Charge(b, from, Cyan, 0.3f, 0.6f, 1.1f, 0.9f, false);
            Beam(b, from, Hit, White, 0.9f, 1.95f, 0.45f, 0.15f);
            Ribbon(b, from, Hit, AuroraCyan, 0.9f, 1.95f, 0.42f, 0.95f, 1.5f, 0f, 0f);
            Ribbon(b, from, Hit, AuroraPink, 0.93f, 1.95f, 0.4f, 0.95f, 1.5f, 2.1f, 60f);
            Ribbon(b, from, Hit, AuroraYellow, 0.96f, 1.95f, 0.36f, 0.95f, 1.5f, 4.2f, 120f);
            var colors = new[] { new Color(1f, 0.6f, 0.9f), new Color(0.6f, 1f, 1f), new Color(1f, 1f, 0.5f), new Color(0.7f, 1f, 0.5f) };
            Sparkles(b, (from + Hit) * 0.5f + new Vector3(0f, 0.3f, 0f), 0.9f, 1.0f, 2.2f, 34f, 1.3f, colors);
            ImpactStar(b, Hit, Cyan, 1.3f, 4.4f, 12, 0);
            IceBurst(b, Hit, 1.32f, 1.3f, 12);
            GroundGlow(b, Target, Cyan, 1.3f, 2.5f, 2.2f);
            HitShell(b, Target, Ice, 1.3f, 0.6f);
            b.Light(Hit + new Vector3(-0.9f, 0.4f, 0f), Lc(0.8f, 0.95f, 1f), 11f, 0f, 0f, 0.9f, 1.8f, 1.32f, 4.5f, 1.8f, 1.8f, 2.5f, 0f);
            b.Shake(1.3f, 0.28f, 0.3f);
        }

        // ---------------------------------------------------------------- 063 Hiperrayo
        // Refs: anime = haz blanco enorme con bordes rosa-rojos que llena la pantalla;
        // juego = haz amarillo-blanco gigante con resplandor naranja y rayas de color.
        static void M063(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.3f, 1.7f, 2.7f, Target, 1f);
            b.Baked(1.3f, 0.85f); // el tubo blanco-rosa grueso del clip es el anime; el núcleo propio lo hace más ancho
            var from = Mouth + new Vector3(0.3f, -0.05f, 0f);
            var to = Hit + new Vector3(0.4f, 0f, 0f);
            Charge(b, from, HyperWarm, 0.25f, 0.65f, 1.6f, 1.5f, false);
            Beam(b, from, to, HyperWarm, 0.9f, 2.1f, 1.15f, 0.1f);
            var glow = Grad(new[] { 0f, 1f, 1f, 1f, 1f, 1f, 0.8f, 0.65f }, new[] { 0f, 0.8f, 0.15f, 1f, 0.8f, 0.9f, 1f, 0f });
            ConeSpray(b, "BeamGlow", from, to, Mat(HyperWarm, "Glow"), 0.92f, 1.05f, 45f, 7f, 1.3f, 3.2f, 0.28f, glow, 0f, 0f, 5);
            Streaks(b, from + new Vector3(0.3f, 0f, 0f), to, White, 0.92f, 1.0f, 70f, 1.5f, 1.6f, 0.22f);
            RingStream(b, from, to, HyperWarm, 0.95f, 0.95f, 7f, 0.3f, 1.6f, 3.2f);
            ImpactStar(b, Hit, HyperWarm, 1.3f, 6.2f, 14, 0);
            Smoke(b, Target + new Vector3(0f, 0.7f, 0f), BotwMaterials.Get("EM_Dust"), 1.42f, 14, 1.7f, 0.5f, 1.3f, 1.6f);
            Shockwave(b, Target, HyperWarm, 1.32f, 5.8f);
            HitShell(b, Target, HyperWarm, 1.3f, 0.8f);
            b.Light(Hit + new Vector3(-1.2f, 0.5f, 0f), Lc(1f, 0.8f, 0.6f), 14f, 0f, 0f, 0.4f, 1.2f, 0.95f, 3.5f, 1.32f, 7f, 2.0f, 3f, 2.7f, 0f);
            b.Shake(0.92f, 0.12f, 1.1f);
            b.Shake(1.3f, 0.5f, 0.5f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 064 Picotazo
        // Refs: anime = picotazo con rayas de velocidad horizontales; juego = púa de luz blanca que baja sobre el rival y destellos.
        static void M064(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.15f, 1.45f, 2.2f, Target, 0.6f);
            b.Baked(1.15f); // la púa blanca del clip es el juego
            var hits = new[] { 1.15f, 1.38f };
            Streaks(b, AttackerChest + new Vector3(0.5f, 0.1f, 0f), Hit, White, 0.68f, 0.57f, 70f, 0.8f, 1.3f, 0.22f);
            SpeedLines(b, Hit, White, 0.9f, 0.5f, 3.4f, 90f, 1.2f);
            // Púa de luz casi vertical por picotazo: su punta inferior cae en el rival.
            b.Cue(Bursts(b.Ps("PeckSpike", Mat(White, "Spark"), Hit + new Vector3(-0.35f, 1.35f, 0f))
                .Life(0.24f).Size3D(new Vector3(0.5f, 3.4f, 1f)).Rotation(-24f, -14f)
                .SizeOverLife(C(0f, 0.3f, 0.2f, 1f, 1f, 1.05f)).AlphaOverLife(0f, 1f, 0.5f, 0.9f, 1f, 0f).Order(8), 1, hits), hits[0]);
            HitStars(b, Hit, White, hits, 3.0f, 12, 0.2f);
            Glints(b, Hit, White, 1.2f, 0.7f, 1.1f, 22f);
            Dust(b, Target, 1.16f, 0.9f, 10);
            HitShell(b, Target, White, 1.15f, 0.5f);
            b.Light(Hit + new Vector3(-0.6f, 0.5f, 0f), Lc(0.9f, 0.95f, 1f), 8f, 0f, 0f, 1.13f, 3f, 1.25f, 1f, 1.37f, 3f, 1.7f, 1f, 2.2f, 0f);
            foreach (float t in hits)
                b.Shake(t, 0.16f, 0.15f);
        }

        // ---------------------------------------------------------------- 065 Pico Taladro
        // Refs: anime = espiral blanca de taladro alrededor del pico y rayas radiales azul-blancas;
        // juego = espiral cian-blanca con destellos y rayas blancas radiales sobre el rival.
        static void M065(EmeraldMoveBuilder b)
        {
            b.Keys(0.8f, 1.3f, 1.75f, 2.5f, Target, 0.9f);
            b.Baked(1.3f); // el taladro azul con espiral blanca del clip es el anime
            var from = AttackerChest + new Vector3(0.6f, 0.3f, 0f);
            var tip = Hit + new Vector3(-0.3f, 0.1f, 0f);
            // Estela helicoidal (descripción): espiral de viento que gira sobre el eje de avance.
            var pivot = b.Node("DrillHelix", from);
            pivot.localRotation = Quaternion.FromToRotation(Vector3.up, (tip - from).normalized);
            var helix = b.MeshPart("HelixMesh", Helix, Mat(Wind, "Stripe"), new Vector3(0f, -0.6f, 0f), Vector3.zero, new Vector3(0.7f, 1.9f, 0.7f), pivot, 5);
            var spin = b.Track(helix.transform, helix, 0.85f, 1.75f);
            spin.spin = new Vector3(0f, -1500f, 0f);
            spin.property = "_Erosion";
            spin.propertyCurve = C(0.85f, 0.5f, 0.92f, 0f, 1.55f, 0.1f, 1.75f, 1f);
            var move = b.Track(pivot, null, 0.85f, 1.75f);
            move.posFrom = from;
            move.posTo = tip;
            move.posCurve = C(0.85f, 0f, 1.3f, 1f, 1.75f, 1.03f);
            Projectile(b, from, tip, Wind, 0.85f, 0.45f, 0.5f, 0.6f, "Glow");
            SpeedLines(b, tip, White, 0.95f, 0.8f, 3.8f, 110f, 1.3f);
            ImpactStar(b, tip, Cyan, 1.3f, 4.4f, 14, 0);
            // Rozamiento del taladro: chispas cian-blancas continuas en la punta.
            b.Cue(b.Ps("DrillSparks", Mat(Cyan, "Spark"), tip)
                .Sphere(0.3f).Speed(6f, 12f).Drag(3f).Gravity(0.6f).Life(0.15f, 0.4f).Size(0.07f, 0.13f).Rate(150f).Duration(0.45f)
                .Stretch(6f, 0.05f).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(6), 1.3f);
            RingPulses(b, tip, Wind, 1.3f, 3, 0.12f, 3.4f);
            Glints(b, tip, White, 1.35f, 0.5f, 1.0f, 26f);
            Dust(b, Target, 1.32f, 1.0f, 12);
            HitShell(b, Target, Cyan, 1.3f, 0.5f);
            b.Light(tip + new Vector3(-0.6f, 0.4f, 0f), Lc(0.75f, 0.92f, 1f), 10f, 0f, 0f, 0.9f, 1f, 1.32f, 4.5f, 1.7f, 2f, 2.5f, 0f);
            b.Shake(1.3f, 0.32f, 0.4f);
            b.SlowMo(1.31f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 066 Sumisión
        // Refs: anime = el usuario agarra al rival y vuela con él entre rayas de movimiento;
        // juego = orbe blanco enorme que envuelve al rival y arco blanco que lo rodea.
        static void M066(EmeraldMoveBuilder b)
        {
            b.Keys(0.95f, 1.4f, 1.65f, 2.6f, Target, 1f); // anticipación = la presa (orbe y arco)
            b.Baked(1.4f); // el orbe blanco sobre el rival y el polvo del derribo del clip encajan con el juego
            Streaks(b, AttackerChest + new Vector3(0.5f, 0.2f, 0f), TargetChest, White, 0.5f, 0.45f, 50f, 0.9f, 1.3f, 0.25f);
            // Presa: orbe blanco enorme que envuelve al rival.
            b.Cue(b.Ps("GrabOrb", Mat(White, "Glow"), TargetChest + new Vector3(-0.2f, 0.25f, 0f))
                .Life(0.6f).Size(3.8f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 0f, 0.15f, 0.9f, 0.75f, 0.9f, 1f, 0f).Order(5), 0.75f);
            // Vueltas de la presa: arco blanco inclinado que gira alrededor del rival y media luna que barre.
            SpinArc(b, TargetChest + new Vector3(0f, 0.25f, 0f), White, 0.8f, 1.42f, 1.8f, 0.55f, -720f, 70f);
            Slash(b, TargetChest + new Vector3(-0.2f, 0.35f, 0f), White, 0.88f, 120f, 2.1f, 0.4f, -500f);
            // Derribo.
            ImpactStar(b, Target + new Vector3(-0.3f, 0.5f, -0.1f), Cream, 1.4f, 5.4f, 12, 30);
            Shockwave(b, Target, Cream, 1.42f, 4.8f);
            Dust(b, Target, 1.42f, 1.4f, 18, 1.45f);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.42f, 12, 7f);
            HitShell(b, Target, White, 0.85f, 0.75f);
            b.Light(TargetChest + new Vector3(-1f, 0.5f, 0f), Lc(1f, 0.95f, 0.85f), 11f, 0f, 0f, 0.8f, 2.5f, 1.3f, 1.5f, 1.42f, 5f, 1.9f, 1.5f, 2.6f, 0f);
            b.Shake(1.4f, 0.45f, 0.45f);
            b.SlowMo(1.41f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 067 Patada Baja
        // Refs: anime = patada a ras de suelo sin efecto; juego = media luna blanca baja que barre los pies del rival.
        static void M067(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.3f, 1.45f, 2.3f, Target, 0.7f);
            b.Baked(1.3f); // el polvo naranja a ras de suelo del clip encaja
            Streaks(b, AttackerFoot + new Vector3(0.3f, -0.15f, 0f), LowHit, White, 1.0f, 0.3f, 60f, 0.35f, 1.1f, 0.22f);
            // Barrido bajo: media luna casi horizontal y arco que rodea los pies.
            Slash(b, LowHit + new Vector3(0f, 0.15f, 0f), White, 1.2f, -100f, 1.9f, 0.35f, -350f);
            SpinArc(b, Target + new Vector3(0f, 0.22f, 0f), White, 1.2f, 1.5f, 1.25f, 0.3f, -1100f);
            ImpactStar(b, LowHit + new Vector3(0f, 0.15f, 0f), Orange, 1.3f, 3.6f, 10, 24);
            Shockwave(b, Target, Orange, 1.32f, 3.4f);
            Dust(b, Target, 1.31f, 1.3f, 16, 1.3f);
            Debris(b, Target + new Vector3(0f, 0.2f, 0f), 1.31f, 10, 6f);
            HitShell(b, Target, Orange, 1.3f, 0.4f);
            b.Light(LowHit + new Vector3(-0.6f, 0.5f, 0f), Lc(1f, 0.8f, 0.55f), 8f, 0f, 0f, 1.27f, 0f, 1.32f, 4f, 1.6f, 1.3f, 2.3f, 0f);
            b.Shake(1.3f, 0.3f, 0.3f);
        }

        // ---------------------------------------------------------------- 068 Contraataque
        // Refs: anime = el usuario brilla con contorno rosa-rojo y rayos blancos; juego = el usuario se tiñe de rojo y brotan
        // columnas de luz blanca con nubes sobre el rival.
        static void M068(EmeraldMoveBuilder b)
        {
            b.Keys(0.72f, 1.4f, 1.65f, 2.6f, Target, 1f); // anticipación = el golpe recibido
            // Clip: golpe blanco en el usuario, aura roja y respuesta con columna roja y nube blanca en el rival
            // (0,73 s después): el golpe recibido cae en 0,67 s y la respuesta en 1,4 s.
            b.Baked(0.67f);
            b.Cue(b.Ps("ReceivedHit", Mat(White, "Star"), AttackerChest + new Vector3(0.3f, 0.2f, -0.2f))
                .Life(0.16f).Size(2.0f).Burst(1).Rotation(0f, 45f).AlphaOverLife(0f, 1f, 1f, 0f).Order(9), 0.64f);
            Aura(b, Attacker, Red, 0.72f, 1.5f, 1.2f, 45f);
            Streaks(b, AttackerChest + new Vector3(0.5f, 0.1f, 0f), Hit, Red, 1.18f, 0.25f, 70f, 0.5f, 1.3f, 0.22f);
            ImpactStar(b, Hit, Red, 1.4f, 5.4f, 14, 34);
            // Columnas de luz blanca que brotan del suelo alrededor del rival.
            b.Cue(b.Ps("LightColumns", Mat(White, "Spark"), Target + new Vector3(0f, 0.1f, 0f))
                .GroundCircle(1.2f, 0.4f).Velocity(new Vector3(0f, 10f, 0f)).Life(0.3f, 0.45f).Size(0.4f, 0.65f).Burst(7)
                .Stretch(5f, 0.05f).AlphaOverLife(0f, 1f, 0.6f, 0.9f, 1f, 0f).Order(6), 1.42f);
            Fx clouds;
            b.Cue(clouds = b.Ps("CounterClouds", BotwMaterials.Get("EM_Mist"), Target + new Vector3(0f, 1.8f, 0f))
                .Sphere(1.1f).Speed(0.8f, 2.2f).Drag(2.2f).Life(1.1f, 1.6f).Size(1.2f, 2.1f).Burst(10)
                .Velocity(new Vector3(0f, 0.9f, 0f)).SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.45f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1), 1.45f);
            // EM_Mist tiene sombra azulada: nubes del juego blancas con sombra gris.
            Tint(clouds, new Color(1f, 0.95f, 0.86f));
            HitShell(b, Target, Red, 1.4f, 0.5f);
            b.Light(Hit + new Vector3(-0.8f, 0.5f, 0f), Lc(1f, 0.55f, 0.45f), 11f, 0f, 0f, 0.67f, 1.5f, 1.0f, 1f, 1.42f, 5.5f, 1.9f, 1.8f, 2.6f, 0f);
            b.Shake(0.67f, 0.12f, 0.15f);
            b.Shake(1.4f, 0.45f, 0.45f);
            b.SlowMo(1.41f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 069 Sísmico
        // Refs: anime = el usuario gira con el rival entre rayas curvas blancas; juego = el rival sube muy alto (vuelta al planeta) y cae.
        static void M069(EmeraldMoveBuilder b)
        {
            b.Keys(0.95f, 1.48f, 1.7f, 2.7f, Target, 1f); // anticipación = el rival sube
            // Clip: destello al levantar al rival y gran polvo con aro al estrellarlo (0,88 s después).
            b.Baked(0.6f);
            // Subida en remolino: arcos inclinados que giran y rayas hacia arriba.
            SpinArc(b, Target + new Vector3(0f, 1.1f, 0f), White, 0.6f, 1.3f, 1.8f, 0.8f, -900f, 25f);
            SpinArc(b, Target + new Vector3(0f, 2.5f, 0f), Wind, 0.75f, 1.35f, 1.5f, 0.7f, 1000f, -20f);
            Streaks(b, Target + new Vector3(0f, 0.3f, 0f), Target + new Vector3(0f, 4.2f, 0f), White, 0.6f, 0.6f, 40f, 0.8f, 1.2f, 0.3f);
            // Caída: rayas hacia abajo y sombra que crece.
            Streaks(b, Target + new Vector3(0f, 4.6f, 0f), Target + new Vector3(0f, 0.6f, 0f), White, 1.22f, 0.26f, 80f, 0.8f, 1.4f, 0.2f);
            GroundShadow(b, Target, 0.9f, 1.48f, 0.6f, 1.9f);
            ImpactStar(b, Target + new Vector3(-0.2f, 0.4f, -0.1f), Cream, 1.48f, 5.8f, 14, 30);
            Shockwave(b, Target, Cream, 1.5f, 5.6f, true);
            Dust(b, Target, 1.5f, 1.6f, 22, 1.6f);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.5f, 16, 9f);
            b.Light(Target + new Vector3(-1f, 0.8f, 0f), Lc(1f, 0.9f, 0.7f), 11f, 0f, 0f, 0.6f, 1.2f, 1.2f, 0.5f, 1.5f, 5.5f, 2.0f, 1.6f, 2.7f, 0f);
            b.Shake(1.48f, 0.55f, 0.5f);
            b.SlowMo(1.49f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 070 Fuerza
        // Refs: anime = orbe amarillo brillante en la mano; juego = columna de energía naranja-dorada que envuelve
        // al usuario, aro dorado en el suelo y rayos de luz hacia arriba.
        static void M070(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.3f, 1.55f, 2.5f, Target, 1f);
            // El clip pinta un aro rojo en el suelo del usuario: el juego lo pinta naranja-dorado.
            b.HideBaked();
            Aura(b, Attacker, Orange, 0.2f, 1.15f, 1.35f, 60f);
            Beam(b, Attacker + new Vector3(0f, 0.05f, 0f), Attacker + new Vector3(0f, 4.6f, 0f), Orange, 0.25f, 1.15f, 1.5f, 0.25f);
            GroundGlow(b, Attacker, Orange, 0.25f, 1.2f, 2.4f);
            Charge(b, Fist + new Vector3(0.2f, 0.2f, 0f), Gold, 0.45f, 0.7f, 1.0f, 0.9f, false);
            // Empuje.
            Streaks(b, Fist, Hit, Gold, 1.12f, 0.2f, 70f, 0.5f, 1.3f, 0.2f);
            ImpactStar(b, Hit, Orange, 1.3f, 5.4f, 14, 36);
            Dust(b, Target, 1.32f, 1.3f, 14, 1.3f);
            HitShell(b, Target, Gold, 1.3f, 0.45f);
            b.Light(AttackerChest + new Vector3(0.8f, 0.8f, -0.5f), Lc(1f, 0.8f, 0.45f), 9f, 0f, 0f, 0.25f, 2.5f, 1.1f, 2.5f, 1.25f, 0f);
            b.Shake(0.3f, 0.08f, 0.8f);
            b.Shake(1.3f, 0.42f, 0.4f);
            b.SlowMo(1.31f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 071 Absorber
        // Refs: anime = haces rojos que salen del rival hacia el usuario; juego = motas verdes con destellos que viajan
        // del rival al usuario y pompa cian-blanca alrededor del rival.
        static void M071(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.0f, 1.6f, 2.5f, Target, 0.4f); // impacto = empieza el drenaje en el rival
            b.Baked(1.4f); // las motas verdes del clip y el brillo final en el usuario encajan con el juego
            HitShell(b, Target, Wind, 0.75f, 1.0f);
            Flash(b, TargetChest + new Vector3(-0.2f, 0.2f, 0f), DrainGreen, 0.95f, 1.8f);
            DrainMotes(b, TargetChest + new Vector3(-0.3f, 0.2f, 0f), AttackerChest + new Vector3(0.4f, 0.25f, 0f), DrainGreen, 0.85f, 0.9f, 60f, 0.6f, 1.5f);
            DrainMotes(b, TargetChest + new Vector3(-0.3f, 0.3f, 0f), AttackerChest + new Vector3(0.4f, 0.3f, 0f), White, 0.9f, 0.85f, 22f, 0.6f, 1.5f, "Star", 0.7f);
            Aura(b, Attacker, DrainGreen, 1.4f, 2.4f, 1.05f, 30f);
            GroundGlow(b, Attacker, DrainGreen, 1.4f, 2.4f, 1.8f);
            Glints(b, AttackerChest + new Vector3(0f, 0.3f, 0f), DrainGreen, 1.45f, 0.8f, 1.0f, 18f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.6f, -0.5f), Lc(0.7f, 1f, 0.6f), 8f, 0f, 0f, 0.9f, 0.8f, 1.5f, 2.5f, 2.4f, 0f);
        }

        // ---------------------------------------------------------------- 072 Megaagotar
        // Refs: anime = brillo verde en cruz sobre la cabeza del rival; juego = resplandor verde-blanco grande en el rival
        // con muchos destellos que vuelven al usuario.
        static void M072(EmeraldMoveBuilder b)
        {
            b.Keys(0.65f, 1.0f, 1.7f, 2.6f, Target, 0.5f);
            b.Baked(1.45f); // los chorros de motas verdes y el halo del usuario del clip encajan con la descripción
            b.Cue(b.Ps("DrainGlow", Mat(DrainGreen, "Glow"), TargetChest + new Vector3(-0.2f, 0.25f, 0f))
                .Life(1.0f).Size(3.4f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.2f, 1f, 1f, 0.8f))
                .AlphaOverLife(0f, 0f, 0.1f, 1f, 0.7f, 0.8f, 1f, 0f).Order(5), 0.85f);
            HitShell(b, Target, DrainGreen, 0.9f, 0.8f);
            // Tres corrientes (descripción) que vuelven serpenteando al usuario.
            var to = AttackerChest + new Vector3(0.4f, 0.3f, 0f);
            DrainMotes(b, TargetChest + new Vector3(-0.3f, 0.6f, 0f), to, DrainGreen, 0.95f, 0.9f, 45f, 0.65f, 1.6f);
            DrainMotes(b, TargetChest + new Vector3(-0.3f, 0.1f, 0.3f), to, DrainGreen, 1.0f, 0.85f, 45f, 0.65f, 1.6f);
            DrainMotes(b, TargetChest + new Vector3(-0.3f, -0.2f, -0.3f), to, White, 1.05f, 0.8f, 30f, 0.65f, 1.6f, "Star", 0.6f);
            var colors = new[] { new Color(0.7f, 1f, 0.6f), new Color(1f, 1f, 1f) };
            Sparkles(b, TargetChest + new Vector3(-0.2f, 0.2f, 0f), 0.85f, 1.0f, 1.5f, 30f, 1.4f, colors);
            Aura(b, Attacker, DrainGreen, 1.3f, 2.6f, 1.25f, 40f);
            GroundGlow(b, Attacker, DrainGreen, 1.3f, 2.6f, 2.1f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.7f, 1f, 0.6f), 10f, 0f, 0f, 0.85f, 3f, 1.4f, 2f, 2.6f, 0f);
        }

        // ---------------------------------------------------------------- 073 Drenadoras
        // Refs: anime = semilla grande naranja-marrón con borde amarillo brillante y halo verde; juego = semillas naranjas
        // brillantes que caen sobre el rival y brotes verdes.
        // Movimiento de estado (planta semillas): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M073(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.0f, 1.4f, 2.6f, Target, 0.3f);
            b.Baked(1.0f); // las semillas con aro amarillo, los brotes y las motas que vuelven del clip encajan
            var from = AttackerHead + new Vector3(0.3f, 0.2f, 0f);
            Projectile(b, from, Target + new Vector3(-0.3f, 0.35f, -0.25f), Seed, 0.55f, 0.45f, 1.05f, 0.3f, "Glow", new Vector3(0f, 1.3f, 0f));
            Projectile(b, from + new Vector3(0f, 0.1f, 0.2f), Target + new Vector3(0.2f, 0.3f, 0.3f), Seed, 0.62f, 0.45f, 0.85f, 0.24f, "Glow", new Vector3(0f, 1.6f, 0f));
            Flash(b, Target + new Vector3(0f, 0.4f, 0f), Leaf, 1.0f, 1.6f);
            // Las lianas brotan alrededor del rival y lo envuelven.
            Vine(b, Target + new Vector3(-0.55f, 0.05f, -0.3f), Target + new Vector3(-0.1f, 2.6f, 0.1f), VineGreen, 1.0f, 1.25f, 2.4f, 0.2f, 0.45f, 0f);
            Vine(b, Target + new Vector3(0.5f, 0.05f, 0.35f), Target + new Vector3(0.05f, 2.7f, -0.1f), VineGreen, 1.05f, 1.3f, 2.4f, 0.2f, 0.45f, 1.7f);
            Vine(b, Target + new Vector3(0.45f, 0.05f, -0.45f), Target + new Vector3(-0.05f, 2.3f, 0.15f), VineGreen, 1.1f, 1.35f, 2.4f, 0.17f, 0.4f, 3.1f);
            Vine(b, Target + new Vector3(-0.4f, 0.05f, 0.45f), Target + new Vector3(0.1f, 2.4f, -0.15f), VineGreen, 1.08f, 1.32f, 2.4f, 0.17f, 0.4f, 4.4f);
            SwirlLeaves(b, Target, 1.05f, 1.2f, 1.0f, 2.2f, 28f);
            GroundGlow(b, Target, Leaf, 1.0f, 2.5f, 1.8f);
            DrainMotes(b, TargetChest + new Vector3(-0.3f, 0.3f, 0f), AttackerChest + new Vector3(0.4f, 0.3f, 0f), Leaf, 1.6f, 0.8f, 30f, 0.7f, 1.4f);
            Glints(b, AttackerChest + new Vector3(0f, 0.3f, 0f), Leaf, 2.1f, 0.5f, 0.9f, 16f);
            b.Light(Target + new Vector3(-0.8f, 1f, 0f), Lc(0.8f, 1f, 0.5f), 8f, 0f, 0f, 0.95f, 2f, 1.6f, 1.2f, 2.6f, 0f);
        }

        // ---------------------------------------------------------------- 074 Desarrollo
        // Refs: anime = chorros de energía verde con destellos de estrella; juego = orbes verdes que flotan alrededor del usuario.
        // Movimiento de estado (sube Ataque): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M074(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.0f, 1.6f, 2.6f, AttackerChest, 0f);
            // El clip pinta una estrella de impacto verde enorme y flechas de colores: no encaja con un movimiento de estado.
            b.HideBaked();
            Aura(b, Attacker, Leaf, 0.4f, 2.4f, 1.35f, 55f);
            GroundGlow(b, Attacker, Leaf, 0.45f, 2.5f, 1.7f);
            RisingArrows(b, Attacker, Leaf, 0.5f, 1.5f, 24f);
            b.Cue(b.Ps("GrowthOrbs", Mat(Leaf, "Glow"), AttackerChest + new Vector3(0f, 0.3f, 0f))
                .Sphere(1.5f, 0.4f).Velocity(new Vector3(0f, 0.45f, 0f)).Speed(0.05f, 0.25f).Life(0.8f, 1.2f).Size(0.25f, 0.5f)
                .Rate(16f).Duration(1.6f).SizeOverLife(C(0f, 0.2f, 0.25f, 1f, 0.8f, 1f, 1f, 0f))
                .AlphaOverLife(0f, 0f, 0.2f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 0.5f);
            var colors = new[] { new Color(0.8f, 1f, 0.5f), new Color(1f, 1f, 1f) };
            Sparkles(b, AttackerChest + new Vector3(0f, 0.4f, 0f), 0.6f, 1.5f, 1.4f, 26f, 1.2f, colors);
            // Pulso de crecimiento.
            RingPulses(b, AttackerChest + new Vector3(0f, 0.2f, 0f), Leaf, 1.0f, 3, 0.2f, 3.4f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(0.75f, 1f, 0.5f), 8f, 0f, 0f, 0.4f, 1f, 1.0f, 2.5f, 2.6f, 0f);
        }

        // ---------------------------------------------------------------- 075 Hoja Afilada
        // Refs: anime = hojas verdes grandes que cortan en primer plano; juego = hojitas amarillo-verdes que vuelan
        // hacia el rival y destellos verdes.
        static void M075(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.55f, 2.4f, Target, 0.6f);
            b.Baked(1.3f); // el abanico de hojas sobre el usuario del clip es la descripción
            var from = AttackerHead + new Vector3(0.4f, 0.5f, 0f);
            const float travel = 0.45f;
            // Hojas que giran, salen en abanico y se cierran sobre el rival (como Pin Misil).
            var leaves = b.Ps("RazorLeaves", BotwMaterials.Get("EM_Leaf"), from)
                .Sphere(0.9f, 0f).Velocity((Hit - from) / travel).Life(travel).Size(0.4f, 0.6f).Rotation(0f, 360f).Spin(-720f, 720f)
                .Rate(46f).Duration(0.55f).AlphaOverLife(0f, 0.5f, 0.12f, 1f, 1f, 1f).Order(6);
            var vel = leaves.ps.velocityOverLifetime;
            vel.radial = new UnityEngine.ParticleSystem.MinMaxCurve(-0.9f / travel);
            // EM_Leaf es verde muy claro (HDR): las referencias son hojas verde intenso.
            Tint(leaves, new Color(0.7f, 0.9f, 0.5f));
            b.Cue(leaves, 0.85f);
            var big = b.Ps("RazorLeavesBig", BotwMaterials.Get("EM_Leaf"), from + new Vector3(0.3f, 0f, -0.3f))
                .Sphere(1.3f, 0f).Velocity((Hit - from) / travel).Life(travel).Size(0.8f, 1.1f).Rotation(0f, 360f).Spin(-400f, 400f)
                .Rate(12f).Duration(0.5f).AlphaOverLife(0f, 0.5f, 0.12f, 1f, 1f, 1f).Order(6);
            var velBig = big.ps.velocityOverLifetime;
            velBig.radial = new UnityEngine.ParticleSystem.MinMaxCurve(-1.3f / travel);
            Tint(big, new Color(0.6f, 0.85f, 0.45f));
            b.Cue(big, 0.85f);
            HitStars(b, Hit, Leaf, new[] { 1.3f, 1.4f, 1.5f }, 2.8f, 8, 0.35f);
            SwirlLeaves(b, Target, 1.32f, 0.8f, 0.9f, 1.6f, 26f);
            HitShell(b, Target, Leaf, 1.3f, 0.45f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(0.75f, 1f, 0.5f), 9f, 0f, 0f, 1.25f, 0f, 1.32f, 3.5f, 1.5f, 2.5f, 1.9f, 1f, 2.4f, 0f);
            b.Shake(1.3f, 0.16f, 0.15f);
            b.Shake(1.4f, 0.16f, 0.15f);
            b.Shake(1.5f, 0.2f, 0.2f);
        }

        // ---------------------------------------------------------------- 076 Rayo Solar
        // Refs: anime = haz blanco-amarillo enorme con resplandor dorado y pompas de luz; juego = aros y resplandor verdes
        // en el usuario durante la carga.
        static void M076(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.35f, 1.75f, 2.7f, Target, 1f);
            b.Baked(1.35f); // la carga solar amarilla del clip en el usuario es la descripción
            var from = Mouth + new Vector3(0.35f, 0.05f, 0f);
            var to = Hit + new Vector3(0.4f, 0f, 0f);
            // Carga larga: partículas que convergen, orbe y aros.
            Charge(b, from, Sun, 0.2f, 1.0f, 2.2f, 1.4f, true);
            GroundGlow(b, Attacker, Leaf, 0.2f, 1.25f, 2.2f);
            Beam(b, from, to, Sun, 1.2f, 2.3f, 1.35f, 0.1f);
            var glow = Grad(new[] { 0f, 1f, 1f, 1f, 1f, 1f, 0.9f, 0.6f }, new[] { 0f, 0.8f, 0.15f, 1f, 0.8f, 0.9f, 1f, 0f });
            ConeSpray(b, "SunGlow", from, to, Mat(Sun, "Glow"), 1.2f, 1.0f, 45f, 8f, 1.4f, 3.6f, 0.26f, glow, 0f, 0f, 5);
            Bubbles(b, from, to, Sun, 1.2f, 0.9f, 10f, 0.7f, 0.3f, 0.8f, 1.6f);
            ImpactStar(b, Hit, Sun, 1.35f, 6.2f, 12, 0);
            Shockwave(b, Target, Sun, 1.37f, 5.4f);
            Dust(b, Target, 1.38f, 1.5f, 18, 1.5f);
            HitShell(b, Target, Sun, 1.35f, 0.8f);
            b.Light(Hit + new Vector3(-1.2f, 0.5f, 0f), Lc(1f, 0.95f, 0.6f), 14f, 0f, 0f, 0.3f, 1.5f, 1.2f, 3.5f, 1.37f, 7f, 2.0f, 3.5f, 2.7f, 0f);
            b.Shake(1.2f, 0.12f, 1.1f);
            b.Shake(1.35f, 0.45f, 0.45f);
            b.SlowMo(1.36f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 077 Polvo Veneno
        // Refs: anime = sin captura; juego = nube densa de purpurina violeta-magenta sobre el rival.
        // Movimiento de estado (envenena): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M077(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.7f, 2.7f, TargetChest, 0f);
            b.Baked(1.3f); // las nubes violetas del clip encajan con el juego
            var poison = BotwMaterials.Get("EM_PoisonSmoke");
            CloudStream(b, AttackerHead + new Vector3(0.3f, 0.3f, 0f), TargetHead + new Vector3(0f, 0.6f, 0f), Violet, poison, new Color(1f, 0.9f, 1f), 0.65f, 0.7f, 1.6f);
            // Purpurina que cae sobre el rival.
            b.Cue(b.Ps("PoisonGlitter", Mat(Violet, "Star"), Target + new Vector3(0f, 2.6f, 0f))
                .Box(new Vector3(2.6f, 0.4f, 2f)).Velocity(new Vector3(0f, -0.9f, 0f)).Speed(0f, 0.3f).Life(1.2f, 1.8f).Size(0.1f, 0.22f)
                .Rate(80f).Duration(1.3f).Rotation(0f, 90f).AlphaOverLife(0f, 0f, 0.15f, 1f, 0.8f, 1f, 1f, 0f).Order(5), 1.05f);
            var colors = new[] { new Color(0.85f, 0.5f, 1f), new Color(1f, 0.45f, 0.85f), new Color(1f, 0.85f, 1f) };
            Sparkles(b, TargetChest + new Vector3(0f, 0.4f, 0f), 1.1f, 1.3f, 1.2f, 32f, 0.9f, colors);
            Fog(b, Target, poison, 1.2f, 1.2f, 0.9f, 10f, 1.1f, 0.6f, 1.6f);
            b.Cue(b.Ps("PoisonBubbles", BotwMaterials.Get("EM_Bubble"), Target + new Vector3(0f, 0.4f, 0f))
                .Sphere(0.6f).Velocity(new Vector3(0f, 0.9f, 0f)).Speed(0.1f, 0.5f).Life(0.6f, 1f).Size(0.14f, 0.3f)
                .Rate(16f).Duration(1.0f).SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 1.4f);
            HitShell(b, Target, Violet, 1.3f, 0.6f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.85f, 0.6f, 1f), 8f, 0f, 0f, 0.7f, 1f, 1.3f, 2f, 2.7f, 0f);
        }

        // ---------------------------------------------------------------- 078 Paralizador
        // Refs: anime = nubes enormes de polvo crema-amarillo con destellos dorados que llenan la pantalla;
        // juego = motas doradas alrededor del rival.
        // Movimiento de estado (paraliza): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M078(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.7f, 2.7f, TargetChest, 0f);
            // El clip pinta bocanadas naranjas con forma de llama: las referencias son nubes crema-amarillas.
            b.HideBaked();
            var dust = BotwMaterials.Get("EM_Dust");
            var yellow = new Color(1f, 1f, 0.86f);
            CloudStream(b, AttackerHead + new Vector3(0.3f, 0.3f, 0f), TargetChest + new Vector3(0f, 0.4f, 0f), Pollen, dust, yellow, 0.45f, 0.95f, 2.8f);
            Tint(Fog(b, Target, dust, 1.05f, 1.5f, 1.7f, 18f, 2.6f, 0.8f, 1.8f), yellow);
            Glints(b, (AttackerChest + TargetChest) * 0.5f + new Vector3(0f, 0.8f, 0f), Pollen, 0.5f, 0.9f, 2.2f, 40f, 1.4f);
            Glints(b, TargetChest, Pollen, 1.1f, 1.5f, 1.6f, 44f, 1.4f);
            // Parálisis: chispazos amarillos en el rival.
            RadialBolts(b, TargetChest + new Vector3(0f, 0.2f, 0f), Electric, 1.55f, 2.3f, 4, 0.9f, 7, 0.12f);
            Crackle(b, TargetChest, Electric, 1.5f, 0.9f, 0.7f, 30f);
            HitShell(b, Target, Pollen, 1.3f, 0.6f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(1f, 0.95f, 0.6f), 9f, 0f, 0f, 0.6f, 1.2f, 1.4f, 2.2f, 2.7f, 0f);
        }

        // ---------------------------------------------------------------- 079 Somnífero
        // Refs: anime = brillo turquesa con destellos sobre el usuario; juego = motas verde-turquesa que bajan sobre el rival.
        // Movimiento de estado (duerme): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M079(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.8f, 2.8f, TargetChest, 0f);
            b.Baked(1.3f); // la nube que baja sobre el rival del clip es la descripción
            var mist = BotwMaterials.Get("EM_Mist");
            var teal = new Color(0.55f, 1f, 0.72f); // más verde: motas verde-turquesa del juego
            CloudStream(b, AttackerHead + new Vector3(0.3f, 0.3f, 0f), TargetHead + new Vector3(0f, 0.9f, 0f), Teal, mist, teal, 0.6f, 0.7f, 1.5f);
            // Polvo que desciende despacio alrededor del rival.
            b.Cue(b.Ps("SleepPowder", Mat(Teal, "Glow"), Target + new Vector3(0f, 2.7f, 0f))
                .Box(new Vector3(2.6f, 0.3f, 2.2f)).Velocity(new Vector3(0f, -0.75f, 0f)).Speed(0f, 0.2f).Life(2.0f, 2.6f).Size(0.08f, 0.16f)
                .Rate(70f).Duration(1.4f).AlphaOverLife(0f, 0f, 0.1f, 1f, 0.8f, 1f, 1f, 0f).Order(5), 1.0f);
            Glints(b, TargetChest + new Vector3(0f, 0.3f, 0f), Teal, 1.2f, 1.4f, 1.3f, 26f, 0.8f);
            Tint(Fog(b, Target, mist, 1.2f, 1.4f, 1.3f, 12f, 1.6f, 0.6f, 1.8f), teal);
            HitShell(b, Target, Teal, 1.3f, 0.7f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.65f, 1f, 0.9f), 8f, 0f, 0f, 0.7f, 1f, 1.4f, 2f, 2.8f, 0f);
        }

        // ---------------------------------------------------------------- 080 Danza Pétalo
        // Refs: anime = medias lunas rosas que giran alrededor del usuario con destellos; juego = pétalos rosa claro
        // que llenan la pantalla y vuelan hacia el rival.
        static void M080(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.35f, 1.7f, 2.7f, Target, 0.8f);
            // El clip pinta pétalos rojos muy saturados: el juego los pinta rosa claro.
            b.HideBaked();
            var petal = Mat(Petal, "Spark");
            // Remolino de pétalos alrededor del usuario.
            var whirl = b.Ps("PetalWhirl", petal, Attacker + new Vector3(0f, 0.3f, 0f))
                .GroundCircle(1.2f, 0.4f).Radial(0.1f, 6f).Life(0.8f, 1.1f).Size(0.3f, 0.5f).Rate(60f).Duration(0.9f)
                .Rotation(0f, 360f).Spin(-360f, 360f).AlphaOverLife(0f, 0f, 0.15f, 1f, 0.8f, 1f, 1f, 0f).Order(5);
            var wv = whirl.ps.velocityOverLifetime;
            wv.y = new UnityEngine.ParticleSystem.MinMaxCurve(1.6f);
            b.Cue(whirl, 0.25f);
            Slash(b, AttackerChest + new Vector3(0.1f, 0.3f, 0f), Petal, 0.5f, 30f, 1.6f, 0.45f, -700f);
            // Espiral de pétalos que avanza hacia el rival.
            var from = AttackerChest + new Vector3(0.4f, 0.3f, 0f);
            DrainMotes(b, from, TargetChest, Petal, 0.85f, 0.85f, 80f, 0.55f, 2.3f, "Spark", 0.9f);
            DrainMotes(b, from, TargetChest, Pink, 0.9f, 0.8f, 50f, 0.5f, 1.6f, "Spark", 1.2f);
            Slash(b, TargetChest + new Vector3(-0.1f, 0.3f, 0f), Petal, 1.35f, 30f, 2.2f, 0.5f, -600f);
            Slash(b, TargetChest + new Vector3(-0.1f, 0.3f, 0f), Pink, 1.42f, 210f, 1.8f, 0.45f, -600f);
            SwirlLeaves(b, Target, 1.35f, 0.9f, 1.0f, 1.8f, 40f, petal);
            var colors = new[] { new Color(1f, 0.75f, 0.9f), new Color(1f, 1f, 1f) };
            Sparkles(b, TargetChest + new Vector3(0f, 0.3f, 0f), 1.35f, 0.8f, 1.3f, 24f, 1.1f, colors);
            HitShell(b, Target, Petal, 1.35f, 0.5f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(1f, 0.75f, 0.88f), 9f, 0f, 0f, 0.3f, 1f, 1.37f, 3.5f, 1.8f, 1.5f, 2.7f, 0f);
            b.Shake(1.35f, 0.22f, 0.3f);
        }
    }
}
