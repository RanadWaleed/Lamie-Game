using UnityEngine;
using UnityEngine.UI;


public class ArtColorManager : MonoBehaviour
{
    public static ArtColorManager Instance;

    public GameObject colorToolBox;
    public GameObject backgroundOverlay;

    private GameObject selectedObject;
    private bool isOpen = false;

    void Awake() { Instance = this; }

    void Start() { HideToolBox(); }


    public void SelectObject(GameObject obj)
    {
        if (obj == null) return;

        if (isOpen && selectedObject == obj) return;

        selectedObject = obj;

        CancelInvoke(nameof(OpenToolBox));
        Invoke(nameof(OpenToolBox), 0.05f);
    }

    private void OpenToolBox()
    {
        if (selectedObject == null) return;
        isOpen = true;

        if (colorToolBox != null) colorToolBox.SetActive(true);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(true);

        Debug.Log($"[Color] فُتح التول بوكس: {selectedObject.name}");
    }


    public void ChangeColor(string colorName)
    {
        if (selectedObject == null) return;

        Image targetImage = selectedObject.GetComponent<Image>();
        if (targetImage == null) return;

        switch (colorName.ToLower())
        {
            case "pink": targetImage.color = new Color(1f, 0.75f, 0.8f); break;
            case "darkblue": targetImage.color = new Color(0f, 0f, 0.5f); break;
            case "beige": targetImage.color = new Color(0.96f, 0.96f, 0.86f); break;
            case "darkgrey": targetImage.color = new Color(0.3f, 0.3f, 0.3f); break;
            case "yellow": targetImage.color = Color.yellow; break;
            case "green": targetImage.color = Color.green; break;
            default: targetImage.color = Color.white; break;
        }

        ArtAssessmentManager.Instance?.OnColorApplied(colorName);
        Debug.Log($"[Color] لون '{colorName}' على {selectedObject.name}");

    }


    public void HideToolBox()
    {
        CancelInvoke(nameof(OpenToolBox));
        isOpen = false;

        if (colorToolBox != null) colorToolBox.SetActive(false);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(false);

        selectedObject = null;
    }
}