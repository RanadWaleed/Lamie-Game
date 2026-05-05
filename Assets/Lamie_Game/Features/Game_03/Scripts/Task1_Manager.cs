using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Task1_Manager : MonoBehaviour
{
    public static Task1_Manager Instance;

    [Header("Levels Data")]
    public List<StageData> taskLevels;
    private int currentLevelIndex = 0;

    [Header("Effects")]
    public AudioSource successSound;
    public ParticleSystem levelCompleteStars;
    public ParticleSystem finalCompleteStars;
    public AudioClip pieceSnapClip;

    [Header("Panel Place")]
    public Transform boardParent;

    private float standardTimePerLevel;
    private GameObject currentActiveBoard;
    private int totalPiecesInCurrentLevel = 0;
    private int correctAttempts = 0;
    private int totalAttempts = 0;
    private int firstTrySuccess = 0;
    private bool pieceAlreadyFailed = false;
    private float levelStartTime = 0f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.SetupNewAspect("Game 3 Aspect", "Game_3");
            PsychometricReportManager.Instance.StartNewIndicator("Table Assembly");
        }
    }

    public void LoadLevel(int index)
    {
        if (index >= taskLevels.Count) return;

        correctAttempts = 0;
        totalAttempts = 0;
        firstTrySuccess = 0;
        pieceAlreadyFailed = false;
        levelStartTime = Time.time;

        StageData levelData = taskLevels[index];
        standardTimePerLevel = levelData.standardTime;

        currentActiveBoard = Instantiate(levelData.stagePrefab, boardParent);
        currentActiveBoard.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        totalPiecesInCurrentLevel = 0;
        SpatialPieceDragDrop[] allPieces = currentActiveBoard.GetComponentsInChildren<SpatialPieceDragDrop>(true);
        foreach (var piece in allPieces)
        {
            if (piece.pieceID != "Distractor") totalPiecesInCurrentLevel++;
        }

        Debug.Log($"<color=white><b>[STARTING TABLE {index + 1}]</b></color> Pieces: {totalPiecesInCurrentLevel} | Time From Prefab: {standardTimePerLevel}s");
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
        totalAttempts++;
        pieceAlreadyFailed = true;
    }

    IEnumerator LevelCompleteSequence()
    {
        float actualTime = Time.time - levelStartTime;
        int itemIndex = currentLevelIndex + 1;

        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.SaveItemData(
                itemIndex, firstTrySuccess, totalPiecesInCurrentLevel, totalAttempts, actualTime, standardTimePerLevel, "Table " + itemIndex
            );
        }

        if (successSound) successSound.Play();

        if (levelCompleteStars)
        {
            levelCompleteStars.gameObject.SetActive(true);

            Vector3 starsPos = boardParent.position;

            if (Camera.main != null)
            {
                starsPos.z = Camera.main.transform.position.z + 1.5f;
            }

            levelCompleteStars.transform.position = starsPos;

            levelCompleteStars.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            levelCompleteStars.Play();
            StartCoroutine(StopStarsManual(levelCompleteStars, 2.5f));
        }

        bool isLastLevel = (currentLevelIndex + 1 >= taskLevels.Count);

        if (isLastLevel)
        {
            yield return new WaitForSeconds(1.5f);

            if (PsychometricReportManager.Instance != null) PsychometricReportManager.Instance.FinishCurrentIndicator();
            if (Game3_MasterManager.Instance != null) Game3_MasterManager.Instance.OnTask1Completed();
        }
        else
        {
            yield return new WaitForSeconds(2f);
            if (currentActiveBoard != null) { Destroy(currentActiveBoard); }

            currentLevelIndex++;
            LoadLevel(currentLevelIndex);
        }
    }

    IEnumerator StopStarsManual(ParticleSystem ps, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}