using UnityEngine;
using UnityEngine.UI;

public class ArtColorManager : MonoBehaviour
{
    public static ArtColorManager Instance;

    public GameObject colorToolBox;
    public GameObject backgroundOverlay;

    private GameObject selectedObject;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        HideToolBox();
    }

    public void SelectObject(GameObject obj)
    {
        selectedObject = obj;

        if (colorToolBox != null)
            colorToolBox.SetActive(true);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(true);
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
    }

    public void HideToolBox()
    {
        if (colorToolBox != null)
            colorToolBox.SetActive(false);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(false);

        selectedObject = null;
    }
}