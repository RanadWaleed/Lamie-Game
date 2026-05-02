using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum GameFlowState
{
    Intro,
    Login,
    CharacterSelection,
    Home,
    Inventory,
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

            string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            currentState = scene switch
            {
                "IntroScene" => GameFlowState.Intro,
                "LoginScene" => GameFlowState.Login,
                "CharacterSelectionScene" => GameFlowState.CharacterSelection,
                "HomeScene" => GameFlowState.Home,
                "InventoryScene" => GameFlowState.Inventory,
                "Game01Scene" => GameFlowState.Game01,
                "Game02Scene" => GameFlowState.Game02,
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
            case GameFlowState.Intro: GoToState(GameFlowState.Login); break;
            case GameFlowState.Login: GoToState(GameFlowState.CharacterSelection); break;
            case GameFlowState.CharacterSelection: GoToState(GameFlowState.Home); break;
            case GameFlowState.Home: GoToState(GameFlowState.Inventory); break;
            case GameFlowState.Inventory: GoToState(GameFlowState.Game01); break;
        }
    }

    private string GetSceneName(GameFlowState state) => state switch
    {
        GameFlowState.Intro => "IntroScene",
        GameFlowState.Login => "LoginScene",
        GameFlowState.CharacterSelection => "CharacterSelectionScene",
        GameFlowState.Home => "HomeScene",
        GameFlowState.Inventory => "InventoryScene",
        GameFlowState.Game01 => "Game01Scene",
        GameFlowState.Game02 => "Game02Scene",
        _ => "HomeScene"
    };

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
    }

}
