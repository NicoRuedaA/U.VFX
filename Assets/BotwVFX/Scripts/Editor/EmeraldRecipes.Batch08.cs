using UnityEngine;
using UnityEditor;
using static BotwVfx.EditorTools.EmeraldLayers;
using static BotwVfx.EditorTools.EmeraldMoveBuilder;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>Moves 141–160. Reference silhouettes take priority over generic type impacts.
    /// Status moves have no impact stars, camera shake or hit-stop. Shared shaders remain unchanged.</summary>
    public static partial class EmeraldRecipes
    {
        static readonly Pal KissRose = new Pal("KissRose", new Color(1f, .002f, .16f), new Color(.3f, .0003f, .02f));
        static readonly Pal SkyGold = new Pal("SkyGold", new Color(2.6f, 1.8f, .12f), new Color(1.4f, .16f, .001f));
        static readonly Pal SporeGold = new Pal("SporeGold", new Color(1.5f, .85f, .005f), new Color(.45f, .2f, .001f));
        static readonly Pal WaveCyan = new Pal("WaveCyan", new Color(.025f, .75f, 1.5f), new Color(.001f, .1f, .4f));
        static readonly Pal SlideStone = new Pal("SlideStone", new Color(.12f, .06f, .025f), new Color(.035f, .012f, .003f));

        // Anime: pointed yellow contact flash. Game: small gold motes; energy returns to the user.
        static void M141(EmeraldMoveBuilder b)
        {
            b.Keys(.75f, 1.05f, 1.6f, 2.8f, TargetChest, .5f); b.HideBaked();
            Streaks(b, AttackerHand, TargetChest, Cream, .6f, .4f, 30f, .2f);
            ImpactStar(b, TargetChest, SporeGold, 1.05f, 3.2f, 8, 12);
            DrainMotes(b, TargetChest, AttackerChest, SporeGold, 1.1f, 1.15f, 30f, .7f, .8f, "Glow", .35f);
            Aura(b, Attacker, Mint, 1.65f, 2.65f, 1.05f, 0f);
            Glints(b, AttackerChest, SporeGold, 1.7f, .7f, .7f, 12f);
            b.Shake(1.05f, .16f, .2f);
        }

        // Anime: a large pink heart. Game: purple hearts around the sleeping target.
        static void M142(EmeraldMoveBuilder b)
        {
            b.Keys(.7f, 1.25f, 1.6f, 2.9f, TargetHead, 0f); b.HideBaked();
            var heart = Batch08Heart();
            var r = b.MeshPart("KissHeart", heart, Mat(KissRose, "Flat"), AttackerHead, new Vector3(0f, 37f, 0f), Vector3.one);
            var tr = b.Track(r.transform, r, .45f, 1.4f);
            tr.posFrom = AttackerHead; tr.posTo = TargetHead; tr.posArc = Vector3.up * .5f; tr.posCurve = C(.45f, 0f, 1.25f, 1f, 1.4f, 1f);
            tr.scaleFrom = Vector3.one * .1f; tr.scaleTo = Vector3.one * 1.15f; tr.scaleCurve = C(.45f, 0f, .75f, 1f, 1.25f, 1f, 1.4f, 0f);
            b.Cue(b.Ps("SleepHearts", Mat(KissRose, "Flat"), TargetHead).Mesh(heart, ParticleSystemRenderSpace.View)
                .Sphere(.55f).Velocity(Vector3.up * .6f).Life(.7f, 1.1f).Size(.25f, .45f).Rate(7f).Duration(1f)
                .AlphaOverLife(0f, 1f, .8f, 1f, 1f, 0f).Order(5), 1.25f);
            Aura(b, Target, Lilac, 1.25f, 2.65f, 1.05f, 0f);
        }

        // Anime: gold bird-shaped rush. Game: broad white wings. Two swept blades outline the wings.
        static void M143(EmeraldMoveBuilder b)
        {
            b.Keys(.85f, 1.3f, 1.5f, 2.6f, Hit, 1f); b.HideBaked();
            Aura(b, Attacker, SkyGold, .15f, 1.1f, 1.4f, 25f);
            Charge(b, AttackerChest, SkyGold, .25f, .6f, 1.3f, 1.0f, false);
            for (int side = -1; side <= 1; side += 2)
            {
                var wing = b.MeshPart("SkyWing", Crescent, Mat(SkyGold, "Solid"), AttackerChest, new Vector3(0f, 37f, side * 60f), new Vector3(1.2f, 2.5f, 1f));
                var tr = b.Track(wing.transform, wing, .75f, 1.42f);
                tr.posFrom = AttackerChest + ScreenRight * side * .7f; tr.posTo = Hit + ScreenRight * side * .7f;
                tr.posArc = Vector3.up * .6f; tr.posCurve = C(.75f, 0f, .95f, .1f, 1.3f, 1f, 1.42f, 1.1f);
            }
            Projectile(b, AttackerChest, Hit, SkyGold, .9f, .4f, 1.2f, .9f);
            ImpactStar(b, Hit, SkyGold, 1.3f, 5.7f, 12, 22);
            Shockwave(b, Target, White, 1.32f, 4f);
            b.Shake(1.3f, .5f, .4f); b.SlowMo(1.31f, .22f, .09f);
        }

        // Anime and game: golden transformation shell; squash into a faceted copy of the target silhouette.
        static void M144(EmeraldMoveBuilder b)
        {
            b.Keys(.65f, 1.05f, 1.5f, 2.8f, AttackerChest, 0f); b.HideBaked();
            var body = b.MeshPart("MorphBody", BotwMeshes.Rock, Mat(SporeGold, "Solid"), AttackerChest, Vector3.zero, Vector3.one);
            var morph = b.Track(body.transform, body, .3f, 2.6f);
            morph.scaleFrom = new Vector3(.8f, .3f, .8f); morph.scaleTo = new Vector3(.42f, .8f, .42f);
            morph.scaleCurve = C(.3f, 1f, .65f, 0f, 1.05f, .2f, 1.5f, 1f, 2.4f, 1f, 2.6f, 0f);
            morph.property = "_Erosion"; morph.propertyCurve = C(.3f, 1f, .5f, 0f, 2.3f, 0f, 2.6f, 1f);
            var head = b.MeshPart("MorphHead", BotwMeshes.Sphere, Mat(SporeGold, "Solid"), AttackerHead, Vector3.zero, Vector3.one * .3f);
            b.Track(head.transform, head, 1.2f, 2.5f);
            Aura(b, Attacker, SporeGold, .3f, 2.5f, 1.25f, 0f);
            Glints(b, AttackerChest, SporeGold, .5f, 1.7f, 1f, 15f);
            RingPulses(b, AttackerChest, SporeGold, 1.05f, 3, .17f, 2.8f);
        }

        // Both references: individual translucent blue bubbles, not a solid beam.
        static void M145(EmeraldMoveBuilder b)
        {
            b.Keys(.7f, 1.3f, 1.65f, 2.8f, TargetChest, .35f); b.HideBaked();
            Bubbles(b, Mouth, TargetChest, WaveCyan, .4f, 1.05f, 18f, .9f, .25f, .6f, .35f);
            BubblePops(b, TargetChest, WaveCyan, new[] { 1.3f, 1.5f, 1.7f, 1.95f }, 1.25f, 8);
            HitShell(b, Target, Cyan, 1.3f, .7f);
            b.Shake(1.3f, .12f, .2f);
        }

        // Anime: rotating fists; game: warm punch flash. Disorientation remains after the strike.
        static void M146(EmeraldMoveBuilder b)
        {
            b.Keys(.8f, 1.2f, 1.65f, 2.8f, TargetChest, .75f); b.HideBaked();
            SpinArc(b, AttackerHand, Cream, .3f, .95f, .7f, .6f, -1000f);
            Streaks(b, AttackerHand, TargetChest, Cream, .85f, .3f, 40f, .25f);
            ImpactStar(b, TargetChest, Cream, 1.2f, 4.2f, 10, 22);
            Dizzy(b, TargetHead, StarYellow, 1.3f, 1.2f);
            b.Shake(1.2f, .3f, .3f); b.SlowMo(1.21f, .3f, .07f);
        }

        // Anime: golden spore cloud. Game: cream puffs released from the cap. Sleep has no damage flash.
        static void M147(EmeraldMoveBuilder b)
        {
            b.Keys(.7f, 1.3f, 1.7f, 2.9f, TargetHead, 0f); b.HideBaked();
            b.Cue(b.Ps("SporeCloud", Mat(SporeGold, "Glow"), AttackerHead + Vector3.up * .3f)
                .Sphere(.55f).Velocity(new Vector3(3.8f, .1f, 0f)).Speed(.1f, .4f).Life(1.5f, 1.8f).Size(.07f, .16f)
                .Rate(65f).Duration(.9f).AlphaOverLife(0f, 0f, .15f, 1f, .8f, 1f, 1f, 0f).Order(4), .35f);
            Tint(Fog(b, Target, BotwMaterials.Get("EM_Dust"), 1.1f, 1.0f, .9f, 7f, .65f, .9f, 1.1f), new Color(1f, .85f, .3f));
            Glints(b, TargetHead, SporeGold, 1.2f, 1.1f, 1f, 12f, .5f);
            Aura(b, Target, SporeGold, 1.3f, 2.6f, 1f, 0f);
        }

        // Both references: white radial glare from the user; no hit star or explosive debris.
        static void M148(EmeraldMoveBuilder b)
        {
            b.Keys(.7f, 1.05f, 1.25f, 2.4f, AttackerHead, 0f); b.HideBaked();
            Charge(b, AttackerHead, White, .25f, .65f, 1.2f, .7f, false);
            Flash(b, AttackerHead, White, 1.05f, 5.5f);
            Rays(b, AttackerHead, White, 1.05f, 24, 8f, .65f);
            RingPulses(b, AttackerHead, White, 1.05f, 3, .12f, 6f);
            b.Light(AttackerHead, Color.white, 14f, 0f, 0f, .9f, 0f, 1.05f, 8f, 1.35f, 3f, 2.1f, 0f);
        }

        // Anime: expanding cyan waves; game: magenta concentric rings. Distinct from a psychic beam.
        static void M149(EmeraldMoveBuilder b)
        {
            b.Keys(.65f, 1.15f, 1.55f, 2.8f, TargetChest, .55f); b.HideBaked();
            RingTunnel(b, AttackerHead, TargetChest, WaveCyan, .4f, 1.1f, 8f, .7f, .3f, 1.5f);
            RingTunnel(b, AttackerHead, TargetChest, Magenta, .55f, .9f, 4f, .7f, .25f, 1.3f);
            Aura(b, Target, WaveCyan, 1.15f, 2.4f, 1.2f, 0f);
            RingPulses(b, TargetChest, WaveCyan, 1.15f, 3, .2f, 3f);
            b.Shake(1.15f, .18f, .3f);
        }

        // Both references: harmless flopping. A small local silhouette hops; nothing happens at the target.
        static void M150(EmeraldMoveBuilder b)
        {
            b.Keys(.55f, 1.0f, 1.55f, 2.65f, Attacker, 0f); b.HideBaked();
            var r = b.MeshPart("HopSilhouette", BotwMeshes.Sphere, Mat(WaveCyan, "Shell"), AttackerChest, Vector3.zero, new Vector3(.55f, .9f, .55f));
            var tr = b.Track(r.transform, r, .2f, 2.5f);
            tr.posFrom = AttackerChest; tr.posTo = AttackerChest + Vector3.up * .8f;
            tr.posCurve = C(.2f, 0f, .55f, 1f, .85f, 0f, 1.2f, .8f, 1.5f, 0f, 1.85f, .6f, 2.15f, 0f, 2.5f, 0f);
            tr.eulerFrom = new Vector3(0f, 0f, -18f); tr.eulerTo = new Vector3(0f, 0f, 18f); tr.rotCurve = tr.posCurve;
            GroundShadow(b, Attacker, .2f, 2.5f, .65f, .9f);
        }

        // Anime: body liquefaction; game: purple melting shell. Retain the baked puddle and body squash.
        static void M151(EmeraldMoveBuilder b)
        {
            b.Keys(.6f, 1.0f, 1.5f, 2.8f, AttackerChest, 0f); b.Baked(1f);
            GroundGlow(b, Attacker, SludgeViolet, .4f, 2.6f, 1.3f);
            Glints(b, AttackerChest, SludgeViolet, 1.1f, 1f, .75f, 10f, .6f);
        }

        // Anime: raised claw; game: blue descending hammer smear and white contact splash.
        static void M152(EmeraldMoveBuilder b)
        {
            b.Keys(.85f, 1.25f, 1.5f, 2.6f, TargetChest, .9f); b.HideBaked();
            var top = TargetChest + Vector3.up * 2.7f;
            Projectile(b, top, TargetChest, WaveCyan, .7f, .55f, 1.5f, 1.1f);
            CutLine(b, TargetChest + Vector3.up * 1.1f, Cyan, 1.08f, 0f, 4.1f, .7f, .4f);
            Splash(b, TargetChest, Water, 1.25f, 1.8f, 30);
            ImpactStar(b, TargetChest, White, 1.25f, 4.4f, 8, 0);
            Shockwave(b, Target, WaveCyan, 1.27f, 3.8f);
            b.Shake(1.25f, .4f, .4f); b.SlowMo(1.26f, .25f, .09f);
        }

        // Anime: enormous warm sphere centered on the user. Game: orange blast and black smoke.
        static void M153(EmeraldMoveBuilder b)
        {
            b.Keys(.8f, 1.15f, 1.4f, 3f, AttackerChest, 1f); b.HideBaked();
            Aura(b, Attacker, SkyGold, .25f, 1.1f, 1.1f, 0f);
            FireBurst(b, AttackerChest, 1.15f, 3.0f, false);
            ImpactStar(b, AttackerChest, SkyGold, 1.15f, 8f, 18, 0);
            Shockwave(b, Attacker, Orange, 1.16f, 8f); Shockwave(b, Attacker, Cream, 1.28f, 6.5f);
            Debris(b, AttackerChest, 1.2f, 24, 9f, 1.5f);
            Smoke(b, AttackerChest, BotwMaterials.Get("EX_Smoke"), 1.35f, 16, 2.1f, .8f, 1.5f, 1.6f);
            b.Light(AttackerChest + Vector3.up, new Color(1f, .6f, .2f), 18f, 0f, 0f, 1.1f, 0f, 1.17f, 12f, 1.5f, 4f, 2.5f, 0f);
            b.Shake(1.15f, .8f, .65f); b.SlowMo(1.16f, .15f, .13f);
        }

        // Anime: white claw ribbons. Game: paired yellow slashes; four alternating strikes.
        static void M154(EmeraldMoveBuilder b)
        {
            b.Keys(.7f, 1f, 1.65f, 2.6f, Hit, .65f); b.HideBaked();
            var times = new[] { 1f, 1.22f, 1.44f, 1.66f };
            for (int i = 0; i < times.Length; i++)
            {
                for (int claw = -1; claw <= 1; claw++)
                {
                    float angle = (i % 2 == 0 ? -42f : 42f) * Mathf.Deg2Rad;
                    var direction = ScreenRight * Mathf.Sin(angle) + Vector3.up * Mathf.Cos(angle);
                    var center = Hit + ScreenRight * claw * .24f;
                    var line = b.LinePart("ClawCut", Mat(White, "Line"), new[] { center - direction * 1.4f, center + direction * 1.4f }, C(0f, .015f, .4f, .22f, 1f, .015f));
                    var tr = b.Track(line.transform, line, times[i], times[i] + .22f);
                    tr.property = "_Erosion"; tr.propertyCurve = C(times[i], 0f, times[i] + .12f, 0f, times[i] + .22f, 1f);
                }
                b.Shake(times[i], .13f, .12f);
            }
            HitStars(b, Hit, SporeGold, times, 2f, 10, .2f);
        }

        // Anime: spinning white bone. Game: bone leaving a curled wake; the return also hits.
        static void M155(EmeraldMoveBuilder b)
        {
            b.Keys(.75f, 1.2f, 1.85f, 2.8f, Hit, .65f); b.HideBaked();
            var r = b.MeshPart("ReturningBone", Bone, Mat(BoneCream, "Solid"), AttackerHand, new Vector3(0f, 37f, 90f), Vector3.one * .85f);
            var tr = b.Track(r.transform, r, .5f, 2.35f);
            tr.posFrom = AttackerHand; tr.posTo = Hit + Vector3.right * .8f; tr.posArc = new Vector3(0f, .7f, .5f);
            tr.posCurve = C(.5f, 0f, 1.2f, .88f, 1.48f, 1f, 1.85f, .88f, 2.35f, 0f); tr.spin = new Vector3(0f, 0f, 1050f);
            SpinArc(b, Hit, White, 1.15f, 2.05f, 1.0f, .3f, -1000f, 25f);
            HitStars(b, Hit, Cream, new[] { 1.2f, 1.85f }, 3.1f, 12);
            b.Shake(1.2f, .22f, .2f); b.Shake(1.85f, .22f, .2f);
        }

        // Anime: closed eyes. Game: floating Zs and sleep bubbles plus healing, all on the user.
        static void M156(EmeraldMoveBuilder b)
        {
            b.Keys(.6f, 1f, 1.5f, 3f, AttackerHead, 0f); b.HideBaked();
            Aura(b, Attacker, Mint, .4f, 2.7f, 1.1f, 0f);
            Glints(b, AttackerChest, Mint, .5f, 1.6f, .7f, 14f, .6f);
            for (int i = 0; i < 3; i++)
            {
                float t = .65f + i * .45f;
                var p = AttackerHead + Vector3.up * (.35f + i * .3f) + ScreenRight * (.2f + i * .15f);
                var pts = new[] { p - ScreenRight * .16f + Vector3.up * .25f, p + ScreenRight * .16f + Vector3.up * .25f, p - ScreenRight * .16f, p + ScreenRight * .16f };
                var line = b.LinePart("SleepZ", Mat(White, "Flat"), pts, C(0f, .07f, 1f, .07f));
                var tr = b.Track(line.transform, line, t, t + 1.05f);
                tr.posFrom = Vector3.zero; tr.posTo = Vector3.up * .35f; tr.posCurve = C(t, 0f, t + 1.05f, 1f);
            }
            Bubbles(b, AttackerHead, AttackerHead + Vector3.up * 1.4f, White, .6f, 1.4f, 3f, 1f, .12f, .25f, .1f);
        }

        // Anime: suspended dark rocks. Game: staggered falling stones and dense tan ground dust.
        static void M157(EmeraldMoveBuilder b)
        {
            b.Keys(.7f, 1.2f, 1.65f, 2.9f, Target, .9f); b.HideBaked();
            var hits = new[] { 1.2f, 1.4f, 1.6f, 1.8f, 2f };
            for (int i = 0; i < hits.Length; i++)
            {
                var landing = Target + new Vector3((i % 3 - 1) * .65f, .15f, (i % 2 == 0 ? -.35f : .4f));
                var rock = b.MeshPart("FallingRock", BotwMeshes.Rock, Mat(SlideStone, "Solid"), landing + Vector3.up * 3.8f, new Vector3(i * 23f, i * 47f, 0f), Vector3.one * (.65f + i * .07f));
                var tr = b.Track(rock.transform, rock, hits[i] - .65f, hits[i] + .35f);
                tr.posFrom = landing + Vector3.up * 3.8f; tr.posTo = landing; tr.posCurve = C(hits[i] - .65f, 0f, hits[i] - .4f, .08f, hits[i], 1f);
                tr.scaleFrom = Vector3.zero; tr.scaleTo = rock.transform.localScale; tr.scaleCurve = C(hits[i] - .65f, 0f, hits[i] - .5f, 1f, hits[i] + .15f, 1f, hits[i] + .35f, 0f);
                b.Shake(hits[i], .24f, .18f);
            }
            b.Cue(Bursts(b.Ps("RockDust", BotwMaterials.Get("EM_Dust"), Target).Sphere(.8f).Speed(1f, 2.4f).Drag(3f)
                .Life(.6f, 1f).Size(.7f, 1.3f).SizeOverLife(C(0f, .4f, .3f, 1f, 1f, 1.2f)).Order(1), 7, hits), hits[0]);
            Debris(b, Target, 1.2f, 24, 4f, .75f, Mat(SlideStone, "Solid"));
            Shockwave(b, Target, Sand, 1.2f, 3.2f);
        }

        // Anime: exaggerated incisors. Game: gold bite flash; two long opposing fangs, not a full jaw.
        static void M158(EmeraldMoveBuilder b)
        {
            b.Keys(.8f, 1.25f, 1.5f, 2.5f, TargetChest, .8f); b.HideBaked();
            Jaws(b, TargetChest, FangGold, .65f, 1.25f, 1.4f, .9f, 2, 1.7f, 1f);
            ImpactStar(b, TargetChest, FangGold, 1.25f, 4.6f, 12, 24);
            HitShell(b, Target, Gold, 1.25f, .5f);
            b.Shake(1.25f, .35f, .35f); b.SlowMo(1.26f, .25f, .08f);
        }

        // Anime: blue polygon facets. Game: polished silver body; no baked contact star or attack arrows.
        static void M159(EmeraldMoveBuilder b)
        {
            b.Keys(.6f, 1f, 1.4f, 2.7f, AttackerChest, 0f); b.HideBaked();
            var facets = b.MeshPart("SharpenFacets", BotwMeshes.Rock, Mat(ClampWater, "Solid"), AttackerChest, new Vector3(0f, 30f, 0f), new Vector3(.8f, 1.25f, .8f));
            var tr = b.Track(facets.transform, facets, .25f, 2.4f);
            tr.scaleFrom = Vector3.zero; tr.scaleTo = facets.transform.localScale;
            tr.scaleCurve = C(.25f, 0f, .6f, 1f, 1.4f, 1f, 2.4f, .8f);
            tr.property = "_Erosion"; tr.propertyCurve = C(.25f, 1f, .5f, 0f, 2f, 0f, 2.4f, 1f);
            Panel(b, AttackerChest + new Vector3(-.3f, 0f, -.4f), new Vector2(1.1f, 1.8f), 37f, White, .5f, 2.2f, .2f, false, true);
            Glints(b, AttackerChest, White, .55f, 1.35f, 1.0f, 17f, .8f);
            RingPulses(b, AttackerChest, Cyan, 1f, 2, .2f, 2.2f);
        }

        // Anime: polygonal body. Game: alternating red/cyan tiles, reconstructed without the baked impact flash.
        static void M160(EmeraldMoveBuilder b)
        {
            b.Keys(.6f, 1.05f, 1.5f, 2.8f, AttackerChest, 0f); b.HideBaked();
            var palettes = new[] { KissRose, WaveCyan, White };
            for (int row = 0; row < 3; row++)
                for (int col = 0; col < 6; col++)
                {
                    float angle = col * Mathf.PI / 3f + row * .2f;
                    var position = Attacker + new Vector3(Mathf.Cos(angle) * .9f, .4f + row * .5f, Mathf.Sin(angle) * .9f);
                    var tile = b.MeshPart("ConversionTile", Quad, Mat(palettes[(row + col) % 3], "Flat"), position, new Vector3(0f, 90f - angle * Mathf.Rad2Deg, 0f), Vector3.one * .38f);
                    var tr = b.Track(tile.transform, tile, .25f + col * .035f, 2.55f);
                    tr.scaleFrom = Vector3.zero; tr.scaleTo = tile.transform.localScale;
                    tr.scaleCurve = C(.25f, 0f, .6f, 1f, 1.05f, .3f, 1.3f, 1f, 1.5f, .6f, 1.7f, 1f, 2.55f, 0f);
                    tr.posFrom = position; tr.posTo = position + (position - AttackerChest).normalized * 1.2f;
                    tr.posCurve = C(.25f, 0f, 1.8f, 0f, 2.55f, 1f);
                }
            Glints(b, AttackerChest, WaveCyan, .6f, 1.3f, 1f, 9f, .5f);
            Glints(b, AttackerChest, KissRose, .75f, 1.2f, 1f, 9f, .5f);
        }

        // Persistent mesh asset: no runtime allocations and no changes to earlier shared meshes.
        static Mesh Batch08Heart()
        {
            const string path = "Assets/BotwVFX/Meshes/M_EM_Heart.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;
            const int count = 48;
            var vertices = new Vector3[count + 1]; var triangles = new int[count * 3];
            var colors = new Color32[count + 1]; var uv = new Vector2[count + 1];
            vertices[0] = new Vector3(0f, .08f, 0f);
            for (int i = 0; i < count; i++)
            {
                float t = i * Mathf.PI * 2f / count;
                float x = 16f * Mathf.Pow(Mathf.Sin(t), 3f) / 32f;
                float y = (13f * Mathf.Cos(t) - 5f * Mathf.Cos(2f * t) - 2f * Mathf.Cos(3f * t) - Mathf.Cos(4f * t)) / 32f;
                vertices[i + 1] = new Vector3(x, y, 0f);
                triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = (i + 1) % count + 1;
            }
            for (int i = 0; i < vertices.Length; i++) { colors[i] = new Color32(255, 255, 255, 255); uv[i] = new Vector2(vertices[i].x + .5f, vertices[i].y + .5f); }
            var mesh = new Mesh { name = "M_EM_Heart", vertices = vertices, triangles = triangles, colors32 = colors, uv = uv };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
        }
    }
}
