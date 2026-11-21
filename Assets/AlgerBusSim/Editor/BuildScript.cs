using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AlgerBusSim.EditorTools
{
    /// <summary>
    /// One-click and batch-mode Android build.
    ///   Unity.exe -quit -batchmode -projectPath . ^
    ///     -executeMethod AlgerBusSim.EditorTools.BuildScript.BuildAndroid
    /// </summary>
    public static class BuildScript
    {
        const string OutputDir = "Builds";
        const string ApkName = "AlgerBusSim.apk";

        [MenuItem("AlgerBusSim/Build APK", priority = 20)]
        public static void BuildAndroid()
        {
            ProjectConfigurator.Configure();

            if (!File.Exists(ProjectConfigurator.ScenePath))
            {
                SceneBootstrapInstaller.CreateScene();
            }

            Directory.CreateDirectory(OutputDir);
            string output = Path.Combine(OutputDir, ApkName);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ProjectConfigurator.ScenePath },
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[AlgerBusSim] APK built: {output} " +
                          $"({summary.totalSize / (1024f * 1024f):F1} MB, {summary.totalTime.TotalSeconds:F0}s)");
            }
            else
            {
                Debug.LogError($"[AlgerBusSim] Build {summary.result}: {summary.totalErrors} error(s)");
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
            }
        }

        [MenuItem("AlgerBusSim/Build APK (Development)", priority = 21)]
        public static void BuildAndroidDevelopment()
        {
            ProjectConfigurator.Configure();
            if (!File.Exists(ProjectConfigurator.ScenePath)) SceneBootstrapInstaller.CreateScene();
            Directory.CreateDirectory(OutputDir);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ProjectConfigurator.ScenePath },
                locationPathName = Path.Combine(OutputDir, "AlgerBusSim-dev.apk"),
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.ConnectWithProfiler
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log($"[AlgerBusSim] Dev build {report.summary.result}");
        }
    }
}
