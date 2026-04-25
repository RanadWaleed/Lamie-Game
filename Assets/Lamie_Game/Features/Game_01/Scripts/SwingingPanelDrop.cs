using UnityEngine;
using System.Collections;

public class SwingingPanelDrop : MonoBehaviour
{
    [Header("Drop Settings")]
    public float delayBeforeDrop = 1f;
    public float dropHeight = 800f;
    public float duration = 2.5f;
    public float maxSwingAngle = 70f;

    private RectTransform rectTransform;
    private Vector2 originalPos;
    private Vector3 originalScale;
    private bool isInitialized = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
        isInitialized = true;
    }

    void OnEnable()
    {
        if (isInitialized)
        {
            StartCoroutine(DropAndSwing());
        }
    }

    IEnumerator DropAndSwing()
    {
        rectTransform.localScale = Vector3.zero;

        if (delayBeforeDrop > 0f)
        {
            yield return new WaitForSeconds(delayBeforeDrop);
        }

        rectTransform.localScale = originalScale;
        Vector2 startPos = originalPos + new Vector2(0, dropHeight);
        rectTransform.anchoredPosition = startPos;
        rectTransform.localRotation = Quaternion.Euler(maxSwingAngle, 0, 0);

        float timer = 0f;
        float dropTime = duration * 0.4f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float dropProgress = Mathf.Clamp01(timer / dropTime);
            float smoothDrop = 1f - Mathf.Pow(1f - dropProgress, 3f);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, originalPos, smoothDrop);

            float swingProgress = timer / duration;
            float currentAngle = maxSwingAngle * Mathf.Cos(swingProgress * Mathf.PI * 6f) * (1f - swingProgress);
            rectTransform.localRotation = Quaternion.Euler(currentAngle, 0, 0);

            yield return null;
        }

        rectTransform.anchoredPosition = originalPos;
        rectTransform.localRotation = Quaternion.identity;
    }
}