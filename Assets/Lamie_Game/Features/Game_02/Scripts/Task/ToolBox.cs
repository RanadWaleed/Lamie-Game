using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToolboxLogic : MonoBehaviour
{
    public List<GameObject> panels;
    public List<RectTransform> tags;
    public ScrollRect scrollRect;
    public float liftAmount = 50f;

    private List<float> baseHeights = new List<float>();

    void Start()
    {
        Canvas.ForceUpdateCanvases();
        foreach (RectTransform t in tags)
        {
            baseHeights.Add(t.anchoredPosition.y);
        }


        SwitchTab(0);
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

        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }
}