using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildWebGL
{
    private static readonly string[] SCENES = {
        "Assets/Scenes/protagonist_room.unity",
        "Assets/Scenes/school_academy.unity",
        "Assets/Scenes/gathering_place.unity",
        "Assets/Scenes/music_studio.unity",
        "Assets/Scenes/underground_access.unity",
        "Assets/Scenes/underground_lab.unity",
    };

    public static void Build()
    {
        string outDir = "/workspace/builds/webgl";
        if (Directory.Exists(outDir))
            Directory.Delete(outDir, true);
        Directory.CreateDirectory(outDir);

        BuildPlayerOptions opts = new BuildPlayerOptions();
        opts.scenes = SCENES;
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
