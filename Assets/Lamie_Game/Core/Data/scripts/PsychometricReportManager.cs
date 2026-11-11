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
    private const string AssessmentVersion = "lamie-fixed-v1";
    private readonly LocalAssessmentStore store = new LocalAssessmentStore(new AssessmentLocalPersistence());
    private readonly HashSet<string> activeUploads = new HashSet<string>();
    private string apiURL = "http://localhost:5194/api/Unity/SaveGameResult";
    private string levelsURL = "http://localhost:5194/api/Unity/GetGameLevels";
    // Ordered by the confirmed platform aspects, not the old folder numbering.
    public string level1ID = ""; // التصور
    public string level2ID = ""; // الإخراج الفني
    public string level3ID = ""; // التمثيل
    public string level4ID = "";
    private string currentChildId;
    private string currentScene;
    private float sessionStartTime;
    private AspectResultDto currentAspect;
    public IndicatorResultDto currentIndicator;
    private float stage1_C, stage1_A;
    private bool hasStage1Data;
    private int attemptNumber;
    public bool IsPractice { get; private set; }
    public static readonly string[] AssessmentScenes = { "Game01Scene", "Game02Scene", "Game_03" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InstallSceneBootstrap()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= EnsureManager;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += EnsureManager;
    }

    private static void EnsureManager(UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (Instance == null && System.Array.IndexOf(AssessmentScenes, scene.name) >= 0)
            new GameObject("PsychometricReportManager").AddComponent<PsychometricReportManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void Start() => StartCoroutine(FetchLevelIDs());
    private void OnDestroy()
    {
        if (Instance != this) return;
        UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
        Instance = null;
    }
    private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
    {
        if (scene.name == currentScene) DiscardIncompleteSession();
    }

    [System.Serializable] public class LevelData { public string levelId; public string levelName; }
    [System.Serializable] public class LevelListWrapper { public List<LevelData> levels; }
    [System.Serializable] private class SaveResponse { public bool success; public SaveResponseData data; }
    [System.Serializable] private class SaveResponseData { public string sessionId; }

    private IEnumerator FetchLevelIDs()
    {
        if (LocalTestAccess.IsTestChild(ChildLocalProfile.CurrentChildId)) yield break;
        using (UnityWebRequest request = UnityWebRequest.Get(levelsURL))
        {
            request.timeout = 10;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Platform unavailable; assessment remains fully local.");
                yield break;
            }
            LevelListWrapper wrapper = null;
            try { wrapper = JsonUtility.FromJson<LevelListWrapper>("{\"levels\":" + request.downloadHandler.text + "}"); }
            catch (System.Exception e) { Debug.LogWarning("Invalid level list: " + e.Message); }
            if (wrapper?.levels == null) yield break;
            foreach (LevelData level in wrapper.levels)
            {
                if (level == null || !System.Guid.TryParse(level.levelId, out _)) continue;
                switch (level.levelName?.Trim())
                {
                    case "لعبة التصور":
                    case "لعبة التصور البصري": level1ID = level.levelId; break;
                    case "لعبة الإخراج الفني": level2ID = level.levelId; break;
                    case "لعبة التمثيل": level3ID = level.levelId; break;
                    case "لعبة الجوانب الأخرى": level4ID = level.levelId; break;
                }
            }
        }
        SyncOfflineData();
    }

    private string ResolveLevel(string aspect) => aspect switch
    {
        "التصور" => level1ID,
        "الإخراج الفني" => level2ID,
        "التمثيل" => level3ID,
        _ => ""
    };

    public void SetupNewAspect(string aspectName, string gameNumber)
    {
        DiscardIncompleteSession();
        currentChildId = ChildLocalProfile.CurrentChildId;
        currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        string expectedAspect = currentScene switch
        {
            "Game01Scene" => "التصور", "Game02Scene" => "الإخراج الفني", "Game_03" => "التمثيل", _ => ""
        };
        if (string.IsNullOrWhiteSpace(currentChildId) || expectedAspect != aspectName)
        {
            Debug.LogError("Assessment requires a logged-in child and the correct game aspect.");
            DiscardIncompleteSession();
            return;
        }
        // gameNumber is retained for compatibility; report identity follows the aspect name.
        IsPractice = store.HasBaseline(currentChildId, currentScene);
        attemptNumber = IsPractice ? 0 : store.BeginAttempt(currentChildId, currentScene);
        sessionStartTime = Time.time;
        currentAspect = new AspectResultDto { aspectName = aspectName };
        Debug.Log(IsPractice ? "Lamie: practice; original assessment is protected." :
            "Lamie: new full assessment attempt " + attemptNumber);
    }

    public void StartNewIndicator(string indicatorName)
    {
        if (currentAspect == null) return;
        currentIndicator = new IndicatorResultDto { indicatorName = indicatorName };
        hasStage1Data = false;
    }

    public void SaveItemData(int index, float c, float n, float a, float actualTime, float standardTime, string itemName = "")
    {
        if (currentAspect == null || currentIndicator == null) return;
        // Item six is derived once from item one, never another weighted gameplay item.
        if (index == 6 && hasStage1Data) return;
        if (index == 1) { stage1_C = c; stage1_A = a; hasStage1Data = true; }
        if (currentIndicator.items.Exists(item => item.itemIndex == index))
        {
            Debug.LogWarning("Duplicate assessment item ignored: " + index);
            return;
        }
        AssessmentItemDto value = new AssessmentItemDto
        {
            itemIndex = index,
            itemName = string.IsNullOrEmpty(itemName) ? "بند " + index : itemName,
            finalScore = (n > 0 ? Mathf.Clamp01(c / n) : 0f) * 0.6f +
                (actualTime > 0 ? Mathf.Clamp01(standardTime / actualTime) : 0f) * 0.2f +
                (a > 0 ? Mathf.Clamp01(c / a) : 0f) * 0.2f
        };
        AssignRating(value);
        currentIndicator.items.Add(value);
    }

    public void FinishCurrentIndicator()
    {
        if (currentAspect == null || currentIndicator == null) return;
        if (hasStage1Data && !currentIndicator.items.Exists(item => item.itemIndex == 6))
        {
            string name = "بند 6";
            string[] names;
            if ((currentScene == "Game01Scene" && MasterManager.ItemNames.TryGetValue(currentIndicator.indicatorName, out names)) ||
                (currentScene == "Game_03" && Game3_MasterManager.ItemNames.TryGetValue(currentIndicator.indicatorName, out names)))
                name = names.Length >= 6 ? names[5] : name;
            AssessmentItemDto derived = new AssessmentItemDto
            {
                itemIndex = 6, itemName = name,
                finalScore = stage1_A > 0 ? Mathf.Clamp01(stage1_C / stage1_A) : 0f
            };
            AssignRating(derived);
            currentIndicator.items.Add(derived);
        }
        hasStage1Data = false;
        float total = 0f;
        foreach (AssessmentItemDto item in currentIndicator.items) total += item.finalScore;
        currentIndicator.indicatorScore = currentIndicator.items.Count > 0 ? total / currentIndicator.items.Count : 0f;
        currentIndicator.indicatorRating = Rating(currentIndicator.indicatorScore);
        currentIndicator.psychometricPts = Points(currentIndicator.indicatorScore);
        currentAspect.indicators.Add(currentIndicator);
        currentIndicator = null;
    }

    public void UploadCurrentGameResult()
    {
        if (currentAspect == null) return;
        if (currentChildId != ChildLocalProfile.CurrentChildId ||
            currentScene != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {
            DiscardIncompleteSession();
            return;
        }
        if (IsPractice) { DiscardIncompleteSession(); return; }
        int required = currentScene == "Game02Scene" ? 8 : 4;
        HashSet<string> indicatorNames = new HashSet<string>();
        bool valid = currentIndicator == null && currentAspect.indicators.Count == required;
        foreach (IndicatorResultDto indicator in currentAspect.indicators)
        {
            valid &= indicatorNames.Add(indicator.indicatorName) && indicator.items.Count > 0;
            HashSet<int> indexes = new HashSet<int>();
            foreach (AssessmentItemDto item in indicator.items) valid &= indexes.Add(item.itemIndex);
        }
        if (!valid)
        {
            Debug.LogError("Incomplete assessment was not finalized. Restart this full game to assess again.");
            return;
        }
        float total = 0;
        foreach (IndicatorResultDto indicator in currentAspect.indicators) total += indicator.indicatorScore;
        currentAspect.aspectScore = total / required;
        currentAspect.aspectRating = Rating(currentAspect.aspectScore);
        currentAspect.psychometricPts = Points(currentAspect.aspectScore);
        CompletedAssessmentRecord record = new CompletedAssessmentRecord
        {
            reportId = System.Guid.NewGuid().ToString("N"), sceneName = currentScene,
            assessmentVersion = currentScene == "Game02Scene" ? "lamie-art-bugfix-v2" : AssessmentVersion,
            completedAtUtc = System.DateTime.UtcNow.ToString("o"),
            attemptNumber = attemptNumber, hadPreviousInterruptedAttempt = attemptNumber > 1,
            result = new SaveGameResultRequest
            {
                childId = currentChildId, levelId = ResolveLevel(currentAspect.aspectName),
                totalTime = Time.time - sessionStartTime, aspectResult = currentAspect
            }
        };
        string child = currentChildId, scene = currentScene;
        bool saved = store.TrySaveFirstCompletion(child, scene, JsonUtility.ToJson(record));
        if (saved)
        {
            PlayerPrefs.SetInt("lamie_" + child + "_game_" + scene, 1);
            PlayerPrefs.Save();
            LocalProgressManager.Instance?.MarkGameComplete(scene);
            Debug.Log("Lamie: first complete assessment saved locally; replay is practice.");
        }
        DiscardIncompleteSession();
        if (saved) QueueUpload(child, scene);
    }

    private static int Points(float score) => score >= 0.8f ? 3 : score >= 0.5f ? 2 : 1;
    private static string Rating(float score) => score >= 0.8f ? "غالبًا" : score >= 0.5f ? "أحيانًا" : "نادرًا";
    private static void AssignRating(AssessmentItemDto item)
    {
        item.rating = Rating(item.finalScore);
        item.psychometricPts = Points(item.finalScore);
    }

    public void SyncOfflineData()
    {
        string child = ChildLocalProfile.CurrentChildId;
        if (string.IsNullOrWhiteSpace(child)) return;
        foreach (string scene in AssessmentScenes) QueueUpload(child, scene);
    }

    private void QueueUpload(string child, string scene)
    {
        if (LocalTestAccess.IsTestChild(child)) return;
        string key = child + ":" + scene;
        if (!store.HasBaseline(child, scene) || store.ReadDelivery(child, scene) != "pending" || activeUploads.Contains(key)) return;
        CompletedAssessmentRecord record = null;
        try { record = JsonUtility.FromJson<CompletedAssessmentRecord>(store.ReadBaseline(child, scene)); }
        catch (System.Exception e) { Debug.LogError("Local report could not be read: " + e.Message); }
        if (record?.result?.aspectResult == null) return;
        // Resolve from the current platform list; never invent an ID or overwrite the baseline.
        string levelId = ResolveLevel(record.result.aspectResult.aspectName);
        if (!System.Guid.TryParse(levelId, out _)) return;
        record.result.levelId = levelId;
        activeUploads.Add(key);
        StartCoroutine(PostSavedResult(child, scene, key, JsonUtility.ToJson(record.result)));
    }

    private IEnumerator PostSavedResult(string child, string scene, string key, string json)
    {
        if (!store.TryBeginDelivery(child, scene)) { activeUploads.Remove(key); yield break; }
        using (UnityWebRequest request = new UnityWebRequest(apiURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;
            yield return request.SendWebRequest();
            SaveResponse response = null;
            if (request.result == UnityWebRequest.Result.Success)
                try { response = JsonUtility.FromJson<SaveResponse>(request.downloadHandler.text); }
                catch (System.Exception) { /* Ambiguous response: keep local result and do not duplicate it. */ }
            if (response != null && response.success && !string.IsNullOrWhiteSpace(response.data?.sessionId))
                store.ConfirmDelivery(child, scene);
            else if (response != null && !response.success)
                store.RejectDelivery(child, scene);
            else
                Debug.LogWarning("Report saved locally; delivery unconfirmed. Automatic resend paused to avoid duplicate sessions.");
        }
        activeUploads.Remove(key);
    }

    private void DiscardIncompleteSession()
    {
        currentAspect = null;
        currentIndicator = null;
        currentScene = null;
        currentChildId = null;
        hasStage1Data = false;
        IsPractice = false;
    }

    // Closing is interruption, never completion. No partial assessment is saved or uploaded.
    private void OnApplicationQuit() => DiscardIncompleteSession();
}
