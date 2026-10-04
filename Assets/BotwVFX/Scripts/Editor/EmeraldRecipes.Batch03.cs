using UnityEngine;
using static BotwVfx.EditorTools.EmeraldLayers;
using static BotwVfx.EditorTools.EmeraldMoveBuilder;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Lote 3 (movimientos 041-060). Mismo criterio que los lotes 1 y 2: cada receta sale de sus referencias
    /// (captura del anime y del juego, resumidas en el comentario) y de la descripción del movimiento.
    /// Movimientos de estado sin estrella de impacto, cámara lenta ni sacudida fuerte: 043, 045, 046, 047, 048, 050, 054.
    /// </summary>
    public static partial class EmeraldRecipes
    {
        // Boca / ojos del atacante (hacia el objetivo).
        static readonly Vector3 Mouth = new Vector3(-2.55f, 1.3f, -0.1f);
        static readonly Vector3 Eyes = new Vector3(-2.62f, 1.42f, -0.12f);

        // ---------------------------------------------------------------- 041 Doble Ataque
        // Refs: anime = sin captura; juego = espiral violeta sobre el rival y aguijón violeta.
        static void M041(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.15f, 1.45f, 2.4f, Target, 0.7f);
            // El clip pinta un aguijón verde-amarillo; el juego lo pinta violeta.
            b.HideBaked();
            var from = AttackerChest + new Vector3(0.6f, 0.15f, 0f);
            var hits = new[] { 1.15f, 1.45f };
            const float travel = 0.2f;
            Charge(b, from, Violet, 0.45f, 0.5f, 1.0f, 0.7f, false);
            // Un aguijón (huso estirado) por golpe.
            b.Cue(Bursts(b.Ps("Stingers", Mat(Violet, "Spark"), from)
                .Sphere(0.12f).Velocity((Hit - from) / travel).Life(travel).Size(0.3f).Stretch(6f, 0.02f)
                .AlphaOverLife(0f, 0.6f, 0.15f, 1f, 1f, 1f).Order(6), 1, hits[0] - travel, hits[1] - travel), hits[0] - travel);
            HitStars(b, Hit, Violet, hits, 3.2f, 12, 0.25f);
            // Espiral violeta: dos medias lunas que giran, una por golpe.
            Bursts(Slash(b, Hit + new Vector3(0f, 0.05f, 0f), Violet, hits[0], 30f, 1.7f, 0.45f, -760f), 1, hits);
            Bursts(Slash(b, Hit + new Vector3(0f, 0.05f, 0f), Lilac, hits[0] + 0.03f, 210f, 1.25f, 0.42f, -760f), 1, hits[0] + 0.03f, hits[1] + 0.03f);
            b.Cue(b.Ps("PoisonBubbles", BotwMaterials.Get("EM_Bubble"), Target + new Vector3(0f, 0.4f, 0f))
                .Sphere(0.55f).Velocity(new Vector3(0f, 0.9f, 0f)).Speed(0.1f, 0.5f).Life(0.6f, 1f).Size(0.14f, 0.3f)
                .Rate(16f).Duration(0.9f).SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 1.5f);
            HitShell(b, Target, Violet, 1.15f, 0.65f);
            RingPulses(b, Hit, Violet, hits[1], 3, 0.08f, 3f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(0.9f, 0.6f, 1f), 9f, 0f, 0f, 1.13f, 0f, 1.17f, 3.5f, 1.3f, 1f, 1.47f, 4f, 1.9f, 1f, 2.4f, 0f);
            b.Shake(1.15f, 0.18f, 0.2f);
            b.Shake(1.45f, 0.24f, 0.25f);
        }

        // ---------------------------------------------------------------- 042 Pin Misil
        // Refs: anime = husos verdes enormes de núcleo blanco que vuelan en abanico; juego = púa blanca hacia el rival y chispitas amarillas en el usuario.
        static void M042(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.15f, 1.55f, 2.4f, Target, 0.7f);
            b.Baked(1.15f); // la aguja verde del clip y sus impactos verdes coinciden con el anime
            var from = AttackerChest + new Vector3(0.6f, 0.35f, 0f);
            var hits = new[] { 1.15f, 1.35f, 1.55f };
            const float travel = 0.3f;
            Glints(b, from, Gold, 0.75f, 0.5f, 0.5f, 22f, 0.8f);
            // Tres agujas por oleada: salen en abanico (esfera) y se cierran hacia el rival.
            var needles = b.Ps("PinNeedles", Mat(NeedleGreen, "Spark"), from)
                .Sphere(0.6f, 0f).Velocity((Hit - from) / travel).Life(travel).Size(0.38f, 0.46f).Stretch(6f, 0f)
                .AlphaOverLife(0f, 0.5f, 0.12f, 1f, 1f, 1f).Order(6);
            var vel = needles.ps.velocityOverLifetime;
            vel.radial = new UnityEngine.ParticleSystem.MinMaxCurve(-0.6f / travel);
            b.Cue(Bursts(needles, 3, hits[0] - travel, hits[1] - travel, hits[2] - travel), hits[0] - travel);
            HitStars(b, Hit, NeedleGreen, hits, 2.8f, 10, 0.3f);
            Flash(b, Hit, White, hits[2], 2.4f);
            HitShell(b, Target, Leaf, 1.15f, 0.6f);
            Dust(b, Target, 1.56f, 0.9f, 10);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(0.7f, 1f, 0.6f), 9f,
                0f, 0f, 1.13f, 0f, 1.17f, 3f, 1.27f, 1f, 1.37f, 3f, 1.47f, 1f, 1.57f, 3.5f, 1.9f, 1f, 2.4f, 0f);
            foreach (float t in hits)
                b.Shake(t, 0.14f, 0.15f);
        }

        // ---------------------------------------------------------------- 043 Malicioso
        // Refs: anime = ojos que brillan azul pálido; juego = destello blanco-rosado de cuatro puntas con aro en los ojos.
        // Movimiento de estado (baja la Defensa): sin impacto, sin cámara lenta, sin sacudida.
        static void M043(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.1f, 1.4f, 2.4f, TargetChest, 0f);
            // El clip pinta una silueta oscura de pinchos y una visera azul que no salen en las referencias.
            b.HideBaked();
            var side = new Vector3(0f, 0f, 0.13f);
            foreach (var eye in new[] { Eyes - side, Eyes + side })
                b.Cue(b.Ps("EyeGlow", Mat(Ice, "Glow"), eye)
                    .Life(1.3f).Size(0.42f).Burst(1)
                    .SizeOverLife(C(0f, 0.2f, 0.25f, 1f, 0.32f, 1.6f, 0.4f, 1f, 1f, 0.9f)).AlphaOverLife(0f, 0f, 0.1f, 1f, 0.75f, 1f, 1f, 0f).Order(7), 0.6f);
            Flash(b, Eyes, White, 1.0f, 1.9f);
            RingPulses(b, Eyes, Pink, 1.0f, 2, 0.14f, 1.7f);
            Glints(b, Eyes, White, 1.0f, 0.45f, 0.35f, 16f, 0.7f);
            // Pulso intimidante hacia el rival.
            RingStream(b, Eyes + new Vector3(0.3f, 0f, 0f), TargetHead, Ice, 1.05f, 0.3f, 8f, 0.35f, 0.5f, 1.6f);
            HitShell(b, Target, Ice, 1.4f, 0.45f);
            b.Light(Eyes + new Vector3(0.3f, 0.2f, -0.3f), Lc(0.75f, 0.9f, 1f), 6f, 0f, 0f, 0.6f, 0.8f, 1.0f, 2.5f, 1.3f, 0.8f, 1.9f, 0f);
        }

        // ---------------------------------------------------------------- 044 Mordisco
        // Refs: anime = estelas blancas del salto; juego = mandíbulas de colmillos amarillo-naranja que se cierran y rayas oscuras en estrella.
        static void M044(EmeraldMoveBuilder b)
        {
            b.Keys(1.05f, 1.3f, 1.45f, 2.3f, Target, 0.8f); // anticipación = mandíbulas abiertas
            // El clip dibuja unas fauces diminutas a rayas: las mandíbulas propias lo sustituyen.
            b.HideBaked();
            var bite = Hit + new Vector3(0f, 0.15f, 0f);
            Streaks(b, AttackerChest + new Vector3(0.5f, 0.1f, 0f), Hit, White, 0.85f, 0.4f, 60f, 0.6f, 1.2f, 0.22f);
            Jaws(b, bite, FangGold, 0.95f, 1.3f, 2.3f, 0.75f, 7, 0.95f, 0.8f);
            b.Cue(b.Ps("JawGlow", Mat(Gold, "Glow"), bite)
                .Life(0.6f).Size(3.2f).Burst(1).SizeOverLife(C(0f, 0.6f, 0.25f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 0f, 0.5f, 0.7f, 0.6f, 0.9f, 1f, 0f).Order(5), 0.95f);
            ImpactStar(b, bite, Gold, 1.3f, 3.8f, 0, 26);
            Rays(b, bite, Ink, 1.3f, 10, 5.5f, 0.35f);
            HitShell(b, Target, Gold, 1.3f, 0.4f);
            Shockwave(b, Target, Orange, 1.32f, 3.4f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.82f, 0.4f), 10f, 0f, 0f, 1.0f, 0.8f, 1.28f, 1.5f, 1.32f, 5f, 1.6f, 1.6f, 2.3f, 0f);
            b.Shake(1.3f, 0.36f, 0.35f);
            b.SlowMo(1.31f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 045 Gruñido
        // Refs: anime = boca abierta sin efecto visible; juego = rayitas blanco-cian cortas junto a la boca del usuario.
        // Movimiento de estado (baja el Ataque): sin impacto, sin cámara lenta, sin sacudida.
        static void M045(EmeraldMoveBuilder b)
        {
            b.Keys(0.62f, 1.0f, 1.3f, 2.2f, TargetChest, 0f);
            // El clip añade siluetas rosas y rojas que no salen en las referencias.
            b.HideBaked();
            Bursts(Rays(b, Mouth, Cyan, 0.55f, 7, 1.7f, 0.28f), 7, 0.55f, 0.8f, 1.05f);
            ArcWaves(b, Mouth + new Vector3(0.2f, 0f, 0f), Mouth + new Vector3(2.4f, -0.2f, 0.1f), Wind, 0.6f, 0.65f, 5f, 0.5f, 0.3f, 0.9f);
            Glints(b, Mouth, White, 0.55f, 0.8f, 0.45f, 12f, 0.6f);
            HitShell(b, Target, Cyan, 1.25f, 0.45f);
            b.Light(Mouth + new Vector3(0.4f, 0.2f, -0.3f), Lc(0.85f, 0.95f, 1f), 6f, 0f, 0f, 0.5f, 1f, 1.2f, 1f, 1.8f, 0f);
        }

        // ---------------------------------------------------------------- 046 Rugido
        // Refs: anime = grandes ondas en media luna blanco-cian desde la boca; juego = destellos amarillos en estrella y chispas doradas en el suelo.
        // Movimiento de estado (expulsa al rival): sin impacto ni cámara lenta; el rival sale despedido (rayas y polvo).
        static void M046(EmeraldMoveBuilder b)
        {
            b.Keys(0.52f, 1.0f, 1.4f, 2.6f, TargetChest, 0f);
            // El clip pone un aro blanco enorme en el suelo del usuario que tapa la escena.
            b.HideBaked();
            Bursts(Rays(b, Mouth, Gold, 0.45f, 10, 2.6f, 0.35f), 10, 0.45f, 0.75f);
            Glints(b, Attacker + new Vector3(0.3f, 0.25f, 0f), Gold, 0.45f, 1.1f, 1.3f, 30f, 0.8f);
            ArcWaves(b, Mouth + new Vector3(0.3f, 0f, 0f), TargetChest + new Vector3(0.3f, 0.1f, 0f), Wind, 0.55f, 0.95f, 6f, 0.5f, 0.9f, 2.8f);
            Streaks(b, Target + new Vector3(-0.5f, 0.8f, 0.2f), Target + new Vector3(2.6f, 0.9f, 0.7f), Wind, 1.0f, 0.7f, 45f, 0.7f, 1.2f, 0.25f);
            Dust(b, Target, 1.0f, 0.8f, 10, 0.9f);
            HitShell(b, Target, White, 1.0f, 0.55f);
            b.Light(Mouth + new Vector3(0.5f, 0.3f, -0.3f), Lc(1f, 0.92f, 0.7f), 8f, 0f, 0f, 0.45f, 1.8f, 1.2f, 1.2f, 2f, 0f);
            b.Shake(0.55f, 0.1f, 0.8f); // retumbo leve del rugido
        }

        // ---------------------------------------------------------------- 047 Canto
        // Refs: anime = pentagramas y notas sobre burbujas rosas y amarillas; juego = aros magenta delante de la boca, notas rosas y humo magenta.
        // Movimiento de estado (duerme al rival): sin impacto, sin cámara lenta, sin sacudida.
        static void M047(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.6f, 2.6f, TargetChest, 0f);
            b.Baked(1.3f, 0.75f); // las notas de colores y el pentagrama del clip son el anime; más lento para que floten
            RingStream(b, Mouth + new Vector3(0.25f, 0f, 0f), Mouth + new Vector3(1.6f, 0.05f, 0.1f), Magenta, 0.55f, 1.2f, 4f, 0.7f, 0.5f, 1.6f);
            var colors = new[] { new Color(1f, 0.55f, 0.85f), new Color(1f, 0.95f, 0.45f), new Color(0.7f, 0.9f, 1f) };
            Sparkles(b, (AttackerChest + TargetChest) * 0.5f + new Vector3(0f, 0.6f, 0f), 0.6f, 1.6f, 2.0f, 16f, 1.4f, colors);
            // Burbujas difusas rosas y amarillas del anime.
            var bokeh = b.Ps("Bokeh", Mat(Pink, "Glow"), AttackerChest + new Vector3(1.0f, 0.6f, 0f))
                .Sphere(1.3f).Velocity(new Vector3(0f, 0.25f, 0f)).Life(0.9f, 1.4f).Size(0.5f, 0.9f).Rate(7f).Duration(1.4f)
                .SizeOverLife(C(0f, 0.6f, 0.4f, 1f, 1f, 1.1f)).AlphaOverLife(0f, 0f, 0.25f, 0.55f, 0.7f, 0.55f, 1f, 0f).Order(1);
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(new Color(1f, 0.6f, 0.9f), 0f), new GradientColorKey(new Color(1f, 0.95f, 0.5f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            g.mode = GradientMode.Fixed;
            var main = bokeh.ps.main;
            main.startColor = new UnityEngine.ParticleSystem.MinMaxGradient(g) { mode = UnityEngine.ParticleSystemGradientMode.RandomColor };
            b.Cue(bokeh, 0.6f);
            // El rival se adormece: bruma magenta en la cabeza.
            Smoke(b, TargetHead + new Vector3(-0.1f, 0.1f, 0f), BotwMaterials.Get("EM_PoisonSmoke"), 1.5f, 6, 0.6f, 0.4f, 0.4f, 1.4f);
            b.Light(AttackerChest + new Vector3(0.8f, 0.8f, -0.4f), Lc(1f, 0.7f, 0.95f), 8f, 0f, 0f, 0.6f, 1.2f, 2f, 1.2f, 2.6f, 0f);
        }

        // ---------------------------------------------------------------- 048 Supersónico
        // Refs: anime = aros amarillos concéntricos alrededor de la boca; juego = estallido blanco-rosado en la boca y aros rosas que viajan.
        // Movimiento de estado (confunde): sin impacto, sin cámara lenta, sin sacudida.
        static void M048(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.25f, 1.5f, 2.5f, TargetChest, 0f);
            b.Baked(1.25f); // la estrella magenta en la boca y los aros rosas del clip son el juego
            RingPulses(b, Mouth, Gold, 0.6f, 4, 0.12f, 2.2f);
            Flash(b, Mouth + new Vector3(0.2f, 0f, 0f), Magenta, 0.75f, 1.6f);
            RingStream(b, Mouth + new Vector3(0.3f, 0f, 0f), TargetHead, Magenta, 0.8f, 0.45f, 9f, 0.45f, 0.45f, 1.3f);
            HitShell(b, Target, Magenta, 1.25f, 0.45f);
            Dizzy(b, TargetHead, Gold, 1.35f, 1.1f);
            b.Light(Mouth + new Vector3(0.4f, 0.3f, -0.3f), Lc(1f, 0.75f, 0.95f), 7f, 0f, 0f, 0.6f, 1.2f, 1.3f, 1f, 1.6f, 0.8f, 2.4f, 0f);
        }

        // ---------------------------------------------------------------- 049 Bomba Sónica
        // Refs: anime = medias lunas blancas con reflejos rosas que barren; juego = estallido blanco-cian de rayas finas sobre el rival.
        static void M049(EmeraldMoveBuilder b)
        {
            b.Keys(1.12f, 1.3f, 1.45f, 2.3f, Target, 0.6f); // anticipación = medias lunas en vuelo
            b.Baked(1.3f); // los aros blancos alrededor del usuario y la estrella cian del clip encajan
            WindBlade(b, Mouth + new Vector3(0.3f, -0.2f, 0f), Hit, White, 1.0f, 0.3f, 1.7f, -8f);
            WindBlade(b, Mouth + new Vector3(0.2f, 0.1f, 0.1f), Hit + new Vector3(0f, 0.25f, 0f), Lilac, 1.04f, 0.28f, 1.2f, 10f);
            ImpactStar(b, Hit, Cyan, 1.3f, 3.6f, 12, 24);
            Rays(b, Hit, White, 1.31f, 10, 5f, 0.3f);
            HitShell(b, Target, Cyan, 1.3f, 0.4f);
            Shockwave(b, Target, Cyan, 1.32f, 3.2f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(0.8f, 0.95f, 1f), 9f, 0f, 0f, 1.27f, 0f, 1.32f, 4.5f, 1.6f, 1.4f, 2.3f, 0f);
            b.Shake(1.3f, 0.28f, 0.3f);
        }

        // ---------------------------------------------------------------- 050 Anulación
        // Refs: anime = aros azul-blancos concéntricos que encierran al rival; juego = resplandor rosa-blanco enorme con aro sobre el rival.
        // Movimiento de estado (bloquea un movimiento): sin impacto, sin cámara lenta, sin sacudida.
        static void M050(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.2f, 1.6f, 2.6f, TargetChest, 0f);
            b.Baked(1.2f); // el destello del usuario y los aros azules del clip son el anime
            Glints(b, Eyes, White, 0.55f, 0.6f, 0.35f, 14f, 0.7f);
            RingStream(b, Eyes + new Vector3(0.3f, 0f, 0f), TargetChest, Cyan, 0.7f, 0.4f, 6f, 0.45f, 0.4f, 1.2f);
            b.Cue(b.Ps("DisableGlow", Mat(Pink, "Glow"), TargetChest + new Vector3(-0.15f, 0.1f, 0f))
                .Life(0.9f).Size(2.8f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.25f, 1f, 1f, 1.05f))
                .AlphaOverLife(0f, 0f, 0.15f, 0.9f, 0.7f, 0.8f, 1f, 0f).Order(5), 1.15f);
            RingPulses(b, TargetChest + new Vector3(-0.15f, 0.1f, 0f), Cyan, 1.2f, 4, 0.16f, 3.2f);
            GroundGlow(b, Target, Cyan, 1.2f, 2.5f, 1.5f);
            HitShell(b, Target, Pink, 1.2f, 0.6f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.4f, 0f), Lc(0.9f, 0.85f, 1f), 8f, 0f, 0f, 1.1f, 0f, 1.25f, 2.5f, 1.8f, 1f, 2.5f, 0f);
        }

        // ---------------------------------------------------------------- 051 Ácido
        // Refs: anime = chorro violeta oscuro y espeso desde la boca; juego = salpicadura de motas naranjas sobre el rival.
        static void M051(EmeraldMoveBuilder b)
        {
            b.Keys(0.8f, 1.3f, 1.45f, 2.4f, Target, 0.6f);
            b.Baked(1.3f); // el chorro violeta del clip es el anime
            Stream(b, Mouth + new Vector3(0.2f, -0.05f, 0f), Hit, Mat(AcidViolet, "Glow"), 0.95f, 0.5f, 55f, 0.32f, 0.2f, 0.5f, 0f, 0.2f);
            Splash(b, Hit, Violet, 1.3f, 1f, 28, BotwMaterials.Get("EM_PoisonSmoke"));
            // Motas naranjas de la salpicadura del juego.
            Sparks(b, Hit, Orange, 1.31f, 18, 3f, 6f, 1.2f, 1.2f);
            b.Cue(b.Ps("AcidBubbles", BotwMaterials.Get("EM_Bubble"), Target + new Vector3(0f, 0.4f, 0f))
                .Sphere(0.55f).Velocity(new Vector3(0f, 0.9f, 0f)).Speed(0.1f, 0.5f).Life(0.6f, 1f).Size(0.14f, 0.3f)
                .Rate(16f).Duration(0.9f).SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 1.4f);
            HitShell(b, Target, Violet, 1.3f, 0.5f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(0.85f, 0.6f, 1f), 9f, 0f, 0f, 0.95f, 1f, 1.3f, 3.5f, 1.7f, 1.2f, 2.4f, 0f);
            b.Shake(1.3f, 0.2f, 0.25f);
        }

        // ---------------------------------------------------------------- 052 Ascuas
        // Refs: anime = llamitas rojo-naranja separadas que vuelan en abanico; juego = chispitas amarillo-naranjas sobre el rival.
        static void M052(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.2f, 1.45f, 2.3f, Target, 0.6f);
            b.Baked(1.25f); // las llamitas rojas del clip son el anime
            var hits = new[] { 1.2f, 1.3f, 1.4f };
            Projectile(b, Mouth + new Vector3(0.2f, 0f, -0.1f), Hit + new Vector3(0f, 0.15f, 0f), Fire, hits[0] - 0.3f, 0.3f, 0.5f, 0.28f, "Glow", new Vector3(0f, 0.35f, 0f));
            Projectile(b, Mouth + new Vector3(0.2f, -0.1f, 0.1f), Hit + new Vector3(0f, -0.15f, 0.1f), Fire, hits[1] - 0.3f, 0.3f, 0.45f, 0.26f, "Glow");
            Projectile(b, Mouth + new Vector3(0.2f, 0.1f, -0.2f), Hit + new Vector3(0f, 0.3f, -0.1f), Fire, hits[2] - 0.3f, 0.3f, 0.45f, 0.26f, "Glow", new Vector3(0f, 0.6f, 0f));
            HitStars(b, Hit, Fire, hits, 2.2f, 12, 0.3f);
            Flames(b, Hit + new Vector3(0f, -0.2f, 0f), 1.22f, 0.5f, 0.35f, 0.9f, 40f);
            Embers(b, Hit, 1.25f, 0.9f, 0.5f, 30f);
            HitShell(b, Target, Fire, 1.2f, 0.5f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.6f, 0.3f), 9f, 0f, 0f, 0.9f, 0.8f, 1.22f, 3.5f, 1.32f, 2f, 1.42f, 3.5f, 1.8f, 1.2f, 2.3f, 0f);
            foreach (float t in hits)
                b.Shake(t, 0.12f, 0.15f);
        }

        // ---------------------------------------------------------------- 053 Lanzallamas
        // Refs: anime = torrente de fuego que llena la pantalla: núcleo amarillo, cuerpo naranja y bordes rojos, se ensancha mucho;
        // juego = masa de fuego amarilla que envuelve al rival entero.
        static void M053(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.3f, 1.7f, 2.6f, Target, 0.9f);
            // El clip pinta un chorro rojo fino de bolas: el cono de lenguas propio lo sustituye.
            b.HideBaked();
            var from = Mouth + new Vector3(0.25f, -0.05f, 0f);
            var to = Hit + new Vector3(0.5f, 0.1f, 0f);
            Charge(b, from, Fire, 0.35f, 0.55f, 1.1f, 0.9f, false);
            // Cono de lenguas: ~0,5 m en la boca, ~4 m de ancho al llegar al rival.
            FlameTongues(b, from, to, 0.88f, 1.05f, 4.2f, 110f, 0.42f);
            // El fuego envuelve al rival (juego): llamas altas, bola de fuego y ascuas.
            FireBurst(b, Hit, 1.3f, 1.5f, false);
            Flames(b, Target + new Vector3(0f, 0.4f, 0f), 1.25f, 0.85f, 0.8f, 2.0f, 70f);
            HitShell(b, Target, Fire, 1.3f, 0.75f);
            b.Light(Hit + new Vector3(-1.2f, 0.5f, 0f), Lc(1f, 0.6f, 0.25f), 14f, 0f, 0f, 0.4f, 0.8f, 0.9f, 3f, 1.32f, 7f, 1.85f, 4f, 2.6f, 0f);
            b.Shake(0.9f, 0.1f, 0.9f);
            b.Shake(1.3f, 0.38f, 0.4f);
            b.SlowMo(1.31f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 054 Neblina
        // Refs: anime = nubes de bruma blanco-rosada alrededor del usuario; juego = bruma blanca que envuelve al Pokémon.
        // Movimiento de estado (protege): sin impacto, sin cámara lenta, sin sacudida.
        static void M054(EmeraldMoveBuilder b)
        {
            b.Keys(0.65f, 1.0f, 1.6f, 2.9f, AttackerChest, 0f);
            // El clip dibuja burbujas azul marino enormes que tapan la escena.
            b.HideBaked();
            var mist = BotwMaterials.Get("EM_Mist");
            Tint(Fog(b, Attacker, mist, 0.4f, 1.9f, 0.9f, 16f, 1.0f, 0.45f, 1.7f), new Color(1f, 0.97f, 1f));
            Tint(Fog(b, Attacker, mist, 0.6f, 1.6f, 0.6f, 9f, 0.8f, 1.2f, 1.5f), new Color(1f, 0.92f, 0.97f));
            Aura(b, Attacker, Ice, 0.8f, 2.8f, 1.1f, 0f);
            Glints(b, AttackerChest + new Vector3(0f, 0.2f, 0f), Ice, 0.6f, 1.9f, 1.1f, 14f, 0.8f);
            GroundGlow(b, Attacker, Ice, 0.7f, 2.9f, 1.8f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.6f, -0.5f), Lc(0.85f, 0.92f, 1f), 7f, 0f, 0f, 0.5f, 1f, 1.2f, 1.5f, 2.4f, 1f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 055 Pistola Agua
        // Refs: anime = chorro blanco-azulado con estela de velocidad; juego = chorro azul con reflejos blancos y salpicadura en la boca.
        static void M055(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.3f, 1.5f, 2.4f, Target, 0.7f);
            b.Baked(1.3f, 0.85f); // el tubo azul de rayas blancas del clip es el chorro
            var from = Mouth + new Vector3(0.2f, -0.05f, 0f);
            Beam(b, from, Hit, White, 0.95f, 1.75f, 0.12f, 0.2f);
            Stream(b, from, Hit, Mat(Water, "Glow"), 0.95f, 0.75f, 45f, 0.3f, 0.2f, 0.42f, 2.2f, 0.1f);
            Splash(b, from + new Vector3(0.1f, 0f, 0f), Water, 0.95f, 0.5f, 14);
            Splash(b, Hit, Water, 1.3f, 1.0f, 30);
            HitShell(b, Target, Water, 1.3f, 0.45f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(0.6f, 0.85f, 1f), 9f, 0f, 0f, 0.95f, 1f, 1.3f, 3.5f, 1.7f, 1.3f, 2.4f, 0f);
            b.Shake(1.3f, 0.22f, 0.3f);
        }

        // ---------------------------------------------------------------- 056 Hidrobomba
        // Refs: anime = torrente blanco-cian enorme con arcos de presión; juego = chorro cian grueso de núcleo blanco.
        static void M056(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.7f, 2.6f, Target, 1f);
            b.Baked(1.3f, 0.8f); // el chorro azul grueso del clip es el juego
            var from = Mouth + new Vector3(0.25f, -0.05f, 0f);
            Charge(b, from, Water, 0.35f, 0.55f, 1.1f, 0.9f, false);
            Beam(b, from, Hit, White, 0.9f, 1.95f, 0.42f, 0.15f);
            Stream(b, from, Hit + new Vector3(0.3f, 0f, 0f), Mat(Water, "Glow"), 0.9f, 0.95f, 60f, 0.3f, 0.45f, 1.3f, 0f, 0.15f);
            RingStream(b, from, Hit, Water, 0.95f, 0.85f, 7f, 0.3f, 0.7f, 1.5f);
            Splash(b, Hit, Water, 1.3f, 1.7f, 40);
            Shockwave(b, Target, Water, 1.32f, 4.2f);
            HitShell(b, Target, Water, 1.3f, 0.6f);
            b.Light(Hit + new Vector3(-0.8f, 0.4f, 0f), Lc(0.55f, 0.85f, 1f), 11f, 0f, 0f, 0.9f, 2f, 1.32f, 5.5f, 1.8f, 2.5f, 2.6f, 0f);
            b.Shake(0.9f, 0.1f, 1.0f);
            b.Shake(1.3f, 0.42f, 0.45f);
            b.SlowMo(1.31f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 057 Surf
        // Refs: anime = ola con cresta de espuma blanca muy marcada; juego = muro de agua cian-azul con chispas blancas que cruza la escena.
        static void M057(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.3f, 1.6f, 2.6f, Target, 0.9f);
            b.Baked(1.3f); // la ola azul con cresta blanca del clip es la referencia
            var mist = BotwMaterials.Get("EM_Mist");
            // Espuma y gotas que salpican de la cresta mientras avanza.
            Streaks(b, Attacker + new Vector3(0.5f, 1.8f, 0f), Target + new Vector3(-0.6f, 1.6f, 0f), White, 0.55f, 0.75f, 60f, 1.2f, 1.3f, 0.4f);
            Glints(b, (Attacker + Target) * 0.5f + new Vector3(0f, 1.8f, 0f), White, 0.6f, 0.9f, 2.0f, 30f, 0.9f);
            Splash(b, Target + new Vector3(-0.2f, 0.6f, 0f), Water, 1.3f, 1.8f, 44);
            Smoke(b, Target + new Vector3(-0.2f, 0.4f, 0f), mist, 1.33f, 10, 1.3f, 0.5f, 1.1f, 1.6f);
            Shockwave(b, Target, Water, 1.32f, 4.6f);
            HitShell(b, Target, Water, 1.3f, 0.5f);
            b.Light(Hit + new Vector3(-0.8f, 0.6f, 0f), Lc(0.6f, 0.85f, 1f), 11f, 0f, 0f, 0.6f, 1.5f, 1.32f, 4.5f, 1.8f, 1.5f, 2.6f, 0f);
            b.Shake(0.7f, 0.1f, 0.6f);
            b.Shake(1.3f, 0.4f, 0.45f);
        }

        // ---------------------------------------------------------------- 058 Rayo Hielo
        // Refs: anime = haz de cristales de hielo azul pálido y destello blanco en la boca; juego = haz blanco-azulado brillante.
        static void M058(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.6f, 2.5f, Target, 0.9f);
            b.Baked(1.3f, 0.85f); // el haz azul con cristales del clip es el anime
            var from = Mouth + new Vector3(0.25f, -0.05f, 0f);
            Charge(b, from, Ice, 0.4f, 0.5f, 1.0f, 0.8f, false);
            b.Cue(b.Ps("MouthFlare", Mat(Ice, "Star"), from)
                .Life(0.09f).Size(1.2f, 1.6f).Rate(18f).Duration(0.85f).Rotation(0f, 90f)
                .AlphaOverLife(0f, 1f, 1f, 0f).Order(7), 0.9f);
            Beam(b, from, Hit, Ice, 0.9f, 1.85f, 0.32f, 0.15f);
            // Cristales que brotan a lo largo del haz.
            var dir = Hit - from;
            var crystals = b.Ps("BeamCrystals", BotwMaterials.Get("EM_IceShard"), from + dir * 0.55f)
                .Mesh(BotwMeshes.Rock).Box(new Vector3(dir.magnitude * 0.8f, 0.25f, 0.25f)).Life(0.5f, 0.8f)
                .Size3D(new Vector3(0.16f, 0.4f, 0.16f)).Rotation3D(Vector3.zero, Vector3.one * 360f).Rate(28f).Duration(0.75f)
                .SizeOverLife(C(0f, 0.2f, 0.15f, 1f, 0.8f, 1f, 1f, 0f)).Order(5);
            crystals.ps.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir.normalized);
            b.Cue(crystals, 1.0f);
            ImpactStar(b, Hit, Ice, 1.3f, 3.4f, 8, 0);
            IceBurst(b, Hit, 1.32f, 1f, 14);
            GroundGlow(b, Target, Ice, 1.3f, 2.4f, 1.6f);
            HitShell(b, Target, Ice, 1.3f, 0.6f);
            b.Light(Hit + new Vector3(-0.8f, 0.4f, 0f), Lc(0.7f, 0.9f, 1f), 10f, 0f, 0f, 0.9f, 2f, 1.32f, 5f, 1.8f, 2f, 2.5f, 0f);
            b.Shake(1.3f, 0.3f, 0.35f);
        }

        // ---------------------------------------------------------------- 059 Ventisca
        // Refs: anime = corriente densa de copos blancos ovalados y rayas de viento en diagonal que llena la pantalla;
        // juego = bolas de nieve blancas grandes que caen alrededor.
        static void M059(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.8f, 2.8f, Target, 0.8f);
            // El clip levanta un tornado de hielo sobre el rival que no sale en las referencias.
            b.HideBaked();
            var from = AttackerChest + new Vector3(0.5f, 0.55f, 0f);
            var to = TargetChest + new Vector3(0.6f, 0.2f, 0f);
            // La ventisca empieza ya en la anticipación (0,35 s) y dura hasta el pico.
            var flakes = Grad(new[] { 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f }, new[] { 0f, 0f, 0.08f, 1f, 0.8f, 1f, 1f, 0f });
            // Copos ovalados en diagonal y rayas de viento largas (billboards alargados: el chorro se aleja de la cámara).
            ConeSpray(b, "SnowFlakes", from, to, Mat(White, "Glow"), 0.3f, 1.55f, 240f, 22f, 0.09f, 0.2f, 0.6f, flakes, 0f, 0f, 6, 1.9f, 65f, 12f);
            ConeSpray(b, "SnowStreaks", from, to, Mat(Wind, "Spark"), 0.3f, 1.55f, 90f, 26f, 0.05f, 0.09f, 0.45f, flakes, 0f, 0f, 5, 10f, 70f, 8f);
            // Masa de bruma azul que da volumen al chorro (el fondo azul del anime).
            var mist = Grad(new[] { 0f, 0.75f, 0.88f, 1f, 1f, 0.6f, 0.78f, 1f }, new[] { 0f, 0f, 0.15f, 0.85f, 0.7f, 0.75f, 1f, 0f });
            ConeSpray(b, "SnowMist", from, to, BotwMaterials.Get("EM_Mist"), 0.35f, 1.45f, 16f, 16f, 0.6f, 2.6f, 0.7f, mist, 0f, 0f, 2);
            // Bolas de nieve grandes cayendo sobre el rival (juego).
            b.Cue(b.Ps("SnowBalls", Mat(White, "Glow"), Target + new Vector3(-0.6f, 3.0f, 0f))
                .Box(new Vector3(4.5f, 0.3f, 3f)).Velocity(new Vector3(0.8f, -1.8f, 0f)).Life(1.1f, 1.5f).Size(0.22f, 0.4f)
                .Rate(26f).Duration(1.7f).AlphaOverLife(0f, 0f, 0.15f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 0.8f);
            // Remolino de nieve que envuelve al rival.
            Snowflakes(b, TargetChest, 1.3f, 34, 1.3f, 1.4f);
            Smoke(b, Target + new Vector3(0f, 0.6f, 0f), BotwMaterials.Get("EM_Mist"), 1.3f, 14, 1.6f, 0.3f, 1.4f, 1.9f);
            GroundGlow(b, Target, Ice, 1.3f, 2.7f, 2.4f);
            HitShell(b, Target, Ice, 1.3f, 0.9f);
            b.Light(TargetChest + new Vector3(-1.2f, 0.6f, 0f), Lc(0.75f, 0.9f, 1f), 12f, 0f, 0f, 0.4f, 1.5f, 1.32f, 3.5f, 2.0f, 2f, 2.8f, 0f);
            b.Shake(0.5f, 0.08f, 1.4f);
            b.Shake(1.3f, 0.26f, 0.35f);
        }

        // ---------------------------------------------------------------- 060 Psicorrayo
        // Refs: anime = haz ondulante rosa, amarillo y verde; juego = haz magenta con estallido rosa-violeta en la boca.
        static void M060(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.6f, 2.5f, Target, 0.8f);
            b.Baked(1.3f, 0.85f); // el haz arcoíris con aros del clip es el anime
            var from = Mouth + new Vector3(0.25f, -0.05f, 0f);
            Charge(b, from, Magenta, 0.4f, 0.5f, 1.0f, 0.9f, false);
            Flash(b, from, Magenta, 0.9f, 2.2f);
            Bolt(b, from, Hit, Magenta, 0.92f, 1.8f, 0.34f, 14, 0.14f, 3, 6, 0.07f);
            RingStream(b, from, Hit, Pink, 0.95f, 0.8f, 7f, 0.35f, 0.5f, 1.1f);
            ImpactStar(b, Hit, Magenta, 1.3f, 3.6f, 10, 20);
            var colors = new[] { new Color(1f, 0.5f, 0.9f), new Color(1f, 0.95f, 0.4f), new Color(0.5f, 1f, 0.9f) };
            Sparkles(b, TargetChest + new Vector3(-0.2f, 0.3f, 0f), 1.3f, 0.9f, 1.0f, 18f, 1.3f, colors);
            HitShell(b, Target, Magenta, 1.3f, 0.5f);
            b.Light(Hit + new Vector3(-0.8f, 0.4f, 0f), Lc(1f, 0.65f, 1f), 10f, 0f, 0f, 0.9f, 1.8f, 1.32f, 4.5f, 1.8f, 1.5f, 2.5f, 0f);
            b.Shake(1.3f, 0.26f, 0.3f);
        }
    }
}
