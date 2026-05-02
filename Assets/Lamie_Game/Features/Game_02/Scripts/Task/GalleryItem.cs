using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GalleryItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject masterPrefab;
    public string itemType;
    public float defaultWidth = 250f;
    public bool isColorable = false;

    private GameObject draggingItem;
    private bool isDragging = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (itemType != "BG") return;

        SpawnItem(eventData.position);

        if (draggingItem != null)
        {
            AssessmentTag placedTag = draggingItem.GetComponent<AssessmentTag>();
            string itemName = (placedTag != null && !string.IsNullOrEmpty(placedTag.itemId))
                ? placedTag.itemId
                : gameObject.name;

            RectTransform rt = draggingItem.GetComponent<RectTransform>();
            ArtAssessmentManager.Instance?.OnItemPlaced(draggingItem, itemName, rt.anchoredPosition);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDragging) return;
        if (itemType == "BG") return;

        isDragging = true;
        SpawnItem(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggingItem == null || itemType == "BG") return;

        RectTransform rt = draggingItem.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransform area = BoardManager.Instance.drawingArea;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            area, eventData.position, eventData.pressEventCamera, out localPoint);

        Rect r = area.rect;
        float halfW = rt.sizeDelta.x * 0.5f;
        float halfH = rt.sizeDelta.y * 0.5f;

        localPoint.x = Mathf.Clamp(localPoint.x, r.xMin + halfW, r.xMax - halfW);
        localPoint.y = Mathf.Clamp(localPoint.y, r.yMin + halfH, r.yMax - halfH);

        rt.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging || draggingItem == null) return;

        isDragging = false;
        GameObject placedItem = draggingItem;
        draggingItem = null;

        placedItem.GetComponent<CanvasGroup>().blocksRaycasts = true;
        BoardManager.Instance.SelectItem(placedItem, itemType == "BG");

        RectTransform rt = placedItem.GetComponent<RectTransform>();
        AssessmentTag placedTag = placedItem.GetComponent<AssessmentTag>();
        string itemName = (placedTag != null && !string.IsNullOrEmpty(placedTag.itemId)) ? placedTag.itemId : gameObject.name;
        ArtAssessmentManager.Instance?.OnItemPlaced(placedItem, itemName, rt.anchoredPosition);

        if (isColorable && placedItem.GetComponent<ColorableItem>() != null)
            ArtColorManager.Instance?.SelectObject(placedItem);
    }

    private void SpawnItem(Vector2 spawnPos)
    {
        Transform layer = BoardManager.Instance.GetLayer(itemType);

        if (itemType != "Decoration")
            foreach (Transform child in layer) Destroy(child.gameObject);

        draggingItem = Instantiate(masterPrefab, layer);

        AssessmentTag sourceTag = GetComponent<AssessmentTag>();
        if (sourceTag != null)
        {
            AssessmentTag spawnedTag = draggingItem.AddComponent<AssessmentTag>();
            spawnedTag.emotion = sourceTag.emotion;
            spawnedTag.category = sourceTag.category;
            spawnedTag.itemId = sourceTag.itemId;
        }

        if (isColorable)
            draggingItem.AddComponent<ColorableItem>();

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