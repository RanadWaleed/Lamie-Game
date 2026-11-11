using UnityEngine;

[DefaultExecutionOrder(-9000)]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class TabletSafeArea : MonoBehaviour
{
    [SerializeField] private Vector2 designSize = new Vector2(2752f, 2064f);
    [SerializeField] private RectTransform[] backdrops;
    [SerializeField] private RectTransform[] leftCurtains;
    [SerializeField] private RectTransform[] rightCurtains;
    [SerializeField] private RectTransform[] navigation;

    private RectTransform content;
    private Canvas canvas;
    private Vector2 lastSize;
    private float lastScale;
    private Vector2[] backdropPositions, backdropSizes, leftMin, leftMax, rightMin, rightMax, navigationPositions;

    private void Start()
    {
        Canvas.ForceUpdateCanvases();
        Refresh();
    }

    private void LateUpdate() => Refresh();

    private void Refresh()
    {
        if (!Initialize()) return;
        Rect viewport = ((RectTransform)canvas.transform).rect;
        if (viewport.size == lastSize && canvas.scaleFactor == lastScale) return;
        ApplyLayout(viewport);
    }

    private bool Initialize()
    {
        if (content != null) return canvas != null;
        content = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>()?.rootCanvas;
        backdropSizes = Read(backdrops, rect => rect.sizeDelta);
        backdropPositions = Read(backdrops, rect => rect.anchoredPosition);
        leftMin = Read(leftCurtains, rect => rect.anchorMin);
        leftMax = Read(leftCurtains, rect => rect.anchorMax);
        rightMin = Read(rightCurtains, rect => rect.anchorMin);
        rightMax = Read(rightCurtains, rect => rect.anchorMax);
        navigationPositions = Read(navigation, rect => rect.anchoredPosition);
        return canvas != null;
    }

    public void ApplyLayout(Rect unusedSafeArea)
    {
        if (!Initialize()) return;
        var canvasRect = (RectTransform)canvas.transform;
        Rect viewport = canvasRect.rect;
        float pixelsPerUnit = canvas.scaleFactor;
        if (viewport.width <= 0f || viewport.height <= 0f || pixelsPerUnit <= 0f) return;
        lastSize = viewport.size;
        lastScale = pixelsPerUnit;

        // Fit the authored composition uniformly at every display density.
        float scale = Mathf.Min(viewport.width / designSize.x, viewport.height / designSize.y);
        content.localScale = Vector3.one * scale;
        content.anchoredPosition = Vector2.zero;

        Vector2 visibleSize = viewport.size / scale;
        for (int i = 0; i < backdrops.Length; i++)
        {
            RectTransform backdrop = backdrops[i];
            Vector2 size = backdropSizes[i];
            if (backdrop == null || size.x <= 0f || size.y <= 0f) continue;
            Vector2 offset = backdropPositions[i];
            float cover = Mathf.Max(1f, (visibleSize.x + 2f * Mathf.Abs(offset.x)) / size.x,
                (visibleSize.y + 2f * Mathf.Abs(offset.y)) / size.y);
            backdrop.sizeDelta = size * cover;
        }

        // Move the anchor origin, leaving each curtain's animated offset untouched.
        float extraWidth = Mathf.Max(0f, visibleSize.x - designSize.x) * 0.5f;
        ShiftCurtains(leftCurtains, leftMin, leftMax, -extraWidth);
        ShiftCurtains(rightCurtains, rightMin, rightMax, extraWidth);
        KeepNavigationVisible(canvasRect, viewport);
    }

    private void KeepNavigationVisible(RectTransform canvasRect, Rect viewport)
    {
        var corners = new Vector3[4];
        const float margin = 18f;
        for (int i = 0; i < navigation.Length; i++)
        {
            RectTransform button = navigation[i];
            if (button == null) continue;
            button.anchoredPosition = navigationPositions[i];
            button.GetWorldCorners(corners);
            Vector3 min = canvasRect.InverseTransformPoint(corners[0]);
            Vector3 max = canvasRect.InverseTransformPoint(corners[2]);
            float x = Mathf.Max(0f, viewport.xMin + margin - min.x) -
                Mathf.Max(0f, max.x - viewport.xMax + margin);
            float y = Mathf.Max(0f, viewport.yMin + margin - min.y) -
                Mathf.Max(0f, max.y - viewport.yMax + margin);
            button.anchoredPosition += (Vector2)button.parent.InverseTransformVector(
                canvasRect.TransformVector(new Vector3(x, y)));
        }
    }

    private static void ShiftCurtains(RectTransform[] curtains, Vector2[] min, Vector2[] max, float shift)
    {
        for (int i = 0; i < curtains.Length; i++)
        {
            RectTransform curtain = curtains[i];
            if (curtain == null || !(curtain.parent is RectTransform parent) || parent.rect.width <= 0f) continue;
            var anchorShift = new Vector2(shift / parent.rect.width, 0f);
            curtain.anchorMin = min[i] + anchorShift;
            curtain.anchorMax = max[i] + anchorShift;
        }
    }

    private static Vector2[] Read(RectTransform[] items, System.Func<RectTransform, Vector2> selector)
    {
        var values = new Vector2[items.Length];
        for (int i = 0; i < items.Length; i++)
            if (items[i] != null) values[i] = selector(items[i]);
        return values;
    }
}
