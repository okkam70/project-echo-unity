using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

/// <summary>
/// Genera le 6 scene ambientali (B1.5) con atmosfera distinta per superficie
/// e sottosuolo (B3.4). Ogni scena ha pavimento, marker colorato, luce
/// direzionale, camera e un AtmosphereController per l'atmosfera dinamica.
///
/// Superficie (luminosa/calda): protagonist_room, school_academy,
///   gathering_place, music_studio.
/// Sottosuolo (scura/fredda): underground_access, underground_lab.
/// </summary>
public static class GenerateEnvironmentScenes
{
    private struct EnvInfo
    {
        public string id;
        public bool underground;
        public Color color;
    }

    private static readonly EnvInfo[] ENVIRONMENTS = {
        new EnvInfo { id = "protagonist_room",  underground = false, color = new Color(0.95f, 0.85f, 0.95f) }, // rosa/lilla
        new EnvInfo { id = "school_academy",   underground = false, color = new Color(0.60f, 0.75f, 0.90f) }, // blu
        new EnvInfo { id = "gathering_place",  underground = false, color = new Color(0.95f, 0.90f, 0.70f) }, // giallo caldo
        new EnvInfo { id = "music_studio",     underground = false, color = new Color(0.70f, 0.50f, 0.85f) }, // viola
        new EnvInfo { id = "underground_access", underground = true, color = new Color(0.35f, 0.35f, 0.40f) }, // grigio scuro
        new EnvInfo { id = "underground_lab",    underground = true, color = new Color(0.20f, 0.50f, 0.35f) }, // verde scuro
    };

    public static void Generate()
    {
        Directory.CreateDirectory("Assets/Scenes");
        for (int i = 0; i < ENVIRONMENTS.Length; i++)
        {
            EnvInfo env = ENVIRONMENTS[i];

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Pavimento ---
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(3, 1, 3);
            floor.GetComponent<Renderer>().sharedMaterial =
                MakeMaterial(env.color * (env.underground ? 0.4f : 0.55f));

            // --- Marker colorato (identifica l'ambiente) ---
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "EnvironmentMarker";
            marker.transform.position = new Vector3(0, 1, 0);
            marker.transform.localScale = new Vector3(2, 2, 2);
            marker.GetComponent<Renderer>().sharedMaterial = MakeMaterial(env.color);

            // --- Luce direzionale (atmosfera base) ---
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = env.underground ? 0.5f : 1.2f;
            light.color = env.underground
                ? new Color(0.5f, 0.55f, 0.7f)   // fredda
                : new Color(1f, 0.95f, 0.82f);    // calda
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // --- Atmosfera dinamica (B3.4) ---
            var atmosphere = new GameObject("Atmosphere");
            var controller = atmosphere.AddComponent<AtmosphereController>();
            // Il sottosuolo parte già "teso", la superficie "calma".
            controller.SetMood(env.underground ? 0.6f : 0f);

            // --- Camera ---
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            var camera = cameraObj.AddComponent<Camera>();
            cameraObj.transform.position = new Vector3(4, 3, -6);
            cameraObj.transform.LookAt(marker.transform);

            // --- Ambiente (surface chiara, underground scura + nebbia) ---
            RenderSettings.ambientLight = env.underground
                ? new Color(0.15f, 0.15f, 0.2f)
                : new Color(0.55f, 0.55f, 0.6f);

            RenderSettings.fog = env.underground;
            if (env.underground)
            {
                RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.08f);
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = 0.03f;
            }

            string path = "Assets/Scenes/" + env.id + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log("ECHO_SCENE_GENERATED: " + path + (env.underground ? " [underground]" : " [surface]"));
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
