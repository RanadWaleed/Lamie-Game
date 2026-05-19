using UnityEngine;
using UnityEngine.UI;

public class Task2_ColorButton : MonoBehaviour
{
    public string colorID;
    public Color paintColor = Color.white;
    public Color selectedTint = Color.gray;

    private Image buttonImage;
    private Button myButton;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnColorClicked);
    }

    private void OnColorClicked()
    {
        Task2_Manager.Instance.SetActiveColor(colorID, paintColor);
        buttonImage.color = selectedTint;
    }

    public void ResetTint()
    {
        if (buttonImage != null) buttonImage.color = Color.white;
    }
}