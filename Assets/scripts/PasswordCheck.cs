using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class PasswordCheck : MonoBehaviour
{

    public TMP_InputField codeInput;
    public string nextSceneName = "ChooseCharacterScene";

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

        Debug.Log("Sending Code: " + code);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Raw Response: " + req.downloadHandler.text);

            LoginResponse response = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);

            if (response.Success)
            {
                Debug.Log("Login success! Child ID: " + response.ChildId);
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("Login Failed: " + response.Message);
            }
        }
        else
        {
            Debug.LogError("Server Error: " + req.error);
            Debug.LogError("Response Code: " + req.responseCode);
        }
    }
}

[System.Serializable]
public class LoginResponse
{
    public bool Success;
    public string ChildId;
    public string Message;
}