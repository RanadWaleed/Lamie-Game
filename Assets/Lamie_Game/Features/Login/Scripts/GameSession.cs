using UnityEngine;
using System.Collections.Generic;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public string ChildId { get; private set; }
    public string Gender { get; private set; }

    // الآن نحفظ الاسم مع الـ id
    private Dictionary<string, BadgeStatus> badgeCache = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Init(string childId, string gender, List<BadgeStatus> badges)
    {
        ChildId = childId;
        Gender = gender;

        badgeCache.Clear();
        foreach (var b in badges)
            badgeCache[b.intelligenceId] = b; // ✅ نحفظ كامل الـ BadgeStatus

        PlayerPrefs.SetString("CurrentChildID", childId);
        PlayerPrefs.SetString("UserGender", gender);
        PlayerPrefs.Save();
    }

    public bool IsUnlocked(string id)
    {
        return badgeCache.TryGetValue(id, out var b) && b.isUnlocked;
    }

    public void UnlockBadgeLocally(string intelligenceId)
    {
        if (badgeCache.ContainsKey(intelligenceId))
        {
            badgeCache[intelligenceId].isUnlocked = true;
            BadgeManager.Instance?.UnlockBadge(intelligenceId);
        }
    }

    public List<BadgeStatus> GetBadges()
    {
        var list = new List<BadgeStatus>();
        foreach (var kvp in badgeCache)
            list.Add(kvp.Value); // ✅ يرجع كامل البيانات مع الاسم
        return list;
    }

    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var b in badgeCache.Values)
            if (b.isUnlocked) count++;
        return count;
    }

    [System.Serializable]
    public class BadgeStatus
    {
        public string intelligenceId;
        public string intelligenceName;
        public bool isUnlocked;
    }

    [System.Serializable]
    public class LoginWithBadgesResponse
    {
        public bool success;
        public string message;
        public string childId;
        public string gender;
        public List<BadgeStatus> badges;
    }
}