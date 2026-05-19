using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameFlowState
{
    Intro,
    Login,
    Story,
    CharacterSelection,
    Home,
    GameLevel,
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
                "STROYSECNE" => GameFlowState.Story,
                "CharacterSelectionScene" => GameFlowState.CharacterSelection,
                "HomeScene" => GameFlowState.Home,
                "GameLevelScene" => GameFlowState.GameLevel,
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
                GoToState(GameFlowState.Story);
                break;
            case GameFlowState.Story:
                GoToState(GameFlowState.CharacterSelection);
                break;
            case GameFlowState.CharacterSelection:
                GoToState(GameFlowState.Home);
                break;
            case GameFlowState.Home:
                GoToState(GameFlowState.GameLevel);
                break;
            case GameFlowState.GameLevel:
                GoToState(GameFlowState.Game01);
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

    private string GetSceneName(GameFlowState state) => state switch
    {
        GameFlowState.Intro => "IntroScene",
        GameFlowState.Login => "LoginScene",
        GameFlowState.Story => "STROYSECNE",
        GameFlowState.CharacterSelection => "CharacterSelectionScene",
        GameFlowState.Home => "HomeScene",
        GameFlowState.GameLevel => "GameLevelScene",
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