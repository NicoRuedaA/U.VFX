using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    /// <summary>
    /// Mallas procedurales de los efectos (Assets/BotwVFX/Meshes).
    /// </summary>
    public static class BotwMeshes
    {
        public const string Folder = "Assets/BotwVFX/Meshes";

        // Color32 (no Color): las partículas de malla leen el color de vértice como 4 bytes.
        static readonly Color32 White = new Color32(255, 255, 255, 255);

        public static Mesh Sphere => Load("M_Sphere");
        public static Mesh SpikyBall => Load("M_SpikyBall");
        public static Mesh Rock => Load("M_Rock");
        public static Mesh Ring => Load("M_Ring");
        public static Mesh Arc => Load("M_Arc");
        public static Mesh Torus => Load("M_Torus");
        public static Mesh ArcWide => Load("M_ArcWide");

        public static void GenerateAll()
        {
            Directory.CreateDirectory(Folder);
            SaveMesh("M_Sphere", BuildSphere(3));
            SaveMesh("M_SpikyBall", BuildSpikyBall());
            SaveMesh("M_Rock", BuildRock(7));
            SaveMesh("M_Ring", BuildRing(0.55f, 1f, 64));
            SaveMesh("M_Arc", BuildArc(Mathf.PI * 4f / 3f, 0.22f, 0.5f));
            SaveMesh("M_ArcWide", BuildArc(Mathf.PI * 1.5f, 0.42f, 0.9f));
            SaveMesh("M_Torus", BuildTorus(1f, 0.08f, 48, 8));
        }

        static Mesh Load(string name) => AssetDatabase.LoadAssetAtPath<Mesh>($"{Folder}/{name}.asset");

        // Reutiliza el asset si ya existe para no romper referencias en prefabs.
        static void SaveMesh(string name, Mesh mesh)
        {
            string path = $"{Folder}/{name}.asset";
            mesh.name = name;
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null)
            {
                existing.Clear();
                EditorUtility.CopySerialized(mesh, existing);
                existing.name = name;
                EditorUtility.SetDirty(existing);
                Object.DestroyImmediate(mesh);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, path);
            }
        }

        // ------------------------------------------------------------ icosfera

        static void Icosphere(int subdivisions, out List<Vector3> verts, out List<int> tris)
        {
            float t = (1f + Mathf.Sqrt(5f)) * 0.5f;
            verts = new List<Vector3>
            {
                new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
                new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
                new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
            };
            for (int i = 0; i < verts.Count; i++)
                verts[i] = verts[i].normalized;
            tris = new List<int>
            {
                0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
                1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
                3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
                4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
            };

            for (int s = 0; s < subdivisions; s++)
            {
                var cache = new Dictionary<long, int>();
                var vlist = verts;
                int Mid(int a, int b)
                {
                    long key = a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
                    if (cache.TryGetValue(key, out int idx))
                        return idx;
                    vlist.Add(((vlist[a] + vlist[b]) * 0.5f).normalized);
                    idx = vlist.Count - 1;
                    cache[key] = idx;
                    return idx;
                }
                var next = new List<int>(tris.Count * 4);
                for (int i = 0; i < tris.Count; i += 3)
                {
                    int a = tris[i], b = tris[i + 1], c = tris[i + 2];
                    int ab = Mid(a, b), bc = Mid(b, c), ca = Mid(c, a);
                    next.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
                }
                tris = next;
            }
        }

        static Mesh BuildSphere(int subdivisions)
        {
            Icosphere(subdivisions, out var verts, out var tris);
            var uvs = new Vector2[verts.Count];
            var colors = new Color32[verts.Count];
            for (int i = 0; i < verts.Count; i++)
            {
                var v = verts[i];
                uvs[i] = new Vector2(Mathf.Atan2(v.z, v.x) / (2f * Mathf.PI) + 0.5f, Mathf.Acos(Mathf.Clamp(v.y, -1f, 1f)) / Mathf.PI);
                colors[i] = White;
            }
            var mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.SetNormals(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        // Bola de pinchos (fase 1 de la explosión del artículo de 80.lv).
        // UV.y = altura relativa del vértice (0 en la base, 1 en la punta).
        static Mesh BuildSpikyBall()
        {
            Icosphere(2, out var verts, out var tris);
            var rng = new System.Random(5);
            var radii = new float[verts.Count];
            float maxR = 1f;
            for (int i = 0; i < verts.Count; i++)
            {
                // Los 42 primeros vértices (niveles 0 y 1) son las puntas.
                radii[i] = i < 42 ? 1.45f + (float)rng.NextDouble() * 0.7f : 0.92f + (float)rng.NextDouble() * 0.12f;
                maxR = Mathf.Max(maxR, radii[i]);
            }
            var positions = new Vector3[verts.Count];
            var uvs = new Vector2[verts.Count];
            var colors = new Color32[verts.Count];
            for (int i = 0; i < verts.Count; i++)
            {
                positions[i] = verts[i] * radii[i];
                uvs[i] = new Vector2(Mathf.Atan2(verts[i].z, verts[i].x) / (2f * Mathf.PI) + 0.5f, Mathf.InverseLerp(0.9f, maxR, radii[i]));
                colors[i] = White;
            }
            var mesh = new Mesh();
            mesh.vertices = positions;
            mesh.uv = uvs;
            mesh.colors32 = colors;
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // Roca low-poly con sombreado plano (escombros).
        static Mesh BuildRock(int seed)
        {
            Icosphere(0, out var verts, out var tris);
            var rng = new System.Random(seed);
            var scale = new Vector3(1f, 0.72f, 0.88f);
            for (int i = 0; i < verts.Count; i++)
                verts[i] = Vector3.Scale(verts[i] * (0.75f + (float)rng.NextDouble() * 0.4f), scale);

            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color32>();
            var indices = new List<int>();
            for (int i = 0; i < tris.Count; i += 3)
            {
                Vector3 a = verts[tris[i]], b = verts[tris[i + 1]], c = verts[tris[i + 2]];
                Vector3 n = Vector3.Cross(b - a, c - a).normalized;
                foreach (var p in new[] { a, b, c })
                {
                    indices.Add(positions.Count);
                    positions.Add(p);
                    normals.Add(n);
                    uvs.Add(new Vector2(0.5f, 0.5f));
                    colors.Add(White);
                }
            }
            var mesh = new Mesh();
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        // Anillo plano en XZ. UV.x = vuelta (0..1), UV.y = radio (0 interior, 1 exterior).
        static Mesh BuildRing(float inner, float outer, int segments)
        {
            var positions = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color32>();
            var normals = new List<Vector3>();
            var indices = new List<int>();
            for (int s = 0; s <= segments; s++)
            {
                float u = (float)s / segments;
                float a = u * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                positions.Add(dir * inner);
                positions.Add(dir * outer);
                uvs.Add(new Vector2(u, 0f));
                uvs.Add(new Vector2(u, 1f));
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                colors.Add(White);
                colors.Add(White);
                if (s < segments)
                {
                    int i = s * 2;
                    indices.AddRange(new[] { i, i + 1, i + 3, i, i + 3, i + 2 });
                }
            }
            var mesh = new Mesh();
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        // Tira curva alrededor del eje Z (las "energy stripes" del rayo Guardián).
        // UV.x = a lo largo del arco, UV.y = a lo ancho.
        static Mesh BuildArc(float arc, float width, float helix)
        {
            const int segments = 40;
            var positions = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color32>();
            var normals = new List<Vector3>();
            var indices = new List<int>();
            for (int s = 0; s <= segments; s++)
            {
                float u = (float)s / segments;
                float a = u * arc;
                var radial = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
                float z = (u - 0.5f) * helix;
                positions.Add(radial + new Vector3(0f, 0f, z - width * 0.5f));
                positions.Add(radial + new Vector3(0f, 0f, z + width * 0.5f));
                uvs.Add(new Vector2(u, 0f));
                uvs.Add(new Vector2(u, 1f));
                normals.Add(radial);
                normals.Add(radial);
                colors.Add(White);
                colors.Add(White);
                if (s < segments)
                {
                    int i = s * 2;
                    indices.AddRange(new[] { i, i + 1, i + 3, i, i + 3, i + 2 });
                }
            }
            var mesh = new Mesh();
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        static Mesh BuildTorus(float radius, float tube, int segments, int sides)
        {
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color32>();
            var indices = new List<int>();
            for (int s = 0; s <= segments; s++)
            {
                float a = (float)s / segments * Mathf.PI * 2f;
                var center = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * radius;
                for (int k = 0; k <= sides; k++)
                {
                    float b = (float)k / sides * Mathf.PI * 2f;
                    var n = Mathf.Cos(b) * center.normalized + Mathf.Sin(b) * Vector3.up;
                    positions.Add(center + n * tube);
                    normals.Add(n);
                    uvs.Add(new Vector2((float)s / segments, (float)k / sides));
                    colors.Add(White);
                }
            }
            int row = sides + 1;
            for (int s = 0; s < segments; s++)
            for (int k = 0; k < sides; k++)
            {
                int i = s * row + k;
                indices.AddRange(new[] { i, i + 1, i + row + 1, i, i + row + 1, i + row });
            }
            var mesh = new Mesh();
            mesh.SetVertices(positions);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
