using UnityEngine;
using UnityEngine.EventSystems;

public class Task4_DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public string itemID;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startAnchoredPosition;
    private Transform originalParent;
    private Canvas mainCanvas;

    public bool isPlacedCorrectly = false;
    private bool isFirstTry = true;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    void Start()
    {
        mainCanvas = GetComponentInParent<Canvas>();

        if (mainCanvas == null)
            mainCanvas = FindFirstObjectByType<Canvas>();
    }

    private Canvas GetMainCanvas()
    {
        if (mainCanvas != null) return mainCanvas;
        mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas == null) mainCanvas = FindFirstObjectByType<Canvas>();
        return mainCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;

        Canvas canvas = GetMainCanvas();
        if (canvas == null)
        {
            return;
        }

        originalParent = transform.parent;
        startAnchoredPosition = rectTransform.anchoredPosition;

        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;

        if (Task4_Manager.Instance != null) Task4_Manager.Instance.SetPanelDragState(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;

        Canvas canvas = GetMainCanvas();
        if (canvas == null) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Task4_Manager.Instance != null) Task4_Manager.Instance.SetPanelDragState(false);

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        if (isPlacedCorrectly) return;

        StartCoroutine(CheckDropResult());
    }

    private System.Collections.IEnumerator CheckDropResult()
    {
        yield return new WaitForEndOfFrame();

        if (!isPlacedCorrectly)
        {
            isFirstTry = false;
            if (Task4_Manager.Instance != null) Task4_Manager.Instance.RegisterAttempt(false, false);

            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = startAnchoredPosition;

            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void PlaceCorrectly(Vector3 correctPos, Transform correctParent)
    {
        isPlacedCorrectly = true;

        transform.SetParent(correctParent, true);

        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(1, 1);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (Task4_Manager.Instance != null) Task4_Manager.Instance.RegisterAttempt(true, isFirstTry);
    }
}