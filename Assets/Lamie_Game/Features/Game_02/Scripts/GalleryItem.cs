using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GalleryItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject masterPrefab;
    public string itemType;
    public float defaultWidth = 250f;
    private GameObject draggingItem;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemType == "BG") SpawnItem(eventData.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemType != "BG") SpawnItem(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingItem != null && itemType != "BG")
        {
            RectTransform rt = draggingItem.GetComponent<RectTransform>();
            Vector2 localPoint;
            RectTransform area = BoardManager.Instance.drawingArea;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position, eventData.pressEventCamera, out localPoint);

            Rect r = area.rect;
            float halfW = rt.sizeDelta.x * 0.5f;
            float halfH = rt.sizeDelta.y * 0.5f;

            localPoint.x = Mathf.Clamp(localPoint.x, r.xMin + halfW, r.xMax - halfW);
            localPoint.y = Mathf.Clamp(localPoint.y, r.yMin + halfH, r.yMax - halfH);

            rt.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (draggingItem != null)
        {
            draggingItem.GetComponent<CanvasGroup>().blocksRaycasts = true;
            // التحديث هنا: أضفنا التحقق من الخلفية للبورد مانجر
            BoardManager.Instance.SelectItem(draggingItem, itemType == "BG");
        }
    }

    private void SpawnItem(Vector2 spawnPos)
    {
        Transform layer = BoardManager.Instance.GetLayer(itemType);

        // التحديث هنا: مسح الطبقة بالطريقة اللي يفهمها البورد مانجر الحالي
        if (itemType != "Decoration")
        {
            foreach (Transform child in layer) Destroy(child.gameObject);
        }

        draggingItem = Instantiate(masterPrefab, layer);
        draggingItem.GetComponent<Image>().sprite = GetComponent<Image>().sprite;

        ItemInteraction interaction = draggingItem.GetComponent<ItemInteraction>();
        if (interaction != null) interaction.itemType = itemType;

        RectTransform rt = draggingItem.GetComponent<RectTransform>();

        if (itemType == "BG")
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = BoardManager.Instance.drawingArea.rect.size;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            // التحديث هنا: أرسلنا true لأنها خلفية
            BoardManager.Instance.SelectItem(draggingItem, true);
        }
        else
        {
            draggingItem.GetComponent<Image>().SetNativeSize();
            float ratio = rt.sizeDelta.y / rt.sizeDelta.x;
            rt.sizeDelta = new Vector2(defaultWidth, defaultWidth * ratio);
            draggingItem.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }
}