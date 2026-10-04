using System;
using UnityEditor;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    /// <summary>Imports the attributed static Substitute model into move 164's existing timeline.</summary>
    public static class EmeraldSubstituteModel
    {
        const string Folder = "Assets/BotwVFX/Models/Substitute";

        public static Material WhiteSmoke()
        {
            const string path = Folder + "/Substitute_WhiteSmoke.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(BotwMaterials.Get("EX_Smoke")) { name = "Substitute_WhiteSmoke" };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_SmokeLight", Color.white);
            material.SetColor("_SmokeShadow", new Color(.7f, .76f, .8f, 1f));
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Transform Add(EmeraldMoveBuilder b, Vector3 position)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Substitute.fbx");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/Source/textures/ob0301_00_Body1_tga_baseColor.png");
            var shader = Shader.Find("BotwVFX/Environment/Toon Lit");
            if (source == null || texture == null || shader == null)
                throw new InvalidOperationException("Substitute model, texture or Toon Lit shader is missing.");

            const string materialPath = Folder + "/Substitute_Toon.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(shader) { name = "Substitute_Toon" };
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.shader = shader;
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetColor("_ShadowColor", new Color(.6f, .67f, .52f, 1f));
            material.SetColor("_RimColor", new Color(1f, .96f, .8f, .08f));
            material.SetFloat("_RampThreshold", .05f);
            material.SetFloat("_RampSoftness", .02f);
            material.SetFloat("_AmbientAmount", .25f);
            EditorUtility.SetDirty(material);

            var pivot = b.Node("SubstituteDoll", position);
            // Keep the model's FBX axis conversion intact; normalize its imported bounds rather than assuming FBX units.
            var instance = UnityEngine.Object.Instantiate(source, pivot, false);
            instance.name = "LunaEagle_Substitute";
            instance.transform.localRotation = Quaternion.Euler(0f, 90f, 0f) * instance.transform.localRotation;
            var renderers = instance.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0)
                throw new InvalidOperationException("Substitute model contains no mesh renderers.");
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            if (bounds.size.y <= .0001f)
                throw new InvalidOperationException("Substitute model has invalid bounds.");
            instance.transform.localScale *= 1.65f / bounds.size.y;
            bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            instance.transform.position += pivot.position - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            foreach (var renderer in renderers)
            {
                var materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++) materials[i] = material;
                renderer.sharedMaterials = materials;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = true;
                b.Track(renderer.transform, renderer, .8f, 2.75f);
            }
            var growth = b.Track(pivot, null, .8f, 2.75f);
            growth.scaleFrom = Vector3.zero;
            growth.scaleTo = Vector3.one;
            growth.scaleCurve = Fx.C(.8f, 0f, 1.02f, 1.08f, 1.12f, 1f, 2.5f, 1f, 2.75f, 0f);
            return pivot;
        }
    }
}
