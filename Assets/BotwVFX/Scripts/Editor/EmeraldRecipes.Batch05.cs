using UnityEngine;
using static BotwVfx.EditorTools.EmeraldLayers;
using static BotwVfx.EditorTools.EmeraldMoveBuilder;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Lote 5 (movimientos 081-100). Mismo criterio que los lotes 1-4: referencias del anime y del juego resumidas en el
    /// comentario de cada receta, colores de las referencias y volumen en el pico escalado a lo que cubren en pantalla.
    /// Movimientos de estado sin estrella de impacto, cámara lenta ni sacudida fuerte: 081, 086, 092, 095, 096, 097, 100.
    /// Presupuesto: como mucho 10 sistemas Shuriken por movimiento (≤ 600 KB por prefab); las líneas y mallas son baratas.
    /// </summary>
    public static partial class EmeraldRecipes
    {
        // Seda de Disparo Demora: blanca con borde lila (anime).
        static readonly Pal Silk = new Pal("Silk", new Color(2.4f, 2.4f, 2.6f), new Color(0.85f, 0.8f, 1.3f));
        // Furia Dragón (anime): remolino azul intenso con borde azul oscuro.
        static readonly Pal DragonBlue = new Pal("DragonBlue", new Color(1.1f, 1.9f, 3.0f), new Color(0.03f, 0.3f, 2.4f));
        // Sólidos (no emiten): tierra y piedra gris (rocas del anime de Lanzarrocas).
        static readonly Pal Earth = new Pal("Earth", new Color(0.5f, 0.36f, 0.2f), new Color(0.22f, 0.14f, 0.07f));
        static readonly Pal Stone = new Pal("Stone", new Color(0.26f, 0.26f, 0.28f), new Color(0.1f, 0.1f, 0.11f));
        // Grieta: casi negra con borde marrón oscuro.
        static readonly Pal CrackDark = new Pal("CrackDark", new Color(0.004f, 0.003f, 0.003f), new Color(0.05f, 0.03f, 0.015f));
        // Cúpula de Confusión (juego): azul-violeta.
        static readonly Pal PsyBlue = new Pal("PsyBlue", new Color(0.3f, 0.25f, 1.3f), new Color(0.15f, 0.08f, 0.9f));
        // Orbes magenta (Confusión, juego) y halo de Meditación (juego): magenta y violeta intensos
        // (canales por encima de ~1,3 se lavan hacia el blanco).
        static readonly Pal PsyOrb = new Pal("PsyOrb", new Color(1.2f, 0.1f, 1.1f), new Color(0.7f, 0.02f, 0.65f));
        static readonly Pal HaloMagenta = new Pal("HaloMagenta", new Color(1.1f, 0.12f, 1.2f), new Color(0.6f, 0.02f, 0.85f));
        static readonly Pal HaloViolet = new Pal("HaloViolet", new Color(0.45f, 0.1f, 1.3f), new Color(0.22f, 0.02f, 0.8f));
        // Aros de Hipnosis (anime): lavanda grisácea.
        static readonly Pal HypnoLilac = new Pal("HypnoLilac", new Color(0.55f, 0.45f, 1.2f), new Color(0.3f, 0.2f, 0.85f));
        // Gotas de Tóxico (juego): violeta profundo, casi sólido.
        static readonly Pal ToxicPurple = new Pal("ToxicPurple", new Color(0.2f, 0.02f, 0.6f), new Color(0.1f, 0.0f, 0.35f));
        // Giro Fuego: cintas naranjas saturadas (con Fire las cintas se ven blanco-amarillas).
        static readonly Pal FireSpin = new Pal("FireSpin", new Color(2.6f, 1.1f, 0.15f), new Color(1.6f, 0.2f, 0.02f));
        // Furia (anime): rojo-rosa intenso.
        static readonly Pal RageRed = new Pal("RageRed", new Color(2.4f, 0.25f, 0.3f), new Color(1.4f, 0.03f, 0.08f));

        // ---------------------------------------------------------------- 081 Disparo Demora
        // Refs: anime = hilo grueso blanco-lila que sale de la boca; juego = muchas vueltas de seda blanca alrededor del rival.
        // Movimiento de estado (baja Velocidad): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M081(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.2f, 1.6f, 2.7f, TargetChest, 0.2f);
            // El clip solo pinta un destello amarillo en el rival: el hilo y las vueltas propias lo sustituyen.
            b.HideBaked();
            var from = Mouth + new Vector3(0.2f, -0.05f, 0f);
            Flash(b, from + new Vector3(0.1f, 0f, 0f), Silk, 0.68f, 0.9f);
            Beam(b, from, TargetChest + new Vector3(-0.3f, 0.15f, 0f), Silk, 0.7f, 1.95f, 0.28f, 0.4f);
            Ribbon(b, from, TargetChest + new Vector3(-0.3f, 0.3f, 0f), Silk, 0.72f, 1.85f, 0.16f, 0.3f, 2.5f, 0f, 30f, 0.45f);
            Streaks(b, from + new Vector3(0.3f, 0f, 0f), TargetChest, Silk, 0.7f, 0.5f, 40f, 0.25f, 0.8f, 0.3f);
            // Vueltas de seda que envuelven al rival de abajo arriba.
            Coils(b, Target, Silk, 1.05f, 2.75f, 8, 1.5f, 2.1f, 0.55f, 0.06f, -900f);
            Glints(b, TargetChest, White, 1.2f, 1.2f, 1.1f, 14f);
            HitShell(b, Target, Silk, 1.15f, 1.1f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.9f, 0.9f, 1f), 7f, 0f, 0f, 0.7f, 0.8f, 1.2f, 1.6f, 2.7f, 0f);
        }

        // ---------------------------------------------------------------- 082 Furia Dragón
        // Refs: anime = remolino de energía azul enorme (núcleo azul oscuro, borde celeste); juego = orbe rosa-magenta con estela
        // violeta y un aro azul fino a su alrededor.
        static void M082(EmeraldMoveBuilder b)
        {
            b.Keys(0.55f, 1.35f, 1.65f, 2.6f, Target, 0.8f);
            b.Baked(1.35f); // el remolino azul y el orbe violeta del clip encajan con las dos referencias
            var from = Mouth + new Vector3(0.25f, -0.05f, 0f);
            Charge(b, from, DragonBlue, 0.3f, 0.6f, 1.2f, 1.0f, false);
            // Esfera azul grande con núcleo magenta que viaja con estela ondulante.
            Projectile(b, from, Hit, DragonBlue, 0.9f, 0.45f, 2.4f, 1.2f, "Glow");
            Projectile(b, from, Hit, Magenta, 0.9f, 0.45f, 0.9f, 0f, "Glow");
            Ribbon(b, from, Hit, DragonBlue, 0.9f, 1.6f, 0.35f, 0.6f, 2f, 0f, 0f, 0.45f);
            Ribbon(b, from, Hit, Violet, 0.92f, 1.6f, 0.22f, 0.5f, 2f, 2.4f, 70f, 0.45f);
            // Masa del remolino azul (anime) con el núcleo magenta (juego) al llegar.
            b.Cue(b.Ps("DragonSwirl", Mat(DragonBlue, "Glow"), Hit)
                .Life(0.55f).Size(4.6f).Burst(1).Spin(-200f, -200f).SizeOverLife(C(0f, 0.4f, 0.25f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.6f, 0.9f, 1f, 0f).Order(5), 1.33f);
            ImpactStar(b, Hit, DragonBlue, 1.35f, 5.0f, 12, 30);
            RingPulses(b, Hit, DragonBlue, 1.35f, 3, 0.1f, 4.0f);
            HitShell(b, Target, DragonBlue, 1.35f, 0.6f);
            b.Light(Hit + new Vector3(-0.8f, 0.4f, 0f), Lc(0.55f, 0.7f, 1f), 10f, 0f, 0f, 0.6f, 1.2f, 1.37f, 4.5f, 1.7f, 1.5f, 2.6f, 0f);
            b.Shake(1.35f, 0.35f, 0.35f);
        }

        // ---------------------------------------------------------------- 083 Giro Fuego
        // Refs: anime = muro de llamas amarillo-naranjas que llena la pantalla; juego = llamas amarillo-naranjas a ras de suelo
        // (vórtice que encierra al rival).
        static void M083(EmeraldMoveBuilder b)
        {
            b.Keys(0.55f, 1.2f, 1.7f, 2.9f, Target, 0.8f);
            // El clip pinta llamas rojas muy saturadas: las referencias son amarillo-naranjas.
            b.HideBaked();
            var from = Mouth + new Vector3(0.3f, -0.1f, 0f);
            // Chorro en espiral que avanza hasta el rival.
            FlameTongues(b, from, TargetChest, 0.6f, 0.75f, 2.4f, 90f, 0.4f);
            Ribbon(b, from, TargetChest, Fire, 0.62f, 1.35f, 0.3f, 0.45f, 2.5f, 0f, 0f, 0.4f);
            // Vórtice vertical de llamas sobre el rival.
            Tornado(b, Target, FireSpin, 1.05f, 2.85f, 3.6f, 1.25f, 30f, false);
            // Lenguas grandes que suben girando alrededor del rival (el cuerpo del vórtice).
            var tongues = b.Ps("VortexTongues", BotwMaterials.Get("EX_Tongue"), Target + new Vector3(0f, 0.1f, 0f))
                .GroundCircle(1.2f, 0.3f).Radial(0.1f, 4f).Life(0.35f, 0.55f).Size(0.9f, 1.5f).Rate(90f).Duration(1.6f)
                .Stretch(1.5f, 0.12f).AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0f).Order(4);
            var tv = tongues.ps.velocityOverLifetime;
            tv.y = new UnityEngine.ParticleSystem.MinMaxCurve(3.4f);
            b.Cue(tongues, 1.1f);
            Embers(b, Target + new Vector3(0f, 0.8f, 0f), 1.15f, 1.5f, 1.2f, 40f);
            HitShell(b, Target, Fire, 1.2f, 0.6f);
            b.Light(Target + new Vector3(-1f, 1f, 0f), Lc(1f, 0.7f, 0.35f), 11f, 0f, 0f, 0.6f, 1.5f, 1.2f, 4f, 2.4f, 3f, 2.9f, 0f);
            b.Shake(1.2f, 0.3f, 0.3f);
            b.Shake(1.5f, 0.06f, 1.2f);
        }

        // ---------------------------------------------------------------- 084 Impactrueno
        // Refs: anime = rayos amarillo-blancos quebrados que salen del usuario y se abren; juego = zigzags blanco-cian cortos
        // alrededor del usuario y chispas amarillas en el rival.
        static void M084(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.0f, 1.3f, 2.2f, Target, 0.7f);
            b.Baked(1.0f); // el rayo amarillo y el aro en el suelo del rival del clip encajan
            Crackle(b, AttackerHead + new Vector3(0.25f, -0.1f, -0.1f), Electric, 0.35f, 0.65f, 0.35f, 60f);
            RadialBolts(b, AttackerChest + new Vector3(0.3f, 0.25f, 0f), Electric, 0.7f, 1.0f, 5, 1.6f, 4, 0.2f);
            Bolt(b, Mouth + new Vector3(0.1f, -0.25f, 0f), Hit, Electric, 0.85f, 1.3f, 0.45f, 12, 0.55f, 3, 11);
            Bolt(b, Mouth + new Vector3(0.1f, -0.15f, 0f), Hit + new Vector3(0f, 0.7f, 0.4f), Electric, 0.88f, 1.25f, 0.16f, 10, 0.5f, 2, 12);
            ImpactStar(b, Hit, Electric, 1.0f, 3.8f, 10, 24);
            RadialBolts(b, Hit, Electric, 1.0f, 1.4f, 5, 1.1f, 8, 0.18f);
            Crackle(b, TargetChest, Electric, 1.0f, 0.6f, 0.6f, 50f);
            HitShell(b, Target, Electric, 1.0f, 0.5f);
            b.Light(Hit + new Vector3(-0.8f, 0.5f, 0f), Lc(1f, 0.95f, 0.55f), 9f, 0f, 0f, 0.85f, 2f, 1.02f, 4f, 1.3f, 1.5f, 2.2f, 0f);
            b.Shake(1.0f, 0.25f, 0.25f);
        }

        // ---------------------------------------------------------------- 085 Rayo
        // Refs: anime = resplandor amarillo que llena la pantalla y rayo enorme; juego = crepitar amarillo-verde sobre el usuario.
        static void M085(EmeraldMoveBuilder b)
        {
            b.Keys(0.55f, 1.3f, 1.65f, 2.6f, Target, 1f);
            b.Baked(1.3f); // la esfera de carga y los zigzags del clip encajan con el anime
            Aura(b, Attacker, Electric, 0.4f, 1.7f, 1.35f, 50f);
            b.Cue(b.Ps("BodyGlow", Mat(Electric, "Glow"), AttackerChest + new Vector3(0.2f, 0.2f, 0f))
                .Life(0.9f).Size(3.8f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.6f, 1f, 1f, 1.15f))
                .AlphaOverLife(0f, 0f, 0.2f, 0.9f, 0.7f, 0.9f, 1f, 0f).Order(4), 0.7f);
            var from = AttackerChest + new Vector3(0.4f, 0.4f, 0f);
            Bolt(b, from, Hit, Electric, 1.15f, 1.9f, 0.55f, 14, 0.8f, 3, 21);
            Bolt(b, AttackerHead + new Vector3(0.3f, 0.1f, 0f), Hit + new Vector3(0f, 0.5f, 0f), Electric, 1.18f, 1.85f, 0.3f, 12, 0.9f, 2, 22);
            ImpactStar(b, Hit, Electric, 1.3f, 5.8f, 14, 34);
            RadialBolts(b, Hit, Electric, 1.3f, 1.9f, 4, 1.8f, 9, 0.24f);
            Shockwave(b, Target, Electric, 1.32f, 5.2f);
            HitShell(b, Target, Electric, 1.3f, 0.7f);
            b.Light(Hit + new Vector3(-1.2f, 0.6f, 0f), Lc(1f, 0.95f, 0.5f), 14f, 0f, 0f, 0.7f, 2f, 1.15f, 3f, 1.32f, 7f, 1.9f, 2f, 2.6f, 0f);
            b.Shake(0.7f, 0.08f, 0.5f);
            b.Shake(1.3f, 0.5f, 0.45f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 086 Onda Trueno
        // Refs: anime = orbe amarillo-blanco en la mano y aros amarillos que crecen hacia el rival; juego = aros amarillos
        // en sarta con rayitos finos.
        // Movimiento de estado (paraliza): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M086(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.3f, 1.7f, 2.8f, TargetChest, 0.2f);
            // El clip pinta aros naranjas pequeños: los aros propios son amarillos y crecen como en el anime.
            b.HideBaked();
            var orb = AttackerHand + new Vector3(0.25f, 0.25f, -0.1f);
            b.Cue(b.Ps("HandOrb", Mat(Electric, "Glow"), orb)
                .Life(1.2f).Size(2.0f).Burst(1).SizeOverLife(C(0f, 0.2f, 0.25f, 1f, 0.8f, 1.05f, 1f, 0.3f))
                .AlphaOverLife(0f, 0f, 0.1f, 1f, 0.85f, 1f, 1f, 0f).Order(6), 0.45f);
            Crackle(b, orb, Electric, 0.5f, 1.0f, 0.6f, 50f);
            RingStream(b, orb + new Vector3(0.3f, 0f, 0f), TargetChest, Electric, 0.8f, 0.75f, 9f, 0.55f, 0.7f, 2.6f);
            Bolt(b, orb, TargetChest, Electric, 0.85f, 1.5f, 0.08f, 14, 0.5f, 3, 31, 0.06f);
            // Parálisis en el rival: arcos pequeños que crepitan.
            RadialBolts(b, TargetChest + new Vector3(0f, 0.1f, 0f), Electric, 1.3f, 2.6f, 5, 0.9f, 32, 0.1f);
            Crackle(b, TargetChest, Electric, 1.3f, 1.3f, 0.7f, 30f);
            Glints(b, TargetChest, Electric, 1.3f, 1.2f, 1.0f, 14f);
            HitShell(b, Target, Electric, 1.3f, 1.2f);
            b.Light(orb + new Vector3(0.4f, 0.4f, -0.4f), Lc(1f, 0.95f, 0.6f), 8f, 0f, 0f, 0.5f, 2f, 1.3f, 1.2f, 2.8f, 0f);
        }

        // ---------------------------------------------------------------- 087 Trueno
        // Refs: anime = columna de luz amarillo-blanca enorme que sube del usuario; juego = rayos amarillos verticales
        // que caen y crepitan.
        static void M087(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.45f, 1.8f, 2.9f, Target, 1f);
            b.Baked(1.45f); // las nubes oscuras sobre el rival y el rayo del clip encajan con la descripción
            // Carga corporal: aura y columna de luz hacia arriba.
            Aura(b, Attacker, Electric, 0.3f, 1.6f, 1.5f, 60f);
            Beam(b, Attacker + new Vector3(0f, 0.05f, 0f), Attacker + new Vector3(0f, 6.5f, 0f), Electric, 0.4f, 1.5f, 1.8f, 0.3f);
            RadialBolts(b, AttackerChest + new Vector3(0f, 0.3f, 0f), Electric, 0.5f, 1.4f, 3, 1.4f, 41, 0.22f);
            // Descarga desde arriba sobre el rival.
            Bolt(b, Target + new Vector3(0.3f, 7.5f, 0.2f), TargetChest, Electric, 1.38f, 1.95f, 0.75f, 14, 0.9f, 3, 42, 0.045f);
            Bolt(b, Target + new Vector3(-0.6f, 7f, -0.3f), Target + new Vector3(0.2f, 0.1f, 0f), Electric, 1.4f, 1.9f, 0.35f, 12, 0.8f, 2, 43);
            Beam(b, Target + new Vector3(0f, 0.02f, 0f), Target + new Vector3(0f, 7f, 0f), Electric, 1.42f, 1.8f, 1.3f, 0.05f);
            ImpactStar(b, TargetChest, Electric, 1.45f, 6.4f, 14, 36);
            RadialBolts(b, TargetChest, Electric, 1.45f, 2.1f, 4, 2.0f, 44, 0.26f);
            Shockwave(b, Target, Electric, 1.47f, 5.8f);
            HitShell(b, Target, Electric, 1.45f, 0.8f);
            b.Light(Target + new Vector3(-1.2f, 1.5f, 0f), Lc(1f, 0.95f, 0.55f), 15f, 0f, 0f, 0.4f, 1.5f, 1.3f, 2f, 1.47f, 8f, 2.0f, 2.5f, 2.9f, 0f);
            b.Shake(0.4f, 0.08f, 1.0f);
            b.Shake(1.45f, 0.6f, 0.5f);
            b.SlowMo(1.46f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 088 Lanzarrocas
        // Refs: anime = rocas grises facetadas con contorno de destellos blancos que vuelan; juego = rayas rosas y estrellas
        // blancas de impacto sobre el rival.
        static void M088(EmeraldMoveBuilder b)
        {
            b.Keys(1.0f, 1.25f, 1.55f, 2.5f, Target, 0.8f); // anticipación = rocas en el aire
            // El clip pinta un bloque marrón junto al usuario: las tres rocas propias lo sustituyen.
            b.HideBaked();
            var rock = Mat(Stone, "Solid");
            var from = AttackerHead + new Vector3(0.4f, 0.9f, -0.2f);
            var hits = new[] { 1.25f, 1.35f, 1.45f };
            ThrownRock(b, from, Hit + new Vector3(0f, 0.1f, 0f), rock, hits[0] - 0.45f, 0.45f, 1.25f, 1.0f, White, 0.5f);
            ThrownRock(b, from + new Vector3(0f, 0.3f, 0.3f), Hit + new Vector3(0f, 0.5f, 0.3f), rock, hits[1] - 0.45f, 0.45f, 1.1f, 1.4f, White, 0.45f);
            ThrownRock(b, from + new Vector3(0f, -0.2f, -0.3f), Hit + new Vector3(0f, -0.3f, -0.3f), rock, hits[2] - 0.45f, 0.45f, 1.15f, 0.7f, White, 0.45f);
            HitStars(b, Hit, Cream, hits, 4.2f, 12, 0.4f);
            Debris(b, Hit, hits[0], 18, 7f, 1.4f, rock);
            Dust(b, Target, 1.27f, 1.2f, 14, 1.3f);
            HitShell(b, Target, Cream, 1.25f, 0.5f);
            b.Light(Hit + new Vector3(-0.6f, 0.5f, 0f), Lc(1f, 0.9f, 0.75f), 8f, 0f, 0f, 1.22f, 0f, 1.27f, 3.5f, 1.47f, 3f, 1.9f, 1f, 2.5f, 0f);
            foreach (float t in hits)
                b.Shake(t, 0.2f, 0.15f);
        }

        // ---------------------------------------------------------------- 089 Terremoto
        // Refs: anime = el suelo revienta en lajas y nubes de tierra beige; juego = suelo agrietado oscuro y piedrecitas.
        static void M089(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.0f, 1.5f, 3.0f, Target, 1f);
            // El clip pinta una explosión de bloques amarillos sobre el usuario: las lajas y grietas propias la sustituyen.
            b.HideBaked();
            // Pisotón y ondas rasantes que recorren el suelo.
            b.Cue(Bursts(b.Ps("QuakeWaves", Mat(Sand, "Shock"), Attacker + new Vector3(0f, 0.06f, 0f))
                .Mesh(BotwMeshes.Ring).Life(0.8f).Size(1f).Rotation3D(Vector3.zero, new Vector3(0f, 360f, 0f))
                .SizeOverLife(C(0f, 1f, 1f, 11f)).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(0), 1, 0.75f, 0.95f, 1.15f), 0.75f);
            GroundCrack(b, Attacker + new Vector3(0.6f, 0f, 0f), Target + new Vector3(-0.2f, 0f, 0.4f), CrackDark, 0.8f, 0.35f, 3.3f, 0.65f, 6, 7);
            GroundCrack(b, Attacker + new Vector3(0.4f, 0f, -0.3f), Attacker + new Vector3(2.5f, 0f, -3.2f), CrackDark, 0.8f, 0.3f, 3.3f, 0.5f, 3, 8);
            RockSpikes(b, Attacker + new Vector3(1.2f, 0f, 0f), Target + new Vector3(0.6f, 0f, 0f), Mat(Earth, "Solid"), 0.85f, 0.5f, 22, 1.5f, 1.0f);
            // Nubes de tierra beige que revientan a lo largo del suelo.
            b.Cue(b.Ps("QuakeDust", BotwMaterials.Get("EM_Dust"), (Attacker + Target) * 0.5f + new Vector3(0.5f, 0.3f, 0f))
                .Box(new Vector3(5f, 0.1f, 1.6f)).Speed(1f, 2.5f).Velocity(new Vector3(0f, 1.6f, 0f)).Drag(2.5f)
                .Life(0.9f, 1.4f).Size(1.0f, 1.8f).Rate(50f).Duration(0.5f)
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.15f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.35f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1), 0.85f);
            Dust(b, Target, 1.0f, 1.6f, 22, 1.8f);
            Debris(b, Target + new Vector3(-1f, 0.3f, 0f), 1.0f, 22, 9f, 1.5f, Mat(Earth, "Solid"));
            ImpactStar(b, TargetChest, Cream, 1.0f, 4.2f, 10, 0);
            HitShell(b, Target, Cream, 1.0f, 0.5f);
            b.Light(Target + new Vector3(-1.5f, 1f, 0f), Lc(1f, 0.85f, 0.6f), 10f, 0f, 0f, 0.75f, 1.2f, 1.02f, 3.5f, 1.6f, 1.2f, 3.0f, 0f);
            b.Shake(0.75f, 0.3f, 0.25f);
            b.Shake(1.0f, 0.55f, 1.3f);
        }

        // ---------------------------------------------------------------- 090 Fisura
        // Refs: anime = grieta negra quebrada con ramas que cruza el suelo; juego = sima profunda que se abre bajo el rival.
        static void M090(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.3f, 1.8f, 3.1f, Target, 1f);
            // El clip pinta una explosión junto al usuario: la grieta propia la sustituye.
            b.HideBaked();
            Shockwave(b, Attacker, Sand, 0.6f, 3.4f);
            Dust(b, Attacker, 0.62f, 0.8f, 8, 0.8f);
            // La grieta corre hacia el rival y se abre en una sima bajo él.
            GroundCrack(b, Attacker + new Vector3(0.5f, 0f, 0f), Target + new Vector3(1.4f, 0f, 0.4f), CrackDark, 0.7f, 0.6f, 3.5f, 1.1f, 7, 13);
            GroundCrack(b, Target + new Vector3(-0.9f, 0f, -1.3f), Target + new Vector3(0.9f, 0f, 1.4f), CrackDark, 1.25f, 0.2f, 3.5f, 1.5f, 4, 14);
            GroundShadow(b, Target, 1.25f, 3.5f, 1.0f, 3.4f);
            b.Cue(b.Ps("CrackDust", BotwMaterials.Get("EM_Dust"), (Attacker + Target) * 0.5f + new Vector3(0.6f, 0.25f, 0.2f))
                .Box(new Vector3(5.5f, 0.1f, 0.5f)).Speed(0.5f, 1.5f).Velocity(new Vector3(0f, 1.2f, 0f)).Drag(2.5f)
                .Life(0.7f, 1.0f).Size(0.45f, 0.8f).Rate(14f).Duration(0.6f)
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.15f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.35f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(1), 0.75f);
            RockSpikes(b, Target + new Vector3(-1.2f, 0f, 0f), Target + new Vector3(1.2f, 0f, 0f), Mat(Earth, "Solid"), 1.27f, 0.15f, 10, 1.0f, 1.3f, 1.4f);
            Flash(b, TargetChest, Cream, 1.3f, 2.6f);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.3f, 16, 6f, 1.3f, Mat(Earth, "Solid"));
            Smoke(b, Target + new Vector3(0f, 0.3f, 0f), BotwMaterials.Get("EM_Dust"), 1.35f, 8, 1.1f, 0.8f, 1.0f, 1.3f);
            HitShell(b, Target, Cream, 1.3f, 0.6f);
            b.Light(Target + new Vector3(-1.5f, 1f, 0f), Lc(1f, 0.85f, 0.6f), 10f, 0f, 0f, 0.6f, 1f, 1.32f, 4f, 1.9f, 1.2f, 3.0f, 0f);
            b.Shake(0.6f, 0.2f, 0.2f);
            b.Shake(1.3f, 0.5f, 0.9f);
            b.SlowMo(1.31f, 0.35f, 0.08f);
        }

        // ---------------------------------------------------------------- 091 Excavar
        // Refs: anime = el usuario gira y se hunde levantando una corona de tierra beige; juego = montículo marrón y terrones
        // que saltan junto al rival.
        static void M091(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.6f, 1.9f, 2.9f, Target, 0.9f);
            b.Baked(1.6f); // el montón de tierra, el rastro subterráneo y la erupción del clip encajan con la descripción
            var earth = Mat(Earth, "Solid");
            // Se hunde: corona de tierra y terrones.
            Petals(b, Attacker, Sand, 0.45f, 16, 0.6f, 7f, 1.8f);
            Dust(b, Attacker, 0.45f, 1.0f, 14, 1.2f);
            Debris(b, Attacker + new Vector3(0.3f, 0.2f, 0f), 0.45f, 12, 5f, 1.1f, earth);
            // Rastro de tierra removida hacia el rival.
            GroundCrack(b, Attacker + new Vector3(0.5f, 0f, 0f), Target + new Vector3(-0.3f, 0f, 0f), Earth, 0.6f, 0.95f, 2.6f, 0.5f, 0, 51, 0.15f);
            // Emerge bajo el rival.
            RockSpikes(b, Target + new Vector3(-0.8f, 0f, 0f), Target + new Vector3(0.8f, 0f, 0f), earth, 1.58f, 0.12f, 10, 1.2f, 0.8f, 1.1f, 0.6f);
            ImpactStar(b, TargetChest, Cream, 1.6f, 4.8f, 12, 0);
            Dust(b, Target, 1.62f, 1.7f, 24, 2.0f);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.6f, 18, 9f, 1.4f, earth);
            HitShell(b, Target, Cream, 1.6f, 0.5f);
            b.Light(Target + new Vector3(-1f, 0.8f, 0f), Lc(1f, 0.85f, 0.6f), 10f, 0f, 0f, 1.55f, 0f, 1.62f, 4f, 2.0f, 1.2f, 2.9f, 0f);
            b.Shake(0.45f, 0.12f, 0.3f);
            b.Shake(1.6f, 0.45f, 0.4f);
            b.SlowMo(1.61f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 092 Tóxico
        // Refs: anime = bocanadas de humo oliva casi negro; juego = gotas violeta-magenta que vuelan al rival.
        // Movimiento de estado (envenena gravemente): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M092(EmeraldMoveBuilder b)
        {
            b.Keys(0.8f, 1.2f, 1.7f, 2.9f, TargetChest, 0.2f); // anticipación = gotas en vuelo
            // El clip pinta un borrón negro y un estallido morado: las gotas y bocanadas propias lo sustituyen.
            b.HideBaked();
            var olive = new Color(0.75f, 0.8f, 0.42f);
            var dark = BotwMaterials.Get("EX_DarkSmoke");
            Stream(b, Mouth + new Vector3(0.2f, -0.1f, 0f), TargetChest, Mat(ToxicPurple, "Glow"), 0.55f, 0.7f, 40f, 0.5f, 0.35f, 0.7f, 0f, 0.3f);
            Stream(b, Mouth + new Vector3(0.2f, -0.1f, 0f), TargetChest + new Vector3(0f, 0.3f, 0f), Mat(ToxicPurple, "Glow"), 0.6f, 0.6f, 20f, 0.5f, 0.5f, 0.9f, 0f, 0.45f);
            b.Cue(Tint(Bursts(b.Ps("ToxicPuffs", dark, TargetChest + new Vector3(0f, 0.2f, 0f))
                .Sphere(1.2f).Speed(1f, 2.4f).Drag(2.2f).Life(1.1f, 1.6f).Size(1.8f, 2.8f).Velocity(new Vector3(0f, 0.4f, 0f))
                .SizeOverLife(C(0f, 0.5f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.45f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(2), 5, 1.15f, 1.35f, 1.55f), olive), 1.15f);
            Tint(Fog(b, Target, dark, 1.3f, 1.4f, 1.0f, 10f, 1.1f, 0.7f, 1.6f), olive);
            Splash(b, TargetChest, ToxicPurple, 1.2f, 1.2f, 22, BotwMaterials.Get("EM_PoisonSmoke"));
            b.Cue(b.Ps("ToxicBubbles", BotwMaterials.Get("EM_Bubble"), Target + new Vector3(0f, 0.4f, 0f))
                .Sphere(0.6f).Velocity(new Vector3(0f, 0.9f, 0f)).Speed(0.1f, 0.5f).Life(0.6f, 1f).Size(0.14f, 0.3f)
                .Rate(16f).Duration(1.1f).SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 1.35f);
            HitShell(b, Target, Violet, 1.2f, 0.8f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.8f, 0.6f, 1f), 8f, 0f, 0f, 0.6f, 0.8f, 1.25f, 2f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 093 Confusión
        // Refs: anime = brillo cian de borde quebrado alrededor del usuario; juego = cúpula azul-violeta grande sobre el usuario,
        // orbes magenta y rayas cian.
        static void M093(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.25f, 1.6f, 2.6f, Target, 0.6f);
            b.Baked(1.25f); // el arco azul delante del usuario y el aro cian del clip encajan con el juego
            Aura(b, Attacker, Cyan, 0.3f, 1.5f, 1.3f, 40f);
            // Cúpula azul-violeta alrededor del usuario.
            var domeScale = Vector3.one * 2.8f;
            var dome = b.MeshPart("PsyDome", BotwMeshes.Sphere, Mat(PsyBlue, "Shell"), AttackerChest + new Vector3(0.3f, 0.1f, 0f), Vector3.zero, domeScale, order: 1);
            var dt = b.Track(dome.transform, dome, 0.45f, 1.5f);
            dt.scaleFrom = domeScale * 0.5f;
            dt.scaleTo = domeScale;
            dt.scaleCurve = C(0.45f, 0f, 0.65f, 1f, 1.5f, 1.08f);
            dt.property = "_Dissolve";
            dt.propertyCurve = C(0.45f, 0.8f, 0.6f, 0f, 1.25f, 0f, 1.5f, 1f);
            b.Cue(b.Ps("PsyOrbs", Mat(PsyOrb, "Glow"), AttackerChest + new Vector3(0.9f, 0.5f, -0.3f))
                .Sphere(0.6f).Life(0.9f).Size(0.8f, 1.0f).Burst(3).SizeOverLife(C(0f, 0.2f, 0.2f, 1f, 0.8f, 1f, 1f, 0f))
                .AlphaOverLife(0f, 0f, 0.15f, 1f, 1f, 1f).Order(5), 0.55f);
            Streaks(b, AttackerChest + new Vector3(0.3f, 1f, 0f), Hit, Cyan, 0.9f, 0.4f, 30f, 1.2f, 0.8f, 0.3f);
            // Pulsos mentales sobre el rival.
            Flash(b, Hit, Cyan, 1.25f, 2.2f);
            RingPulses(b, TargetChest, Cyan, 1.2f, 4, 0.12f, 2.6f);
            HitShell(b, Target, Cyan, 1.25f, 0.8f);
            b.Light(AttackerChest + new Vector3(0.8f, 0.8f, -0.5f), Lc(0.6f, 0.8f, 1f), 9f, 0f, 0f, 0.5f, 1.5f, 1.25f, 2.5f, 2.6f, 0f);
            b.Shake(1.25f, 0.15f, 0.5f);
        }

        // ---------------------------------------------------------------- 094 Psíquico
        // Refs: anime = los objetivos quedan contorneados de cian brillante y suspendidos; juego = aura rosa-violeta en el usuario
        // con rayas blancas.
        static void M094(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.45f, 1.8f, 2.8f, Target, 0.9f);
            b.Baked(1.45f); // los aros magenta-azules del usuario y el estallido rosa del clip encajan con el juego
            Aura(b, Attacker, Magenta, 0.3f, 1.6f, 1.3f, 45f);
            Streaks(b, AttackerChest + new Vector3(0.4f, 0.3f, 0f), TargetChest, White, 0.9f, 0.5f, 50f, 0.9f, 1.1f, 0.25f);
            // Presa telequinética: contorno cian, crepitar y órbitas alrededor del rival.
            Aura(b, Target, Cyan, 0.8f, 2.4f, 1.1f, 0f);
            Crackle(b, TargetChest, Cyan, 0.85f, 1.4f, 0.7f, 40f);
            Coils(b, Target, Cyan, 0.85f, 2.3f, 2, 1.0f, 1.0f, 0.25f, 0.1f, -700f, 30f);
            ImpactStar(b, TargetChest, Magenta, 1.45f, 4.8f, 12, 26);
            RingPulses(b, TargetChest, Cyan, 1.45f, 3, 0.12f, 3.2f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.7f, 0.85f, 1f), 10f, 0f, 0f, 0.8f, 1.5f, 1.47f, 4f, 1.9f, 1.5f, 2.8f, 0f);
            b.Shake(1.45f, 0.4f, 0.4f);
        }

        // ---------------------------------------------------------------- 095 Hipnosis
        // Refs: anime = aros concéntricos lila-grises delante del usuario; juego = remolino azul-violeta con núcleo blanco
        // delante de la cabeza del usuario, hacia el rival.
        // Movimiento de estado (duerme): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M095(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.3f, 1.8f, 2.9f, TargetHead, 0.2f);
            b.Baked(1.3f); // los aros violetas que viajan del clip encajan con las dos referencias
            var eye = Eyes + new Vector3(0.6f, 0.1f, -0.1f);
            b.Cue(b.Ps("HypnoCore", Mat(PsyBlue, "Glow"), eye)
                .Life(1.3f).Size(1.5f).Burst(1).SizeOverLife(C(0f, 0.2f, 0.2f, 1f, 0.85f, 1.05f, 1f, 0.3f))
                .AlphaOverLife(0f, 0f, 0.1f, 1f, 0.85f, 1f, 1f, 0f).Order(5), 0.45f);
            RingPulses(b, eye, HypnoLilac, 0.55f, 6, 0.15f, 2.8f);
            RingStream(b, eye + new Vector3(0.3f, 0f, 0f), TargetHead, HypnoLilac, 0.65f, 1.0f, 8f, 0.7f, 0.8f, 2.4f);
            Glints(b, TargetHead, Lilac, 1.3f, 1.3f, 0.9f, 12f);
            HitShell(b, Target, Lilac, 1.3f, 1.0f);
            b.Light(eye + new Vector3(0.3f, 0.3f, -0.4f), Lc(0.8f, 0.7f, 1f), 8f, 0f, 0f, 0.5f, 1.5f, 1.4f, 1f, 2.9f, 0f);
        }

        // ---------------------------------------------------------------- 096 Meditación
        // Refs: anime = el usuario flota sin efecto; juego = halo de aros concéntricos magenta-violeta detrás del usuario.
        // Movimiento de estado (sube Ataque): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M096(EmeraldMoveBuilder b)
        {
            b.Keys(0.4f, 1.0f, 1.6f, 2.8f, AttackerChest, 0f);
            // El clip pinta un disco azul saturado: el halo del juego es magenta-violeta.
            b.HideBaked();
            // Halo detrás del usuario (más lejos de la cámara que él).
            var halo = AttackerChest + new Vector3(0.6f, 0.35f, 0f);
            b.Cue(b.Ps("HaloOuter", Mat(HaloViolet, "Ring"), halo)
                .Life(2.0f).Size(3.6f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.12f, 1f, 1f, 1.05f))
                .AlphaOverLife(0f, 0f, 0.08f, 1f, 0.85f, 1f, 1f, 0f).Order(1), 0.5f);
            b.Cue(b.Ps("HaloInner", Mat(HaloMagenta, "Ring"), halo)
                .Life(1.9f).Size(2.3f).Burst(1).SizeOverLife(C(0f, 0.3f, 0.12f, 1f, 1f, 1.05f))
                .AlphaOverLife(0f, 0f, 0.08f, 1f, 0.85f, 1f, 1f, 0f).Order(2), 0.6f);
            RingPulses(b, halo, HaloMagenta, 0.55f, 8, 0.2f, 3.2f);
            Aura(b, Attacker, HaloMagenta, 0.4f, 2.5f, 1.2f, 25f);
            GroundGlow(b, Attacker, HaloViolet, 0.45f, 2.6f, 1.4f);
            Glints(b, AttackerChest + new Vector3(0f, 0.3f, 0f), Pink, 0.8f, 1.6f, 1.2f, 12f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(1f, 0.6f, 1f), 8f, 0f, 0f, 0.5f, 1.5f, 1.4f, 1.5f, 2.8f, 0f);
        }

        // ---------------------------------------------------------------- 097 Agilidad
        // Refs: anime = carrera rápida con estela de polvo marrón; juego = rayas blancas horizontales de velocidad
        // e imágenes residuales.
        // Movimiento de estado (sube Velocidad): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M097(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.0f, 1.6f, 2.8f, AttackerChest, 0f);
            // El clip pinta rayas cian y un estallido rosa: las rayas blancas y residuales propias lo sustituyen.
            b.HideBaked();
            // Rayas horizontales en pantalla que cruzan sobre el usuario.
            Streaks(b, AttackerChest - ScreenRight * 3f, AttackerChest + ScreenRight * 3f, White, 0.45f, 1.6f, 90f, 1.3f, 1.4f, 0.3f);
            // Avances y retrocesos: residuales a un lado y al otro.
            Afterimages(b, Attacker, -ScreenRight * 0.3f, Wind, 0.5f, 3, 0.12f, 0.4f);
            Afterimages(b, Attacker, ScreenRight * 0.3f, Wind, 1.15f, 3, 0.12f, 0.4f);
            Dust(b, Attacker, 0.5f, 0.9f, 10, 1.0f);
            Dust(b, Attacker, 1.15f, 0.9f, 10, 1.0f);
            RisingArrows(b, Attacker, Wind, 1.0f, 1.3f, 14f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(0.8f, 0.9f, 1f), 7f, 0f, 0f, 0.5f, 1f, 1.6f, 1f, 2.8f, 0f);
        }

        // ---------------------------------------------------------------- 098 Ataque Rápido
        // Refs: anime = estrella de impacto naranja con rayas de velocidad azul-blancas; juego = rayas cian-blancas al arrancar.
        static void M098(EmeraldMoveBuilder b)
        {
            b.Keys(0.4f, 0.9f, 1.2f, 2.2f, Target, 0.8f);
            b.Baked(0.9f); // las rayas cian y la estrella del clip encajan con el juego
            Dust(b, Attacker, 0.42f, 0.9f, 10, 1.1f);
            Streaks(b, AttackerChest + new Vector3(0.3f, 0f, 0f), TargetChest, White, 0.45f, 0.45f, 90f, 0.8f, 1.5f, 0.2f);
            Afterimages(b, Attacker, new Vector3(0.95f, 0f, 0f), White, 0.45f, 5, 0.07f, 0.35f);
            SpeedLines(b, Hit, White, 0.6f, 0.35f, 3.6f, 100f, 1.2f);
            ImpactStar(b, Hit, Orange, 0.9f, 4.6f, 12, 26);
            Dust(b, Target, 0.92f, 1.1f, 12, 1.2f);
            HitShell(b, Target, White, 0.9f, 0.45f);
            b.Light(Hit + new Vector3(-0.6f, 0.5f, 0f), Lc(1f, 0.8f, 0.55f), 9f, 0f, 0f, 0.87f, 0f, 0.92f, 4f, 1.2f, 1.5f, 2.2f, 0f);
            b.Shake(0.9f, 0.35f, 0.3f);
        }

        // ---------------------------------------------------------------- 099 Furia
        // Refs: anime = el usuario brilla rojo-rosa con vapor oscuro; juego = bocanadas de vapor blanco sobre la cabeza.
        static void M099(EmeraldMoveBuilder b)
        {
            b.Keys(0.4f, 1.5f, 1.8f, 2.8f, Target, 0.7f);
            // El clip pinta las llamas rojas en punta junto al rival en el impacto: el aura roja propia las sustituye.
            b.HideBaked();
            Aura(b, Attacker, RageRed, 0.25f, 1.4f, 1.4f, 55f);
            // Pulsos de rabia: resplandor rojo que tiñe al usuario.
            b.Cue(Bursts(b.Ps("RageGlow", Mat(RageRed, "Glow"), AttackerChest + new Vector3(0.1f, 0.2f, 0f))
                .Life(0.32f).Size(2.8f).SizeOverLife(C(0f, 0.6f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 0f, 0.25f, 0.9f, 1f, 0f).Order(3), 1, 0.35f, 0.7f, 1.05f), 0.35f);
            // Vapor de enfado sobre la cabeza.
            b.Cue(Tint(Bursts(b.Ps("Steam", BotwMaterials.Get("EM_Dust"), AttackerHead + new Vector3(0.1f, 0.4f, 0f))
                .Sphere(0.25f).Speed(0.3f, 0.8f).Velocity(new Vector3(0f, 1.2f, 0f)).Drag(2.5f).Life(0.6f, 0.8f).Size(0.6f, 0.9f)
                .SizeOverLife(C(0f, 0.4f, 0.3f, 1f, 1f, 1.2f))
                .Custom(Const(1f), Const(0f), Curve(0f, 0f, 0.45f, 0.05f, 1f, 1f), Rand(0f, 1f)).Order(3), 3, 0.5f, 0.8f, 1.1f), new Color(1f, 1f, 1f)), 0.5f); // EM_Mist es azulado; EM_Dust es crema claro
            Streaks(b, AttackerChest + new Vector3(0.5f, 0.1f, 0f), Hit, RageRed, 1.25f, 0.25f, 70f, 0.5f, 1.3f, 0.22f);
            ImpactStar(b, Hit, Red, 1.5f, 4.4f, 12, 24);
            HitShell(b, Target, RageRed, 1.5f, 0.45f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(1f, 0.45f, 0.4f), 9f, 0f, 0f, 0.3f, 1.5f, 1.3f, 1.5f, 1.52f, 3f, 2.0f, 1f, 2.8f, 0f);
            b.Shake(0.4f, 0.1f, 0.9f);
            b.Shake(1.5f, 0.35f, 0.3f);
        }

        // ---------------------------------------------------------------- 100 Teletransporte
        // Refs: anime = silueta brillante arcoíris pastel; juego = silueta blanca con resplandor cian y cintas magenta-violeta
        // que giran alrededor.
        // Movimiento de estado (huida): sin estrella de impacto, sin cámara lenta, sin sacudida.
        static void M100(EmeraldMoveBuilder b)
        {
            b.Keys(0.45f, 1.1f, 1.95f, 2.7f, AttackerChest, 0f); // pico = reaparece
            // El clip pinta un estallido rosa: la silueta que se contrae propia lo sustituye.
            b.HideBaked();
            var body = new Vector3(0.62f, 1f, 0.62f);
            var center = Attacker + new Vector3(0f, 0.82f, 0f);
            // Se contrae en una línea vertical y desaparece.
            var shrink = b.MeshPart("VanishShell", BotwMeshes.Sphere, Mat(Cyan, "Shell"), center, Vector3.zero, body, order: 2);
            var st = b.Track(shrink.transform, shrink, 0.3f, 1.15f);
            st.scaleFrom = body * 1.05f;
            st.scaleTo = new Vector3(0.03f, 1.7f, 0.03f);
            st.scaleCurve = C(0.3f, 0f, 0.75f, 0.05f, 1.12f, 1f);
            b.Cue(b.Ps("WhiteBody", Mat(Cyan, "Glow"), AttackerChest)
                .Life(0.85f).Size(3.2f).Burst(1).SizeOverLife(C(0f, 0.6f, 0.4f, 1f, 1f, 0.15f))
                .AlphaOverLife(0f, 0f, 0.15f, 1f, 1f, 1f).Order(3), 0.3f);
            SpinArc(b, AttackerChest, HaloMagenta, 0.35f, 1.2f, 1.9f, 1.3f, -800f, 20f);
            SpinArc(b, AttackerChest + new Vector3(0f, 0.3f, 0f), HaloViolet, 0.4f, 1.2f, 1.7f, 1.1f, 900f, -25f);
            SpinArc(b, AttackerChest + new Vector3(0f, -0.2f, 0f), HaloMagenta, 1.75f, 2.5f, 1.7f, 1.0f, 800f, 35f);
            CutLine(b, AttackerChest + new Vector3(0f, 0.2f, 0f), White, 1.1f, 0f, 3.5f, 0.3f, 0.25f);
            Flash(b, AttackerChest, White, 1.12f, 2.0f);
            var colors = new[] { new Color(1f, 0.6f, 0.9f), new Color(0.6f, 1f, 1f), new Color(1f, 1f, 0.5f), new Color(0.7f, 1f, 0.5f) };
            Sparkles(b, AttackerChest, 1.1f, 0.6f, 1.3f, 40f, 1.0f, colors);
            // Reaparece: la silueta se expande desde la línea.
            var grow = b.MeshPart("AppearShell", BotwMeshes.Sphere, Mat(Cyan, "Shell"), center, Vector3.zero, body, order: 2);
            var gt = b.Track(grow.transform, grow, 1.75f, 2.5f);
            gt.scaleFrom = new Vector3(0.03f, 1.7f, 0.03f);
            gt.scaleTo = body * 1.05f;
            gt.scaleCurve = C(1.75f, 0f, 2.05f, 1f);
            gt.property = "_Dissolve";
            gt.propertyCurve = C(1.75f, 0f, 2.15f, 0f, 2.5f, 1f);
            Glints(b, AttackerChest, Cyan, 1.75f, 0.6f, 1.1f, 20f);
            b.Light(AttackerChest + new Vector3(0.6f, 0.7f, -0.5f), Lc(0.75f, 0.95f, 1f), 8f, 0f, 0f, 0.3f, 1.5f, 1.12f, 3f, 1.4f, 0.3f, 1.8f, 1.5f, 2.8f, 0f);
        }
    }
}
