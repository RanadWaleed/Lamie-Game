using UnityEngine;
using UnityEngine.EventSystems;

public class ItemInteraction : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public string itemType;

    public void OnPointerDown(PointerEventData eventData)
    {
        BoardManager.Instance.SelectItem(this.gameObject, itemType == "BG");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (itemType == "BG") return;

        RectTransform rt = GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransform area = BoardManager.Instance.drawingArea;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position, eventData.pressEventCamera, out localPoint);

        Rect r = area.rect;
        float halfW = rt.sizeDelta.x * 0.5f;
        float halfH = rt.sizeDelta.y * 0.5f;

        localPoint.x = Mathf.Clamp(localPoint.x, r.xMin + halfW, r.xMax - halfW);
        localPoint.y = Mathf.Clamp(localPoint.y, r.yMin + halfH, r.yMax - halfH + 20f);

        rt.anchoredPosition = localPoint;

        // التحديث هنا
        BoardManager.Instance.SelectItem(this.gameObject, false);
    }
}