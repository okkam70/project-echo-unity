using UnityEngine;

/// <summary>
/// Atmosfera dinamica (B3.4): regola luce e ambienza in base a un "mood"
/// (0 = calma/luminosa, 1 = tensione/scura). Il client può impostare ``mood``
/// a runtime per cambiare l'atmosfera della scena (es. quando la storia si
/// fa tesa o si entra nel sottosuolo).
/// </summary>
public class AtmosphereController : MonoBehaviour
{
    [Header("Stato")]
    [Range(0f, 1f)]
    public float mood = 0f;

    [Header("Luce — calma")]
    public Color calmLight = new Color(1f, 0.95f, 0.82f);
    public float calmIntensity = 1.2f;

    [Header("Luce — tensione")]
    public Color tenseLight = new Color(0.4f, 0.45f, 0.65f);
    public float tenseIntensity = 0.45f;

    [Header("Ambiente")]
    public Color calmAmbient = new Color(0.55f, 0.55f, 0.6f);
    public Color tenseAmbient = new Color(0.12f, 0.12f, 0.18f);

    private Light _light;

    void Start()
    {
        _light = GetComponentInChildren<Light>();
        if (_light == null)
        {
            var go = GameObject.Find("Directional Light");
            if (go != null) _light = go.GetComponent<Light>();
        }
    }

    void Update()
    {
        if (_light == null) return;

        _light.color = Color.Lerp(calmLight, tenseLight, mood);
        _light.intensity = Mathf.Lerp(calmIntensity, tenseIntensity, mood);
        RenderSettings.ambientLight = Color.Lerp(calmAmbient, tenseAmbient, mood);
    }

    /// <summary>Imposta il mood (clampato 0..1) — chiamabile dal client a runtime.</summary>
    public void SetMood(float value)
    {
        mood = Mathf.Clamp01(value);
    }
}
