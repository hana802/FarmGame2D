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
            ShowLoadingOnLightBackground(outputPath);
            Debug.Log($"WebGL site built successfully: {outputPath}");
        }

        private static void CustomizeIndexHtml(string outputPath)
        {
            var indexPath = Path.Combine(outputPath, "index.html");
            var cacheVersion = DateTime.UtcNow.ToString("yyyyMMddHHmm");
            var html = File.ReadAllText(indexPath)
                .Replace("<html lang=\"en-us\">", "<html lang=\"ko\">")
                .Replace("<title>Unity Web Player | FarmGame2D</title>", "<title>FarmGame2D 프로토타입</title>")
                .Replace("width=\"960\" height=\"600\"", "width=\"1280\" height=\"720\"")
                .Replace("/WebGL.loader.js", $"/WebGL.loader.js?v={cacheVersion}")
                .Replace("/WebGL.data.gz", $"/WebGL.data.gz?v={cacheVersion}")
                .Replace("/WebGL.framework.js.gz", $"/WebGL.framework.js.gz?v={cacheVersion}")
                .Replace("/WebGL.wasm.gz", $"/WebGL.wasm.gz?v={cacheVersion}")
                .Replace("// config.autoSyncPersistentDataPath = true;", "config.autoSyncPersistentDataPath = true;")
                .Replace(
                    "fullscreenContainer.style.width = \"960px\";\n        fullscreenContainer.style.height = \"600px\";",
                    "fullscreenContainer.style.width = \"min(1280px, calc(100vw - 32px))\";\n        fullscreenContainer.style.aspectRatio = \"16 / 9\";\n        fullscreenContainer.style.height = \"auto\";");
            File.WriteAllText(indexPath, html);
        }

        private static void ShowLoadingOnLightBackground(string outputPath)
        {
            var stylePath = Path.Combine(outputPath, "TemplateData", "style.css");
            if (!File.Exists(stylePath))
            {
                return;
            }

            var style = File.ReadAllText(stylePath)
                .Replace(
                    "body { padding: 0; margin: 0 }",
                    "body { padding: 0; margin: 0; background: #d5e2b4; min-height: 100vh; display: flex; align-items: center; justify-content: center; }")
                .Replace("#unity-container { position: absolute }", "#unity-container { position: relative }")
                .Replace(
                    "#unity-container.unity-desktop { left: 50%; top: 50%; transform: translate(-50%, -50%) }",
                    "#unity-container.unity-desktop { left: auto; top: auto; transform: none }")
                .Replace("background: #1F1F20", "background: #d5e2b4")
                .Replace(
                    "#unity-loading-container { position: absolute; width: 100%; height: 100%; backdrop-filter: blur(5px); display: none; }",
                    "#unity-loading-container { position: absolute; width: 100%; height: 100%; background: rgba(213, 226, 180, 0.92); display: block; }");
            File.WriteAllText(stylePath, style);
        }
    }
}
