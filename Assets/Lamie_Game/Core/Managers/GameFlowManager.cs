using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameFlowState
{
    Intro,
    Login,
    Home,
    CharacterSelection,
    Game01,
    Game02
}

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance;

    public GameFlowState currentState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoToState(GameFlowState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameFlowState.Intro:
                SceneManager.LoadScene("IntroScene");
                break;
            case GameFlowState.Login:
                SceneManager.LoadScene("LoginScene");
                break;
            case GameFlowState.Home:
                SceneManager.LoadScene("HomeScene");
                break;
            case GameFlowState.CharacterSelection:
                SceneManager.LoadScene("CharacterSelectionScene");
                break;
            case GameFlowState.Game01:
                SceneManager.LoadScene("Game01Scene");
                break;
            case GameFlowState.Game02:
                SceneManager.LoadScene("Game02Scene");
                break;
        }
    }

    public void GoToNextState()
    {
        switch (currentState)
        {
            case GameFlowState.Intro:
                GoToState(GameFlowState.Login);
                break;
            case GameFlowState.Login:
                GoToState(GameFlowState.Home);
                break;
            case GameFlowState.Home:
                GoToState(GameFlowState.CharacterSelection);
                break;
            case GameFlowState.CharacterSelection:
                GoToState(GameFlowState.Game01);
                break;
            case GameFlowState.Game01:
                GoToState(GameFlowState.Game02);
                break;
        }
    }
}