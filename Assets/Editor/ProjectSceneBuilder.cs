using FarmGame2D.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FarmGame2D.Editor
{
    public static class ProjectSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";

        [MenuItem("FarmGame2D/Build Main Scene")]
        public static void Build()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            new GameObject("Farm Game Bootstrap").AddComponent<FarmGameBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log($"Created playable scene at {ScenePath}");
        }

    }
}
