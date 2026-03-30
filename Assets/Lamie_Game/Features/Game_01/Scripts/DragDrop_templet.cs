using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform[] allMolds;
    public float snapDistance = 150f;
    public float scaleMultiplier = 1.15f;

    public string candyID;
    public string selectedMoldName = "";
    public bool isPlaced = false;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    private Vector3 originalScale;

    private Game2Manager game2Manager;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalScale = transform.localScale;
        startPosition = rectTransform.anchoredPosition;

        game2Manager = FindObjectOfType<Game2Manager>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();

        selectedMoldName = "";
        isPlaced = false;
        transform.localScale = originalScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        RectTransform closestMold = null;
        float minDistance = Mathf.Infinity;

        foreach (RectTransform mold in allMolds)
        {
            float distance = Vector2.Distance(rectTransform.anchoredPosition, mold.anchoredPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestMold = mold;
            }
        }

        if (minDistance <= snapDistance && closestMold != null)
        {
            bool isOccupied = false;
            DragDrop[] allCandies = FindObjectsOfType<DragDrop>();

            foreach (DragDrop candy in allCandies)
            {
                if (candy != this && candy.selectedMoldName == closestMold.name)
                {
                    isOccupied = true;
                    break;
                }
            }

            if (!isOccupied)
            {
                rectTransform.anchoredPosition = closestMold.anchoredPosition;
                transform.localScale = originalScale * scaleMultiplier;
                selectedMoldName = closestMold.name;
                isPlaced = true;

                if (game2Manager != null)
                {
                    game2Manager.CheckAllCandiesPlaced();
                }
                return;
            }
        }

        rectTransform.anchoredPosition = startPosition;
        transform.localScale = originalScale;
        selectedMoldName = "";
        isPlaced = false;
    }
}