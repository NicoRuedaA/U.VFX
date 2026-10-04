using UnityEngine;
using static BotwVfx.EditorTools.EmeraldLayers;
using static BotwVfx.EditorTools.EmeraldMoveBuilder;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Lote 2 (movimientos 021-040). Mismo criterio que el lote 1: cada receta sale de sus dos referencias
    /// (captura del anime y del juego, resumidas en el comentario) y de la descripción del movimiento.
    /// Los movimientos de estado (028, 039) no llevan estrella de impacto, cámara lenta ni sacudida fuerte.
    /// </summary>
    public static partial class EmeraldRecipes
    {
        // Altura de una patada (más baja que el puño).
        static readonly Vector3 KickHit = new Vector3(2.62f, 0.7f, -0.12f);
        static readonly Vector3 AttackerFoot = new Vector3(-2.5f, 0.45f, -0.1f);

        /// <summary>
        /// Cuerno/taladro (malla Cone/Drill a lo largo de +Y) montado en un pivote que mira de from a to.
        /// Devuelve el pivote (su pista mueve el conjunto); el cono gira sobre su eje a <paramref name="spin"/> °/s.
        /// </summary>
        static EmeraldMoveVfx.PartTrack HornPart(EmeraldMoveBuilder b, Mesh mesh, Pal p, Vector3 from, Vector3 to, float t0, float t1, Vector3 scale, float spin = 0f)
        {
            var pivot = b.Node("Horn", from);
            pivot.localRotation = Quaternion.FromToRotation(Vector3.up, (to - from).normalized);
            var r = b.MeshPart("HornMesh", mesh, Mat(p, "Solid"), Vector3.zero, Vector3.zero, scale, pivot, 5);
            var cone = b.Track(r.transform, r, t0, t1);
            cone.spin = new Vector3(0f, spin, 0f);
            cone.property = "_Erosion";
            cone.propertyCurve = C(t0, 0.6f, t0 + 0.06f, 0f, t1 - 0.12f, 0f, t1, 1f);
            cone.scaleFrom = scale * 0.4f;
            cone.scaleTo = scale;
            cone.scaleCurve = C(t0, 0f, t0 + 0.1f, 1f);
            var tr = b.Track(pivot, null, t0, t1);
            tr.posFrom = from;
            tr.posTo = to;
            return tr;
        }

        // ---------------------------------------------------------------- 021 Atizar
        // Refs: anime = barras de velocidad verticales blancas; juego = nube de polvo crema que barre con rocas, estallido amarillo-blanco y rayas azules.
        static void M021(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.3f, 1.45f, 2.4f, Target, 0.9f);
            b.Baked(1.3f); // el arco descendente rojo-naranja del clip es el propio golpe
            Aura(b, Attacker, Cream, 0.2f, 1.05f, 1f, 25f);
            Streaks(b, Target + new Vector3(-0.3f, 4.2f, 0f), Target + new Vector3(-0.3f, 0.2f, 0f), White, 1.05f, 0.28f, 60f, 1.1f, 1.5f, 0.22f);
            Slash(b, Hit + new Vector3(0f, 0.45f, 0f), Cream, 1.17f, -90f, 1.9f, 0.3f, -260f);
            ImpactStar(b, Hit, Gold, 1.3f, 4.6f, 12, 32);
            Rays(b, Hit, Cyan, 1.31f, 9, 5.5f);
            Smoke(b, Target + new Vector3(-0.3f, 0.45f, 0f), BotwMaterials.Get("EM_Dust"), 1.32f, 14, 1.45f, 0.45f, 1.1f, 1.7f);
            Dust(b, Target, 1.33f, 1.3f, 16, 1.3f);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.32f, 14, 8f, 1.2f);
            Shockwave(b, Target, Cream, 1.32f, 4.2f);
            HitShell(b, Target, Cream, 1.3f, 0.4f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(1f, 0.88f, 0.6f), 10f, 0f, 0f, 1.28f, 0f, 1.33f, 5f, 1.6f, 2f, 2.4f, 0f);
            b.Shake(1.3f, 0.45f, 0.45f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 022 Látigo Cepa
        // Refs: anime = dos lianas verdes largas que hacen bucles; juego = estallido verde de pinchos con centro amarillo sobre el rival.
        static void M022(EmeraldMoveBuilder b)
        {
            b.Keys(0.85f, 1.3f, 1.45f, 2.2f, Target, 0.6f);
            // El clip dibuja una barra verde suelta junto a la cámara: las lianas propias lo sustituyen.
            b.HideBaked();
            var hand = AttackerHand + new Vector3(0f, 0.15f, 0f);
            Vine(b, hand + new Vector3(0f, 0.1f, -0.15f), Hit + new Vector3(0f, 0.25f, 0f), VineGreen, 0.8f, 1.3f, 1.95f, 0.24f, 0.45f, 0f);
            Vine(b, hand + new Vector3(0f, -0.1f, 0.15f), Hit + new Vector3(0f, -0.15f, 0.1f), VineGreen, 0.9f, 1.45f, 2.05f, 0.24f, 0.5f, 2.2f);
            HitStars(b, Hit, Leaf, new[] { 1.3f, 1.45f }, 4f, 14, 0.2f);
            CutLine(b, Hit + new Vector3(0f, 0.2f, 0f), White, 1.3f, -60f, 2.6f, 0.2f);
            CutLine(b, Hit, White, 1.45f, 50f, 2.6f, 0.2f);
            b.Cue(Bursts(b.Ps("Leaves", BotwMaterials.Get("EM_Leaf"), Hit)
                .Sphere(0.3f).Speed(3f, 6.5f).Drag(2.5f).Life(0.6f, 1.1f).Size(0.18f, 0.3f).Rotation(0f, 360f).Spin(-400f, 400f)
                .Gravity(0.35f).AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 10, 1.3f, 1.45f), 1.3f);
            HitShell(b, Target, Leaf, 1.3f, 0.4f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(0.75f, 1f, 0.5f), 9f, 0f, 0f, 1.28f, 0f, 1.32f, 3.5f, 1.4f, 1.2f, 1.47f, 3f, 1.8f, 0.8f, 2.2f, 0f);
            b.Shake(1.3f, 0.18f, 0.2f);
            b.Shake(1.45f, 0.22f, 0.25f);
        }

        // ---------------------------------------------------------------- 023 Pisotón
        // Refs: anime = el usuario salta muy alto; juego = aro naranja en el suelo bajo el rival y corona de "plumas" crema que estalla hacia fuera.
        static void M023(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.45f, 2.4f, Target, 0.9f);
            b.Baked(1.3f); // el disco naranja del clip coincide con el aro del juego
            Dust(b, Attacker, 0.45f, 0.9f, 10, 0.9f);
            GroundShadow(b, Target, 0.85f, 1.3f, 0.4f, 2.2f);
            Streaks(b, Target + new Vector3(-0.2f, 4.5f, 0f), Target + new Vector3(-0.2f, 0.3f, 0f), White, 1.0f, 0.3f, 50f, 0.8f, 1.3f, 0.22f);
            Petals(b, Target, Cream, 1.3f, 26, 0.7f, 9f, 1.4f);
            ImpactStar(b, Target + new Vector3(-0.2f, 0.4f, -0.1f), Cream, 1.3f, 3.4f, 0, 0);
            Shockwave(b, Target, Orange, 1.31f, 4.8f, true);
            GroundGlow(b, Target, Orange, 1.3f, 2.3f, 2.4f);
            Dust(b, Target, 1.32f, 1.4f, 18, 1.3f);
            Debris(b, Target + new Vector3(0f, 0.2f, 0f), 1.32f, 12, 7f);
            b.Light(Target + new Vector3(-0.8f, 0.8f, 0f), Lc(1f, 0.75f, 0.45f), 10f, 0f, 0f, 1.28f, 0f, 1.33f, 5f, 1.7f, 2f, 2.4f, 0f);
            b.Shake(1.3f, 0.5f, 0.5f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 024 Doble Patada
        // Refs: anime = pies blancos deslumbrantes sobre líneas de velocidad azules; juego = huella roja-naranja estampada en el rival y rayitas blancas.
        static void M024(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.2f, 1.47f, 2.3f, Target, 0.75f);
            b.Baked(1.45f);
            Aura(b, Attacker, Orange, 0.2f, 1.0f, 1f, 25f);
            SpeedLines(b, KickHit, Cyan, 1.0f, 0.55f, 3.2f, 80f);
            Projectile(b, AttackerFoot + new Vector3(0f, 0f, -0.15f), KickHit + new Vector3(0f, 0.1f, 0f), White, 1.0f, 0.2f, 1.2f, 0.5f);
            Projectile(b, AttackerFoot + new Vector3(0f, 0f, 0.15f), KickHit + new Vector3(0f, -0.1f, 0f), White, 1.25f, 0.2f, 1.2f, 0.5f);
            var hits = new[] { 1.2f, 1.45f };
            Footprint(b, KickHit + new Vector3(-0.1f, 0.3f, -0.25f), FootRed, hits, 2.1f, -25f);
            HitStars(b, KickHit, Orange, hits, 3.6f, 14, 0.25f);
            Flash(b, KickHit, White, 1.45f, 3f);
            HitShell(b, Target, Orange, 1.2f, 0.5f);
            Dust(b, Target, 1.47f, 1f, 12);
            b.Light(KickHit + new Vector3(-0.5f, 0.4f, 0f), Lc(1f, 0.7f, 0.45f), 9f, 0f, 0f, 1.18f, 0f, 1.22f, 3.5f, 1.35f, 1f, 1.47f, 4.5f, 1.8f, 1.2f, 2.3f, 0f);
            b.Shake(1.2f, 0.2f, 0.2f);
            b.Shake(1.45f, 0.32f, 0.3f);
        }

        // ---------------------------------------------------------------- 025 Megapatada
        // Refs: anime = remolino de viento blanco enorme alrededor de la patada; juego = nube de humo beige que llena el plano, huella roja gigante y rayas amarillas.
        static void M025(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.3f, 1.45f, 2.5f, Target, 1f);
            // El clip abre con una estrella azul en el pie que no sale en ninguna referencia.
            b.HideBaked();
            Charge(b, AttackerFoot, White, 0.25f, 0.8f, 1.5f, 1.2f);
            Aura(b, Attacker, Cream, 0.2f, 1.1f, 1.05f, 30f);
            WindBlade(b, AttackerChest + new Vector3(0.5f, 0f, 0f), KickHit + new Vector3(-0.4f, 0.1f, 0f), White, 1.05f, 0.25f, 2.0f, -10f);
            Slash(b, KickHit + new Vector3(-0.2f, 0.2f, 0f), White, 1.2f, 160f, 2.3f, 0.35f, -320f);
            Footprint(b, KickHit + new Vector3(0f, 0.5f, -0.2f), FootRed, new[] { 1.3f }, 2.6f, -15f, 0.7f);
            ImpactStar(b, KickHit, Gold, 1.3f, 5.2f, 16, 50);
            Smoke(b, KickHit + new Vector3(0.2f, 0.2f, 0f), BotwMaterials.Get("EM_Dust"), 1.31f, 18, 1.7f, 0.5f, 1.2f, 1.8f);
            Shockwave(b, Target, Gold, 1.32f, 4.6f, true);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.32f, 12, 9f);
            HitShell(b, Target, Gold, 1.3f, 0.45f);
            b.Light(KickHit + new Vector3(-0.6f, 0.5f, 0f), Lc(1f, 0.8f, 0.45f), 12f, 0f, 0f, 0.3f, 1f, 0.85f, 1.5f, 1.28f, 0f, 1.33f, 6.5f, 1.7f, 2.5f, 2.5f, 0f);
            b.Shake(1.3f, 0.5f, 0.5f);
            b.SlowMo(1.31f, 0.2f, 0.12f);
        }

        // ---------------------------------------------------------------- 026 Patada Salto
        // Refs: anime = estela crema tras el usuario sobre líneas de velocidad verdes y azules; juego = estela de cometa amarilla-naranja enorme con rayas rosas.
        static void M026(EmeraldMoveBuilder b)
        {
            b.Keys(1.1f, 1.3f, 1.45f, 2.3f, Target, 1f); // anticipación = el cometa en pleno vuelo
            b.Baked(1.3f);
            Dust(b, Attacker, 0.6f, 0.9f, 10, 0.9f);
            Shockwave(b, Attacker, Cream, 0.62f, 2.2f);
            Projectile(b, AttackerChest + new Vector3(0.3f, 0.2f, 0f), KickHit, Fire, 0.82f, 0.48f, 2.2f, 2.1f, "Glow", new Vector3(0f, 1.6f, 0f));
            Streaks(b, AttackerChest + new Vector3(0.5f, 1.2f, 0f), KickHit + new Vector3(0f, 0.6f, 0f), Pink, 0.95f, 0.35f, 70f, 0.7f, 1.4f, 0.22f);
            SpeedLines(b, KickHit, Mint, 0.95f, 0.4f, 3.4f, 90f, 1.2f);
            ImpactStar(b, KickHit, Gold, 1.3f, 4.8f, 14, 40);
            Flash(b, KickHit, White, 1.3f, 3.4f);
            Shockwave(b, Target, Orange, 1.32f, 3.8f);
            Dust(b, Target, 1.33f, 1.1f, 12);
            HitShell(b, Target, Gold, 1.3f, 0.4f);
            b.Light(KickHit + new Vector3(-0.6f, 0.6f, 0f), Lc(1f, 0.85f, 0.45f), 11f, 0f, 0f, 0.85f, 1.5f, 1.25f, 2f, 1.32f, 6f, 1.7f, 2f, 2.3f, 0f);
            b.Shake(1.3f, 0.42f, 0.45f);
            b.SlowMo(1.31f, 0.25f, 0.1f);
        }

        // ---------------------------------------------------------------- 027 Patada Giro
        // Refs: anime = líneas de velocidad diagonales violeta-azul; juego = estallido amarillo-naranja con arcos concéntricos en remolino y rayas amarillas.
        static void M027(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.45f, 2.3f, Target, 0.8f);
            b.Baked(1.3f);
            SpinArc(b, Attacker + new Vector3(0f, 0.55f, 0f), White, 0.45f, 1.05f, 0.95f, 1.6f, -1100f, 8f);
            SpeedLines(b, KickHit, Violet, 0.95f, 0.45f, 3.4f, 90f);
            Streaks(b, KickHit + new Vector3(-3f, 0.3f, 0.4f), KickHit + new Vector3(1.6f, 0f, -0.2f), Gold, 1.05f, 0.35f, 60f, 0.6f, 1.3f, 0.2f);
            SpinArc(b, Target + new Vector3(0f, 0.75f, 0f), Gold, 1.18f, 1.75f, 1.35f, 2.2f, -1300f, 12f);
            RingPulses(b, KickHit, Orange, 1.3f, 4, 0.07f, 3.6f);
            ImpactStar(b, KickHit, Gold, 1.3f, 4.4f, 12, 36);
            Shockwave(b, Target, Orange, 1.32f, 3.8f);
            Dust(b, Target, 1.33f, 1.1f, 12);
            HitShell(b, Target, Gold, 1.3f, 0.4f);
            b.Light(KickHit + new Vector3(-0.6f, 0.5f, 0f), Lc(1f, 0.82f, 0.4f), 10f, 0f, 0f, 1.25f, 0f, 1.32f, 5.5f, 1.7f, 1.8f, 2.3f, 0f);
            b.Shake(1.3f, 0.38f, 0.4f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 028 Ataque Arena
        // Refs: anime = grandes nubes de arena crema que suben delante del usuario; juego = chorro de arena marrón del usuario a la cara del rival.
        // Movimiento de estado (baja la precisión): sin estrella de impacto, sin cámara lenta, sin sacudida fuerte.
        static void M028(EmeraldMoveBuilder b)
        {
            b.Keys(0.95f, 1.25f, 1.55f, 2.6f, TargetChest, 0.15f);
            // El chorro del clip es amarillo-naranja saturado; el de las referencias es arena marrón/crema.
            b.HideBaked();
            var dust = BotwMaterials.Get("EM_Dust");
            var sand = new Color(0.92f, 0.78f, 0.58f);
            Dust(b, Attacker + new Vector3(0.5f, 0f, 0f), 0.65f, 0.7f, 10, 1f);
            Smoke(b, Attacker + new Vector3(1.0f, 0.3f, -0.2f), dust, 0.75f, 7, 0.9f, 1.1f, 0.5f, 1.5f);
            CloudStream(b, Attacker + new Vector3(0.7f, 0.35f, 0f), TargetHead + new Vector3(-0.3f, -0.15f, 0f), Sand, dust, sand, 0.85f, 0.55f, 0.6f);
            Smoke(b, TargetHead + new Vector3(-0.2f, -0.1f, 0f), dust, 1.25f, 12, 1.1f, 0.25f, 0.55f, 1.8f);
            b.Cue(b.Ps("SandFall", Mat(Sand, "Spark"), TargetHead)
                .Sphere(0.5f).Speed(0.2f, 1f).Life(0.5f, 0.9f).Size(0.04f, 0.08f).Rate(60f).Duration(0.9f).Gravity(0.6f)
                .Stretch(3f, 0.05f).AlphaOverLife(0f, 1f, 0.7f, 1f, 1f, 0f).Order(4), 1.3f);
            b.Light(TargetHead + new Vector3(-0.8f, 0.4f, 0f), Lc(1f, 0.9f, 0.7f), 7f, 0f, 0f, 1.2f, 0f, 1.35f, 1.2f, 2.4f, 0f);
            b.Shake(1.28f, 0.06f, 0.2f);
        }

        // ---------------------------------------------------------------- 029 Golpe Cabeza
        // Refs: anime = líneas de velocidad horizontales azules y blancas; juego = estallido dorado con arcos naranjas concéntricos alrededor del rival.
        static void M029(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.45f, 2.3f, Target, 0.75f);
            b.Baked(1.3f);
            var head = TargetHead + new Vector3(-0.4f, -0.15f, -0.1f);
            Streaks(b, AttackerHead + new Vector3(0.4f, 0f, 0f), head, Cyan, 0.9f, 0.4f, 60f, 0.7f, 1.3f, 0.25f);
            Streaks(b, AttackerChest + new Vector3(0.4f, 0f, 0f), Hit, White, 0.95f, 0.35f, 50f, 0.6f, 1.1f, 0.25f);
            SpeedLines(b, head, White, 1.0f, 0.35f, 3.2f, 80f);
            ImpactStar(b, head, Gold, 1.3f, 4.4f, 12, 34);
            RingPulses(b, head, Orange, 1.3f, 3, 0.08f, 4.2f);
            Shockwave(b, Target, Gold, 1.32f, 3.6f);
            Dust(b, Target, 1.33f, 1f, 12);
            HitShell(b, Target, Gold, 1.3f, 0.4f);
            b.Light(head + new Vector3(-0.6f, 0.3f, 0f), Lc(1f, 0.85f, 0.45f), 10f, 0f, 0f, 1.27f, 0f, 1.32f, 5f, 1.6f, 1.8f, 2.3f, 0f);
            b.Shake(1.3f, 0.36f, 0.35f);
            b.SlowMo(1.31f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 030 Cornada
        // Refs: anime = rayos radiales amarillo-naranja de fondo; juego = cuerno blanco que sale disparado hacia el rival.
        static void M030(EmeraldMoveBuilder b)
        {
            b.Keys(1.2f, 1.3f, 1.45f, 2.3f, Target, 0.7f); // anticipación = el cuerno en vuelo
            b.Baked(1.3f);
            var from = AttackerHead + new Vector3(0.6f, 0.3f, -0.4f);
            var dir = (Hit - from).normalized;
            var to = Hit - dir * 1.1f; // la punta del cuerno (1,1 m) queda en el rival
            Charge(b, from, Cream, 0.35f, 0.6f, 1.0f, 0.8f, false);
            var horn = HornPart(b, Cone, Horn, from, to, 0.95f, 1.5f, new Vector3(0.55f, 1.1f, 0.55f), 720f);
            horn.posCurve = C(0.95f, 0f, 1.12f, 0.08f, 1.3f, 1f, 1.5f, 1.04f);
            Projectile(b, from, to, Cream, 1.12f, 0.18f, 0.7f, 0.45f);
            Rays(b, Hit, Orange, 1.29f, 14, 6.5f, 0.35f);
            ImpactStar(b, Hit, Gold, 1.3f, 4f, 10, 28);
            CutLine(b, Hit, White, 1.3f, 75f, 4f, 0.3f);
            Dust(b, Target, 1.32f, 1f, 10);
            HitShell(b, Target, Gold, 1.3f, 0.4f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.85f, 0.5f), 10f, 0f, 0f, 1.27f, 0f, 1.32f, 5f, 1.6f, 1.6f, 2.3f, 0f);
            b.Shake(1.3f, 0.32f, 0.35f);
            b.SlowMo(1.31f, 0.35f, 0.06f);
        }

        // ---------------------------------------------------------------- 031 Ataque Furia
        // Refs: anime = pico blanco brillante sobre líneas de velocidad verdes; juego = cuerno blanco-gris que pica varias veces al rival.
        static void M031(EmeraldMoveBuilder b)
        {
            b.Keys(1.02f, 1.15f, 1.65f, 2.4f, Target, 0.6f); // anticipación = el cuerno a punto de picar
            b.Baked(1.15f);
            var hits = new[] { 1.15f, 1.4f, 1.65f };
            // Picotazos en diagonal desde arriba y del lado de la cámara: el cuerno se ve de perfil.
            var dir = (Hit - (Hit + new Vector3(-1.6f, 0.9f, -0.7f))).normalized;
            var to = Hit - dir * 1.1f;
            var from = to - dir * 0.9f;
            var horn = HornPart(b, Cone, Horn, from, to, 0.85f, 1.95f, new Vector3(0.5f, 1.1f, 0.5f));
            horn.posFrom = from - dir * 0.6f;
            horn.posCurve = C(0.85f, 0f, 1.0f, 0.3f, 1.15f, 1f, 1.25f, 0.4f, 1.4f, 1f, 1.5f, 0.4f, 1.65f, 1f, 1.95f, 0.3f);
            Projectile(b, AttackerHead + new Vector3(0.4f, 0.2f, 0f), from, White, 0.75f, 0.2f, 0.8f, 0.4f);
            SpeedLines(b, Hit, Mint, 0.95f, 0.85f, 3.2f, 80f);
            HitStars(b, Hit, White, hits, 3f, 12, 0.3f);
            Bursts(Rays(b, Hit, Gold, hits[0], 8, 4.5f, 0.25f), 8, hits);
            Shockwave(b, Target, Cream, 1.67f, 3.2f);
            Dust(b, Target, 1.67f, 1f, 10);
            HitShell(b, Target, White, 1.15f, 0.65f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.95f, 0.8f), 9f,
                0f, 0f, 1.13f, 0f, 1.17f, 3f, 1.3f, 1f, 1.42f, 3f, 1.55f, 1f, 1.67f, 4f, 2f, 1f, 2.4f, 0f);
            foreach (float t in hits)
                b.Shake(t, t > 1.6f ? 0.28f : 0.16f, 0.2f);
        }

        // ---------------------------------------------------------------- 032 Perforador
        // Refs: anime = líneas de velocidad diagonales verdes, amarillas y cian; juego = taladro en espiral con estela anaranjada y chispas blanco-amarillas en la punta.
        static void M032(EmeraldMoveBuilder b)
        {
            b.Keys(1.1f, 1.3f, 2.0f, 2.8f, Target, 1f); // anticipación = el taladro en camino
            // El clip pinta un taladro azul con rayas cian: el taladro propio lo sustituye.
            b.HideBaked();
            var from = AttackerChest + new Vector3(0.6f, 0.15f, 0f);
            var tip = Hit + new Vector3(-0.35f, 0f, 0f);
            var drill = HornPart(b, Drill, DrillGrey, from, tip - new Vector3(1.6f, 0f, 0f), 0.4f, 2.0f, new Vector3(1.0f, 1.6f, 1.0f), 1500f);
            drill.posCurve = C(0.4f, 0f, 0.95f, 0.02f, 1.3f, 1f, 1.9f, 1.06f, 2.0f, 1.1f);
            drill.posArc = Vector3.zero;
            // Estrías naranjas girando alrededor del taladro (como la estela del juego).
            var stripes = b.MeshPart("DrillStripes", BotwMeshes.Arc, Mat(Fire, "Stripe"), new Vector3(0f, 0.6f, 0f), new Vector3(-90f, 0f, 0f), new Vector3(0.62f, 0.62f, 4f), drill.part, 4);
            var st = b.Track(stripes.transform, stripes, 0.95f, 1.95f);
            st.eulerFrom = new Vector3(-90f, 0f, 0f);
            st.spin = new Vector3(0f, -1400f, 0f);
            Charge(b, from, Gold, 0.35f, 0.6f, 1.2f, 0.9f, false);
            Projectile(b, from + new Vector3(-0.4f, 0f, 0f), tip - new Vector3(1.4f, 0f, 0f), Fire, 0.95f, 0.35f, 0.05f, 1.1f); // solo la estela
            SpeedLines(b, tip, Mint, 0.95f, 0.95f, 3.6f, 100f, 1.2f);
            // Rozamiento: chispas continuas en la punta mientras taladra.
            b.Cue(b.Ps("DrillSparks", Mat(Gold, "Spark"), tip)
                .Sphere(0.25f).Speed(6f, 12f).Drag(3f).Gravity(1f).Life(0.15f, 0.4f).Size(0.06f, 0.12f).Rate(160f).Duration(0.65f)
                .Stretch(6f, 0.05f).AlphaOverLife(0f, 1f, 0.6f, 0.8f, 1f, 0f).Order(6), 1.3f);
            b.Cue(b.Ps("DrillGlow", Mat(White, "Star"), tip)
                .Life(0.09f).Size(1.6f, 2.4f).Rate(22f).Duration(0.6f).Rotation(0f, 90f)
                .AlphaOverLife(0f, 1f, 1f, 0f).Order(7), 1.3f);
            ImpactStar(b, tip, Gold, 1.95f, 5.6f, 16, 50);
            Flash(b, tip, White, 1.95f, 4f);
            Shockwave(b, Target, Gold, 1.96f, 4.6f, true);
            Debris(b, Target + new Vector3(0f, 0.4f, 0f), 1.96f, 14, 9f);
            Smoke(b, TargetChest, BotwMaterials.Get("EM_Dust"), 1.98f, 10, 1.2f, 0.5f, 0.8f, 1.6f);
            HitShell(b, Target, Gold, 1.3f, 0.75f);
            b.Light(tip + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.85f, 0.5f), 11f,
                0f, 0f, 1.25f, 0f, 1.32f, 3.5f, 1.9f, 3f, 1.97f, 7f, 2.3f, 2f, 2.8f, 0f);
            b.Shake(1.3f, 0.15f, 0.65f);
            b.Shake(1.95f, 0.5f, 0.5f);
            b.SlowMo(1.96f, 0.15f, 0.16f);
        }

        // ---------------------------------------------------------------- 033 Placaje
        // Refs: anime = nube de polvo marrón con rocas al chocar; juego = estallido amarillo de pinchos sobre el rival.
        static void M033(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.45f, 2.3f, Target, 0.85f);
            b.Baked(1.3f); // las nubecillas del clip marcan la carrera
            Dust(b, Attacker, 0.75f, 0.9f, 10, 0.9f);
            Streaks(b, AttackerChest + new Vector3(0.5f, 0f, 0f), Hit, White, 0.95f, 0.35f, 70f, 0.6f, 1.2f, 0.22f);
            SpeedLines(b, Hit, White, 1.0f, 0.3f, 3f, 70f);
            ImpactStar(b, Hit, Gold, 1.3f, 4.8f, 14, 36);
            Smoke(b, Target + new Vector3(-0.4f, 0.4f, 0f), BotwMaterials.Get("EM_Dust"), 1.32f, 12, 1.2f, 0.4f, 0.9f, 1.5f);
            Debris(b, Hit, 1.31f, 16, 8f, 1.2f);
            Shockwave(b, Target, Cream, 1.32f, 3.8f);
            HitShell(b, Target, Gold, 1.3f, 0.4f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.9f, 0.5f), 10f, 0f, 0f, 1.27f, 0f, 1.32f, 5f, 1.6f, 1.6f, 2.3f, 0f);
            b.Shake(1.3f, 0.35f, 0.35f);
            b.SlowMo(1.31f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 034 Golpe Cuerpo
        // Refs: anime = cuerpo enorme que cae desde arriba; juego = rayas negras verticales que caen y sombra en el suelo.
        static void M034(EmeraldMoveBuilder b)
        {
            b.Keys(1.0f, 1.3f, 1.45f, 2.5f, Target, 1f); // anticipación = rayas cayendo y sombra creciendo
            // El disco rojo del clip bajo el rival no aparece en las referencias.
            b.HideBaked();
            Dust(b, Attacker, 0.4f, 1f, 12, 1f);
            Shockwave(b, Attacker, Cream, 0.42f, 1.6f);
            GroundShadow(b, Target, 0.6f, 1.3f, 0.3f, 2.8f);
            b.Cue(b.Ps("FallLines", Mat(Ink, "Spark"), Target + new Vector3(-0.2f, 5f, 0f))
                .Box(new Vector3(2.2f, 0.5f, 1.6f)).Velocity(new Vector3(0f, -14f, 0f)).Life(0.3f, 0.38f).Size(0.13f, 0.22f)
                .Rate(90f).Duration(0.72f).Stretch(12f, 0.04f).AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0.6f).Order(3), 0.55f);
            ImpactStar(b, Hit + new Vector3(0f, -0.15f, 0f), Cream, 1.3f, 4.8f, 14, 40);
            Shockwave(b, Target, Cream, 1.31f, 5.4f, true);
            Dust(b, Target, 1.32f, 1.6f, 22, 1.5f);
            Smoke(b, Target + new Vector3(0f, 0.3f, 0f), BotwMaterials.Get("EM_Dust"), 1.33f, 10, 1.3f, 0.3f, 1.2f, 1.5f);
            Debris(b, Target + new Vector3(0f, 0.2f, 0f), 1.32f, 16, 9f, 1.2f);
            HitShell(b, Target, Cream, 1.3f, 0.45f);
            b.Light(Hit + new Vector3(-0.6f, 0.6f, 0f), Lc(1f, 0.9f, 0.7f), 11f, 0f, 0f, 1.27f, 0f, 1.32f, 6f, 1.7f, 2f, 2.5f, 0f);
            b.Shake(1.3f, 0.55f, 0.55f);
            b.SlowMo(1.31f, 0.2f, 0.12f);
        }

        // ---------------------------------------------------------------- 035 Constricción
        // Refs: anime = cuerpos largos que se enroscan; juego = espiral lila que envuelve al rival y aro blanco-verdoso en el suelo.
        static void M035(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.3f, 1.8f, 2.6f, Target, 0.5f);
            b.Baked(1.3f);
            Aura(b, Attacker, Lilac, 0.2f, 1.0f, 1f, 18f, false);
            GroundGlow(b, Target, Mint, 0.95f, 2.7f, 1.9f);
            // Espiral: baja girando, se cierra sobre el rival y aprieta tres veces.
            var basePos = Target + new Vector3(0f, 0.85f, 0f);
            var coil = b.MeshPart("Coil", Helix, Mat(LilacSolid, "Solid"), basePos, Vector3.zero, new Vector3(1.3f, 1.4f, 1.3f), order: 4);
            var tr = b.Track(coil.transform, coil, 0.95f, 2.75f);
            tr.posFrom = basePos + new Vector3(0f, 1.6f, 0f);
            tr.posTo = basePos;
            tr.posCurve = C(0.95f, 0f, 1.25f, 1f);
            tr.scaleFrom = new Vector3(1.6f, 1.7f, 1.6f);
            tr.scaleTo = new Vector3(0.72f, 1.5f, 0.72f);
            tr.scaleCurve = C(0.95f, 0f, 1.3f, 1f, 1.5f, 1f, 1.55f, 1.12f, 1.65f, 1f, 1.85f, 1f, 1.9f, 1.12f, 2.0f, 1f, 2.2f, 1f, 2.25f, 1.14f, 2.35f, 1f);
            tr.spin = new Vector3(0f, -240f, 0f);
            tr.property = "_Erosion";
            tr.propertyCurve = C(0.95f, 0.6f, 1.05f, 0f, 2.5f, 0f, 2.75f, 1f);
            Shockwave(b, Target, Mint, 1.3f, 3.2f);
            HitShell(b, Target, Lilac, 1.3f, 0.4f);
            var squeezes = new[] { 1.55f, 1.9f, 2.25f };
            HitStars(b, Hit + new Vector3(0f, 0.2f, 0f), Lilac, squeezes, 2.2f, 10, 0.35f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(0.95f, 0.8f, 1f), 9f,
                0f, 0f, 1.0f, 0.8f, 1.3f, 2.5f, 1.56f, 3f, 1.75f, 1.2f, 1.91f, 3f, 2.1f, 1.2f, 2.26f, 3f, 2.7f, 0f);
            b.Shake(1.3f, 0.15f, 0.2f);
            foreach (float t in squeezes)
                b.Shake(t, 0.14f, 0.15f);
        }

        // ---------------------------------------------------------------- 036 Derribo
        // Refs: anime = líneas de velocidad horizontales amarillo-blancas sobre azul; juego = contorno amarillo brillante alrededor del usuario que carga.
        static void M036(EmeraldMoveBuilder b)
        {
            b.Keys(0.65f, 1.3f, 1.5f, 2.4f, Target, 0.9f);
            b.Baked(1.3f);
            Aura(b, Attacker, Gold, 0.15f, 1.25f, 1.1f, 45f);
            Streaks(b, AttackerChest + new Vector3(-0.8f, 0.1f, 0f), Hit, Gold, 0.85f, 0.45f, 80f, 0.8f, 1.4f, 0.24f);
            SpeedLines(b, Hit, White, 1.0f, 0.3f, 3.2f, 80f);
            ImpactStar(b, Hit, Gold, 1.3f, 4.6f, 12, 36);
            Shockwave(b, Target, Gold, 1.32f, 4f);
            Dust(b, Target, 1.33f, 1.2f, 16, 1.2f);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.32f, 10, 7f);
            HitShell(b, Target, Gold, 1.3f, 0.4f);
            // Retroceso: el usuario también se lleva un golpe.
            HitShell(b, Attacker, Red, 1.5f, 0.35f);
            Flash(b, AttackerChest + new Vector3(0.2f, 0.2f, -0.2f), Red, 1.5f, 1.8f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.85f, 0.4f), 10f, 0f, 0f, 0.2f, 1.2f, 1.2f, 2f, 1.32f, 5.5f, 1.7f, 1.8f, 2.4f, 0f);
            b.Shake(1.3f, 0.42f, 0.4f);
            b.Shake(1.5f, 0.15f, 0.15f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 037 Saña / Golpiza
        // Refs: anime = estallido radial rojo y blanco de fondo; juego = estrellas de pinchos naranja-amarillas con puño y rayas blancas.
        static void M037(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.1f, 1.62f, 2.6f, Target, 0.8f);
            b.Baked(1.1f);
            Aura(b, Attacker, Red, 0.1f, 0.95f, 1.05f, 50f);
            var hits = new[] { 1.1f, 1.35f, 1.6f };
            Streaks(b, AttackerChest + new Vector3(0.6f, 0.3f, 0f), Hit, White, 0.95f, 0.65f, 50f, 0.6f, 1.2f, 0.22f);
            HitStars(b, Hit, Orange, hits, 3.8f, 14, 0.4f);
            Bursts(Rays(b, Hit, Red, hits[0], 12, 6f, 0.3f), 12, hits);
            Flash(b, Hit, White, 1.6f, 3.4f);
            Shockwave(b, Target, Orange, 1.62f, 4f);
            Dust(b, Target, 1.62f, 1.1f, 12);
            HitShell(b, Target, Orange, 1.1f, 0.7f);
            // Tras la furia, el usuario queda confuso.
            Dizzy(b, AttackerHead, Gold, 2.0f, 1.2f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.7f, 0.4f), 10f,
                0f, 0f, 1.08f, 0f, 1.12f, 3.5f, 1.25f, 1.2f, 1.37f, 3.5f, 1.5f, 1.2f, 1.62f, 5.5f, 2f, 1.5f, 2.6f, 0f);
            foreach (float t in hits)
                b.Shake(t, t > 1.5f ? 0.38f : 0.22f, 0.22f);
            b.SlowMo(1.61f, 0.35f, 0.07f);
        }

        // ---------------------------------------------------------------- 038 Doble Filo
        // Refs: anime = líneas de velocidad menta-cian y resplandor blanco; juego = esfera de luz naranja-roja enorme con estrella y ascuas.
        static void M038(EmeraldMoveBuilder b)
        {
            b.Keys(0.6f, 1.3f, 1.45f, 2.5f, Target, 1f);
            b.Baked(1.3f); // las llamas doradas del clip en el usuario son la carga
            Aura(b, Attacker, Gold, 0.15f, 1.2f, 1.15f, 50f);
            Streaks(b, AttackerChest + new Vector3(-0.6f, 0.1f, 0f), Hit, Mint, 0.85f, 0.45f, 80f, 0.8f, 1.4f, 0.24f);
            b.Cue(b.Ps("BlastSphere", Mat(Ember, "Glow"), Hit)
                .Life(0.55f).Size(5.5f).Burst(1)
                .SizeOverLife(C(0f, 0.3f, 0.2f, 1f, 1f, 1.1f)).AlphaOverLife(0f, 1f, 0.5f, 0.9f, 1f, 0f).Order(3), 1.3f);
            ImpactStar(b, Hit, Fire, 1.3f, 5f, 14, 30);
            b.Cue(b.Ps("BlastEmbers", BotwMaterials.Get("EX_Ember"), Hit)
                .Sphere(0.8f).Speed(2f, 7f).Drag(2f).Life(0.6f, 1.3f).Size(0.07f, 0.14f).Burst(45).Velocity(new Vector3(0f, 0.6f, 0f))
                .AlphaOverLife(0f, 1f, 0.7f, 1f, 1f, 0f).Order(4), 1.32f);
            Shockwave(b, Target, Orange, 1.32f, 4.6f, true);
            Dust(b, Target, 1.33f, 1.2f, 14, 1.2f);
            HitShell(b, Target, Fire, 1.3f, 0.45f);
            // Retroceso dorado sobre el usuario.
            HitShell(b, Attacker, Orange, 1.5f, 0.4f);
            Flash(b, AttackerChest + new Vector3(0.2f, 0.2f, -0.2f), Orange, 1.5f, 2f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.62f, 0.3f), 12f, 0f, 0f, 0.2f, 1.5f, 1.2f, 2f, 1.32f, 7f, 1.8f, 2.5f, 2.5f, 0f);
            b.Shake(1.3f, 0.5f, 0.5f);
            b.Shake(1.5f, 0.15f, 0.15f);
            b.SlowMo(1.31f, 0.2f, 0.12f);
        }

        // ---------------------------------------------------------------- 039 Agitacola / Látigo
        // Refs: anime = sin efecto visible (escena tranquila); juego = destellos de cuatro puntas amarillos, cian y rosas alrededor y un barrido de cola crema.
        // Movimiento de estado (baja la Defensa): sin impacto, sin cámara lenta, sin sacudida.
        static void M039(EmeraldMoveBuilder b)
        {
            b.Keys(0.5f, 1.0f, 1.35f, 2.4f, TargetChest, 0f);
            // El clip añade corazones (no salen en las referencias): el barrido y los destellos propios lo sustituyen.
            b.HideBaked();
            var colors = new[] { new Color(1f, 0.95f, 0.45f), new Color(0.5f, 1f, 1f), new Color(1f, 0.55f, 0.85f) };
            Sparkles(b, AttackerChest + new Vector3(0.3f, 0.6f, 0f), 0.3f, 1.6f, 1.5f, 24f, 1.7f, colors);
            Sparkles(b, TargetChest + new Vector3(-0.3f, 0.4f, 0f), 0.9f, 1.3f, 1.2f, 12f, 1.4f, colors);
            // Meneo de cola: dos barridos cortos crema a un lado del usuario.
            Bursts(Slash(b, Attacker + new Vector3(0.3f, 0.45f, -0.5f), Cream, 0.55f, 200f, 0.9f, 0.3f, 260f), 1, 0.55f, 0.85f, 1.15f);
            Glints(b, TargetChest + new Vector3(0f, 0.5f, 0f), Cyan, 1.2f, 0.8f, 0.8f, 8f, 0.8f);
            b.Light(AttackerChest + new Vector3(0.4f, 0.8f, -0.4f), Lc(1f, 0.95f, 0.85f), 7f, 0f, 0f, 0.3f, 1f, 1.5f, 1f, 2.4f, 0f);
        }

        // ---------------------------------------------------------------- 040 Picotazo Veneno
        // Refs: anime = agujas magenta rodeadas de aros blancos en espiral sobre fondo azul; juego = aguja violeta plana que vuela hacia el rival.
        static void M040(EmeraldMoveBuilder b)
        {
            b.Keys(0.65f, 1.3f, 1.45f, 2.5f, Target, 0.6f);
            b.Baked(1.3f); // las agujas rosas del clip delante del usuario son la anticipación
            Charge(b, AttackerChest + new Vector3(0.6f, 0.1f, 0f), Violet, 0.3f, 0.6f, 1.1f, 0.9f, false);
            Needles(b, AttackerChest + new Vector3(0.7f, 0.15f, 0f), Hit + new Vector3(-0.2f, 0f, 0f), Violet, White, 0.95f, 0.3f, 22f, 0.32f, 1.5f);
            ImpactStar(b, Hit, Violet, 1.3f, 3.4f, 8, 20);
            b.Cue(b.Ps("PoisonBubbles", BotwMaterials.Get("EM_Bubble"), Target + new Vector3(0f, 0.4f, 0f))
                .Sphere(0.55f).Velocity(new Vector3(0f, 0.9f, 0f)).Speed(0.1f, 0.5f).Life(0.6f, 1f).Size(0.14f, 0.3f)
                .Rate(18f).Duration(1.0f).SizeOverLife(C(0f, 0.3f, 0.3f, 1f, 1f, 1.1f))
                .AlphaOverLife(0f, 1f, 0.8f, 1f, 1f, 0f).Order(4), 1.35f);
            Smoke(b, TargetChest, BotwMaterials.Get("EM_PoisonSmoke"), 1.35f, 8, 0.8f, 0.6f, 0.5f, 1.4f);
            HitShell(b, Target, Violet, 1.3f, 0.9f);
            Shockwave(b, Target, Violet, 1.32f, 3.2f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(0.9f, 0.6f, 1f), 9f, 0f, 0f, 0.3f, 0.8f, 0.9f, 1f, 1.28f, 0.5f, 1.32f, 4f, 1.7f, 1.4f, 2.5f, 0f);
            b.Shake(1.3f, 0.22f, 0.25f);
        }
    }
}
