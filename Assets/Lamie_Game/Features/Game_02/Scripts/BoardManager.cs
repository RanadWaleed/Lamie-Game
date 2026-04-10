using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [Header("UI References")]
    public RectTransform selectionFrame;
    public Button deleteBtn;
    public Image frameDottedImage;
    public GameObject scaleBtn;
    public RectTransform drawingArea;

    [Header("Layers")]
    public Transform bgLayer;
    public Transform buildingLayer;
    public Transform decoLayer;
    public Transform charLayer;

    private GameObject currentItem;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip startTaskVoice;

    void Awake()
    {
        Instance = this;
        ClearAllLayers();
    }

    void Start()
    {
        if (deleteBtn) deleteBtn.onClick.AddListener(DeleteCurrent);

        if (selectionFrame) selectionFrame.gameObject.SetActive(false);

        if (drawingArea.GetComponent<Button>() == null)
            drawingArea.gameObject.AddComponent<Button>().onClick.AddListener(Deselect);
    }

    public Transform GetLayer(string type)
    {
        switch (type)
        {
            case "BG": return bgLayer;
            case "Building": return buildingLayer;
            case "Character": return charLayer;
            default: return decoLayer;
        }
    }

    public void SelectItem(GameObject obj, bool isBackground)
    {
        if (obj == null) return;

        currentItem = obj;

        if (selectionFrame == null) return;

        selectionFrame.SetParent(obj.transform);
        selectionFrame.gameObject.SetActive(true);
        selectionFrame.anchoredPosition = Vector2.zero;
        selectionFrame.localScale = Vector3.one;

        RectTransform frameRT = selectionFrame.GetComponent<RectTransform>();

        if (isBackground)
        {
            if (frameDottedImage) frameDottedImage.enabled = false;
            if (scaleBtn) scaleBtn.SetActive(false);

            frameRT.anchorMin = Vector2.zero;
            frameRT.anchorMax = Vector2.one;
            frameRT.offsetMin = Vector2.zero;
            frameRT.offsetMax = Vector2.zero;
        }
        else
        {
            if (frameDottedImage) frameDottedImage.enabled = true;
            if (scaleBtn) scaleBtn.SetActive(true);

            RectTransform objRect = obj.GetComponent<RectTransform>();
            frameRT.anchorMin = new Vector2(0.5f, 0.5f);
            frameRT.anchorMax = new Vector2(0.5f, 0.5f);
            frameRT.sizeDelta = objRect.sizeDelta;
        }
    }

    public void Deselect()
    {
        if (selectionFrame == null) return;
        selectionFrame.gameObject.SetActive(false);
        selectionFrame.SetParent(this.transform);
        currentItem = null;
    }

    public void DeleteCurrent()
    {
        if (currentItem != null)
        {
            GameObject toDestroy = currentItem;
            Deselect();
            Destroy(toDestroy);
        }
    }

    public void ClearAllLayers()
    {
        if (selectionFrame) Deselect();

        Transform[] layers = { bgLayer, buildingLayer, decoLayer, charLayer };
        foreach (Transform layer in layers)
        {
            if (layer == null) continue;
            foreach (Transform child in layer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    void OnEnable()
    {
        if (audioSource != null && startTaskVoice != null)
        {
            audioSource.PlayOneShot(startTaskVoice);
        }
    }
}