using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
using UnityEngine;

// Editor-only build entry point; never included in the tablet application.
public static class LamieTabletBuild
{
    private static string RequestPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/LamieTabletBuild.request"));
    private static string StatusPath => Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/LamieTabletBuild.status"));

    [InitializeOnLoadMethod]
    private static void InstallRequestHandler()
    {
        EditorApplication.update -= ProcessRequest;
        EditorApplication.update += ProcessRequest;
    }

    private static void ProcessRequest()
    {
        if (!File.Exists(RequestPath) || EditorApplication.isCompiling || EditorApplication.isUpdating ||
            EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return;
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isDirty)
            {
                File.WriteAllText(StatusPath, "Awaiting scene save: " + scene.path);
                return;
            }
        }
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            File.WriteAllText(StatusPath, "Switching build target to Android.");
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                File.Delete(RequestPath);
                File.WriteAllText(StatusPath, "FAILED: could not switch build target.");
            }
            return;
        }
        string output = File.ReadAllText(RequestPath).Trim();
        File.Delete(RequestPath);
        File.WriteAllText(StatusPath, "Building development APK: " + output);
        try
        {
            BuildTo(output);
            File.WriteAllText(StatusPath, "SUCCEEDED: " + output);
        }
        catch (Exception error)
        {
            File.WriteAllText(StatusPath, "FAILED: " + error);
            Debug.LogException(error);
        }
    }

    public static void Build()
    {
        string output = Environment.GetCommandLineArgs()
            .SkipWhile(value => value != "-lamieOutput").Skip(1).FirstOrDefault();
        BuildTo(output);
    }

    private static void BuildTo(string output)
    {
        if (string.IsNullOrWhiteSpace(output) || Path.GetExtension(output) != ".apk")
            throw new ArgumentException("Pass an absolute APK path using -lamieOutput.");
        output = Path.GetFullPath(output);
        if (File.Exists(output)) throw new IOException("APK already exists: " + output);

        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled)
            .Select(scene => scene.path).Distinct().ToArray();
        if (!scenes.Any(scene => scene.EndsWith("LoginScene.unity")) ||
            !scenes.Any(scene => scene.EndsWith("interactive_Game_Level.unity")))
            throw new InvalidOperationException("Required login/map scenes are missing.");

        string previousName = PlayerSettings.productName;
        string previousId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
        bool previousBundle = EditorUserBuildSettings.buildAppBundle;
        bool previousExport = EditorUserBuildSettings.exportAsGoogleAndroidProject;
        bool previousDevelopment = EditorUserBuildSettings.development;
        bool previousExpansion = PlayerSettings.Android.splitApplicationBinary;
        bool previousCustomKeystore = PlayerSettings.Android.useCustomKeystore;
        try
        {
            PlayerSettings.productName = "Lamie Test";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.lamieteam.lamie.tablettest");
            PlayerSettings.Android.splitApplicationBinary = false;
            PlayerSettings.Android.useCustomKeystore = false;
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            EditorUserBuildSettings.development = true;
            AssetDatabase.SaveAssets();

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.CompressWithLz4
            });
            Debug.Log("Lamie tablet build result: " + report.summary.result +
                "; errors=" + report.summary.totalErrors + "; output=" + output);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Android build failed; inspect the build log.");
            File.WriteAllText(output + ".build-info.txt",
                "Product: Lamie Test\nPackage: com.lamieteam.lamie.tablettest\n" +
                "Development build: true\n" +
                "Scenes:\n" + string.Join("\n", scenes) + "\n" +
                "Build size: " + report.summary.totalSize + " bytes\n" +
                "Built UTC: " + DateTime.UtcNow.ToString("o") + "\n");
        }
        finally
        {
            PlayerSettings.productName = previousName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, previousId);
            PlayerSettings.Android.splitApplicationBinary = previousExpansion;
            PlayerSettings.Android.useCustomKeystore = previousCustomKeystore;
            EditorUserBuildSettings.buildAppBundle = previousBundle;
            EditorUserBuildSettings.exportAsGoogleAndroidProject = previousExport;
            EditorUserBuildSettings.development = previousDevelopment;
            AssetDatabase.SaveAssets();
        }
    }
}
