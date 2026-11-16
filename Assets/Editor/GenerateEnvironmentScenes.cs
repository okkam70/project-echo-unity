using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class GenerateEnvironmentScenes
{
    private static readonly string[] ENVIRONMENTS = {
        "protagonist_room", "school_academy", "gathering_place",
        "music_studio", "underground_access", "underground_lab"
    };

    private static readonly Color[] COLORS = {
        new Color(0.95f, 0.85f, 0.95f),  // rosa/lilla
        new Color(0.60f, 0.75f, 0.90f),  // blu
        new Color(0.95f, 0.90f, 0.70f),  // giallo caldo
        new Color(0.70f, 0.50f, 0.85f),  // viola
        new Color(0.35f, 0.35f, 0.40f),  // grigio scuro
        new Color(0.20f, 0.50f, 0.35f),  // verde scuro
    };

    public static void Generate()
    {
        Directory.CreateDirectory("Assets/Scenes");
        for (int i = 0; i < ENVIRONMENTS.Length; i++)
        {
            string id = ENVIRONMENTS[i];
            Color color = COLORS[i];

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Pavimento
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(3, 1, 3);
            floor.GetComponent<Renderer>().sharedMaterial = MakeMaterial(color * 0.55f);

            // Marker cubo (identifica l'ambiente)
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "EnvironmentMarker";
            marker.transform.position = new Vector3(0, 1, 0);
            marker.transform.localScale = new Vector3(2, 2, 2);
            marker.GetComponent<Renderer>().sharedMaterial = MakeMaterial(color);

            // Luce direzionale
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // Camera
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            var camera = cameraObj.AddComponent<Camera>();
            cameraObj.transform.position = new Vector3(4, 3, -6);
            cameraObj.transform.LookAt(marker.transform);

            string path = "Assets/Scenes/" + id + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log("ECHO_SCENE_GENERATED: " + path);
        }
        Debug.Log("ECHO_ALL_SCENES_GENERATED: " + ENVIRONMENTS.Length);
    }

    private static Material MakeMaterial(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }
}
