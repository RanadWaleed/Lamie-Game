using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ArtColorManager : MonoBehaviour
{
    public static ArtColorManager Instance;
    public GameObject colorToolBox;
    private GameObject selectedObject;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        if (colorToolBox != null)
            colorToolBox.SetActive(false);

        selectedObject = null;
    }

    void Update()
    {
        bool pressed =
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) ||
            (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame);

        if (!pressed) return;
        if (colorToolBox == null || !colorToolBox.activeSelf) return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        StartCoroutine(CheckHideNextFrame());
    }

    private System.Collections.IEnumerator CheckHideNextFrame()
    {
        yield return null;

        Vector2 position =
            Mouse.current != null ? Mouse.current.position.ReadValue() :
            Touchscreen.current.primaryTouch.position.ReadValue();

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = position;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == colorToolBox ||
                result.gameObject.transform.IsChildOf(colorToolBox.transform))
                yield break;

            if (result.gameObject.GetComponentInParent<ColorableItem>() != null)
                yield break;
        }

        HideToolBox();
    }

    public void SelectObject(GameObject obj)
    {
        selectedObject = obj;

        if (colorToolBox != null)
            colorToolBox.SetActive(true);
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

        selectedObject = null;
    }
}