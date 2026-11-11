using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Create the cover before other scene scripts start their opening animations.
[DefaultExecutionOrder(-10000)]
[DisallowMultipleComponent]
public sealed class SceneIntroFade : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float duration = 2f;
    [SerializeField, Min(0f)] private float holdDuration = 0.15f;

    public bool IsComplete { get; private set; }

    private CanvasGroup cover;

    private void Awake()
    {
        var overlay = new GameObject("SceneIntroOverlay", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasGroup), typeof(GraphicRaycaster));
        overlay.transform.SetParent(transform, false);
        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;
        cover = overlay.GetComponent<CanvasGroup>();
        cover.alpha = 1f;
        cover.interactable = false;
        cover.blocksRaycasts = true;

        var panel = new GameObject("BlackCover", typeof(RectTransform), typeof(Image));
        var rect = panel.GetComponent<RectTransform>();
        rect.SetParent(overlay.transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = Color.black;
    }

    private IEnumerator Start()
    {
        // Keep the first rendered frame fully covered before revealing the scene.
        yield return null;
        if (holdDuration > 0f) yield return new WaitForSecondsRealtime(holdDuration);
        float elapsed = 0f;
        float fadeDuration = Mathf.Max(0.1f, duration);
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            // Quintic easing avoids abrupt changes in speed at either end.
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = t * t * t * (t * (6f * t - 15f) + 10f);
            cover.alpha = 1f - eased;
            yield return null;
        }
        cover.alpha = 0f;
        IsComplete = true;
        cover.blocksRaycasts = false;
        Destroy(cover.gameObject);
    }
}
