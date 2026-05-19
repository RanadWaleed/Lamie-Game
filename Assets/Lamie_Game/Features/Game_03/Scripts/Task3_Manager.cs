using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Task3_Manager : MonoBehaviour
{
    public static Task3_Manager Instance;

    [Header("Levels Data")]
    public List<StageData> taskLevels;
    public List<Sprite> finalImages;
    private int currentLevelIndex = 0;

    [Header("Time & Delay Settings")]
    public float timerStartDelay = 0.5f;

    [Header("Effects")]
    public AudioSource successSound;
    public ParticleSystem dustParticles;
    public AudioClip pieceSnapClip;
    public GameObject finalGrandImageToShow;

    [Header("Panel Place & Animation")]
    public Transform boardParent;
    public RectTransform finalDisplayImage;
    public float slideDistance = 800f;
    public float slideDuration = 1.0f;

    private float standardTimePerLevel;
    private GameObject currentActiveBoard;

    private int totalPiecesInCurrentLevel = 0;
    private int correctAttempts = 0;
    private int totalAttempts = 0;
    private int firstTrySuccess = 0;
    private int wrongAttempts = 0;
    private bool pieceAlreadyFailed = false;
    private float levelStartTime = 0f;

    private int globalTotalPieces = 0;
    private int globalTotalAttempts = 0;
    private int globalWrongAttempts = 0;
    private int globalFirstTrySuccess = 0;
    private float globalTotalTime = 0f;

    private int item6_firstTrySuccess = 0;
    private int item6_totalPieces = 0;
    private int item6_totalAttempts = 0;
    private float item6_actualTime = 0f;
    private float item6_standardTime = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.StartNewIndicator("تمثيل الرموز البصرية المألوفة");
        }

        if (finalDisplayImage != null)
        {
            finalDisplayImage.gameObject.SetActive(false);
        }
    }

    public void LoadLevel(int index)
    {
        if (index >= taskLevels.Count) return;

        correctAttempts = 0;
        totalAttempts = 0;
        firstTrySuccess = 0;
        wrongAttempts = 0;
        pieceAlreadyFailed = false;

        StageData levelData = taskLevels[index];
        standardTimePerLevel = levelData.standardTime;

        currentActiveBoard = Instantiate(levelData.stagePrefab, boardParent);
        currentActiveBoard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        currentActiveBoard.transform.localPosition = Vector3.zero;
        currentActiveBoard.transform.localScale = Vector3.one;

        totalPiecesInCurrentLevel = 0;
        SpatialPieceDragDrop[] allPieces = currentActiveBoard.GetComponentsInChildren<SpatialPieceDragDrop>(true);
        foreach (var piece in allPieces)
        {
            if (piece.pieceID != "Distractor") totalPiecesInCurrentLevel++;
        }

        StartCoroutine(StartLevelTimer());
    }

    private IEnumerator StartLevelTimer()
    {
        yield return new WaitForSeconds(timerStartDelay);
        levelStartTime = Time.time;
    }

    public void PiecePlacedCorrectly()
    {
        if (successSound != null && pieceSnapClip != null)
        {
            successSound.PlayOneShot(pieceSnapClip);
        }
        if (!pieceAlreadyFailed)
        {
            firstTrySuccess++;
        }

        correctAttempts++;
        totalAttempts++;
        pieceAlreadyFailed = false;

        if (correctAttempts >= totalPiecesInCurrentLevel)
        {
            StartCoroutine(LevelCompleteSequence());
        }
    }
    public void PiecePlacedWrong()
    {
        if (successSound != null && pieceSnapClip != null)
        {
            successSound.PlayOneShot(pieceSnapClip);
        }
        wrongAttempts++;
        totalAttempts++;
        pieceAlreadyFailed = true;
    }

    IEnumerator LevelCompleteSequence()
    {
        float actualTime = Mathf.Max(0.1f, Time.time - levelStartTime);
        int itemIndex = currentLevelIndex + 1;

        globalTotalPieces += totalPiecesInCurrentLevel;
        globalTotalAttempts += totalAttempts;
        globalWrongAttempts += wrongAttempts;
        globalFirstTrySuccess += firstTrySuccess;
        globalTotalTime += actualTime;

        float accuracy = totalAttempts > 0 ? ((float)totalPiecesInCurrentLevel / totalAttempts) * 100f : 0f;
        float randomness = totalPiecesInCurrentLevel > 0 ? Mathf.Clamp01((float)wrongAttempts / (totalPiecesInCurrentLevel * 2f)) * 100f : 0f;

        float accScore = totalPiecesInCurrentLevel > 0 ? Mathf.Clamp01((float)firstTrySuccess / totalPiecesInCurrentLevel) : 0f;
        float errScore = totalAttempts > 0 ? Mathf.Clamp01((float)firstTrySuccess / totalAttempts) : 0f;
        float spdScore = actualTime > 0 ? Mathf.Clamp01(standardTimePerLevel / actualTime) : 0f;
        float finalScore = (accScore * 0.6f) + (spdScore * 0.2f) + (errScore * 0.2f);
        float finalScorePercentage = finalScore * 100f;

        float globalAccuracy = globalTotalAttempts > 0 ? ((float)globalTotalPieces / globalTotalAttempts) * 100f : 0f;
        float globalRandomness = globalTotalPieces > 0 ? Mathf.Clamp01((float)globalWrongAttempts / (globalTotalPieces * 2f)) * 100f : 0f;

        string logMsg = $"<color=cyan><b>[ ITEM {itemIndex} METRICS ]</b></color>\n" +
                        $"Time: {actualTime:F2}s | Std Time: {standardTimePerLevel}s\n" +
                        $"Attempts (Total/Wrong/FirstTry): {totalAttempts} / {wrongAttempts} / {firstTrySuccess}\n" +
                        $"Level Accuracy: {accuracy:F1}% | Level Randomness: {randomness:F1}%\n" +
                        $"<color=yellow><b>Level Final Score: {finalScorePercentage:F1}%</b></color>\n" +
                        $"<color=orange><b>[ CUMULATIVE METRICS SO FAR ]</b></color>\n" +
                        $"Total Time: {globalTotalTime:F2}s | Global Accuracy: {globalAccuracy:F1}% | Global Randomness: {globalRandomness:F1}%";
        Debug.Log(logMsg);

        if (PsychometricReportManager.Instance != null)
        {
            string itemName = "بند " + itemIndex;
            if (Game3_MasterManager.ItemNames.TryGetValue("تمثيل الرموز البصرية المألوفة", out var names))
            {
                int nameIndex = itemIndex - 1;
                if (nameIndex >= 0 && nameIndex < names.Length) itemName = names[nameIndex];
            }
            PsychometricReportManager.Instance.SaveItemData(itemIndex, firstTrySuccess, totalPiecesInCurrentLevel, totalAttempts, actualTime, standardTimePerLevel, itemName);
        }

        if (itemIndex == 1)
        {
            item6_firstTrySuccess = firstTrySuccess;
            item6_totalPieces = totalPiecesInCurrentLevel;
            item6_totalAttempts = totalAttempts;
            item6_actualTime = actualTime;
            item6_standardTime = standardTimePerLevel;
        }

        if (successSound) successSound.Play();

        if (dustParticles)
        {
            dustParticles.gameObject.SetActive(true);
            dustParticles.transform.position = boardParent.position;
            dustParticles.Play();
        }

        yield return new WaitForSeconds(0.3f);

        if (currentActiveBoard != null) { Destroy(currentActiveBoard); }

        Image finalImgComponent = finalDisplayImage.GetComponent<Image>();
        if (currentLevelIndex < finalImages.Count && finalImages[currentLevelIndex] != null)
        {
            finalImgComponent.sprite = finalImages[currentLevelIndex];
        }

        finalImgComponent.color = new Color(finalImgComponent.color.r, finalImgComponent.color.g, finalImgComponent.color.b, 1f);
        finalDisplayImage.anchoredPosition = Vector2.zero;
        finalDisplayImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.0f);

        float animTimer = 0f;
        Vector2 startPos = finalDisplayImage.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(slideDistance, 0f);

        while (animTimer < slideDuration)
        {
            animTimer += Time.deltaTime;
            float progress = animTimer / slideDuration;
            finalDisplayImage.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            float alpha = Mathf.Lerp(1f, 0f, progress);
            finalImgComponent.color = new Color(finalImgComponent.color.r, finalImgComponent.color.g, finalImgComponent.color.b, alpha);
            yield return null;
        }

        finalDisplayImage.gameObject.SetActive(false);

        if (dustParticles != null)
        {
            dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dustParticles.gameObject.SetActive(false);
        }

        bool isLastLevel = (currentLevelIndex + 1 >= taskLevels.Count);

        if (isLastLevel)
        {
            float item6_acc = item6_totalPieces > 0 ? Mathf.Clamp01((float)item6_firstTrySuccess / item6_totalPieces) : 0f;
            float item6_err = item6_totalAttempts > 0 ? Mathf.Clamp01((float)item6_firstTrySuccess / item6_totalAttempts) : 0f;
            float item6_spd = item6_actualTime > 0 ? Mathf.Clamp01(item6_standardTime / item6_actualTime) : 0f;
            float item6_finalScore = ((item6_acc * 0.6f) + (item6_spd * 0.2f) + (item6_err * 0.2f)) * 100f;

            Debug.Log($"<color=magenta><b>[ SECRET ITEM 6 METRICS ]</b></color>\n" +
                      $"Total Attempts: {item6_totalAttempts} | Time: {item6_actualTime:F2}s | Score: {item6_finalScore:F1}%");

            if (PsychometricReportManager.Instance != null)
            {
                PsychometricReportManager.Instance.SaveItemData(6, item6_firstTrySuccess, item6_totalPieces, item6_totalAttempts, item6_actualTime, item6_standardTime, Game3_MasterManager.ItemNames["تمثيل الرموز البصرية المألوفة"][5]);
                PsychometricReportManager.Instance.FinishCurrentIndicator();
            }

            if (dustParticles != null)
            {
                dustParticles.gameObject.SetActive(true);
                dustParticles.Play();
            }

            if (finalGrandImageToShow != null)
            {
                finalGrandImageToShow.SetActive(true);
            }

            if (Game3_MasterManager.Instance != null)
            {
                Game3_MasterManager.Instance.OnTask3Completed();
            }
        }
        else
        {
            currentLevelIndex++;
            LoadLevel(currentLevelIndex);
        }

    }
}