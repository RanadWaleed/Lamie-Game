using UnityEngine;

public class SceneFlowTrigger : MonoBehaviour
{
    public void GoToNext()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.GoToNextState();
        }
    }

    [Header("Specified Go To")]
    public bool useSpecificState = false;
    public GameFlowState targetState;

    public void GoToSpecific()
    {
        GameFlowManager.Instance?.GoToState(targetState);
    }

    public void GoToGame01()
    {
        GameFlowManager.Instance?.GoToState(GameFlowState.Game01);
    }

    public void GoToGame02()
    {
        GameFlowManager.Instance?.GoToState(GameFlowState.Game02);
    }

    public void GoToGame03()
    {
        GameFlowManager.Instance?.GoToState(GameFlowState.Game03);
    }
}