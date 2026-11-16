using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildWebGL
{
    public static void Build()
    {
        EditorSceneManager.OpenScene("Assets/ECHO_FirstScene.unity");

        string outDir = "/workspace/builds/webgl";
        if (Directory.Exists(outDir))
            Directory.Delete(outDir, true);
        Directory.CreateDirectory(outDir);

        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = new[] { "Assets/ECHO_FirstScene.unity" };
        opts.locationPathName = outDir;
        opts.target = BuildTarget.WebGL;
        opts.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError("ECHO_WEBGL_BUILD_FAILED: " + report.summary.result);
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log("ECHO_WEBGL_BUILD_SUCCESS: " + outDir);
    }
}
