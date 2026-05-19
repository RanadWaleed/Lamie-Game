using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using RTLTMPro;

public class ProfileManager : MonoBehaviour
{
    [Header("UI Elements")]
    public RTLTextMeshPro nameText;
    public RTLTextMeshPro ageText;

    private string getProfileURL = "http://localhost:5194/api/Unity/GetChildProfile/";

    void Start()
    {
        string childId = PlayerPrefs.GetString("CurrentChildID", "");
        if (!string.IsNullOrEmpty(childId))
        {
            StartCoroutine(FetchProfileData(childId));
        }

        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.SyncOfflineData();
        }
    }

    IEnumerator FetchProfileData(string id)
    {
        UnityWebRequest req = UnityWebRequest.Get(getProfileURL + id);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = System.Text.Encoding.UTF8.GetString(req.downloadHandler.data);
            ChildProfile profile = JsonUtility.FromJson<ChildProfile>(json);

            if (profile != null)
            {
                if (nameText != null) nameText.text = profile.fullName;
                if (ageText != null) ageText.text = profile.age + " سنوات";

                PlayerPrefs.SetString("SavedChildName", profile.fullName);
                PlayerPrefs.SetInt("SavedChildAge", profile.age);
                PlayerPrefs.Save();
            }
        }
        else
        {
            string savedName = PlayerPrefs.GetString("SavedChildName", "بطل لامع");
            int savedAge = PlayerPrefs.GetInt("SavedChildAge", 6);

            if (nameText != null) nameText.text = savedName;
            if (ageText != null) ageText.text = savedAge + " سنوات";
        }
    }
}

[System.Serializable]
public class ChildProfile
{
    public string fullName;
    public int age;
    public string gender;
}