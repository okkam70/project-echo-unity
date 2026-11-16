using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Client HTTP verso il backend ECHO (API /api/v1). In WebGL il gioco è servito
/// dallo stesso backend (stessa origine), quindi baseUrl = "" (path relativi).
/// </summary>
public class BackendClient : MonoBehaviour
{
    [Tooltip("Base URL dell'API. Vuoto = stessa origine del gioco (default in produzione).")]
    public string baseUrl = "";

    public const string API = "/api/v1";

    public IEnumerator PostJson(string path, string json, Action<string> onOk, Action<string> onErr)
    {
        string url = baseUrl + path;
        using (var req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                onOk?.Invoke(req.downloadHandler.text);
            else
                onErr?.Invoke(req.error);
        }
    }

    public IEnumerator GetJson(string path, Action<string> onOk, Action<string> onErr)
    {
        string url = baseUrl + path;
        using (var req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
                onOk?.Invoke(req.downloadHandler.text);
            else
                onErr?.Invoke(req.error);
        }
    }

    // ---------------------------------------------------------------- response DTO
    // I campi usano gli stessi nomi (snake_case) del JSON del backend, così
    // JsonUtility li mappa senza adattatori.

    [Serializable] public class Profile { public string id; public string name; public int xp; }

    [Serializable] public class Exercise { public string id; public string prompt; public string expression; public string answer; }

    [Serializable] public class Task
    {
        public string task_id;
        public string subject;
        public string topic;
        public string raw_ocr;
        public Exercise[] exercises;
    }

    [Serializable] public class Objective { public string id; public string type; }

    [Serializable] public class Step { public string id; public string mechanic; public string objective_ref; }

    [Serializable] public class Mission
    {
        public string mission_id;
        public string environment_id;
        public int reward_xp;
        public Objective[] objectives;
        public Step[] steps;
    }

    [Serializable] public class Verify { public bool understood; public string feedback; }

    [Serializable] public class Complete { public int xp_awarded; }

    [Serializable] public class DialogueLine { public string character_id; public string line; }

    // ---------------------------------------------------------------- chiamate tipizzate

    public IEnumerator CreateProfile(string name, Action<string> onOk, Action<string> onErr)
    {
        string body = "{\"name\":\"" + name + "\",\"avatar_id\":\"avatar_default\"}";
        yield return PostJson(API + "/profile", body, onOk, onErr);
    }

    public IEnumerator AcquireTask(string expression, Action<string> onOk, Action<string> onErr)
    {
        string body =
            "{\"subject\":\"matematica\",\"topic\":\"equazioni lineari\",\"raw_ocr\":\"" + expression +
            "\",\"exercises\":[{\"id\":\"e1\",\"prompt\":\"Risolvi\",\"expression\":\"" + expression + "\"}],\"difficulty\":2}";
        yield return PostJson(API + "/tasks/acquire", body, onOk, onErr);
    }

    public IEnumerator PlanMission(string profileId, string taskId, Action<string> onOk, Action<string> onErr)
    {
        string body = "{\"profile_id\":\"" + profileId + "\",\"task_id\":\"" + taskId + "\"}";
        yield return PostJson(API + "/missions/plan", body, onOk, onErr);
    }

    public IEnumerator VerifyAnswer(string taskId, string answer, Action<string> onOk, Action<string> onErr)
    {
        string body = "{\"task_id\":\"" + taskId + "\",\"explanation\":\"Risposta\",\"answers\":[\"" + answer + "\"]}";
        yield return PostJson(API + "/verify/understanding", body, onOk, onErr);
    }

    public IEnumerator CompleteMission(string missionId, string[] objectiveIds, Action<string> onOk, Action<string> onErr)
    {
        string objs = string.Join(",", Array.ConvertAll(objectiveIds, o => "\"" + o + "\""));
        string body = "{\"success\":true,\"objectives_done\":[" + objs + "]}";
        yield return PostJson(API + "/missions/" + missionId + "/complete", body, onOk, onErr);
    }

    public IEnumerator Dialogue(string characterId, string context, string profileId, Action<string> onOk, Action<string> onErr)
    {
        string body = "{\"character_id\":\"" + characterId + "\",\"context\":\"" + context +
                      "\",\"profile_id\":\"" + profileId + "\"}";
        yield return PostJson(API + "/narrative/dialogue", body, onOk, onErr);
    }
}
