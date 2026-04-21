using UnityEngine;
using UnityEngine.U2D.Animation;
using uLipSync;

public class LipSync2D : MonoBehaviour
{
    public SpriteResolver spriteResolver;
    public uLipSync.uLipSync lipSync;

    [Header("Speed Settings")]
    public float changeDelay = 0.1f;

    private float timer = 0f;
    private string currentLabel = "Closed";

    void OnEnable()
    {
        if (lipSync != null) lipSync.onLipSyncUpdate.AddListener(OnLipSyncUpdate);
    }

    void OnDisable()
    {
        if (lipSync != null) lipSync.onLipSyncUpdate.RemoveListener(OnLipSyncUpdate);
    }

    void OnLipSyncUpdate(LipSyncInfo info)
    {
        timer += Time.deltaTime;

        if (timer < changeDelay) return;

        string targetLabel = "Closed";

        if (info.volume > 0.01f)
        {
            switch (info.phoneme)
            {
                case "A": targetLabel = "Ah"; break;
                case "I":
                case "E": targetLabel = "Eee"; break;
                case "O": targetLabel = "Oh"; break;
                case "U": targetLabel = "Ooo"; break;
                default: targetLabel = "L"; break;
            }
        }

        if (currentLabel != targetLabel)
        {
            currentLabel = targetLabel;
            if (spriteResolver != null)
            {
                spriteResolver.SetCategoryAndLabel("Talking", currentLabel);
            }
            timer = 0f;
        }
    }
}