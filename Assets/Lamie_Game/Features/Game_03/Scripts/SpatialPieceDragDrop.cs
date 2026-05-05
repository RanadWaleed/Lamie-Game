using UnityEngine;
using UnityEngine.EventSystems;

public class SpatialPieceDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform[] allMolds;
    public float snapDistance = 50f;
    public float scaleMultiplier = 1.15f;

    private int originalSiblingIndex;
    public string pieceID;
    public string selectedMoldName = "";
    public bool isPlaced = false;

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;

    void Start()
    {
        originalSiblingIndex = transform.GetSiblingIndex();
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

        selectedMoldName = "";
        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;
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
            if (closestMold.name == pieceID)
            {
                if (MasterManager.Instance != null) MasterManager.Instance.PlayDropSound();
                if (MasterManager.Instance != null) MasterManager.Instance.PlaySuccessSound();

                rectTransform.position = closestMold.position;
                rectTransform.rotation = closestMold.rotation;

                rectTransform.sizeDelta = closestMold.sizeDelta;
                transform.localScale = closestMold.localScale;
                selectedMoldName = closestMold.name;
                isPlaced = true;

                canvasGroup.blocksRaycasts = false;
                transform.SetSiblingIndex(originalSiblingIndex);

                if (Task1_Manager.Instance != null && Task1_Manager.Instance.boardParent != null && transform.IsChildOf(Task1_Manager.Instance.boardParent))
                {
                    Task1_Manager.Instance.PiecePlacedCorrectly();
                }
                else if (Task3_Manager.Instance != null && Task3_Manager.Instance.boardParent != null && transform.IsChildOf(Task3_Manager.Instance.boardParent))
                {
                    Task3_Manager.Instance.PiecePlacedCorrectly();
                }

                Debug.Log("<color=green>SUCCESS! Placed perfectly. Distance:</color> " + minDistance);
                return;
            }
            else
            {
                selectedMoldName = "";
                isPlaced = false;

                if (Task1_Manager.Instance != null && Task1_Manager.Instance.boardParent != null && transform.IsChildOf(Task1_Manager.Instance.boardParent))
                {
                    Task1_Manager.Instance.PiecePlacedWrong();
                }
                else if (Task3_Manager.Instance != null && Task3_Manager.Instance.boardParent != null && transform.IsChildOf(Task3_Manager.Instance.boardParent))
                {
                    Task3_Manager.Instance.PiecePlacedWrong();
                }

                Debug.Log("<color=red>WRONG MOLD! Attempt Counted. Distance:</color> " + minDistance + " | Snap Limit: " + snapDistance);
            }
        }
        else
        {
            selectedMoldName = "";
            isPlaced = false;

            Debug.Log("<color=blue>FAR AWAY! Ignored. Distance:</color> " + minDistance + " | Snap Limit: " + snapDistance);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (allMolds == null) return;

        foreach (RectTransform mold in allMolds)
        {
            if (mold != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(mold.position, snapDistance);
            }
        }
    }
}