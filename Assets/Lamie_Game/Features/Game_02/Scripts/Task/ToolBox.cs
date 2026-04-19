using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ToolboxLogic : MonoBehaviour
{
    public List<GameObject> panels;
    public List<RectTransform> tags;
    public ScrollRect scrollRect;
    public float liftAmount = 50f;

    public GameObject leftArrowButton;
    public GameObject rightArrowButton;

    public RectTransform decorationContent;
    public float scrollStep = 200f;
    public float maxX = 0f;

    private float minX = 0f;
    private List<float> baseHeights = new List<float>();

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        foreach (RectTransform t in tags)
            baseHeights.Add(t.anchoredPosition.y);

        SwitchTab(0);
    }

    private float CalculateContentWidth()
    {
        if (decorationContent == null) return 0f;

        GridLayoutGroup grid = decorationContent.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            int childCount = 0;
            foreach (Transform child in decorationContent)
                if (child.gameObject.activeSelf) childCount++;

            float cellWidth = grid.cellSize.x;
            float spacing = grid.spacing.x;
            RectOffset padding = grid.padding;

            float totalWidth = padding.left + padding.right
                             + (childCount * cellWidth)
                             + (Mathf.Max(0, childCount - 1) * spacing);

            return totalWidth;
        }

        // fallback
        return decorationContent.rect.width;
    }

    public void SwitchTab(int index)
    {
        for (int i = 0; i < tags.Count; i++)
        {
            panels[i].SetActive(i == index);
            Vector2 pos = tags[i].anchoredPosition;
            pos.y = (i == index) ? baseHeights[i] + liftAmount : baseHeights[i];
            tags[i].anchoredPosition = pos;
        }

        bool isDecorationTab = (index == 2);

        if (leftArrowButton != null) leftArrowButton.SetActive(isDecorationTab);
        if (rightArrowButton != null) rightArrowButton.SetActive(isDecorationTab);

        if (isDecorationTab && decorationContent != null)
        {
            float contentWidth = CalculateContentWidth();
            float viewportWidth = scrollRect.viewport.rect.width;
            minX = -(contentWidth - viewportWidth);
            if (minX > 0) minX = 0;

            Debug.Log($"contentWidth={contentWidth} viewportWidth={viewportWidth} minX={minX}");

            Vector2 p = decorationContent.anchoredPosition;
            p.x = maxX;
            decorationContent.anchoredPosition = p;
        }

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    public void ScrollRight()
    {
        if (decorationContent == null) return;
        Vector2 pos = decorationContent.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x - scrollStep, minX, maxX);
        decorationContent.anchoredPosition = pos;
        Debug.Log($"ScrollRight → x={pos.x}");
    }

    public void ScrollLeft()
    {
        if (decorationContent == null) return;
        Vector2 pos = decorationContent.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x + scrollStep, minX, maxX);
        decorationContent.anchoredPosition = pos;
        Debug.Log($"ScrollLeft → x={pos.x}");
    }
}