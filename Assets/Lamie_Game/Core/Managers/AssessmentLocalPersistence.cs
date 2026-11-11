using UnityEngine;

public sealed class AssessmentLocalPersistence : ILocalAssessmentPersistence
{
    public string Read(string key) => PlayerPrefs.GetString(key, "");
    public void Write(string key, string value) => PlayerPrefs.SetString(key, value);
    public void Save() => PlayerPrefs.Save();
}

[System.Serializable]
public sealed class CompletedAssessmentRecord
{
    public string reportId;
    public string sceneName;
    public string assessmentVersion;
    public string completedAtUtc;
    public int attemptNumber;
    public bool hadPreviousInterruptedAttempt;
    public SaveGameResultRequest result;
}
