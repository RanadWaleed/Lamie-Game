using System;
using UnityEngine;

/// <summary>Local preferences belong to a child, not the device's last user.</summary>
public static class ChildLocalProfile
{
    public static string CurrentChildId => PlayerPrefs.GetString("CurrentChildID", "");
    public static bool HasIdentity => !string.IsNullOrWhiteSpace(CurrentChildId) &&
        LocalTestAccess.CanUseChild(CurrentChildId);
    public static string Key(string childId, string field) =>
        "lamie_profile_v1_" + Uri.EscapeDataString(childId) + "_" + field;
    public static string GetString(string field, string fallback = "") => HasIdentity
        ? PlayerPrefs.GetString(Key(CurrentChildId, field), fallback) : fallback;
    public static int GetInt(string field, int fallback = 0) => HasIdentity
        ? PlayerPrefs.GetInt(Key(CurrentChildId, field), fallback) : fallback;
    public static bool HasCharacter => GetInt("CharacterSelected") == 1;
    public static bool IsBoy(string gender) => gender == "ذكر" || gender == "ركذ" ||
        string.Equals(gender, "Boy", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(gender, "Male", StringComparison.OrdinalIgnoreCase);

    public static bool SaveCharacter(int index)
    {
        if (!HasIdentity || index < 0) return false;
        PlayerPrefs.SetInt(Key(CurrentChildId, "SelectedCharacterIndex"), index);
        PlayerPrefs.SetInt(Key(CurrentChildId, "CharacterSelected"), 1);
        PlayerPrefs.Save();
        return true;
    }

    public static void RememberLogin(string code, string childId, string gender)
    {
        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(childId)) return;
        PlayerPrefs.SetString("lamie_login_v1_" + Uri.EscapeDataString(code), childId);
        PlayerPrefs.SetString(Key(childId, "UserGender"), gender ?? "");
        PlayerPrefs.SetString("CurrentChildID", childId);
        // Compatibility for existing scenes that display the active child's gender.
        PlayerPrefs.SetString("UserGender", gender ?? "");
        PlayerPrefs.Save();
    }

    public static bool TryRestoreLogin(string code)
    {
        string childId = PlayerPrefs.GetString("lamie_login_v1_" + Uri.EscapeDataString(code), "");
        if (string.IsNullOrEmpty(childId) || !LocalTestAccess.CanUseChild(childId)) return false;
        PlayerPrefs.SetString("CurrentChildID", childId);
        PlayerPrefs.SetString("UserGender", PlayerPrefs.GetString(Key(childId, "UserGender"), ""));
        PlayerPrefs.Save();
        return true;
    }

    public static void SaveProfile(string childId, string name, int age, string gender)
    {
        if (string.IsNullOrWhiteSpace(childId)) return;
        PlayerPrefs.SetString(Key(childId, "ChildName"), name ?? "");
        PlayerPrefs.SetInt(Key(childId, "ChildAge"), age);
        if (!string.IsNullOrWhiteSpace(gender)) PlayerPrefs.SetString(Key(childId, "UserGender"), gender.Trim());
        PlayerPrefs.Save();
    }
}
