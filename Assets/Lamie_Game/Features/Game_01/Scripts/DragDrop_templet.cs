using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform[] allMolds;
    public float snapDistance = 100f;
    public float scaleMultiplier = 1.15f;

    public string candyID;
    public string selectedMoldName = "";
    public bool isPlaced = false;

    private bool hasFailed = false;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector3 startWorldPosition;
    private Vector3 originalScale;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();

        startWorldPosition = rectTransform.position;
        selectedMoldName = "";
        transform.localScale = originalScale;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        canvasGroup.blocksRaycasts = true;

        RectTransform closestMold = null;
        float minDistance = Mathf.Infinity;

        foreach (RectTransform mold in allMolds)
        {
            float distance = Vector2.Distance(rectTransform.position, mold.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestMold = mold;
            }
        }

        if (closestMold != null && minDistance <= snapDistance)
        {
            if (closestMold.name == candyID)
            {
                if (MasterManager.Instance != null) MasterManager.Instance.PlayDropSound();
                if (MasterManager.Instance != null) MasterManager.Instance.PlaySuccessSound();
                rectTransform.position = closestMold.position;
                transform.localScale = originalScale * scaleMultiplier;
                selectedMoldName = closestMold.name;
                isPlaced = true;

                canvasGroup.blocksRaycasts = false;

                if (MasterManager.Instance != null)
                {
                    MasterManager.Instance.RegisterAttempt(true, !hasFailed);
                }

                if (LevelSpawner.Instance != null && transform.parent.name == "AllCandiesGroup")
                {
                    LevelSpawner.Instance.CandyPlacedCorrectly();
                }
                else if (Game2Spawner.Instance != null && transform.parent.name == "AllShapesGroup")
                {
                    Game2Spawner.Instance.ShapePlacedCorrectly();
                }
                return;
            }
        }

        rectTransform.position = startWorldPosition;
        transform.localScale = originalScale;
        selectedMoldName = "";
        isPlaced = false;

        hasFailed = true;

        if (MasterManager.Instance != null)
        {
            MasterManager.Instance.RegisterAttempt(false, false);
        }
    }
}