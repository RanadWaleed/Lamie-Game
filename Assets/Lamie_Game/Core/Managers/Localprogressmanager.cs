using UnityEngine;

public class LocalProgressManager : MonoBehaviour
{
    public static LocalProgressManager Instance { get; private set; }

    [Header("All Intelligences")]
    public IntelligenceData[] allIntelligences;

    private string KeyPrefix => "lamie_" + PlayerPrefs.GetString("CurrentChildID", "default") + "_";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool MarkGameComplete(string sceneNameCompleted)
    {
        PlayerPrefs.SetInt(KeyPrefix + "game_" + sceneNameCompleted, 1);
        PlayerPrefs.Save();

        try
        {
            IntelligenceData intel = GetIntelligenceForScene(sceneNameCompleted);
            if (intel != null)
            {
                if (!IsBadgeUnlocked(intel.intelligenceId) && AreAllGamesComplete(intel))
                {
                    UnlockBadge(intel.intelligenceId);
                    return true;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }

        return false;
    }

    public bool IsGameComplete(string sceneName)
    {
        return PlayerPrefs.GetInt(KeyPrefix + "game_" + sceneName, 0) == 1;
    }

    public bool IsBadgeUnlocked(string intelligenceId)
    {
        return PlayerPrefs.GetInt(KeyPrefix + "badge_" + intelligenceId, 0) == 1;
    }

    public bool AreAllGamesComplete(IntelligenceData intel)
    {
        foreach (string scene in intel.gameSceneNames)
        {
            if (!IsGameComplete(scene)) return false;
        }
        return true;
    }

    public IntelligenceData GetIntelligenceForScene(string sceneName)
    {
        foreach (var intel in allIntelligences)
        {
            foreach (var scene in intel.gameSceneNames)
            {
                if (scene == sceneName) return intel;
            }
        }
        return null;
    }

    private void UnlockBadge(string intelligenceId)
    {
        PlayerPrefs.SetInt(KeyPrefix + "badge_" + intelligenceId, 1);
        PlayerPrefs.Save();
    }

#if UNITY_EDITOR
    [ContextMenu("Reset All Progress")]
    public void ResetAllProgress()
    {
        string childId = PlayerPrefs.GetString("CurrentChildID", "default");
        string prefix = "lamie_" + childId + "_";

        PlayerPrefs.DeleteKey(prefix + "game_Game01Scene");
        PlayerPrefs.DeleteKey(prefix + "game_Game02Scene");
        PlayerPrefs.DeleteKey(prefix + "game_Game_03");
        PlayerPrefs.DeleteKey(prefix + "badge_A78BE3B0-39D1-F011-8780-3003C8C8982E");
        PlayerPrefs.Save();

        Debug.Log("Lamie: All progress reset for child: " + childId);
    }
#endif
}