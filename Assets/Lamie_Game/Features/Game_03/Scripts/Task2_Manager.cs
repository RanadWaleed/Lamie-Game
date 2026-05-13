using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Task2_Manager : MonoBehaviour
{
    public static Task2_Manager Instance;

    [Header("Cursor Settings")]
    public Texture2D brushCursor;
    public Vector2 cursorHotspot = Vector2.zero;

    [Header("UI Elements")]
    public GameObject topTable;
    public Transform levelParent;

    [Header("Button Settings")]
    public Button nextButton;
    public Image nextButtonImage;

    [Header("Audio")]
    public AudioSource errorSound;
    public AudioSource successSound;
    public AudioClip colorFillClip;

    [Header("Levels Data")]
    public List<StageData> taskLevels;
    private int currentLevelIndex = 0;
    private GameObject currentActiveBoard;

    [Header("Metrics")]
    private float levelStartTime;
    private int wrongAttempts = 0;
    private int totalAttempts = 0;
    private int totalPiecesInCurrentLevel = 0;
    private int correctAttempts = 0;
    private int firstTrySuccess = 0;
    private float standardTimePerLevel = 10f;

    private bool isColoringComplete = false;
    private bool isAdvancing = false;
    private Coroutine autoAdvanceCoroutine;

    private int globalTotalPieces = 0;
    private int globalTotalAttempts = 0;
    private int globalWrongAttempts = 0;
    private float globalTotalTime = 0f;

    private string currentActiveColorID = "";
    private Color currentActiveColorValue = Color.white;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (topTable != null) topTable.SetActive(false);
        SetButtonState(false);

        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.StartNewIndicator("اختيار الألوان الواقعية للعناصر");
        }
    }

    public void EnableBrushCursor()
    {
        if (brushCursor != null)
        {
            Cursor.SetCursor(brushCursor, cursorHotspot, CursorMode.ForceSoftware);
        }
        else
        {
            Debug.LogWarning("<color=red><b>نسيتي تحطين صورة الفرشة في المانجر!</b></color>");
        }
    }

    public void DisableBrushCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void ShowTableOnly()
    {
        if (topTable != null) topTable.SetActive(true);
    }

    public void PlayColorSound()
    {
        if (successSound != null && colorFillClip != null)
        {
            successSound.PlayOneShot(colorFillClip);
        }
    }

    public void LoadLevel(int index)
    {
        if (index >= taskLevels.Count) return;

        currentLevelIndex = index;
        isColoringComplete = false;
        isAdvancing = false;
        SetButtonState(false);

        if (nextButton != null) nextButton.gameObject.SetActive(true);

        wrongAttempts = 0;
        totalAttempts = 0;
        correctAttempts = 0;
        firstTrySuccess = 0;
        SetActiveColor("", Color.white);

        if (currentActiveBoard != null) Destroy(currentActiveBoard);

        StageData levelData = taskLevels[index];

        if (levelData.stagePrefab == null)
        {
            Debug.LogError($"<color=red><b>🚨 اللعبة الثانية: المرحلة رقم {index + 1} ما فيها بريفاب! تأكدي إنك سحبتي البريفاب لملف الـ StageData حقها! 🚨</b></color>");
            return;
        }

        standardTimePerLevel = levelData.standardTime;

        currentActiveBoard = Instantiate(levelData.stagePrefab, levelParent);
        currentActiveBoard.SetActive(true);

        RectTransform rt = currentActiveBoard.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localPosition = new Vector3(rt.localPosition.x, rt.localPosition.y, 0f);
        }

        Task2_ColorablePart[] parts = currentActiveBoard.GetComponentsInChildren<Task2_ColorablePart>(true);
        totalPiecesInCurrentLevel = parts.Length;

        StartCoroutine(AnimatePrefabAppearance(currentActiveBoard));
    }

    private IEnumerator AnimatePrefabAppearance(GameObject board)
    {
        CanvasGroup cg = board.GetComponent<CanvasGroup>();
        if (cg == null) cg = board.AddComponent<CanvasGroup>();

        float animDuration = 0.8f;
        float elapsed = 0f;

        Vector3 startScale = new Vector3(2f, 2f, 2f);
        Vector3 endScale = Vector3.one;

        board.transform.localScale = startScale;
        cg.alpha = 0f;

        while (elapsed < animDuration)
        {
            float progress = elapsed / animDuration;
            cg.alpha = Mathf.Lerp(0f, 1f, progress);
            board.transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cg.alpha = 1f;
        board.transform.localScale = endScale;

        levelStartTime = Time.time;
    }

    private void SetButtonState(bool isActive)
    {
        if (nextButtonImage != null)
        {
            nextButtonImage.color = isActive ? Color.white : Color.gray;
        }
    }

    public void OnNextButtonClicked()
    {
        if (!isColoringComplete)
        {
            if (errorSound != null) errorSound.Play();

            wrongAttempts++;
            totalAttempts++;
            return;
        }

        if (autoAdvanceCoroutine != null)
        {
            StopCoroutine(autoAdvanceCoroutine);
        }

        AdvanceToNextLevel();
    }
   
    
    /** Tracks each color attempt — records accuracy, 
    * first-try success, and triggers completion check*/

    public void RegisterColorAttempt(bool isCorrect, bool isFirstTry)
    {
        totalAttempts++;

        if (isCorrect)
        {
            // First-try count if no wrong color was applied before
            correctAttempts++;
            if (isFirstTry) firstTrySuccess++;

            CheckLevelCompletion();
        }
        
        else
        {
            wrongAttempts++;
        }
    }

    private void CheckLevelCompletion()
    {
        if (correctAttempts >= totalPiecesInCurrentLevel)
        {
            isColoringComplete = true;
            SetButtonState(true);
            if (successSound != null) successSound.Play();

            CalculateAndLogMetrics();

            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceTimer());
        }
    }

    private IEnumerator AutoAdvanceTimer()
    {
        yield return new WaitForSeconds(2.0f);
        AdvanceToNextLevel();
    }

    private void AdvanceToNextLevel()
    {
        if (isAdvancing) return;
        isAdvancing = true;

        if (currentLevelIndex + 1 >= taskLevels.Count)
        {
            DisableBrushCursor();

            if (nextButton != null) nextButton.gameObject.SetActive(false);

            if (PsychometricReportManager.Instance != null)
            {
                PsychometricReportManager.Instance.FinishCurrentIndicator();
            }

            if (Game3_MasterManager.Instance != null)
            {
                Game3_MasterManager.Instance.OnTask2Completed();
            }
        }
        else
        {
            LoadLevel(currentLevelIndex + 1);
        }
    }

    public IEnumerator FadeOutTask2(float fadeDuration)
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 0f;

        if (currentActiveBoard != null) Destroy(currentActiveBoard);
        if (topTable != null) topTable.SetActive(false);
        gameObject.SetActive(false);
        cg.alpha = 1f; 
    }

    private void CalculateAndLogMetrics()
    {
        float actualTime = Mathf.Max(0.1f, Time.time - levelStartTime);
        int itemIndex = currentLevelIndex + 1;

        globalTotalPieces += totalPiecesInCurrentLevel;
        globalTotalAttempts += totalAttempts;
        globalWrongAttempts += wrongAttempts;
        globalTotalTime += actualTime;

        float accuracyScore = totalPiecesInCurrentLevel > 0 ? (float)firstTrySuccess / totalPiecesInCurrentLevel : 0f;
        float randomness = totalAttempts > 0 ? (float)wrongAttempts / totalAttempts : 0f;
        float speedScore = actualTime > 0 ? Mathf.Clamp01(standardTimePerLevel / actualTime) : 0f;

        if (PsychometricReportManager.Instance != null)
        {
            string itemName = "بند " + itemIndex;
            if (Game3_MasterManager.ItemNames.TryGetValue("اختيار الألوان الواقعية للعناصر", out var names))
            {
                int nameIndex = itemIndex - 1;
                if (nameIndex >= 0 && nameIndex < names.Length) itemName = names[nameIndex];
            }
            PsychometricReportManager.Instance.SaveItemData(itemIndex, firstTrySuccess, totalPiecesInCurrentLevel, totalAttempts, actualTime, standardTimePerLevel, itemName);
        }
    }

    public void SetActiveColor(string colorID, Color paintColor)
    {
        currentActiveColorID = colorID;
        currentActiveColorValue = paintColor;

        Task2_ColorButton[] allButtons = FindObjectsByType<Task2_ColorButton>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
        {
            if (btn.colorID != colorID) btn.ResetTint();
        }
    }

    public string GetActiveColor() => currentActiveColorID;
    public Color GetActiveColorValue() => currentActiveColorValue;
}