using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Orchestrazione del loop didattico nel gioco + UI di test (IMGUI).
///
/// Flusso: avvio → crea profilo → acquisisce un compito demo → pianifica la
/// missione → mostra il problema → la giocatrice risponde → verifica → se
/// corretta completa la missione (XP) → dialogo narrativo (DeepSeek via Nous).
/// </summary>
public class GameManager : MonoBehaviour
{
    private BackendClient _client;

    // stato
    private string _profileId;
    private string _taskId;
    private string _missionId;
    private string[] _objectiveIds;
    private string _problem = "";
    private int _rewardXp = 0;
    private int _xp = 0;

    // UI
    private string _answerInput = "";
    private string _feedback = "";
    private string _dialogue = "";
    private string _status = "Avvio...";
    private bool _busy = false;

    private void Start()
    {
        _client = GetComponent<BackendClient>();
        if (_client == null)
            _client = gameObject.AddComponent<BackendClient>();
        StartCoroutine(Boot());
    }

    private IEnumerator Boot()
    {
        // 1) profilo
        _status = "Creo il profilo...";
        string profileJson = null;
        yield return _client.CreateProfile("Viola", ok => profileJson = ok, err => _status = "Errore profilo: " + err);
        if (profileJson == null) yield break;
        var profile = JsonUtility.FromJson<BackendClient.Profile>(profileJson);
        _profileId = profile.id;
        _xp = profile.xp;

        // 2) compito demo
        _status = "Acquisisco il compito...";
        string taskJson = null;
        yield return _client.AcquireTask("2x + 3 = 11", ok => taskJson = ok, err => _status = "Errore compito: " + err);
        if (taskJson == null) yield break;
        var task = JsonUtility.FromJson<BackendClient.Task>(taskJson);
        _taskId = task.task_id;
        _problem = (task.exercises != null && task.exercises.Length > 0)
            ? task.exercises[0].expression
            : task.raw_ocr;

        // 3) missione
        _status = "Pianifico la missione...";
        string missionJson = null;
        yield return _client.PlanMission(_profileId, _taskId, ok => missionJson = ok, err => _status = "Errore missione: " + err);
        if (missionJson == null) yield break;
        var mission = JsonUtility.FromJson<BackendClient.Mission>(missionJson);
        _missionId = mission.mission_id;
        _rewardXp = mission.reward_xp;
        if (mission.objectives != null)
        {
            _objectiveIds = new string[mission.objectives.Length];
            for (int i = 0; i < mission.objectives.Length; i++)
                _objectiveIds[i] = mission.objectives[i].id;
        }

        _status = "";
        _feedback = "";
        // dialogo di benvenuto
        StartCoroutine(WelcomeDialogue());
    }

    private IEnumerator WelcomeDialogue()
    {
        yield return _client.Dialogue("vittorio",
            "Viola è appena arrivata a scuola e sta per affrontare un problema di matematica",
            _profileId,
            ok =>
            {
                var d = JsonUtility.FromJson<BackendClient.DialogueLine>(ok);
                _dialogue = d.line;
            },
            err => _dialogue = "(dialogo non disponibile)");
    }

    private void SubmitAnswer()
    {
        if (_busy || string.IsNullOrWhiteSpace(_answerInput) || string.IsNullOrEmpty(_taskId))
            return;
        _busy = true;
        _status = "Verifico...";
        StartCoroutine(DoVerify(_answerInput.Trim()));
    }

    private IEnumerator DoVerify(string answer)
    {
        string verifyJson = null;
        yield return _client.VerifyAnswer(_taskId, answer,
            ok => verifyJson = ok,
            err => { _status = "Errore verifica: " + err; _busy = false; });

        if (verifyJson == null) yield break;
        var v = JsonUtility.FromJson<BackendClient.Verify>(verifyJson);
        _feedback = v.feedback;

        if (v.understood && !string.IsNullOrEmpty(_missionId))
        {
            _status = "Missione completata!";
            string completeJson = null;
            yield return _client.CompleteMission(_missionId, _objectiveIds ?? new string[0],
                ok => completeJson = ok,
                err => _status = "Errore completamento: " + err);
            if (completeJson != null)
            {
                var cmp = JsonUtility.FromJson<BackendClient.Complete>(completeJson);
                _xp += cmp.xp_awarded;
                StartCoroutine(RewardDialogue());
            }
        }
        else if (!v.understood)
        {
            _status = "Riprova: la risposta non è ancora giusta.";
        }
        _busy = false;
    }

    private IEnumerator RewardDialogue()
    {
        yield return _client.Dialogue("vittorio",
            "Viola ha appena risolto correttamente 2x+3=11",
            _profileId,
            ok =>
            {
                var d = JsonUtility.FromJson<BackendClient.DialogueLine>(ok);
                _dialogue = d.line;
            },
            err => _dialogue = "(dialogo non disponibile)");
    }

    private void OnGUI()
    {
        GUI.skin.label.fontSize = 20;
        GUI.skin.textField.fontSize = 20;
        GUI.skin.button.fontSize = 20;

        GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));

        GUILayout.Label("Project ECHO — demo giocabile");
        GUILayout.Space(10);

        if (!string.IsNullOrEmpty(_problem))
            GUILayout.Label("Compito:  " + _problem);
        if (_rewardXp > 0)
            GUILayout.Label("Ricompensa: " + _rewardXp + " XP");
        GUILayout.Label("XP attuale: " + _xp);

        GUILayout.Space(15);
        GUILayout.Label("La tua risposta:");
        GUILayout.BeginHorizontal();
        _answerInput = GUILayout.TextField(_answerInput, GUILayout.Width(160));
        if (GUILayout.Button("Invia", GUILayout.Width(90)) || (Event.current.isKey && Event.current.keyCode == KeyCode.Return))
            SubmitAnswer();
        GUILayout.EndHorizontal();

        GUILayout.Space(15);
        if (!string.IsNullOrEmpty(_feedback))
            GUILayout.Label("Feedback: " + _feedback);
        if (!string.IsNullOrEmpty(_status))
            GUILayout.Label(_status);

        GUILayout.Space(20);
        if (!string.IsNullOrEmpty(_dialogue))
        {
            GUILayout.Label("Vittorio:");
            GUILayout.Label("  \"" + _dialogue + "\"");
        }

        GUILayout.EndArea();
    }
}
