using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class PasswordCheck : MonoBehaviour
{
    public TMP_InputField codeInput;

    public string apiURL = "http://localhost:5194/api/Unity/CheckLoginCode";

    public void CheckCode()
    {
        string code = codeInput.text;
        StartCoroutine(TryOnlineThenOffline(code));
    }

    IEnumerator TryOnlineThenOffline(string code)
    {
        UnityWebRequest ping = UnityWebRequest.Get("http://localhost:5194/");
        ping.timeout = 3;
        yield return ping.SendWebRequest();

        if (ping.result == UnityWebRequest.Result.Success)
        {
            StartCoroutine(VerifyCodeFromAPI(code));
        }
        else
        {
            string savedCode = PlayerPrefs.GetString("SavedLoginCode", "");
            if (!string.IsNullOrEmpty(savedCode) && code == savedCode)
            {
                Debug.Log("تم التحقق محلياً بنجاح");
                ProceedToNextState();
            }
            else
            {
                Debug.LogWarning("الكود غلط ولا يوجد اتصال بالسيرفر");
            }
        }
    }

    IEnumerator VerifyCodeFromAPI(string code)
    {
        string json = "{\"Code\":\"" + code + "\"}";

        UnityWebRequest req = new UnityWebRequest(apiURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);

            if (response.Success)
            {
                PlayerPrefs.SetString("SavedLoginCode", code);
                PlayerPrefs.SetString("CurrentChildID", response.ChildId);
                PlayerPrefs.SetString("UserGender", response.Gender.Trim());
                PlayerPrefs.Save();

                ProceedToNextState();
            }
            else
            {
                Debug.LogWarning(response.Message);
            }
        }
        else
        {
            Debug.LogError("فشل الاتصال بالسيرفر.");
        }
    }

    private void ProceedToNextState()
    {
        if (GameFlowManager.Instance != null)
        {
            string childId = PlayerPrefs.GetString("CurrentChildID", "default");

            if (PlayerPrefs.GetInt("HasSeenStory_" + childId, 0) == 0)
            {
                GameFlowManager.Instance.GoToState(GameFlowState.Story);
            }
            else if (PlayerPrefs.GetInt("CharacterSelected", 0) == 0)
            {
                GameFlowManager.Instance.GoToState(GameFlowState.CharacterSelection);
            }
            else
            {
                GameFlowManager.Instance.GoToState(GameFlowState.Home);
            }
        }
    }
}
[System.Serializable]
public class LoginResponse
{
    public bool Success;
    public string ChildId;
    public string Gender;
    public string Message;
}