using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class CreateEchoScene
{
    public static void Generate()
    {
        var scene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Single
        );

        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "ECHO_Floor";
        floor.transform.localScale = new Vector3(2, 1, 2);

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "ECHO_TestCube";
        cube.transform.position = new Vector3(0, 1, 0);

        var lightObj = new GameObject("Directional Light");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.5f;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        var cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        var camera = cameraObj.AddComponent<Camera>();
        cameraObj.transform.position = new Vector3(4, 3, -6);
        cameraObj.transform.LookAt(cube.transform);

        EditorSceneManager.SaveScene(
            scene,
            "Assets/ECHO_FirstScene.unity"
        );

        Debug.Log("ECHO_SCENE_CREATED");
    }
}
