using UnityEngine;
using UnityEngine.SceneManagement;

public class GameCompleteHandler : MonoBehaviour
{
    public void OnNextPressed()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        bool badgeUnlocked = LocalProgressManager.Instance.MarkGameComplete(currentScene);

        if (badgeUnlocked)
        {
            GameFlowManager.Instance?.GoToState(GameFlowState.Inventory);
        }
        else
        {
            GameFlowManager.Instance?.GoToNextState();
        }
    }

    public void OnBackPressed()
    {
        GameFlowManager.Instance?.GoToState(GameFlowState.Home);
    }
}