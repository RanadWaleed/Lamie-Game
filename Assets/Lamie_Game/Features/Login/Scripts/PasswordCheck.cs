using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

public class PasswordCheck : MonoBehaviour
{
    public TMP_InputField codeInput;

    private const string API_URL = "http://localhost:5194/api/Unity/CheckLoginCode";

    public void CheckCode()
    {
        StartCoroutine(Login());

    }

    private IEnumerator Login()
    {
        string json = "{\"Code\":\"" + codeInput.text + "\"}";

        UnityWebRequest req = new UnityWebRequest(API_URL, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);
            yield break;
        }

        var res = JsonUtility.FromJson<GameSession.LoginWithBadgesResponse>(
            req.downloadHandler.text);

        if (!res.success)
        {
            Debug.LogWarning(res.message);
            yield break;
        }
        Debug.Log($"عدد الباجات: {res.badges.Count}");
        foreach (var b in res.badges)
            Debug.Log($"id='{b.intelligenceId}' | name='{b.intelligenceName}' | unlocked={b.isUnlocked}");
        GameSession.Instance.Init(res.childId, res.gender, res.badges);

        GameFlowManager.Instance.GoToNextState();
    }
}