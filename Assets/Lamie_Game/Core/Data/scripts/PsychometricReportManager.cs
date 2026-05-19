using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;


[System.Serializable]
public class AssessmentItemDto
{
    public int itemIndex;
    public string itemName;
    public float finalScore;
    public string rating;
    public int psychometricPts;
}

[System.Serializable]
public class IndicatorResultDto
{
    public string indicatorName;
    public float indicatorScore;
    public string indicatorRating;
    public int psychometricPts;
    public List<AssessmentItemDto> items = new List<AssessmentItemDto>();
}

[System.Serializable]
public class AspectResultDto
{
    public string aspectName;
    public float aspectScore;
    public string aspectRating;
    public int psychometricPts;
    public List<IndicatorResultDto> indicators = new List<IndicatorResultDto>();
}

[System.Serializable]
public class SaveGameResultRequest
{
    public string childId;
    public string levelId;
    public float totalTime;
    public AspectResultDto aspectResult;
}



public class PsychometricReportManager : MonoBehaviour
{
    public static PsychometricReportManager Instance;

    private string apiURL = "http://localhost:5194/api/Unity/SaveGameResult";
    private string levelsURL = "http://localhost:5194/api/Unity/GetGameLevels";

    public string level1ID = "";
    public string level2ID = "";
    public string level3ID = "";
    public string level4ID = "";
    private string currentChildId = "";
    private string currentLevelId = "";
    private float sessionStartTime = 0f;
    private AspectResultDto currentAspect = null;
    public IndicatorResultDto currentIndicator = null;
    private float stage1_C, stage1_A;
    private bool hasStage1Data = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(FetchLevelIDs());
    }

    [System.Serializable] public class LevelData { public string levelId; public string levelName; }
    [System.Serializable] public class LevelListWrapper { public List<LevelData> levels; }

    private IEnumerator FetchLevelIDs()
    {
        UnityWebRequest request = UnityWebRequest.Get(levelsURL);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string rawJson = request.downloadHandler.text;
            Debug.Log("[Levels] Raw Data: " + rawJson);

            var wrapper = JsonUtility.FromJson<LevelListWrapper>("{\"levels\":" + rawJson + "}");

            foreach (var level in wrapper.levels)
            {
                if (level.levelName.Trim() == "لعبة التصور") level1ID = level.levelId;
                else if (level.levelName.Trim() == "لعبة التمثيل") level2ID = level.levelId;
                else if (level.levelName.Trim() == "لعبة الإخراج الفني") level3ID = level.levelId;
                else if (level.levelName.Trim() == "لعبة الجوانب الأخرى") level4ID = level.levelId;
            }
            Debug.Log($"[Levels] Done! Game1 ID: {level1ID}");
        }
        else
        {
            Debug.LogWarning("[Levels] Server not available, running offline: " + request.error);
        }
    }

    public void SetupNewAspect(string aspectName, string gameNumber)
    {
        currentChildId = PlayerPrefs.GetString("CurrentChildID", "No_ID_Found");
        sessionStartTime = Time.time;

        currentLevelId = gameNumber switch
        {
            "Game_1" => level1ID,
            "Game_2" => level2ID,
            "Game_3" => level3ID,
            _ => level1ID
        };

        currentAspect = new AspectResultDto { aspectName = aspectName };

        Debug.Log($"[Aspect] بدأ جانب جديد: {aspectName} | LevelID: {currentLevelId}");
    }

    public void StartNewIndicator(string indicatorName)
    {
        currentIndicator = new IndicatorResultDto { indicatorName = indicatorName };
        hasStage1Data = false;

        Debug.Log($"[Indicator] بدأ مؤشر: {indicatorName}");
    }

    public void SaveItemData(int index, float c, float n, float a, float actualTime, float standardTime, string itemName = "")
    {
        if (index == 1)
        {
            stage1_C = c; stage1_A = a; hasStage1Data = true;
        }

        AssessmentItemDto item = BuildItem(index, c, n, a, actualTime, standardTime, itemName);
        currentIndicator?.items.Add(item);
        Debug.Log($"[Item {index}] Final={item.finalScore:F2} → {item.rating}");
    }

    public void FinishCurrentIndicator()
    {
        if (currentIndicator == null) return;

        if (hasStage1Data)
        {
            string item6Name = "بند 6";
            if (MasterManager.Instance != null &&
                MasterManager.ItemNames.TryGetValue(currentIndicator.indicatorName, out var names) &&
                names.Length >= 6)
            {
                item6Name = names[5];
            }

            AssessmentItemDto item6 = new AssessmentItemDto
            {
                itemIndex = 6,
                itemName = item6Name,
                finalScore = stage1_A > 0 ? Mathf.Clamp01(stage1_C / stage1_A) : 0f
            };
            AssignRating(item6);
            currentIndicator.items.Add(item6);
            hasStage1Data = false;

            Debug.Log($"[Item 6 - Randomness] Final={item6.finalScore:F2} → {item6.rating}");
        }

        float total = 0f;
        foreach (var item in currentIndicator.items) total += item.finalScore;
        currentIndicator.indicatorScore = currentIndicator.items.Count > 0
            ? total / currentIndicator.items.Count : 0f;

        AssignIndicatorRating(currentIndicator);
        currentAspect?.indicators.Add(currentIndicator);
        currentIndicator = null;

        Debug.Log($"[Indicator Done] Score={currentAspect?.indicators[^1].indicatorScore:F2}");
    }

    public void UploadCurrentGameResult()
    {
        if (currentAspect == null)
        {
            Debug.LogError("[Upload] لا يوجد aspect للرفع!");
            return;
        }

        float total = 0f;
        foreach (var ind in currentAspect.indicators) total += ind.indicatorScore;
        currentAspect.aspectScore = currentAspect.indicators.Count > 0
            ? total / currentAspect.indicators.Count : 0f;

        AssignAspectRating(currentAspect);

        float totalTime = Time.time - sessionStartTime;

        var payload = new SaveGameResultRequest
        {
            childId = currentChildId,
            levelId = currentLevelId,
            totalTime = totalTime,
            aspectResult = currentAspect
        };

        string json = JsonUtility.ToJson(payload, true);
        Debug.Log("[Upload] Payload:\n" + json);

        PlayerPrefs.SetString("LastBackup_" + currentChildId + "_" + currentLevelId, json);
        PlayerPrefs.Save();

        StartCoroutine(PostWithRetry(apiURL, json, retryCount: 3));
        currentAspect = null;
    }

    private AssessmentItemDto BuildItem(int index, float c, float n, float a, float actualTime, float standardTime, string itemName = "")
    {
        float accuracy = n > 0 ? Mathf.Clamp01(c / n) : 0f;
        float speedScore = actualTime > 0 ? Mathf.Clamp01(standardTime / actualTime) : 0f;
        float errorRate = a > 0 ? Mathf.Clamp01(c / a) : 0f;
        float finalScore = (accuracy * 0.6f) + (speedScore * 0.2f) + (errorRate * 0.2f);

        var item = new AssessmentItemDto
        {
            itemIndex = index,
            itemName = string.IsNullOrEmpty(itemName) ? "بند " + index : itemName,
            finalScore = finalScore
        };
        AssignRating(item);
        return item;
    }

    private void AssignRating(AssessmentItemDto item)
    {
        if (item.finalScore >= 0.80f) { item.rating = "غالبًا"; item.psychometricPts = 3; }
        else if (item.finalScore >= 0.50f) { item.rating = "أحيانًا"; item.psychometricPts = 2; }
        else { item.rating = "نادرًا"; item.psychometricPts = 1; }
    }

    private void AssignIndicatorRating(IndicatorResultDto ind)
    {
        if (ind.indicatorScore >= 0.80f) { ind.indicatorRating = "غالبًا"; ind.psychometricPts = 3; }
        else if (ind.indicatorScore >= 0.50f) { ind.indicatorRating = "أحيانًا"; ind.psychometricPts = 2; }
        else { ind.indicatorRating = "نادرًا"; ind.psychometricPts = 1; }
    }

    private void AssignAspectRating(AspectResultDto asp)
    {
        if (asp.aspectScore >= 0.80f) { asp.aspectRating = "غالبًا"; asp.psychometricPts = 3; }
        else if (asp.aspectScore >= 0.50f) { asp.aspectRating = "أحيانًا"; asp.psychometricPts = 2; }
        else { asp.aspectRating = "نادرًا"; asp.psychometricPts = 1; }
    }

    private IEnumerator PostWithRetry(string url, string json, int retryCount)
    {
        int attempt = 0;
        bool success = false;

        while (attempt < retryCount && !success)
        {
            attempt++;
            Debug.Log($"[Upload] محاولة {attempt} من {retryCount}...");

            var request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();
            Debug.Log("Response Code: " + request.responseCode);
            Debug.Log("Response: " + request.downloadHandler.text);
            Debug.Log("Error: " + request.error);

            if (request.result == UnityWebRequest.Result.Success)
            {
                success = true;
                PlayerPrefs.DeleteKey("LastBackup_" + currentChildId + "_" + currentLevelId);
                PlayerPrefs.Save();
                Debug.Log("<color=green>[Upload] نجح: " + request.downloadHandler.text + "</color>");
            }
            else
            {
                Debug.LogWarning($"[Upload] فشل المحاولة {attempt}: {request.error}");
                if (attempt < retryCount) yield return new WaitForSeconds(2f);
            }
        }

        if (!success)
            Debug.LogError($"[Upload] فشلت كل المحاولات. البيانات محفوظة محلياً.");
    }

    public void SyncOfflineData()
    {
        string childId = PlayerPrefs.GetString("CurrentChildID", "");
        if (string.IsNullOrEmpty(childId)) return;

        string[] levels = { level1ID, level2ID, level3ID, level4ID };
        foreach (var lvl in levels)
        {
            string key = "LastBackup_" + childId + "_" + lvl;
            if (PlayerPrefs.HasKey(key))
            {
                string jsonPayload = PlayerPrefs.GetString(key);
                Debug.Log($"[Sync] Attempting to upload previous data for level {lvl}");
                StartCoroutine(PostOfflineBackup(apiURL, jsonPayload, key));
            }
        }
    }

    private IEnumerator PostOfflineBackup(string url, string json, string prefsKey)
    {
        var request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            PlayerPrefs.DeleteKey(prefsKey);
            PlayerPrefs.Save();
            Debug.Log("<color=green>[Sync] Delayed data uploaded successfully.</color>");
        }
    }

    void OnApplicationQuit()
    {
        if (currentAspect != null)
        {
            FinishCurrentIndicator();
            UploadCurrentGameResult();
        }
    }
}