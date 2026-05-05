using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // 🚨 ضفنا هذي عشان نتعامل مع الصور

public class Task4_DropZone : MonoBehaviour, IDropHandler
{
    public string expectedItemID; // اسم العنصر المطلوب (مثلاً: "Table")

    public void OnDrop(PointerEventData eventData)
    {
        // 1. نجيب العنصر اللي الطفل سحبه وفك يده فوق هذي المنطقة
        Task4_DragItem draggedItem = eventData.pointerDrag.GetComponent<Task4_DragItem>();

        if (draggedItem != null)
        {
            // 2. هل هذا هو العنصر المطلوب بالضبط؟
            if (draggedItem.itemID == expectedItemID)
            {
                // 3. نعم صح! نعطيه أمر يثبت نفسه هنا
                draggedItem.PlaceCorrectly(transform.position, this.transform);

                // 4. 🔒 نقفل استقبال الضغطات على هذا الظل عشان ما ينحط فيه شيء ثاني بالغلط
                Image zoneImage = GetComponent<Image>();
                if (zoneImage != null)
                {
                    zoneImage.raycastTarget = false;
                }
            }
            // إذا كان العنصر غلط، السكريبت حق العنصر (DragItem) هو اللي بيحسب الغلط ويرجعه مكانه!
        }
    }
}