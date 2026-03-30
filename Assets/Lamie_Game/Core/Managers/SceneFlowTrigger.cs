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
}