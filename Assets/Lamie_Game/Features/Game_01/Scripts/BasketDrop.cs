using UnityEngine;
using UnityEngine.EventSystems;

public class BasketDrop : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            RectTransform candy = eventData.pointerDrag.GetComponent<RectTransform>();
            candy.SetParent(transform);
            candy.anchoredPosition = new Vector2(0, 30);
        }
    }
}