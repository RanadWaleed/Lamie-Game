using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCompleteHandler : MonoBehaviour
{
    private bool isReturning;

    public void OnNextPressed()
    {
        if (!isReturning) isReturning = TryReturnToMap();
    }

    // All completion screens share the same validation and navigation path.
    public static bool TryReturnToMap()
    {
        const string mapScene = "interactive_Game_Level";
        string currentScene = SceneManager.GetActiveScene().name;
        var localStore = new LocalAssessmentStore(new AssessmentLocalPersistence());
        if (!Application.isPlaying || !ChildLocalProfile.HasIdentity ||
            System.Array.IndexOf(PsychometricReportManager.AssessmentScenes, currentScene) < 0 ||
            !localStore.HasBaseline(ChildLocalProfile.CurrentChildId, currentScene))
        {
            Debug.LogWarning("A completed assessment is required before returning from the win screen.");
            return false;
        }
        if (!Application.CanStreamedLevelBeLoaded(mapScene))
        {
            Debug.LogError("Interactive map is missing from the build scene list.");
            return false;
        }
        LocalProgressManager.Instance?.MarkGameComplete(currentScene);
        if (GameFlowManager.Instance != null) GameFlowManager.Instance.GoToState(GameFlowState.GameLevel);
        else SceneManager.LoadScene(mapScene);
        return true;
    }

    public void OnBackPressed()
    {
        GameFlowManager.Instance?.GoToState(GameFlowState.Home);
    }
}
