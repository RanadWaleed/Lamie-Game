using UnityEngine;
using UnityEngine.EventSystems;

public class ResizeHandler : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        RectTransform selectionFrame = transform.parent.GetComponent<RectTransform>();
        RectTransform targetItem = selectionFrame.parent.GetComponent<RectTransform>();

        if (targetItem == null || targetItem == BoardManager.Instance.drawingArea) return;

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetItem, eventData.position, eventData.pressEventCamera, out localMousePos);

        float ratio = targetItem.sizeDelta.y / targetItem.sizeDelta.x;
        float newWidth = Mathf.Max(50f, localMousePos.x * 2f);
        float newHeight = newWidth * ratio;

        RectTransform area = BoardManager.Instance.drawingArea;
        Vector2 pos = targetItem.anchoredPosition;

        if (pos.x + (newWidth / 2) > area.rect.xMax + 5f || pos.x - (newWidth / 2) < area.rect.xMin - 5f ||
            pos.y + (newHeight / 2) > area.rect.yMax + 5f || pos.y - (newHeight / 2) < area.rect.yMin - 5f)
            return;

        targetItem.sizeDelta = new Vector2(newWidth, newHeight);
        selectionFrame.sizeDelta = targetItem.sizeDelta;
    }
}