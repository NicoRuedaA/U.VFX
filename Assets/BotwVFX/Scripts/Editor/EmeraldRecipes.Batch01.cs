using UnityEngine;
using static BotwVfx.EditorTools.EmeraldLayers;
using static BotwVfx.EditorTools.EmeraldMoveBuilder;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Lote 1 (movimientos 001-020). Cada receta sale de sus dos referencias (captura del anime y del
    /// juego, ver el comentario de cada una) y de la descripción del movimiento.
    /// Timeline común: 3,6 s · anticipación en el atacante · impacto ~1,3 s · pico · disipación lenta.
    /// </summary>
    public static partial class EmeraldRecipes
    {
        // Punto de impacto: delante del objetivo, del lado del atacante (queda visible desde la vista de combate).
        static readonly Vector3 Hit = new Vector3(2.62f, 0.95f, -0.12f);
        // Mano/puño adelantado del atacante.
        static readonly Vector3 Fist = new Vector3(-2.4f, 0.95f, -0.15f);
        // Eje "derecha de pantalla" de compromiso entre la vista de combate y la frontal de la galería.
        static readonly Vector3 ScreenRight = new Vector3(0.8f, 0f, -0.6f);

        static Color Lc(float r, float g, float b) => new Color(r, g, b);

        // ---------------------------------------------------------------- 001 Destructor
        // Refs: anime = coletazo con líneas radiales blancas y estallido rosa-blanco; juego = estallido amarillo-naranja sobre el rival.
        static void M001(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.3f, 1.45f, 2.3f, Target, 0.6f);
            b.Baked(1.3f);
            Aura(b, Attacker, Cream, 0.25f, 1.15f, 1f, 30f);
            Streaks(b, AttackerChest + new Vector3(0.6f, 0.1f, 0f), Hit, White, 1.02f, 0.22f, 60f, 0.35f);
            SpeedLines(b, Hit, White, 1.0f, 0.55f, 3.2f, 90f);
            ImpactStar(b, Hit, Cream, 1.3f, 3.8f, 10, 30);
            Flash(b, Hit, White, 1.3f, 2.6f);
            HitShell(b, Target, Cream, 1.3f, 0.35f);
            Shockwave(b, Target, Cream, 1.32f, 3f);
            Dust(b, Target, 1.33f, 1f, 12);
            Glints(b, Hit, Cream, 1.42f, 0.6f, 1.1f, 22f);
            b.Light(Hit + new Vector3(-0.5f, 0.3f, 0f), Lc(1f, 0.86f, 0.6f), 9f, 0f, 0f, 1.28f, 0f, 1.33f, 4.5f, 1.5f, 2f, 2.1f, 0f);
            b.Shake(1.3f, 0.25f, 0.3f);
        }

        // ---------------------------------------------------------------- 002 Golpe Kárate
        // Refs: anime = tajo de canto con rayas de movimiento blancas; juego = mano naranja con estallido ardiente naranja-amarillo.
        static void M002(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.3f, 1.45f, 2.3f, Target, 1f);
            b.Baked(1.3f);
            Aura(b, Attacker, Orange, 0.2f, 1.15f, 1f, 40f);
            Charge(b, Fist + new Vector3(0f, 0.45f, 0f), Orange, 0.35f, 0.7f, 1.2f, 1f);
            Streaks(b, Fist, Hit, White, 1.05f, 0.22f, 70f, 0.4f, 1.2f);
            Slash(b, Hit + new Vector3(0f, 0.2f, 0f), Orange, 1.22f, -60f, 1.4f, 0.3f, -200f);
            CutLine(b, Hit, White, 1.3f, -35f, 4.5f, 0.3f);
            ImpactStar(b, Hit, Orange, 1.3f, 4.2f, 12, 40);
            Flash(b, Hit, White, 1.3f, 3f);
            Sparks(b, Hit, Fire, 1.31f, 26, 8f, 15f, 1.2f, 1.5f);
            HitShell(b, Target, Orange, 1.3f, 0.4f);
            Shockwave(b, Target, Orange, 1.32f, 3.4f);
            Dust(b, Target, 1.33f, 1.1f, 14, 1.1f);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.33f, 10, 7f);
            b.Light(Hit + new Vector3(-0.5f, 0.3f, 0f), Lc(1f, 0.65f, 0.3f), 9f, 0f, 0f, 1.28f, 0f, 1.33f, 5f, 1.5f, 2.2f, 2.2f, 0f);
            b.Shake(1.3f, 0.38f, 0.4f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 003 Doble Bofetón
        // Refs: anime = fondo de líneas de velocidad magenta; juego = guante blanco/mano amarilla y estrellas puntiagudas naranja-amarillas.
        static void M003(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.1f, 1.55f, 2.4f, Target, 0.7f);
            b.BakedWindow(0.5f, 2f); // golpes del clip (0,60/0,75/0,88/1,03 s) -> 1,10/1,25/1,38/1,53 s
            Aura(b, Attacker, Pink, 0.25f, 1.0f, 1f, 25f);
            SpeedLines(b, TargetChest, Pink, 0.95f, 0.75f, 3.4f, 110f);
            float[] hits = { 1.1f, 1.25f, 1.38f, 1.53f };
            for (int i = 0; i < hits.Length; i++)
            {
                float side = i % 2 == 0 ? 1f : -1f;
                bool last = i == hits.Length - 1;
                var p = TargetChest + new Vector3(-0.35f, 0.15f, 0f) + ScreenRight * (0.45f * side);
                Slash(b, p, Pink, hits[i] - 0.05f, side > 0f ? 160f : 20f, 1.1f, 0.2f, side * 300f);
                ImpactStar(b, p, Gold, hits[i], last ? 3.6f : 2.6f, last ? 10 : 0, last ? 24 : 0);
                if (last)
                    Flash(b, p, White, hits[i], 2.4f);
                b.Shake(hits[i], last ? 0.25f : 0.14f, 0.2f);
            }
            HitShell(b, Target, Cream, 1.1f, 0.6f);
            Dust(b, Target, 1.55f, 0.9f, 10);
            Glints(b, TargetHead + new Vector3(0f, 0.3f, 0f), Gold, 1.6f, 0.8f, 0.7f, 14f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.4f, 0f), Lc(1f, 0.85f, 0.5f), 8f,
                0f, 0f, 1.08f, 0f, 1.12f, 3f, 1.2f, 1.5f, 1.26f, 3f, 1.35f, 1.5f, 1.39f, 3f, 1.48f, 1.5f, 1.54f, 4f, 1.8f, 1.2f, 2.3f, 0f);
        }

        // ---------------------------------------------------------------- 004 Puño Cometa
        // Refs: anime = puñetazos sobre líneas de velocidad verdes; juego = varias estrellas puntiagudas naranja-amarillas sobre el rival.
        static void M004(EmeraldMoveBuilder b)
        {
            b.Keys(0.9f, 1.66f, 1.8f, 2.6f, Target, 0.75f);
            b.Baked(1.66f);
            Aura(b, Attacker, Gold, 0.2f, 1.7f, 1f, 30f);
            SpeedLines(b, TargetChest, Cyan, 0.85f, 1.0f, 3.3f, 90f);
            float[] hits = { 1.0f, 1.16f, 1.32f, 1.48f, 1.66f };
            for (int i = 0; i < hits.Length; i++)
            {
                bool last = i == hits.Length - 1;
                float side = (i % 2 == 0 ? 1f : -1f) * (last ? 0f : 1f);
                var target = TargetChest + new Vector3(-0.35f, 0.25f - 0.12f * i, 0f) + ScreenRight * (0.4f * side);
                var from = Fist + new Vector3(0.3f, 0.15f * side, 0f);
                Projectile(b, from, target, Gold, hits[i] - 0.22f, 0.22f, last ? 1.0f : 0.7f, last ? 0.55f : 0.35f, "Star");
                ImpactStar(b, target, Orange, hits[i], last ? 4.2f : 2.4f, last ? 12 : 0, last ? 40 : 0);
                if (last)
                    Flash(b, target, White, hits[i], 3f);
                b.Shake(hits[i], last ? 0.32f : 0.12f, last ? 0.35f : 0.15f);
            }
            HitShell(b, Target, Gold, 1.66f, 0.4f);
            Shockwave(b, Target, Orange, 1.68f, 3.2f);
            Dust(b, Target, 1.68f, 1f, 12);
            b.Light(TargetChest + new Vector3(-0.8f, 0.4f, 0f), Lc(1f, 0.8f, 0.4f), 9f,
                0f, 0f, 0.98f, 0f, 1.02f, 2.5f, 1.12f, 1f, 1.18f, 2.5f, 1.28f, 1f, 1.34f, 2.5f, 1.44f, 1f, 1.5f, 2.5f, 1.62f, 1f, 1.68f, 5f, 1.9f, 2f, 2.5f, 0f);
            b.SlowMo(1.67f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 005 Megapuño
        // Refs: anime = líneas de velocidad rojas; juego = estallido amarillo-naranja que llena la pantalla, rayos rojos/blancos y puño.
        static void M005(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.35f, 1.5f, 2.5f, Target, 1f);
            b.Baked(1.35f);
            var windup = new Vector3(-3.3f, 1.0f, -0.35f);
            Aura(b, Attacker, Gold, 0.1f, 1.2f, 1.1f, 55f);
            Charge(b, windup, Gold, 0.15f, 0.85f, 1.8f, 1.6f);
            Glints(b, windup, Gold, 0.2f, 0.8f, 0.9f, 25f);
            Projectile(b, Fist, Hit, Gold, 1.12f, 0.23f, 1.3f, 0.9f);
            Streaks(b, Fist, Hit, Gold, 1.05f, 0.28f, 90f, 0.5f, 1.4f);
            SpeedLines(b, Hit, Red, 1.0f, 0.4f, 4f, 120f, 1.4f);
            ImpactStar(b, Hit, Gold, 1.35f, 5.5f, 16, 60);
            Flash(b, Hit, White, 1.35f, 4.2f);
            FireBurst(b, Hit, 1.37f, 1.1f, false); // la referencia no deja humo
            Rays(b, Hit, Red, 1.36f, 12, 5.5f);
            Rays(b, Hit, White, 1.37f, 10, 6.5f);
            HitShell(b, Target, Gold, 1.35f, 0.45f);
            Shockwave(b, Target, Gold, 1.36f, 4.5f, true);
            Dust(b, Target, 1.37f, 1.3f, 18, 1.3f);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.36f, 14, 9f);
            b.Light(Hit + new Vector3(-0.6f, 0.4f, 0f), Lc(1f, 0.78f, 0.35f), 12f,
                0f, 0f, 0.2f, 0.8f, 0.9f, 1.5f, 1.3f, 0f, 1.37f, 7f, 1.6f, 3f, 2.4f, 0f);
            b.Shake(1.35f, 0.5f, 0.5f);
            b.SlowMo(1.36f, 0.2f, 0.14f);
        }

        // ---------------------------------------------------------------- 006 Día de Pago
        // Refs: anime = orbes/monedas blancas brillantes y líneas radiales; juego = moneda dorada grande y destellos verdes y amarillos.
        static void M006(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.4f, 1.6f, 2.7f, Target, 0.5f);
            b.Baked(1.4f);
            Charge(b, AttackerHead + new Vector3(0.15f, 0.05f, 0f), Gold, 0.2f, 0.6f, 0.9f, 0.9f, false);
            Aura(b, Attacker, Gold, 0.2f, 1.0f, 1f, 20f);

            // Abanico de monedas (mallas sólidas) con parábola, rebote en el suelo y brillo.
            var coins = b.Ps("Coins", Mat(CoinGold, "Solid"), AttackerHand + new Vector3(0.2f, 0.3f, 0f))
                .Mesh(Coin).Speed(9f, 10.5f).Life(1.8f, 2.2f).Size(0.7f, 0.85f).Rate(40f).Duration(0.25f)
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Spin(-720f, 720f, true).Gravity(0.55f).Collide(0.5f, 0.25f)
                .SizeOverLife(C(0f, 1f, 0.85f, 1f, 1f, 0f)).Order(3);
            var shape = coins.ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 14f;
            shape.radius = 0.1f;
            shape.rotation = new Vector3(-12f, 90f, 0f);
            b.Cue(coins, 0.78f);
            var bigCoin = b.Ps("BigCoin", Mat(CoinGold, "Solid"), AttackerHand + new Vector3(0.2f, 0.4f, 0f))
                .Mesh(Coin).Life(2.0f).Size(1.8f).Burst(1).Velocity(new Vector3(10f, 1.4f, 0f)).Gravity(0.5f).Collide(0.45f, 0.25f)
                .Rotation3D(new Vector3(90f, 0f, 0f), new Vector3(90f, 0f, 0f)).Spin(500f, 600f, true)
                .SizeOverLife(C(0f, 1f, 0.85f, 1f, 1f, 0f)).Order(3);
            b.Cue(bigCoin, 1.05f);
            Glints(b, AttackerHand + new Vector3(1.5f, 0.6f, 0f), Gold, 0.85f, 0.5f, 1.2f, 20f);

            ImpactStar(b, Hit, Gold, 1.4f, 2.8f, 8, 24);
            ImpactStar(b, Hit + new Vector3(0f, 0.2f, 0f), Gold, 1.62f, 3.4f, 10, 30);
            Flash(b, Hit, White, 1.62f, 2.4f);
            HitShell(b, Target, Gold, 1.4f, 0.4f);
            Glints(b, Target + new Vector3(0.6f, 0.4f, 0f), Gold, 1.7f, 1.5f, 1.4f, 26f);
            Glints(b, Target + new Vector3(0.3f, 0.8f, 0f), Mint, 1.6f, 1.2f, 1.2f, 10f, 0.8f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(1f, 0.85f, 0.4f), 9f,
                0f, 0f, 0.2f, 1f, 0.8f, 1.5f, 1.38f, 0.5f, 1.42f, 3f, 1.55f, 1.2f, 1.64f, 4f, 2f, 1.5f, 2.8f, 0f);
            b.Shake(1.4f, 0.12f, 0.2f);
            b.Shake(1.62f, 0.2f, 0.25f);
        }

        // ---------------------------------------------------------------- 007 Puño Fuego
        // Refs: anime = puño envuelto en una bola de fuego amarilla-naranja; juego = estallido de fuego amarillo que llena la pantalla.
        static void M007(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.3f, 1.45f, 2.5f, Target, 1f);
            b.Baked(1.3f);
            var fist = Fist + new Vector3(0.1f, 0.1f, -0.1f);
            Flames(b, fist, 0.15f, 0.95f, 0.25f, 1.1f, 60f);
            Charge(b, fist, Fire, 0.15f, 0.8f, 1.4f, 1.3f, false);
            Aura(b, Attacker, Fire, 0.2f, 1.15f, 1f, 45f);
            Projectile(b, fist, Hit, Fire, 1.07f, 0.23f, 1.2f, 0.8f);
            FireBurst(b, Hit, 1.3f, 1.3f);
            ImpactStar(b, Hit, Fire, 1.3f, 4.6f, 12, 30);
            Flash(b, Hit, White, 1.3f, 3.2f);
            HitShell(b, Target, Fire, 1.3f, 0.45f);
            Flames(b, TargetChest, 1.45f, 1.0f, 0.45f, 0.8f, 30f);
            Embers(b, Target + new Vector3(0f, 0.8f, 0f), 1.4f, 1.6f, 0.6f, 18f);
            Smoke(b, TargetChest + new Vector3(0f, 0.6f, 0f), BotwMaterials.Get("EX_DarkSmoke"), 1.7f, 5, 1.2f, 0.8f, 0.6f, 1.6f);
            Shockwave(b, Target, Fire, 1.32f, 3.6f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(1f, 0.62f, 0.3f), 11f,
                0f, 0f, 0.15f, 1.5f, 0.95f, 2f, 1.28f, 0.5f, 1.33f, 6f, 1.6f, 3f, 2.5f, 1f, 3f, 0f);
            b.Shake(1.3f, 0.42f, 0.45f);
            b.SlowMo(1.31f, 0.25f, 0.1f);
        }

        // ---------------------------------------------------------------- 008 Puño Hielo
        // Refs: anime = puño con brillo cian helado y burbujas/destellos; juego = puño azul-blanco con cristales de hielo y copos de nieve.
        static void M008(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.3f, 1.45f, 2.55f, Target, 1f);
            b.Baked(1.3f);
            var fist = Fist + new Vector3(0.1f, 0.1f, -0.1f);
            Charge(b, fist, Ice, 0.15f, 0.85f, 1.5f, 1.2f);
            Tornado(b, fist - new Vector3(0f, 0.55f, 0f), Ice, 0.15f, 1.0f, 1.1f, 0.5f, 18f, false);
            Aura(b, Attacker, Ice, 0.2f, 1.15f, 1f, 35f);
            Projectile(b, fist, Hit, Ice, 1.07f, 0.23f, 1.1f, 0.7f);
            ImpactStar(b, Hit, Ice, 1.3f, 4.4f, 12, 24);
            Flash(b, Hit, White, 1.3f, 3f);
            IceBurst(b, Hit, 1.3f, 1.2f, 22);
            Shockwave(b, Target, Ice, 1.32f, 3.4f);

            // Escarcha sobre el objetivo: casco helado + cristales pegados que acaban rompiéndose.
            Aura(b, Target, Ice, 1.35f, 2.75f, 1.05f, 10f);
            b.Cue(b.Ps("IceCrust", BotwMaterials.Get("EM_IceShard"), TargetChest)
                .Mesh(BotwMeshes.Rock).Sphere(0.42f, 0.2f).Life(1.32f).Size3D(new Vector3(0.13f, 0.36f, 0.13f))
                .Rotation3D(Vector3.zero, Vector3.one * 360f).Burst(14)
                .SizeOverLife(C(0f, 0f, 0.12f, 1f, 0.97f, 1f, 1f, 0f)).Order(2), 1.38f);
            Glints(b, Target + new Vector3(0f, 1f, 0f), Ice, 1.4f, 1.3f, 1.1f, 14f);
            IceBurst(b, TargetChest, 2.72f, 0.8f, 14);
            Flash(b, TargetChest, Ice, 2.72f, 2f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(0.6f, 0.85f, 1f), 10f,
                0f, 0f, 0.2f, 1f, 0.9f, 1.5f, 1.28f, 0.3f, 1.33f, 5f, 1.6f, 2f, 2.4f, 0.8f, 2.74f, 3f, 3.1f, 0f);
            b.Shake(1.3f, 0.38f, 0.4f);
            b.Shake(2.72f, 0.15f, 0.2f);
            b.SlowMo(1.31f, 0.3f, 0.08f);
        }

        // ---------------------------------------------------------------- 009 Puño Trueno
        // Refs: anime = rayos amarillos alrededor del puño y destello blanco al contacto; juego = estallido amarillo-verdoso con rayas radiales amarillas.
        static void M009(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.3f, 1.45f, 2.4f, Target, 1f);
            b.Baked(1.3f);
            var fist = Fist + new Vector3(0.1f, 0.1f, -0.1f);
            Charge(b, fist, Electric, 0.15f, 0.85f, 1.4f, 1.1f);
            RadialBolts(b, fist, Electric, 0.3f, 1.05f, 5, 1.1f, 5, 0.15f);
            Crackle(b, fist, Electric, 0.3f, 0.75f, 0.4f, 90f);
            Aura(b, Attacker, Electric, 0.2f, 1.15f, 1f, 40f);
            Bolt(b, fist, Hit, Electric, 1.08f, 1.32f, 0.3f, 12, 0.45f, 3, 7);
            ImpactStar(b, Hit, Electric, 1.3f, 4.6f, 16, 40);
            Flash(b, Hit, White, 1.3f, 3.4f);
            RadialBolts(b, Hit, Electric, 1.3f, 1.75f, 8, 2.8f, 11, 0.3f);
            var shell = Aura(b, Target, Electric, 1.32f, 2.3f, 1.05f, 20f);
            shell.property = "_Opacity";
            shell.propertyCurve = Flicker(1.32f, 2.3f, 0.05f, 0, 2, 1f, 0.25f);
            Crackle(b, TargetChest, Electric, 1.35f, 0.9f, 0.6f, 80f);
            RadialBolts(b, TargetChest, Electric, 1.8f, 2.3f, 4, 1.2f, 21, 0.16f);
            Shockwave(b, Target, Electric, 1.32f, 3.6f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(1f, 0.95f, 0.55f), 11f,
                0f, 0f, 0.2f, 1.2f, 0.9f, 1.8f, 1.28f, 0.4f, 1.32f, 6f, 1.45f, 2f, 1.55f, 4f, 1.7f, 1.5f, 2.3f, 0f);
            b.Shake(1.3f, 0.42f, 0.45f);
            b.SlowMo(1.31f, 0.25f, 0.1f);
        }

        // ---------------------------------------------------------------- 010 Arañazo
        // Refs: anime = arco blanco curvo que sigue a la garra; juego = tres zarpazos diagonales amarillo-naranja sobre el rival.
        static void M010(EmeraldMoveBuilder b)
        {
            b.Keys(0.8f, 1.3f, 1.42f, 2.1f, Target, 0.4f);
            b.Baked(1.3f);
            Flash(b, AttackerHand + new Vector3(0f, 0.5f, 0f), White, 0.45f, 1.2f);
            Streaks(b, AttackerHand, Hit, White, 1.05f, 0.2f, 50f, 0.3f);
            Slash(b, Hit, White, 1.24f, 120f, 1.3f, 0.25f, 300f);
            for (int i = 0; i < 3; i++)
                CutLine(b, Hit + ScreenRight * (0.4f * (i - 1)) + new Vector3(0f, 0.06f * (1 - i), 0f), Gold, 1.28f + 0.04f * i, 35f, 3.2f, 0.34f, 0.4f);
            ImpactStar(b, Hit, Cream, 1.3f, 1.8f, 6, 16);
            HitShell(b, Target, White, 1.3f, 0.3f);
            Dust(b, Target, 1.32f, 0.8f, 6, 0.8f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(1f, 0.92f, 0.75f), 8f, 0f, 0f, 1.27f, 0f, 1.32f, 3f, 1.6f, 0.6f, 2f, 0f);
            b.Shake(1.3f, 0.14f, 0.2f);
        }

        // ---------------------------------------------------------------- 011 Agarre
        // Refs: anime = salto sobre fondo de velocidad azul; juego = dos medias lunas naranjas que se cierran con un destello blanco entre ellas.
        static void M011(EmeraldMoveBuilder b)
        {
            b.Keys(0.9f, 1.3f, 1.45f, 2.4f, Target, 0.7f);
            b.Baked(1.3f);
            Aura(b, Attacker, Orange, 0.2f, 1.0f, 1f, 30f);

            // Pinzas: dos medias lunas enfrentadas "( )" que se cierran y aprietan tres veces.
            var pivot = b.Node("Pincers", TargetChest + new Vector3(-0.15f, 0.1f, 0f));
            pivot.localRotation = Quaternion.Euler(0f, 45f, 0f);
            var squeeze = C(1.0f, 0f, 1.28f, 1f, 1.55f, 1f, 1.62f, 1.15f, 1.72f, 1f, 1.85f, 1f, 1.92f, 1.15f, 2.02f, 1f, 2.15f, 1f, 2.22f, 1.18f, 2.32f, 1f, 2.45f, 1f, 2.62f, 0.4f);
            for (int s = -1; s <= 1; s += 2)
            {
                var r = b.MeshPart("Pincer", Crescent, Mat(Fire, "Blade"), new Vector3(1.4f * s, 0f, 0f),
                    new Vector3(0f, 0f, s > 0 ? 0f : 180f), Vector3.one * 1.5f, pivot, 5);
                var tr = b.Track(r.transform, r, 1.0f, 2.62f);
                tr.posFrom = new Vector3(1.7f * s, 0f, 0f);
                tr.posTo = new Vector3(0.6f * s, 0f, 0f);
                tr.posCurve = squeeze;
                tr.property = "_Erosion";
                tr.propertyCurve = C(1.0f, 0.7f, 1.1f, 0f, 2.45f, 0f, 2.62f, 1f);
            }
            Flash(b, Hit, White, 1.3f, 2.2f);
            ImpactStar(b, Hit, Gold, 1.3f, 2.4f, 8, 20);
            foreach (float t in new[] { 1.62f, 1.92f, 2.22f })
            {
                Flash(b, Hit, Gold, t, 2.2f);
                Sparks(b, Hit, Orange, t, 10, 4f, 8f);
                b.Shake(t, 0.12f, 0.15f);
            }
            HitShell(b, Target, Orange, 1.3f, 0.4f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(1f, 0.7f, 0.35f), 9f,
                0f, 0f, 1.28f, 0f, 1.32f, 4f, 1.5f, 1.5f, 1.63f, 3f, 1.8f, 1.2f, 1.93f, 3f, 2.1f, 1.2f, 2.23f, 3f, 2.6f, 0f);
            b.Shake(1.3f, 0.25f, 0.3f);
        }

        // ---------------------------------------------------------------- 012 Guillotina
        // Refs: anime = pinza en media luna blanca brillante con líneas radiales azul-violeta; juego = dos cuchillas blancas gigantes que se cierran sobre el rival.
        static void M012(EmeraldMoveBuilder b)
        {
            b.Keys(1.22f, 1.4f, 1.55f, 2.7f, Target, 1f);
            // El clip horneado oscurece con un plano frontal que en la vista de combate se ve como una caja: se oculta.
            b.HideBaked();
            Aura(b, Attacker, Steel, 0.15f, 1.2f, 1f, 35f);
            Charge(b, AttackerHead + new Vector3(0f, 0.4f, 0f), Steel, 0.2f, 0.9f, 1.4f, 1.0f);

            // Dos cuchillas gigantes que se abren y se cierran de golpe en X.
            var pivot = b.Node("Blades", TargetChest + new Vector3(-0.2f, 0.4f, 0f));
            pivot.localRotation = Quaternion.Euler(0f, 45f, 0f);
            var snap = C(0.95f, 0f, 1.2f, 0.1f, 1.33f, 0.2f, 1.4f, 1f);
            for (int s = -1; s <= 1; s += 2)
            {
                float baseZ = s < 0 ? 0f : 180f;
                var r = b.MeshPart("Blade", Crescent, Mat(Steel, "Blade"), new Vector3(0.75f * s, 0f, 0f),
                    new Vector3(0f, 0f, baseZ), Vector3.one * 2f, pivot, 6);
                var tr = b.Track(r.transform, r, 0.95f, 1.75f);
                tr.eulerFrom = new Vector3(0f, 0f, baseZ + 60f * -s);
                tr.eulerTo = new Vector3(0f, 0f, baseZ - 30f * -s);
                tr.rotCurve = snap;
                tr.property = "_Erosion";
                tr.propertyCurve = C(0.95f, 0.8f, 1.05f, 0f, 1.5f, 0f, 1.75f, 1f);
            }
            SpeedLines(b, TargetChest, Ink, 1.36f, 0.4f, 4f, 160f, 1.6f);
            ImpactStar(b, Hit, White, 1.4f, 3.6f, 16, 40);
            Flash(b, Hit, White, 1.4f, 3f);
            CutLine(b, Hit, White, 1.4f, 45f, 5f, 0.35f);
            CutLine(b, Hit, White, 1.41f, -45f, 5f, 0.35f);
            HitShell(b, Target, Steel, 1.4f, 0.5f);
            Shockwave(b, Target, Steel, 1.42f, 4.5f);
            Dust(b, Target, 1.45f, 1.2f, 16, 1.2f);

            // Fuera de combate: estrellas de mareo girando sobre la cabeza.
            b.Cue(b.Ps("DizzyStars", Mat(Gold, "Star"), TargetHead + new Vector3(0f, 0.45f, 0f))
                .GroundCircle(0.45f, 0f).Radial(0f, 6f).Life(1f).Size(0.22f, 0.3f).Rate(6f).Duration(1.4f).Spin(-200f, 200f)
                .AlphaOverLife(0f, 0f, 0.15f, 1f, 0.8f, 1f, 1f, 0f).Order(5), 1.8f);
            b.Light(Hit + new Vector3(-0.6f, 0.5f, 0f), Lc(0.8f, 0.88f, 1f), 12f,
                0f, 0f, 0.3f, 1f, 1.0f, 1.5f, 1.38f, 0f, 1.41f, 8f, 1.7f, 2.5f, 2.6f, 0f);
            b.Shake(1.4f, 0.55f, 0.6f);
            b.SlowMo(1.41f, 0.12f, 0.2f);
        }

        // ---------------------------------------------------------------- 013 Viento Cortante
        // Refs: anime = rayas de viento verdes en diagonal; juego = remolino de viento cian-blanco alrededor del usuario y cuchilla en media luna.
        static void M013(EmeraldMoveBuilder b)
        {
            b.Keys(0.8f, 1.5f, 1.66f, 2.6f, Target, 0.6f);
            b.Baked(1.5f); // las tres cuchillas del clip llegan a 1,50/1,58/1,66 s
            Tornado(b, Attacker, Wind, 0.1f, 1.25f, 2.0f, 0.9f, 26f);
            GroundGlow(b, Attacker, Mint, 0.1f, 1.3f, 1.3f);
            Aura(b, Attacker, Mint, 0.15f, 1.2f, 1f, 20f);
            for (int i = 0; i < 3; i++)
            {
                float arrive = 1.5f + 0.08f * i;
                var p = Hit + new Vector3(0f, 0.25f - 0.2f * i, 0f);
                WindBlade(b, AttackerChest + new Vector3(0.5f, 0.25f, 0f), p, Mint, arrive - 0.3f, 0.3f, 1.3f);
                Slash(b, p, Mint, arrive, 90f + 20f * (i - 1), 1.3f, 0.25f);
                CutLine(b, p, White, arrive, 60f - 30f * i, 3.2f, 0.2f);
                Sparks(b, p, Mint, arrive, 10, 4f, 8f);
                b.Shake(arrive, i == 2 ? 0.25f : 0.15f, 0.2f);
            }
            Streaks(b, Target, Target + new Vector3(0f, 3f, 0f), Mint, 1.7f, 0.5f, 60f, 0.6f);
            SwirlLeaves(b, Target, 1.7f, 0.6f, 0.7f, 2.5f, 25f);
            HitShell(b, Target, Mint, 1.5f, 0.4f);
            Dust(b, Target, 1.52f, 0.9f, 10);
            b.Light(AttackerChest + new Vector3(0.3f, 0.8f, -0.5f), Lc(0.75f, 1f, 0.9f), 10f,
                0f, 0f, 0.2f, 1.5f, 1.2f, 2f, 1.4f, 0.5f, 1.7f, 0f);
        }

        // ---------------------------------------------------------------- 014 Danza Espada
        // Refs: anime = espadas cian brillantes verticales alrededor del usuario; juego = espadas que se cruzan sobre el usuario y chispas naranjas que suben.
        static void M014(EmeraldMoveBuilder b)
        {
            var clash = Attacker + new Vector3(0f, 2.5f, 0f);
            b.Keys(0.75f, 1.5f, 1.65f, 2.6f, clash, 0.5f);
            // El clip horneado remata con una estrella roja-amarilla gigante (impropia de un movimiento de estado): se oculta.
            b.HideBaked();
            b.View(new Vector2(-5.5f, 0f), new Vector2(0f, 3.8f));
            GroundGlow(b, Attacker, Cyan, 0.1f, 2.1f, 1.5f);

            // Corona de seis espadas: brotan del suelo, giran cada vez más rápido, suben y se inclinan hasta chocar.
            var pivot = b.Node("SwordRing", Attacker);
            var ring = b.Track(pivot, null, 0.1f, 2.75f);
            ring.eulerFrom = Vector3.zero;
            ring.eulerTo = new Vector3(0f, 900f, 0f);
            ring.rotCurve = C(0.1f, 0f, 0.6f, 0.06f, 1.0f, 0.3f, 1.45f, 1f, 2.75f, 1.35f);
            ring.posFrom = Attacker;
            ring.posTo = Attacker + new Vector3(0f, 0.6f, 0f);
            ring.posCurve = C(0.6f, 0f, 1.4f, 1f);
            const float tilt = 32f;
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f;
                float start = 0.12f + 0.04f * i;
                var dir = Quaternion.Euler(0f, -a, 0f) * Vector3.right;
                var r = b.MeshPart("Sword", Blade, Mat(Cyan, "Blade"), dir * 0.95f, new Vector3(0f, -a, 0f), new Vector3(0.32f, 1.5f, 0.32f), pivot, 4);
                var tr = b.Track(r.transform, r, start, 2.7f);
                tr.scaleFrom = new Vector3(0.32f, 0f, 0.32f);
                tr.scaleTo = new Vector3(0.32f, 1.5f, 0.32f);
                tr.scaleCurve = C(start, 0f, start + 0.3f, 1f);
                tr.eulerFrom = new Vector3(0f, -a, 0f);
                tr.eulerTo = new Vector3(0f, -a, tilt);
                tr.rotCurve = C(1.0f, 0f, 1.45f, 1f, 2.0f, 1f, 2.5f, 0.4f);
                tr.property = "_Erosion";
                tr.propertyCurve = C(start, 0.5f, start + 0.1f, 0f, 2.3f, 0f, 2.7f, 1f);
            }
            // Choque de las hojas: destello metálico pequeño en cruz (no una estrella de impacto: es un movimiento de estado)
            // y chispas naranjas que saltan y suben, como en la captura del juego.
            Flash(b, clash, White, 1.5f, 1.1f);
            CutLine(b, clash, White, 1.5f, 0f, 1.6f, 0.14f, 0.22f);
            CutLine(b, clash, White, 1.5f, 90f, 1.6f, 0.14f, 0.22f);
            Sparks(b, clash, Orange, 1.5f, 26, 3f, 7f, 0.5f, 1.2f);
            b.Cue(b.Ps("ClashEmbers", Mat(Orange, "Spark"), Attacker + new Vector3(0f, 0.3f, 0f))
                .GroundCircle(1.1f, 0.4f).Velocity(new Vector3(0f, 4.5f, 0f)).Speed(0.2f, 0.8f).Life(0.4f, 0.8f).Size(0.05f, 0.1f)
                .Rate(40f).Duration(1.2f).Stretch(5f, 0.05f).AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0f).Order(4), 1.55f);
            Aura(b, Attacker, Red, 1.55f, 3.3f, 1.05f, 50f);
            RisingArrows(b, Attacker, Red, 1.6f, 1.4f);
            Glints(b, Attacker + new Vector3(0f, 1.6f, 0f), Cyan, 2.3f, 0.6f, 1.0f, 30f);
            b.Light(clash, Lc(0.7f, 0.9f, 1f), 10f, 0f, 0f, 0.2f, 1.5f, 1.4f, 2f, 1.52f, 3f, 1.8f, 2f, 3f, 1f, 3.5f, 0f);
            b.Shake(1.5f, 0.08f, 0.15f);
        }

        // ---------------------------------------------------------------- 015 Corte
        // Refs: anime = tajo blanco brillante con la hoja; juego = línea de corte fina blanca-verdosa que cruza al rival.
        static void M015(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.3f, 1.42f, 2.1f, Target, 0.5f);
            b.Baked(1.3f);
            var paw = AttackerHand + new Vector3(0.1f, 0.4f, 0f);
            Charge(b, paw, Mint, 0.2f, 0.85f, 1.0f, 0.9f, false);
            CutLine(b, paw + new Vector3(0f, 0.15f, 0f), Mint, 0.3f, 20f, 1.1f, 0.2f, 0.75f);
            Streaks(b, AttackerHand, Hit, White, 1.0f, 0.25f, 60f, 0.35f);
            Slash(b, Hit, Mint, 1.24f, 90f, 1.8f, 0.32f, -250f);
            CutLine(b, Hit, White, 1.3f, 78f, 6f, 0.28f, 0.35f);
            CutLine(b, Hit, Mint, 1.3f, 78f, 6.6f, 0.5f, 0.3f);
            Flash(b, Hit, White, 1.3f, 3f);
            ImpactStar(b, Hit, Mint, 1.3f, 3f, 8, 22);
            HitShell(b, Target, Mint, 1.3f, 0.35f);
            Glints(b, Hit, Mint, 1.4f, 0.5f, 1f, 20f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(0.85f, 1f, 0.9f), 9f, 0f, 0f, 0.3f, 1f, 0.9f, 1f, 1.28f, 0f, 1.32f, 4f, 1.6f, 1f, 2.1f, 0f);
            b.Shake(1.3f, 0.22f, 0.25f);
        }

        // ---------------------------------------------------------------- 016 Tornado
        // Refs: anime = arcos de viento blancos/marrones en espiral; juego = rayas de viento blancas girando alrededor del rival.
        static void M016(EmeraldMoveBuilder b)
        {
            b.Keys(0.75f, 1.35f, 1.6f, 2.6f, Target, 0.5f);
            b.BakedWindow(0.6f, 2f); // las medias lunas del clip llegan al objetivo hacia 1,35 s
            Aura(b, Attacker, Wind, 0.2f, 1.0f, 1f, 20f);
            Streaks(b, AttackerChest + new Vector3(-0.6f, 0.3f, 0f), AttackerChest + new Vector3(1.6f, 0.3f, 0f), Wind, 0.3f, 0.6f, 50f, 0.7f, 1f, 0.3f);
            float[] angles = { 0f, 20f, -20f };
            for (int i = 0; i < 3; i++)
                WindBlade(b, AttackerChest + new Vector3(0.6f, 0.2f, 0f), TargetChest + new Vector3(-0.2f, 0.2f * (1 - i), 0f), Wind, 0.85f + 0.1f * i, 0.35f, 1.2f, angles[i]);
            Tornado(b, Target, Wind, 1.3f, 3.0f, 3.0f, 1.25f, 34f);
            SwirlLeaves(b, Target, 1.35f, 1.4f, 0.9f, 2.4f, 14f);
            Streaks(b, Target + new Vector3(-2.5f, 1f, 0f), Target + new Vector3(1.5f, 1.2f, 0f), White, 1.3f, 1.2f, 40f, 0.8f);
            HitShell(b, Target, Wind, 1.35f, 0.35f);
            Dust(b, Target, 1.36f, 1f, 10);
            b.Light(TargetChest + new Vector3(-0.8f, 0.6f, 0f), Lc(0.85f, 0.95f, 1f), 9f, 0f, 0f, 1.25f, 0f, 1.4f, 2.5f, 2.6f, 1.5f, 3.1f, 0f);
            b.Shake(1.35f, 0.15f, 0.3f);
        }

        // ---------------------------------------------------------------- 017 Ataque Ala
        // Refs: anime = alas blancas brillantes con líneas de velocidad verdes/azules; juego = estela de ala blanco-azulada y estrella amarilla con borde azul.
        static void M017(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.3f, 1.45f, 2.2f, Target, 1f);
            b.Baked(1.3f);
            Aura(b, Attacker, Wind, 0.15f, 1.15f, 1f, 35f);
            for (int s = -1; s <= 1; s += 2)
                Slash(b, AttackerChest + new Vector3(0.1f, 0.45f, 0f) + ScreenRight * (0.55f * s), White, 0.3f, s > 0 ? 0f : 180f, 1.0f, 0.7f);
            Streaks(b, AttackerChest, TargetChest, White, 0.95f, 0.35f, 90f, 0.6f, 1.2f);
            SpeedLines(b, Hit, Wind, 1.0f, 0.35f, 3.2f, 80f);
            Slash(b, Hit, Wind, 1.26f, 45f, 1.6f, 0.3f, 200f);
            Slash(b, Hit, Wind, 1.29f, 225f, 1.6f, 0.3f, -200f);
            ImpactStar(b, Hit, YellowBlue, 1.3f, 4f, 10, 28);
            Flash(b, Hit, White, 1.3f, 3f);
            HitShell(b, Target, Wind, 1.3f, 0.35f);
            Shockwave(b, Target, Wind, 1.32f, 3.2f);
            Dust(b, Target, 1.33f, 1f, 10);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(0.9f, 0.95f, 1f), 9f, 0f, 0f, 1.28f, 0f, 1.32f, 4.5f, 1.6f, 1.5f, 2.2f, 0f);
            b.Shake(1.3f, 0.3f, 0.35f);
            b.SlowMo(1.31f, 0.3f, 0.07f);
        }

        // ---------------------------------------------------------------- 018 Remolino
        // Refs: anime = rayas de viento blancas en diagonal; juego = rayas de viento grises y blancas que barren hacia el rival.
        static void M018(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.4f, 1.8f, 2.8f, Target, 0.4f);
            b.BakedWindow(0.2f, 3.2f); // clip a 0,625x: ocupa toda la timeline
            Tornado(b, Attacker, White, 0.1f, 1.2f, 1.6f, 0.8f, 20f, false);
            Streaks(b, AttackerChest + new Vector3(0.6f, 0f, 0f), TargetChest, White, 0.8f, 0.6f, 80f, 0.7f, 1.2f, 0.35f);
            WindBlade(b, AttackerChest + new Vector3(0.6f, 0.3f, 0f), TargetChest + new Vector3(-0.3f, 0.3f, 0f), White, 0.9f, 0.4f, 1.3f, 15f);
            WindBlade(b, AttackerChest + new Vector3(0.6f, 0f, 0f), TargetChest + new Vector3(-0.3f, -0.1f, 0f), White, 1.05f, 0.4f, 1.3f, -15f);
            Tornado(b, Target, White, 1.3f, 3.1f, 3.4f, 1.3f, 34f);
            SwirlLeaves(b, Target, 1.4f, 1.5f, 1.2f, 3f, 18f);
            Dust(b, Target, 1.4f, 1.3f, 14, 1.2f);
            Dust(b, Target, 2.6f, 1.2f, 10, 1f);
            HitShell(b, Target, Wind, 1.4f, 0.4f);
            b.Light(TargetChest + new Vector3(-0.8f, 0.8f, 0f), Lc(0.9f, 0.95f, 1f), 9f, 0f, 0f, 1.3f, 0f, 1.45f, 2.5f, 2.8f, 1.5f, 3.3f, 0f);
            b.Shake(1.4f, 0.18f, 0.5f);
        }

        // ---------------------------------------------------------------- 019 Vuelo
        // Refs: anime = el usuario despega con un chorro azul; juego = el usuario sale disparado hacia arriba con rayas verticales.
        static void M019(EmeraldMoveBuilder b)
        {
            b.Keys(0.55f, 1.9f, 2.05f, 2.9f, Target, 1f);
            b.Baked(1.9f);
            Dust(b, Attacker, 0.25f, 0.8f, 8, 0.8f);
            Shockwave(b, Attacker, Wind, 0.5f, 2.6f);
            Dust(b, Attacker, 0.5f, 1.1f, 14, 1.1f);
            b.Cue(b.Ps("LiftStreaks", Mat(White, "Spark"), Attacker + new Vector3(0f, 0.4f, 0f))
                .Box(new Vector3(1f, 0.3f, 1f)).Velocity(new Vector3(0f, 16f, 0f)).Life(0.3f).Size(0.08f, 0.14f)
                .Rate(90f).Duration(0.45f).Stretch(6f, 0.05f).AlphaOverLife(0f, 0.3f, 0.2f, 1f, 1f, 0f).Order(3), 0.45f);
            Projectile(b, AttackerChest, AttackerChest + new Vector3(0f, 9f, 0f), Wind, 0.5f, 0.4f, 1.2f, 1.0f);
            Flash(b, new Vector3(2.5f, 4.2f, 0.5f), White, 1.3f, 2f);
            GroundShadow(b, Target, 1.2f, 1.92f, 0.4f, 2.2f);
            var above = Target + new Vector3(-1.2f, 5.5f, 0f);
            Projectile(b, above, TargetChest, White, 1.7f, 0.2f, 1.5f, 1.2f);
            WindBlade(b, above, Hit, Wind, 1.7f, 0.2f, 1.6f, -90f);
            Streaks(b, above, TargetChest, White, 1.65f, 0.25f, 90f, 0.8f, 1.3f, 0.2f);
            ImpactStar(b, Hit, White, 1.9f, 4.6f, 14, 40);
            Flash(b, Hit, White, 1.9f, 3.4f);
            Shockwave(b, Target, Wind, 1.91f, 4.2f);
            Dust(b, Target, 1.92f, 1.3f, 18, 1.3f);
            Debris(b, Target + new Vector3(0f, 0.3f, 0f), 1.92f, 12, 8f);
            HitShell(b, Target, White, 1.9f, 0.4f);
            b.Light(Hit + new Vector3(-0.5f, 0.5f, 0f), Lc(0.9f, 0.95f, 1f), 11f, 0f, 0f, 1.88f, 0f, 1.92f, 6f, 2.2f, 2f, 2.9f, 0f);
            b.Shake(0.5f, 0.12f, 0.2f);
            b.Shake(1.9f, 0.45f, 0.5f);
            b.SlowMo(1.91f, 0.25f, 0.1f);
        }

        // ---------------------------------------------------------------- 020 Atadura
        // Refs: anime = cuerpo enroscado sobre fondo de estallido radial naranja; juego = anillos plateados apilados que rodean al rival.
        static void M020(EmeraldMoveBuilder b)
        {
            b.Keys(0.7f, 1.45f, 1.95f, 2.95f, Target, 0.6f);
            // Las bandas propias sustituyen a las del clip horneado (si no, se ven dobles).
            b.HideBaked();
            Aura(b, Attacker, Steel, 0.15f, 1.0f, 1f, 25f);

            // Cuatro bandas gruesas de acero caen una tras otra, se cierran y aprietan tres veces.
            float[] heights = { 0.25f, 0.6f, 0.95f, 1.3f };
            for (int i = 0; i < heights.Length; i++)
            {
                float t0 = 0.8f + 0.11f * i;
                var pos = Target + new Vector3(0f, heights[i], 0f);
                var r = b.MeshPart("Band", Band, Mat(Metal, "Solid"), pos + new Vector3(0f, 2.5f, 0f), Vector3.zero, new Vector3(1.1f, 0.6f, 1.1f), order: 3);
                var tr = b.Track(r.transform, r, t0, 2.85f);
                tr.posFrom = pos + new Vector3(0f, 2.5f, 0f);
                tr.posTo = pos;
                tr.posCurve = C(t0, 0f, t0 + 0.18f, 1f);
                tr.scaleFrom = new Vector3(1.1f, 0.6f, 1.1f);
                tr.scaleTo = new Vector3(0.62f, 0.6f, 0.62f);
                tr.scaleCurve = C(t0, 0f, t0 + 0.2f, 0.25f, 1.38f, 1f, 1.45f, 1.12f, 1.55f, 1f, 1.8f, 1f, 1.85f, 1.12f, 1.95f, 1f, 2.2f, 1f, 2.25f, 1.15f, 2.35f, 1f, 2.85f, 1f);
                tr.spin = new Vector3(0f, 40f * (i % 2 == 0 ? 1f : -1f), 0f);
                tr.property = "_Erosion";
                tr.propertyCurve = C(t0, 0.5f, t0 + 0.08f, 0f, 2.85f, 0f);
            }
            float[] squeezes = { 1.45f, 1.85f, 2.25f };
            for (int i = 0; i < squeezes.Length; i++)
            {
                ImpactStar(b, Hit + ScreenRight * 0.7f + new Vector3(0f, 0.5f, 0f), Orange, squeezes[i], i == 2 ? 2.6f : 2.2f, 10, 18);
                b.Shake(squeezes[i], i == 2 ? 0.28f : 0.2f, 0.25f);
            }
            // Al final las bandas saltan en pedazos.
            Debris(b, TargetChest, 2.85f, 22, 7f, 1.3f, Mat(Metal, "Solid"));
            Flash(b, TargetChest, White, 2.85f, 3f);
            Sparks(b, TargetChest, Steel, 2.85f, 24, 6f, 12f);
            Shockwave(b, Target, Steel, 2.86f, 3f);
            b.Light(Hit + new Vector3(-0.5f, 0.4f, 0f), Lc(1f, 0.7f, 0.4f), 9f,
                0f, 0f, 1.43f, 0f, 1.47f, 3.5f, 1.6f, 1f, 1.86f, 3.5f, 2f, 1f, 2.26f, 4f, 2.5f, 1f, 2.86f, 4f, 3.2f, 0f);
            b.Shake(2.85f, 0.3f, 0.3f);
        }
    }
}
