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

        string savedCode = PlayerPrefs.GetString("SavedLoginCode", "");

        if (!string.IsNullOrEmpty(savedCode) && code == savedCode)
        {
            Debug.Log("The code was successfully verified locally");
            ProceedToNextState();
        }
        else
        {
            StartCoroutine(VerifyCodeFromAPI(code));
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
                PlayerPrefs.SetString("UserGender", response.Gender);
                PlayerPrefs.Save();

                ProceedToNextState();
            }
            else
            {
                Debug.LogWarning("The code does not exist in the database.");
            }
        }
        else
        {
            Debug.LogError("The connection to the server failed.");
        }
    }

    private void ProceedToNextState()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToState(GameFlowState.CharacterSelection);
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