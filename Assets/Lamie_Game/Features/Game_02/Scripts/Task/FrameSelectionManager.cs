using UnityEngine;
using UnityEngine.UI;

public class FrameSelectionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image frameDisplay;
    public Button leftBtn;
    public Button rightBtn;
    public Button confirmBtn;

    [Header("Target Destination")]
    public Image targetBoardFrame;

    [Header("Data")]
    public Sprite[] frameSprites;

    private int currentIndex = 0;

    void Start()
    {
        if (leftBtn) leftBtn.onClick.AddListener(PreviousFrame);
        if (rightBtn) rightBtn.onClick.AddListener(NextFrame);
        if (confirmBtn) confirmBtn.onClick.AddListener(ConfirmSelection);

        UpdateDisplay();
    }

    public void NextFrame()
    {
        if (frameSprites.Length == 0) return;
        currentIndex = (currentIndex + 1) % frameSprites.Length;
        UpdateDisplay();
    }

    public void PreviousFrame()
    {
        if (frameSprites.Length == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = frameSprites.Length - 1;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (frameDisplay && frameSprites.Length > 0)
            frameDisplay.sprite = frameSprites[currentIndex];
    }

    public Sprite GetSelectedFrame()
    {
        if (frameSprites.Length > 0) return frameSprites[currentIndex];
        return null;
    }

    public void ConfirmSelection()
    {
        if (targetBoardFrame)
            targetBoardFrame.sprite = GetSelectedFrame();
    }
}