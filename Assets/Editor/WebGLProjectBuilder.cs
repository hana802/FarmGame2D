using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace FarmGame2D.Editor
{
    public static class WebGLProjectBuilder
    {
        [MenuItem("FarmGame2D/Build WebGL Site")]
        public static void BuildWebGLSite()
        {
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("Could not resolve the Unity project root.");
            var outputPath = Path.Combine(projectRoot, "Builds", "WebGL");
            Directory.CreateDirectory(outputPath);

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
            if (scenes.Length == 0)
            {
                throw new BuildFailedException("No enabled scenes are registered in Build Settings.");
            }

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"WebGL build failed: {report.summary.result}");
            }

            CustomizeIndexHtml(outputPath);
            Debug.Log($"WebGL site built successfully: {outputPath}");
        }

        private static void CustomizeIndexHtml(string outputPath)
        {
            var indexPath = Path.Combine(outputPath, "index.html");
            var html = File.ReadAllText(indexPath)
                .Replace("<html lang=\"en-us\">", "<html lang=\"ko\">")
                .Replace("<title>Unity Web Player | FarmGame2D</title>", "<title>FarmGame2D 프로토타입</title>")
                .Replace("width=\"960\" height=\"600\"", "width=\"1280\" height=\"720\"")
                .Replace("// config.autoSyncPersistentDataPath = true;", "config.autoSyncPersistentDataPath = true;")
                .Replace(
                    "fullscreenContainer.style.width = \"960px\";\n        fullscreenContainer.style.height = \"600px\";",
                    "fullscreenContainer.style.width = \"min(1280px, calc(100vw - 32px))\";\n        fullscreenContainer.style.aspectRatio = \"16 / 9\";\n        fullscreenContainer.style.height = \"auto\";");
            File.WriteAllText(indexPath, html);
        }
    }
}
