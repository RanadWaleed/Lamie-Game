using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
        string sceneName = GetSceneName(newState);
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private string GetSceneName(GameFlowState state)
    {
        switch (state)
        {
            case GameFlowState.Intro: return "IntroScene";
            case GameFlowState.Login: return "LoginScene";
            case GameFlowState.Home: return "HomeScene";
            case GameFlowState.CharacterSelection: return "CharacterSelectionScene";
            case GameFlowState.Game01: return "Game01Scene";
            case GameFlowState.Game02: return "Game02Scene";
            default: return "HomeScene";
        }
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
        {
            yield return null;
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
                GoToState(GameFlowState.CharacterSelection);
                break;
            case GameFlowState.CharacterSelection:
                GoToState(GameFlowState.Home);
                break;
            case GameFlowState.Home:
                GoToState(GameFlowState.Game01);
                break;
        }
    }
}