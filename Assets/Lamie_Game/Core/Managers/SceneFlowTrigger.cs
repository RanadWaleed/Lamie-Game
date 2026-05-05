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
}