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

        mainCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;

   
        originalParent = transform.parent;
        startAnchoredPosition = rectTransform.anchoredPosition;

        transform.SetParent(mainCanvas.transform, true);
        transform.SetAsLastSibling();

 
        canvasGroup.blocksRaycasts = false;

      
        if (Task4_Manager.Instance != null) Task4_Manager.Instance.SetPanelDragState(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlacedCorrectly) return;

      
        rectTransform.anchoredPosition += eventData.delta / mainCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        
        if (Task4_Manager.Instance != null) Task4_Manager.Instance.SetPanelDragState(false);

        canvasGroup.blocksRaycasts = true;

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

        if (Task4_Manager.Instance != null) Task4_Manager.Instance.RegisterAttempt(true, isFirstTry);
    }
}