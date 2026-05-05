using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Task4_Manager : MonoBehaviour
{
    public static Task4_Manager Instance;

    [Header("Panel Settings")]
    public RectTransform bottomPanel;
    public CanvasGroup panelCanvasGroup;
    public float animationSpeed = 8f;
    public float hideDropDistance = 500f;

    private float panelVisibleY;
    private float panelHiddenY;

    [Header("Images to Fade Out")]
    public CanvasGroup image1ToFade;
    public CanvasGroup image2ToFade;
    public float fadeDuration = 1.0f;

    [Header("Audio")]
    public AudioSource successSound;
    public AudioSource errorSound;

    [Header("Levels Setup")]
    public List<GameObject> levelItemsParents;
    public List<GameObject> levelDropZonesParents;

    public List<float> standardTimes;

    private int currentLevelIndex = 0;

    [Header("Metrics (القياسات)")]
    private int totalPiecesInCurrentLevel = 0;
    private int correctAttempts = 0;
    private int totalAttempts = 0;
    private int wrongAttempts = 0; 
    private int firstTrySuccess = 0;
    private float levelStartTime = 0f; 

    private float targetY;
    private float targetAlpha;
    private bool isDraggingItem = false;

    void Awake()
    {
        Instance = this;
        if (bottomPanel != null)
        {
            panelVisibleY = bottomPanel.anchoredPosition.y;
            panelHiddenY = panelVisibleY - hideDropDistance;
        }
    }

    void Start()
    {
        if (PsychometricReportManager.Instance != null)
        {
         
            PsychometricReportManager.Instance.StartNewIndicator("Spatial Placement");
        }

        foreach (var item in levelItemsParents) if (item != null) item.SetActive(false);
        foreach (var zone in levelDropZonesParents) if (zone != null) zone.SetActive(false);

        targetY = panelHiddenY;
        targetAlpha = 0f;
        if (bottomPanel != null) bottomPanel.anchoredPosition = new Vector2(bottomPanel.anchoredPosition.x, targetY);
    }

    void Update()
    {
        if (bottomPanel != null)
        {
            float newY = Mathf.Lerp(bottomPanel.anchoredPosition.y, targetY, Time.deltaTime * animationSpeed);
            bottomPanel.anchoredPosition = new Vector2(bottomPanel.anchoredPosition.x, newY);
            panelCanvasGroup.alpha = Mathf.Lerp(panelCanvasGroup.alpha, targetAlpha, Time.deltaTime * animationSpeed);
        }
    }

    public void StartGame4AfterLubnah()
    {
        StartCoroutine(FadeOutImagesSmoothly());
        LoadLevel(0);
        ShowPanelNormal();
    }

    public void LoadLevel(int index)
    {
        if (index >= levelItemsParents.Count) return;

        if (currentLevelIndex < levelItemsParents.Count && currentLevelIndex != index)
        {
            if (levelItemsParents[currentLevelIndex] != null)
                levelItemsParents[currentLevelIndex].SetActive(false);
        }

        currentLevelIndex = index;

        // 🚨 تصفير العدادات للبند الجديد
        correctAttempts = 0;
        totalAttempts = 0;
        wrongAttempts = 0;
        firstTrySuccess = 0;

        if (levelItemsParents[currentLevelIndex] != null) levelItemsParents[currentLevelIndex].SetActive(true);
        if (levelDropZonesParents[currentLevelIndex] != null) levelDropZonesParents[currentLevelIndex].SetActive(true);

        Task4_DragItem[] pieces = levelItemsParents[currentLevelIndex].GetComponentsInChildren<Task4_DragItem>(true);
        totalPiecesInCurrentLevel = pieces.Length;

        ShowPanelNormal();

        // ⏱️ التايمر يبدأ هنااااا! (بعد ما يطلع البانل وتبان العناصر للطفل)
        levelStartTime = Time.time;
    }

    public void SetPanelDragState(bool isDragging)
    {
        isDraggingItem = isDragging;
        if (isDragging)
        {
            targetY = panelHiddenY;
            targetAlpha = 0f;
        }
        else
        {
            ShowPanelNormal();
        }
    }

    private void ShowPanelNormal()
    {
        targetY = panelVisibleY;
        targetAlpha = 0.9f;
    }

    public void HidePanelCompletely()
    {
        targetY = panelHiddenY;
        targetAlpha = 0f;
    }

    public void RegisterAttempt(bool isCorrect, bool isFirstTry)
    {
        totalAttempts++;
        if (isCorrect)
        {
            correctAttempts++; //  CorrectPlacements
            if (isFirstTry) firstTrySuccess++;
            if (successSound != null) successSound.Play();

            if (correctAttempts >= totalPiecesInCurrentLevel)
            {
                StartCoroutine(LevelCompleteSequence());
            }
        }
        else
        {
            wrongAttempts++;
            if (errorSound != null) errorSound.Play();
        }
    }

    IEnumerator LevelCompleteSequence()
    {
      
        float actualTime = Mathf.Max(0.1f, Time.time - levelStartTime);
        float stdTime = currentLevelIndex < standardTimes.Count ? standardTimes[currentLevelIndex] : 10f; 
        int itemIndex = currentLevelIndex + 1;

        
        float accuracy = totalPiecesInCurrentLevel > 0 ? ((float)correctAttempts / totalPiecesInCurrentLevel) * 100f : 0f;
        float randomness = totalAttempts > 0 ? ((float)wrongAttempts / totalAttempts) * 100f : 0f;
        float speed = actualTime > 0 ? (stdTime / actualTime) : 0f;
        float firstTryIndex = totalPiecesInCurrentLevel > 0 ? ((float)firstTrySuccess / totalPiecesInCurrentLevel) * 100f : 0f;

        Debug.Log($"<color=cyan><b>[ SPATIAL ITEM {itemIndex} METRICS ]</b></color>\n" +
                  $"Time: {actualTime:F2}s | Std Time: {stdTime}s\n" +
                  $"Attempts (Total/Wrong/FirstTry): {totalAttempts} / {wrongAttempts} / {firstTrySuccess}\n" +
                  $"Accuracy: {accuracy:F1}% | Randomness: {randomness:F1}% | Speed: {speed:F2}\n" +
                  $"First Try Index: {firstTryIndex:F1}%");

        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.SaveItemData(itemIndex, firstTrySuccess, totalPiecesInCurrentLevel, totalAttempts, actualTime, stdTime, "Spatial " + itemIndex);
        }

        yield return new WaitForSeconds(1.0f);

        if (currentLevelIndex + 1 >= levelItemsParents.Count)
        {
            if (PsychometricReportManager.Instance != null)
            {
                PsychometricReportManager.Instance.FinishCurrentIndicator();

                PsychometricReportManager.Instance.UploadCurrentGameResult();
            }

            if (Game3_MasterManager.Instance != null)
            {
                Game3_MasterManager.Instance.PlayEndSequence();
            }
        }
        else
        {
            LoadLevel(currentLevelIndex + 1);
        }
    }

    private IEnumerator FadeOutImagesSmoothly()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float currentAlpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);

            if (image1ToFade != null) image1ToFade.alpha = currentAlpha;
            if (image2ToFade != null) image2ToFade.alpha = currentAlpha;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (image1ToFade != null) image1ToFade.gameObject.SetActive(false);
        if (image2ToFade != null) image2ToFade.gameObject.SetActive(false);
    }

}