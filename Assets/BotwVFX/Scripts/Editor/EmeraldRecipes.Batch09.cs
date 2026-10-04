using UnityEngine;
using static BotwVfx.EditorTools.EmeraldLayers;
using static BotwVfx.EditorTools.EmeraldMoveBuilder;
using static BotwVfx.EditorTools.Fx;

namespace BotwVfx.EditorTools
{
    /// <summary>Final five reference-led recipes. Batch-local palettes avoid changes to the earlier effects.</summary>
    public static partial class EmeraldRecipes
    {
        static readonly Pal TriRed = new Pal("TriRed", new Color(2.3f, .02f, .001f), new Color(.65f, .001f, .0001f));
        static readonly Pal TriBlue = new Pal("TriBlue", new Color(.015f, .6f, 2f), new Color(.001f, .09f, .8f));
        static readonly Pal TriYellow = new Pal("TriYellow", new Color(2.5f, 1.8f, .005f), new Color(.7f, .35f, .0001f));
        static readonly Pal DollGreen = new Pal("DollGreen", new Color(.018f, .11f, .002f), new Color(.004f, .035f, .0005f));
        static readonly Pal DollBelly = new Pal("DollBelly", new Color(.12f, .19f, .014f), new Color(.035f, .07f, .003f));
        static readonly Pal DollEyes = new Pal("DollEyes", new Color(.00002f, .00002f, .00002f), new Color(.00001f, .00001f, .00001f));

        // Anime: three elemental streams (fire, ice and electricity). Game: three separate coloured orbs.
        // Three staggered arrivals keep all three identities readable without one white generic explosion.
        static void M161(EmeraldMoveBuilder b)
        {
            b.Keys(.85f, 1.25f, 1.65f, 2.7f, TargetChest, .85f);
            b.HideBaked();
            var palettes = new[] { TriRed, TriBlue, TriYellow };
            var offsets = new[] { new Vector3(0f, .9f, 0f), new Vector3(0f, -.2f, -.65f), new Vector3(0f, -.2f, .65f) };
            for (int i = 0; i < 3; i++)
            {
                float hit = 1.25f + i * .18f;
                var source = AttackerChest + offsets[i];
                var contact = TargetChest + offsets[i] * .3f;
                Projectile(b, source, contact, palettes[i], hit - .65f, .65f, 1.05f, .35f, "Glow", Vector3.up * .2f);
                HitStars(b, contact, palettes[i], new[] { hit }, 2.8f, 0, .1f, .3f);
                b.Shake(hit, .2f, .18f);
            }
            // Thin branching rays distinguish electricity from the other two orbs without another particle system.
            RadialBolts(b, TargetChest + offsets[2] * .3f, TriYellow, 1.61f, 2.05f, 5, 1.1f, 161, .08f);
            b.Light(TargetChest + new Vector3(-.7f, .5f, 0f), new Color(.8f, .85f, 1f), 8f,
                0f, 0f, 1.23f, 0f, 1.26f, 3f, 1.43f, 2f, 1.61f, 3f, 2.15f, 0f);
        }

        // Anime: enlarged glowing incisors and sharp golden contact rays. Catalogue: two large opposing teeth.
        // Distinct from 158's narrow paired fang rows: broad solid incisors close on the target.
        static void M162(EmeraldMoveBuilder b)
        {
            b.Keys(.8f, 1.2f, 1.4f, 2.55f, TargetChest, .85f);
            b.HideBaked();
            for (int row = 0; row < 2; row++)
            {
                float sign = row == 0 ? 1f : -1f;
                var center = TargetChest + Vector3.up * sign * 1.25f;
                var tooth = b.MeshPart("SuperIncisor", Cone, Mat(EggWhite, "Solid"), center,
                    new Vector3(0f, 37f, row == 0 ? 180f : 0f), new Vector3(.85f, 1.25f, .5f));
                var tr = b.Track(tooth.transform, tooth, .55f, 1.65f);
                tr.posFrom = center; tr.posTo = TargetChest + Vector3.up * sign * .22f;
                tr.posCurve = C(.55f, 0f, .95f, .06f, 1.2f, 1f, 1.65f, 1f);
                tr.scaleFrom = Vector3.zero; tr.scaleTo = tooth.transform.localScale;
                tr.scaleCurve = C(.55f, 0f, .8f, 1f, 1.4f, 1f, 1.65f, .3f);
                tr.property = "_Erosion"; tr.propertyCurve = C(.55f, 0f, 1.35f, 0f, 1.65f, 1f);
            }
            ImpactStar(b, TargetChest, FangGold, 1.2f, 4.7f, 14, 18);
            RingPulses(b, TargetChest, Cream, 1.22f, 2, .12f, 3.4f);
            HitShell(b, Target, Gold, 1.2f, .45f);
            b.Shake(1.2f, .4f, .35f); b.SlowMo(1.21f, .22f, .09f);
        }

