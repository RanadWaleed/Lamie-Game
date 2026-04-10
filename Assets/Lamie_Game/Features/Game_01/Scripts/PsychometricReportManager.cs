using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;

[System.Serializable]
public class ItemReport
{
    public int itemIndex;
    public float accuracyScore;
    public float speedScore;
    public float lowErrorScore;
    public float finalScore;
    public string assessmentRating;
    public int psychometricPoints;
}

[System.Serializable]
public class IndicatorReport
{
    public string indicatorName;
    public List<ItemReport> items = new List<ItemReport>();
    public int totalIndicatorPoints;
}

[System.Serializable]
public class PsychometricReportData
{
    public string childID;
    public string dimensionName;
    public float totalDimensionScore;
    public List<IndicatorReport> indicators = new List<IndicatorReport>();
}

public class PsychometricReportManager : MonoBehaviour
{
    public static PsychometricReportManager Instance;
    public PsychometricReportData finalReport = new PsychometricReportData();

    private IndicatorReport currentIndicator;
    private string apiURL = "http://localhost:5194/api/Unity/SavePsychometricReport";

    private float stage1_C, stage1_N, stage1_A, stage1_Time, stage1_StdTime;
    private bool hasStage1Data = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetupNewDimension(string newDimensionName)
    {
        finalReport = new PsychometricReportData();
        finalReport.childID = PlayerPrefs.GetString("CurrentChildID", "No_ID_Found");
        finalReport.dimensionName = newDimensionName;
    }

    public void StartNewIndicator(string name)
    {
        currentIndicator = new IndicatorReport();
        currentIndicator.indicatorName = name;
        hasStage1Data = false;
    }

    public void SaveItemData(int index, float c, float n, float a, float actualTime, float standardTime)
    {
        if (index == 1)
        {
            stage1_C = c;
            stage1_N = n;
            stage1_A = a;
            stage1_Time = actualTime;
            stage1_StdTime = standardTime;
            hasStage1Data = true;
        }

        Debug.Log($"\n<color=yellow>=== [Stage {index}] RAW DATA RECEIVED ===</color>");
        Debug.Log($"Correct From First Try (C): {c} | Required (N): {n} | Total Attempts (A): {a} | Time: {actualTime}s | Std Time: {standardTime}s");

        ItemReport item = new ItemReport();
        item.itemIndex = index;

        item.accuracyScore = (n > 0) ? Mathf.Clamp01(c / n) : 0;
        item.lowErrorScore = (a > 0) ? Mathf.Clamp01(c / a) : 0;

        float speed = (actualTime > 0) ? (standardTime / actualTime) : 0;
        item.speedScore = Mathf.Clamp01(speed);

        item.finalScore = (item.accuracyScore * 0.6f) + (item.speedScore * 0.2f) + (item.lowErrorScore * 0.2f);

        if (item.finalScore >= 0.80f)
        {
            item.assessmentRating = "Always";
            item.psychometricPoints = 3;
        }
        else if (item.finalScore >= 0.50f)
        {
            item.assessmentRating = "Sometimes";
            item.psychometricPoints = 2;
        }
        else
        {
            item.assessmentRating = "Rarely";
            item.psychometricPoints = 1;
        }

        Debug.Log($"<color=cyan>--- [Stage {index}] CALCULATED SCORES ---</color>");
        Debug.Log($"Accuracy (C/N): {item.accuracyScore} (Weight: 60%)");
        Debug.Log($"Low Error (C/A): {item.lowErrorScore} (Weight: 20%)");
        Debug.Log($"Speed (Std/Act): {item.speedScore} (Weight: 20%)");
        Debug.Log($"<color=green>FINAL SCORE: {item.finalScore} | Rating: {item.assessmentRating} | Points: {item.psychometricPoints}</color>\n");

        if (currentIndicator != null)
        {
            currentIndicator.items.Add(item);
            currentIndicator.totalIndicatorPoints += item.psychometricPoints;
        }
    }

    private void SaveStage6RandomnessData(float c, float a)
    {
        Debug.Log("\n<color=magenta>=== [AUTO-GENERATING STAGE 6 - RANDOMNESS ONLY: C/A] ===</color>");
        Debug.Log($"Correct From First Try (C): {c} | Total Attempts (A): {a}");

        ItemReport item = new ItemReport();
        item.itemIndex = 6;

        item.accuracyScore = 0f;
        item.speedScore = 0f;
        item.lowErrorScore = (a > 0) ? Mathf.Clamp01(c / a) : 0f;

        item.finalScore = item.lowErrorScore;

        if (item.finalScore >= 0.80f)
        {
            item.assessmentRating = "Always";
            item.psychometricPoints = 3;
        }
        else if (item.finalScore >= 0.50f)
        {
            item.assessmentRating = "Sometimes";
            item.psychometricPoints = 2;
        }
        else
        {
            item.assessmentRating = "Rarely";
            item.psychometricPoints = 1;
        }

        Debug.Log($"<color=cyan>--- [Stage 6] RANDOMNESS SCORE ---</color>");
        Debug.Log($"Low Error / Randomness (C/A): {item.lowErrorScore} (Weight: 100%)");
        Debug.Log($"<color=green>FINAL SCORE: {item.finalScore} | Rating: {item.assessmentRating} | Points: {item.psychometricPoints}</color>\n");

        if (currentIndicator != null)
        {
            currentIndicator.items.Add(item);
            currentIndicator.totalIndicatorPoints += item.psychometricPoints;
        }
    }

    public void FinishCurrentIndicator()
    {
        if (currentIndicator != null)
        {
            if (hasStage1Data)
            {
                SaveStage6RandomnessData(stage1_C, stage1_A);
                hasStage1Data = false;
            }

            finalReport.indicators.Add(currentIndicator);
            currentIndicator = null;
        }
    }

    public void UploadReportToDatabase()
    {
        float total = 0;
        int count = 0;

        foreach (var ind in finalReport.indicators)
        {
            foreach (var item in ind.items)
            {
                total += item.finalScore;
                count++;
            }
        }

        finalReport.totalDimensionScore = (count > 0) ? (total / count) : 0;

        string json = JsonUtility.ToJson(finalReport, true);
        Debug.Log("Payload Generated:\n" + json);

        StartCoroutine(PostRequest(apiURL, json));
    }

    private IEnumerator PostRequest(string url, string json)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Upload Error: " + request.error);
        }
        else
        {
            Debug.Log("Upload Success: " + request.downloadHandler.text);
        }
    }
}