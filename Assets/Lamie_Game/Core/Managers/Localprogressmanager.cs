using UnityEngine;

/// <summary>
/// Tracks game completion and badge unlocks locally via PlayerPrefs.
/// Singleton — survives scene loads.
/// </summary>
public class LocalProgressManager : MonoBehaviour
{
    public static LocalProgressManager Instance { get; private set; }

    [Header("All Intelligences")]
    public IntelligenceData[] allIntelligences;

    private const string KeyPrefix = "lamie_";

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
            Debug.LogWarning("[LocalProgressManager] تم حفظ المرحلة، لكن هناك قائمة فارغة في الـ Inspector تخص الشارات: " + e.Message);
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
        foreach (var intel in allIntelligences)
        {
            PlayerPrefs.DeleteKey(KeyPrefix + "badge_" + intel.intelligenceId);
            foreach (var scene in intel.gameSceneNames)
                PlayerPrefs.DeleteKey(KeyPrefix + "game_" + scene);
        }
        PlayerPrefs.Save();
        Debug.Log("Lamie: All progress reset.");
    }
#endif
}