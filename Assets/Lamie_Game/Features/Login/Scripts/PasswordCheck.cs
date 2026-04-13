using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class PasswordCheck : MonoBehaviour
{
    public TMP_InputField codeInput;
    public string apiURL = "http://192.168.56.1:5194/api/Unity/CheckLoginCode";

    public void CheckCode()
    {
        StartCoroutine(VerifyCodeFromAPI());
    }

    IEnumerator VerifyCodeFromAPI()
    {
        string code = codeInput.text;
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
                PlayerPrefs.SetString("CurrentChildID", response.ChildId);
                PlayerPrefs.SetString("UserGender", response.Gender);
                PlayerPrefs.Save();

                if (GameFlowManager.Instance != null)
                {
                    GameFlowManager.Instance.GoToState(GameFlowState.CharacterSelection);
                }
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