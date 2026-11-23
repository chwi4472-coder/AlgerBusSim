using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace AlgerBusSim.EditorTools
{
    /// <summary>
    /// Applies every build-critical project setting from code, so no binary
    /// ProjectSettings.asset has to be hand-authored or trusted.
    /// Menu: AlgerBusSim / Configure Project
    /// </summary>
    public static class ProjectConfigurator
    {
        public const string PackageId = "com.dz.bussim";
        public const string ProductName = "AlgerBusSim";
        public const string CompanyName = "AlgerBusSim";
        public const string ScenePath = "Assets/Scenes/Bootstrap.unity";

        [MenuItem("AlgerBusSim/Configure Project", priority = 1)]
        public static void Configure()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);

            // --- Android ---
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.forceInternetPermission = true;
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;

            // --- Orientation: landscape only, left is canonical ---
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // --- Rendering: Built-in RP, linear, Vulkan with GLES3 fallback ---
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,
                new[] { GraphicsDeviceType.Vulkan, GraphicsDeviceType.OpenGLES3 });

            // --- Textures: ASTC keeps the atlas budget inside mobile VRAM ---
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

            // Ensure our custom shaders are included in the player build by
            // creating material assets under Resources that reference them.
            string resDir = "Assets/Resources/Shaders";
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(resDir)) AssetDatabase.CreateFolder("Assets/Resources", "Shaders");
            string[] shaderNames = {
                "AlgerBusSim/Unlit/Color", "AlgerBusSim/Unlit/Transparent",
                "AlgerBusSim/Unlit/Tilable", "AlgerBusSim/Unlit/Alpha",
                "AlgerBusSim/Sky/Dome",
                "AlgerBusSim/Post/Composite", "AlgerBusSim/Post/BrightPass", "AlgerBusSim/Post/Blur" };
            foreach (var n in shaderNames)
            {
                Shader sh = Shader.Find(n);
                if (sh == null) continue;
                string matPath = resDir + "/" + n.Replace("/", "_") + ".mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(matPath) == null)
                {
                    var mat = new Material(sh);
                    AssetDatabase.CreateAsset(mat, matPath);
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[AlgerBusSim] Project configured: {PackageId} · IL2CPP · ARM64 · ASTC · Vulkan/GLES3");
        }

        [MenuItem("AlgerBusSim/Validate Project", priority = 2)]
        public static void Validate()
        {
            int problems = 0;
            void Check(bool ok, string message)
            {
                if (ok) { Debug.Log("  OK   " + message); }
                else { Debug.LogError("  FAIL " + message); problems++; }
            }

            Check(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) == PackageId,
                "application id is " + PackageId);
            Check(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) == ScriptingImplementation.IL2CPP,
                "scripting backend is IL2CPP");
            Check(PlayerSettings.Android.targetArchitectures == AndroidArchitecture.ARM64,
                "architecture is ARM64 only");
            Check(PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft,
                "orientation is landscape-left");

            var scenes = EditorBuildSettings.scenes;
            Check(scenes != null && scenes.Length > 0 && scenes[0].path == ScenePath,
                "bootstrap scene is first in build settings");

            Debug.Log(problems == 0
                ? "[AlgerBusSim] Validation passed."
                : $"[AlgerBusSim] Validation found {problems} problem(s).");
        }
    }
}
