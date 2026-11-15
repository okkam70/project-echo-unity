using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class RenderEchoScene
{
    public static void Render()
    {
        EditorSceneManager.OpenScene(
            "Assets/ECHO_FirstScene.unity"
        );

        Camera camera = Camera.main;

        if (camera == null)
        {
            throw new System.Exception("ECHO: Camera not found");
        }

        RenderTexture rt = new RenderTexture(1920, 1080, 24);
        rt.Create();

        camera.targetTexture = rt;
        RenderTexture.active = rt;

        camera.Render();

        Texture2D image = new Texture2D(
            1920, 1080, TextureFormat.RGB24, false
        );

        image.ReadPixels(
            new Rect(0, 0, 1920, 1080),
            0, 0
        );

        image.Apply();

        string path = "/workspace/output/ECHO_Render.png";
        File.WriteAllBytes(path, image.EncodeToPNG());

        camera.targetTexture = null;
        RenderTexture.active = null;

        Object.DestroyImmediate(image);
        Object.DestroyImmediate(rt);

        Debug.Log("ECHO_GPU: " + SystemInfo.graphicsDeviceName);
        Debug.Log("ECHO_API: " + SystemInfo.graphicsDeviceType);
        Debug.Log("ECHO_RENDER_SUCCESS: " + path);
    }
}
