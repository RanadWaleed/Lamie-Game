using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to the GameObject that holds the "Next" button in each game scene.
/// It marks the game complete, then navigates: 
///   - badge just unlocked → Inventory (badges page)
///   - otherwise           → next game in sequence
/// </summary>
public class GameCompleteHandler : MonoBehaviour
{
    // Called by the Next button's OnClick()
    public void OnNextPressed()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        bool badgeUnlocked = LocalProgressManager.Instance.MarkGameComplete(currentScene);

        if (badgeUnlocked)
        {
            // All games for this intelligence done → show badges
            GameFlowManager.Instance?.GoToState(GameFlowState.Inventory);
        }
        else
        {
            // Go to next game as usual
            GameFlowManager.Instance?.GoToNextState();
        }
    }

    // Called by the Back button's OnClick()
    public void OnBackPressed()
    {
        GameFlowManager.Instance?.GoToState(GameFlowState.Home);
    }
}