using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameFlowState
{
    Intro,
    Login,
    CharacterSelection,
    Home,
    Game01,
    Game02,
    Game03,
    Inventory
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

            string scene = SceneManager.GetActiveScene().name;
            currentState = scene switch
            {
                "IntroScene" => GameFlowState.Intro,
                "LoginScene" => GameFlowState.Login,
                "CharacterSelectionScene" => GameFlowState.CharacterSelection,
                "HomeScene" => GameFlowState.Home,
                "Game01Scene" => GameFlowState.Game01,
                "Game02Scene" => GameFlowState.Game02,
                "Game_03" => GameFlowState.Game03,
                "InventoryScene" => GameFlowState.Inventory,
                _ => GameFlowState.Intro
            };
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GoToState(GameFlowState newState)
    {
        currentState = newState;
        StartCoroutine(LoadSceneAsync(GetSceneName(newState)));
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
                GoToState(GetResumeState());
                break;
            case GameFlowState.Game01:
                GoToState(GameFlowState.Game02);
                break;
            case GameFlowState.Game02:
                GoToState(GameFlowState.Game03);
                break;
            case GameFlowState.Game03:
                GoToState(GameFlowState.Inventory);
                break;
            case GameFlowState.Inventory:
                GoToState(GameFlowState.Home);
                break;
        }
    }

    private GameFlowState GetResumeState()
    {
        if (LocalProgressManager.Instance != null)
        {
            if (!LocalProgressManager.Instance.IsGameComplete("Game01Scene"))
                return GameFlowState.Game01;

            if (!LocalProgressManager.Instance.IsGameComplete("Game02Scene"))
                return GameFlowState.Game02;

            if (!LocalProgressManager.Instance.IsGameComplete("Game_03"))
                return GameFlowState.Game03;
        }

        return GameFlowState.Inventory;
    }

    private string GetSceneName(GameFlowState state) => state switch
    {
        GameFlowState.Intro => "IntroScene",
        GameFlowState.Login => "LoginScene",
        GameFlowState.CharacterSelection => "CharacterSelectionScene",
        GameFlowState.Home => "HomeScene",
        GameFlowState.Game01 => "Game01Scene",
        GameFlowState.Game02 => "Game02Scene",
        GameFlowState.Game03 => "Game_03",
        GameFlowState.Inventory => "InventoryScene",
        _ => "HomeScene"
    };

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
    }
}