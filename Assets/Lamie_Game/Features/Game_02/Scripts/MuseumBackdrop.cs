using UnityEngine;

[RequireComponent(typeof(Canvas))]
public sealed class MuseumBackdrop : MonoBehaviour
{
    [SerializeField] private RectTransform[] backgrounds = System.Array.Empty<RectTransform>();
    private RectTransform canvasRect;
    private Vector2[] originalSizes;
    private Vector2 previousSize;

    private void Awake()
    {
        canvasRect = (RectTransform)transform;
        originalSizes = new Vector2[backgrounds.Length];
        for (int i = 0; i < backgrounds.Length; i++)
            originalSizes[i] = backgrounds[i].sizeDelta;
    }

    private void LateUpdate()
    {
        Vector2 viewport = canvasRect.rect.size;
        if (viewport == previousSize || viewport.x <= 0f || viewport.y <= 0f) return;
        previousSize = viewport;
        for (int i = 0; i < backgrounds.Length; i++)
        {
            RectTransform background = backgrounds[i];
            Vector2 size = originalSizes[i];
            Vector2 offset = background.anchoredPosition;
            float scale = Mathf.Max(1f, Mathf.Max(
                (viewport.x + 2f * Mathf.Abs(offset.x)) / size.x,
                (viewport.y + 2f * Mathf.Abs(offset.y)) / size.y));
            background.sizeDelta = size * scale;
        }
    }
}