        // Anime: Sneasel's white claws; game: long bright yellow/white slash. Three simultaneous parallel cuts.
        // Deliberately broader and longer than the repeated light scratches in 154.
        static void M163(EmeraldMoveBuilder b)
        {
            b.Keys(.75f, 1.15f, 1.35f, 2.5f, Hit, .8f);
            b.HideBaked();
            Streaks(b, AttackerHand, Hit, White, .65f, .4f, 28f, .25f, .8f);
            for (int i = -1; i <= 1; i++)
                CutLine(b, Hit + ScreenRight * i * .45f, White, 1.15f + (i + 1) * .025f, -50f, 4.4f, .38f, .45f);
            ImpactStar(b, Hit, Cream, 1.18f, 3.5f, 0, 0);
            Dust(b, Target, 1.22f, .65f, 8, .6f);
            HitShell(b, Target, White, 1.15f, .5f);
            b.Shake(1.17f, .3f, .3f); b.SlowMo(1.18f, .25f, .07f);
        }

        // Anime: a duplicate decoy. Game: a squat green doll with belly, pointed ears, muzzle and tail in white smoke.
        // LunaEagle's attributed model temporarily replaces the bound gallery attacker.
        static void M164(EmeraldMoveBuilder b)
        {
            b.Keys(.65f, 1.05f, 1.5f, 2.85f, AttackerChest, 0f);
            b.HideBaked();
            b.root.gameObject.AddComponent<EmeraldActorReplacement>();
            var doll = EmeraldSubstituteModel.Add(b, Attacker);
            var whiteSmoke = EmeraldSubstituteModel.WhiteSmoke();
            Smoke(b, doll.localPosition + Vector3.up * .4f, whiteSmoke, .8f, 12, .75f, .35f, .7f, .85f);
            Smoke(b, doll.localPosition + Vector3.up * .4f, whiteSmoke, 2.5f, 6, .5f, .35f, .5f, .7f);
            Glints(b, doll.localPosition + Vector3.up, Mint, 1.05f, .7f, .7f, 8f, .45f);
        }

        // No verified anime reference. Game: plain yellow contact flash; catalogue requires an irregular rush and recoil.
        // An animated shell conveys the lunge; a smaller delayed flash on the user explicitly communicates recoil.
        static void M165(EmeraldMoveBuilder b)
        {
            b.Keys(.75f, 1.2f, 1.65f, 2.7f, Hit, .65f);
            b.HideBaked();
            var r = b.MeshPart("StruggleRush", BotwMeshes.Sphere, Mat(Cream, "Shell"), AttackerChest, Vector3.zero, new Vector3(.6f, .9f, .6f));
            var tr = b.Track(r.transform, r, .3f, 1.7f);
            tr.posFrom = AttackerChest; tr.posTo = Hit;
            tr.posCurve = C(.3f, 0f, .5f, -.04f, .7f, .18f, .85f, .1f, 1.2f, 1f, 1.4f, .8f, 1.7f, 0f);
            tr.posArc = Vector3.up * .4f;
            tr.eulerFrom = new Vector3(0f, 0f, -18f); tr.eulerTo = new Vector3(0f, 0f, 28f); tr.rotCurve = tr.posCurve;
            Streaks(b, AttackerChest, Hit, Cream, .8f, .4f, 24f, .3f, .7f);
            ImpactStar(b, Hit, Cream, 1.2f, 3.8f, 8, 14);
            Dust(b, Target, 1.25f, .7f, 9, .7f);
            HitStars(b, AttackerChest, FootRed, new[] { 1.75f }, 1.6f, 0, .1f);
            HitShell(b, Attacker, FootRed, 1.75f, .35f);
            b.Shake(1.2f, .25f, .25f); b.Shake(1.75f, .1f, .15f);
        }
    }
}
