using UnityEngine;
using UnityEngine.EventSystems;

public class DrawingAreaClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerPress == this.gameObject)
            BoardManager.Instance.Deselect();
    }
}