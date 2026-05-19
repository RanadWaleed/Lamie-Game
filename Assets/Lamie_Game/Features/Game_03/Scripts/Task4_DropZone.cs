using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Task4_DropZone : MonoBehaviour, IDropHandler
{
    public string expectedItemID;

    public void OnDrop(PointerEventData eventData)
    {
        Task4_DragItem draggedItem = eventData.pointerDrag.GetComponent<Task4_DragItem>();

        if (draggedItem != null)
        {
            if (draggedItem.itemID == expectedItemID)
            {
                draggedItem.PlaceCorrectly(transform.position, this.transform);

                Image zoneImage = GetComponent<Image>();
                if (zoneImage != null)
                {
                    zoneImage.raycastTarget = false;
                }
            }
        }
    }
}