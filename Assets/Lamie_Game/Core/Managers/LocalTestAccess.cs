using System;
using UnityEngine;

// Local test identities never represent platform administrators or real children.
public static class LocalTestAccess
{
    private const string EditorChildId = "TestChild_000";
    private const string TabletChildId = "TabletTestAdmin";

    public static bool IsTestChild(string childId) =>
        string.Equals(childId, EditorChildId, StringComparison.Ordinal) ||
        string.Equals(childId, TabletChildId, StringComparison.Ordinal);

    public static bool CanUseChild(string childId)
    {
        if (!IsTestChild(childId)) return true;
#if UNITY_EDITOR
        return childId == EditorChildId && Application.isEditor;
#elif UNITY_ANDROID && DEVELOPMENT_BUILD
        return childId == TabletChildId && Application.platform == RuntimePlatform.Android && Debug.isDebugBuild;
#else
        return false;
#endif
    }

    public static bool TryResolveCode(string code, out string childId)
    {
        childId = null;
#if UNITY_EDITOR
        if (code == "000" && CanUseChild(EditorChildId)) childId = EditorChildId;
#elif UNITY_ANDROID && DEVELOPMENT_BUILD
        if (code == "842619" && CanUseChild(TabletChildId)) childId = TabletChildId;
#endif
        return childId != null;
    }
}
