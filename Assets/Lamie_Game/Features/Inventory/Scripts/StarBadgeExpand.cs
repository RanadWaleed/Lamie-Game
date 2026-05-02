using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class StarBadgeExpand : MonoBehaviour, IPointerClickHandler
{
    [Header("Star & Background")]
    public GameObject starBackground;
    public GameObject starObject;

    [Header("Expanded Badge Image")]
    public Image expandedBadgeImage;

    [Header("Animation Settings")]
    public float expandDuration = 0.4f;
    public AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isExpanded = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isExpanded) CollapseToStar();
        else ExpandBadge();
    }

    private void ExpandBadge()
    {
        isExpanded = true;
        StartCoroutine(ExpandAnim(true));
    }

    private void CollapseToStar()
    {
        isExpanded = false;
        StartCoroutine(ExpandAnim(false));
    }

    private IEnumerator ExpandAnim(bool show)
    {
        float elapsed = 0f;

        if (show && expandedBadgeImage != null)
            expandedBadgeImage.gameObject.SetActive(true);
        if (!show && starBackground != null)
            starBackground.SetActive(true);
        if (!show && starObject != null)
            starObject.SetActive(true);

        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = expandCurve.Evaluate(elapsed / expandDuration);

            if (starBackground != null) SetGroupAlpha(starBackground, show ? 1f - t : t);
            if (starObject != null) SetGroupAlpha(starObject, show ? 1f - t : t);

            if (expandedBadgeImage != null)
            {
                Color c = expandedBadgeImage.color;
                c.a = show ? t : 1f - t;
                expandedBadgeImage.color = c;
                expandedBadgeImage.transform.localScale = Vector3.Lerp(
                    show ? Vector3.one * 0.3f : Vector3.one,
                    show ? Vector3.one : Vector3.one * 0.3f,
                    t);
            }

            yield return null;
        }

        if (!show && expandedBadgeImage != null)
            expandedBadgeImage.gameObject.SetActive(false);
        if (show && starBackground != null)
            starBackground.SetActive(false);
        if (show && starObject != null)
            starObject.SetActive(false);
    }

    private void SetGroupAlpha(GameObject go, float alpha)
    {
        foreach (var img in go.GetComponentsInChildren<Image>())
        {
            Color c = img.color; c.a = alpha; img.color = c;
        }
    }
}