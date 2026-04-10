using UnityEngine;
using UnityEngine.UI;

public class FrameSelectionManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image frameDisplay;
    public Button leftBtn;
    public Button rightBtn;
    public Button confirmBtn; 

    [Header("Audio")]
    public AudioSource audioSource;       // مصدر الصوت
    public AudioClip chooseFrameVoice;    // ملف الصوت "اختر إطار"

    [Header("Objects to Hide/Show")]
    public GameObject toolBox;      
    public GameObject drawingBoard; 

    [Header("Target Destination")]
    public Image targetBoardFrame;  // مكان الإطار الفاضي في شاشة الرسم

    [Header("Data")]
    public Sprite[] frameSprites;

    private int currentIndex = 0;

    void OnEnable()
    {
        // نخفي التول بوكس والبورد
        if (toolBox) toolBox.SetActive(false);
        if (drawingBoard) drawingBoard.SetActive(false);

        // تشغيل الصوت أول ما تفتح الشاشة أوتوماتيك
        if (audioSource && chooseFrameVoice)
        {
            audioSource.PlayOneShot(chooseFrameVoice);
        }
    }

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
        {
            frameDisplay.sprite = frameSprites[currentIndex];
        }
    }

    public Sprite GetSelectedFrame()
    {
        if (frameSprites.Length > 0) return frameSprites[currentIndex];
        return null;
    }

    public void ConfirmSelection()
    {
        // 1. نرسل صورة الإطار المختار إلى شاشة الرسم!
        if (targetBoardFrame)
        {
            targetBoardFrame.sprite = GetSelectedFrame();
        }

        // 2. نرجع التول بوكس والبورد عشان يبدأ يرسم
        if (toolBox) toolBox.SetActive(true);
        if (drawingBoard) drawingBoard.SetActive(true);
        
        // 3. نخفي شاشة اختيار الإطارات
        this.gameObject.SetActive(false);
    }
}