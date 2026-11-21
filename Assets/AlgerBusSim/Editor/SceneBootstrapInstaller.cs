using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AlgerBusSim.EditorTools
{
    /// <summary>
    /// Creates the single authored scene the build needs. Everything else in the
    /// game is generated at runtime, so this scene contains exactly three objects:
    /// camera, sun, bootstrap.
    /// </summary>
    [InitializeOnLoad]
    public static class SceneBootstrapInstaller
    {
        const string ScenePath = ProjectConfigurator.ScenePath;
        const string SessionKey = "AlgerBusSim.SceneChecked";

        static SceneBootstrapInstaller()
        {
            EditorApplication.delayCall += AutoInstallOnce;
        }

        static void AutoInstallOnce()
        {
            if (SessionState.GetBool(SessionKey, false)) return;
            SessionState.SetBool(SessionKey, true);

            if (File.Exists(ScenePath))
            {
                RegisterScene();
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            // Never throw away unsaved work.
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty) return;

            CreateScene();
        }

        [MenuItem("AlgerBusSim/Create Bootstrap Scene", priority = 3)]
        public static void CreateScene()
        {
            Directory.CreateDirectory("Assets/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.72f, 0.88f);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.15f;
            cam.farClipPlane = 900f;
            camGo.transform.SetPositionAndRotation(new Vector3(0f, 8f, -16f), Quaternion.Euler(8f, 0f, 0f));
            camGo.AddComponent<AudioListener>();

            var sunGo = new GameObject("Sun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.96f, 0.87f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

            var bootGo = new GameObject("Bootstrap");
            bootGo.AddComponent<AlgerBusSim.Core.Bootstrap>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            RegisterScene();

            Debug.Log($"[AlgerBusSim] Bootstrap scene created at {ScenePath}");
        }

        static void RegisterScene()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes != null && scenes.Length > 0 && scenes[0].path == ScenePath) return;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
