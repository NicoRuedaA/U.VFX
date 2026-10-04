using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BotwVfx.EditorTools
{
    public static class EmeraldPackageExport
    {
        public static void RunBatch()
        {
            var output = Environment.GetEnvironmentVariable("EMERALD_PACKAGE_OUTPUT");
            if (string.IsNullOrEmpty(output)) throw new InvalidOperationException("Set EMERALD_PACKAGE_OUTPUT to a new .unitypackage path.");
            if (File.Exists(output)) throw new IOException("Refusing to overwrite an existing package: " + output);
            Export(output);
        }

        public static void RunOrganizedBatch()
        {
            var output = Environment.GetEnvironmentVariable("EMERALD_PACKAGE_OUTPUT");
            if (string.IsNullOrEmpty(output) || File.Exists(output))
                throw new IOException("Set EMERALD_PACKAGE_OUTPUT to a new package path.");
            var temporary = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".unitypackage");
            try
            {
                Export(temporary);
                var script = Path.GetFullPath("Tools/organize_emerald_package.py");
                var start = new System.Diagnostics.ProcessStartInfo("python3",
                    "\"" + script + "\" \"" + temporary + "\" \"" + output + "\"") { UseShellExecute = false };
                using var process = System.Diagnostics.Process.Start(start);
                process.WaitForExit();
                if (process.ExitCode != 0) throw new IOException("Package organization failed.");
            }
            finally
            {
                File.Delete(temporary); File.Delete(temporary + ".contents.txt");
            }
        }

        static void Export(string output)
        {
            var roots = Directory.GetFiles("Assets/BotwVFX/Prefabs/Emerald", "*.prefab");
            if (roots.Length != 165) throw new InvalidDataException("Expected exactly 165 prefabs.");
            var paths = new HashSet<string>(AssetDatabase.GetDependencies(roots, true));
            // C# inheritance and static calls are not serialized asset dependencies.
            foreach (var name in new[] { "EmeraldMoveVfx", "EmeraldActorReplacement", "VfxTimeline", "VfxDirector", "FaceCamera" })
                paths.Add("Assets/BotwVFX/Scripts/Runtime/" + name + ".cs");
            paths.Add("Assets/EmeraldMoveVFX/Runtime/EmeraldVfxPlayer.cs");
            paths.Add("Assets/BotwVFX/Shaders/BotwVFX.cginc");
            paths.Add("Assets/EmeraldMoveVFX/UPSTREAM-LICENSE.txt");
            paths.Add("Assets/EmeraldMoveVFX/README.md");
            paths.Add("Assets/BotwVFX/Models/Substitute/LICENSE.txt");
            paths.Add("Assets/BotwVFX/Models/Substitute/Source/license.txt");
            paths.Add("Assets/BotwVFX/EmeraldPackageREADME.md");
            paths.UnionWith(Directory.GetFiles("Assets/BotwVFX/Documentation/Previews", "*.jpg"));
            var assets = paths.Where(p => p.StartsWith("Assets/") && File.Exists(p)).OrderBy(p => p).ToArray();
            if (assets.Any(p => p.Contains("/Editor/") || p.EndsWith(".unity")))
                throw new InvalidDataException("Unexpected editor or scene dependency.");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            // Closure is explicit: IncludeDependencies can pull unrelated project scripts.
            AssetDatabase.ExportPackage(assets, output, ExportPackageOptions.Default);
            File.WriteAllLines(output + ".contents.txt", assets);
            Debug.Log("Exported 165 Emerald prefabs with " + assets.Length + " assets: " + output);
        }
    }
}
