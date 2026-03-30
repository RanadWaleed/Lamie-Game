using UnityEngine;

public class MahmoudController : MonoBehaviour
{
    public Animator characterAnimator;

    public void StartTalking()
    {
        if (characterAnimator != null)
        {
            characterAnimator.speed = 1f;
        }
    }

    public void StopTalking()
    {
        if (characterAnimator != null)
        {
            characterAnimator.speed = 0f;
        }
    }
}