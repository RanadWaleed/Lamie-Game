using UnityEngine;

public class Task4_LockToBackground : MonoBehaviour
{
    [Header(" (01- Final bg_0)")]
    public Transform backgroundSprite;

    void LateUpdate()
    {
        if (backgroundSprite != null)
        {
   
            transform.position = new Vector3(backgroundSprite.position.x, backgroundSprite.position.y, transform.position.z);
        }
    }
}