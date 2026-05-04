using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterManager : MonoBehaviour
{
    public static readonly Dictionary<string, string[]> ItemNames = new Dictionary<string, string[]>
{
    { "مطابقة العناصر البصرية وفق اللون", new[] {
        "يحدد اللون المطابق للعنصر المعروض بدقة دون تردد",
        "يميز بين الألوان المختلفة دون خلط",
        "يطابق العناصر بناءً على اللون رغم اختلاف الشكل",
        "يتجنب الأخطاء عند وجود ألوان متقاربة",
        "يحافظ على دقة الأداء 'وليس السرعة' رغم زيادة عدد الخيارات",
        "يقل اعتماده على المحاولة العشوائية أثناء المطابقة"
    }},
    { "مطابقة العناصر البصرية وفق الشكل", new[] {
        "يحدد الشكل المطابق بدقة وباستجابة واثقة دون تردد ملحوظ",
        "يميز بين الأشكال الهندسية المختلفة بدقة دون خلط بينها",
        "يطابق الشكل رغم اختلاف اللون أو الحجم",
        "يتجنب الأخطاء عند التعامل مع أشكال متشابهة بصريًا",
        "يحافظ على مستوى دقة أدائه مع زيادة عدد البدائل المتاحة",
        "يُظهر نمط أداء منظمًا يقل فيه الاعتماد على المحاولة العشوائية"
    }},
    { "إكمال الأنماط البصرية", new[] {
        "يتعرف على النمط البصري البسيط ويكمله بدقة",
        "يميز القاعدة المنظمة للنمط دون خلط",
        "يكمل النمط مع تجاهل المشتتات غير المرتبطة",
        "يتجنب الأخطاء عند التعامل مع أنماط متشابهة",
        "يحافظ على دقة الأداء مع زيادة تعقيد النمط",
        "يُظهر نمط أداء منظمًا يقل فيه الاعتماد على المحاولة العشوائية"
    }}
};

    public static MasterManager Instance;

    [Header("Backend Logic Trackers")]
    public float maxTimeForCurrentLevel = 15f;
    public float timeTaken = 0f;
    public int currentStageIndex = 1;
    public int totalRequiredMatches = 3;
    public int Score = 0;
    public int Attempts = 0;
    public int scoreFirstAttempt = 0;
    public bool isGameActive = false;
    private string currentIndicatorName = "";

    [Header("Original UI Variables")]
    public int currentPhase = 1;
    public GameObject nextButton;
    public RectTransform environmentPanel;

    public float zoomScale = 1.3f;
    public float zoomDuration = 1.5f;

    public float game4PanDistance = 2000f;
    public float panDuration = 2f;

    public GameObject game1Elements;
    public GameObject game2Elements;
    public GameObject game3Elements;
    public GameObject game4Elements;

    public CanvasGroup game1UI_CG;

    public GameObject lubnaGame2;
    public Animator lubnaAnimator2;
    public AudioSource lubnaAudio2;

    public CanvasGroup overlayCG;
    public float gameplayOverlayAlpha = 0.6f;

    public GameObject lubnaGame3;
    public Animator lubnaAnimator3;
    public AudioSource lubnaAudio3;

    public CanvasGroup game3OddOneOutCG;
    public RectTransform animatedCandyBoxRT;
    public GameObject boxClosedImg;
    public GameObject boxOpenImg;
    public CanvasGroup lightGlowCG;
    public CanvasGroup finalCandiesCG;

    public float boxMoveDuration = 1.5f;
    public float backgroundDarknessTarget = 0.95f;
    public Vector2 boxCenterPos = new Vector2(0, -150f);
    public float boxEndScale = 1.6f;
    public Vector2 candiesStartOffset = new Vector2(0, -150f);
    public float popOutDuration = 0.6f;

    public RectTransform blueTableRT;
    public float tableMoveDuration = 2.5f;

    public GameObject lubnaGame4;
    public Animator lubnaAnimator4;
    public AudioSource lubnaAudio4;

    private Vector2 boxOriginalPos;
    private Vector2 tableOriginalPos;

    public GameObject shelfJarsFake;
    public GameObject floorJarsGroup;


    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (animatedCandyBoxRT != null) boxOriginalPos = animatedCandyBoxRT.anchoredPosition;
        if (blueTableRT != null) tableOriginalPos = blueTableRT.anchoredPosition;

        if (nextButton != null) nextButton.SetActive(false);

        if (game1Elements != null) game1Elements.SetActive(true);
        if (game2Elements != null) game2Elements.SetActive(false);
        if (game3Elements != null) game3Elements.SetActive(false);
        if (game4Elements != null) game4Elements.SetActive(false);

        if (lubnaGame2 != null) lubnaGame2.SetActive(false);
        if (lubnaGame3 != null) lubnaGame3.SetActive(false);
        if (lubnaGame4 != null) lubnaGame4.SetActive(false);

        if (blueTableRT != null) blueTableRT.gameObject.SetActive(false);

        if (overlayCG != null) overlayCG.alpha = gameplayOverlayAlpha;

        if (game3OddOneOutCG != null) game3OddOneOutCG.blocksRaycasts = false;
        if (lightGlowCG != null) lightGlowCG.alpha = 0f;

        // Setup dimension with childID from PlayerPrefs before any tracking starts
        if (PsychometricReportManager.Instance != null)
        {
            // الجانب الأول = التصور، واللعبة = Game_1
            PsychometricReportManager.Instance.SetupNewAspect("التصور", "Game_1");
        }

        StartBackendTracking("مطابقة العناصر البصرية وفق اللون");
    }

    void Update()
    {
        if (isGameActive)
        {
            timeTaken += Time.deltaTime;
        }
    }

    public void StartBackendTracking(string gameName)
    {
        currentIndicatorName = gameName;
        currentStageIndex = 1;
        timeTaken = 0f;
        Score = 0;
        Attempts = 0;
        scoreFirstAttempt = 0;
        isGameActive = true;

        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.StartNewIndicator(gameName);
        }
    }

    public void ResetStageTrackers()
    {
        timeTaken = 0f;
        Score = 0;
        Attempts = 0;
        scoreFirstAttempt = 0;
        isGameActive = true;
    }

    public void RegisterAttempt(bool isCorrect, bool isFirstAttempt = false)
    {
        Attempts++;
        if (isCorrect && isFirstAttempt) scoreFirstAttempt++;
        if (isCorrect) Score++;
    }

    public void SubmitStageData()
    {
        isGameActive = false;
        string itemName = "";

        if (PsychometricReportManager.Instance != null)
        {

            if (ItemNames.TryGetValue(currentIndicatorName, out var names))
            {
                int nameIndex = currentStageIndex - 1;
                itemName = nameIndex < names.Length ? names[nameIndex] : "بند " + currentStageIndex;
            }

            PsychometricReportManager.Instance.SaveItemData(
                currentStageIndex,
                scoreFirstAttempt,
                totalRequiredMatches,
                Attempts,
                timeTaken,
                maxTimeForCurrentLevel,
                itemName
            );
        }

        currentStageIndex++;
        timeTaken = 0f;
        Score = 0;
        Attempts = 0;
        scoreFirstAttempt = 0;

        // Resume timer for next stage
        isGameActive = true;
    }

    public void FinalizeAndUploadReport()
    {
        isGameActive = false;

        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.FinishCurrentIndicator();
            PsychometricReportManager.Instance.UploadCurrentGameResult();
        }
    }

    void OnApplicationQuit()
    {
        // Safety net: if game closes before Game4 finishes, try to upload whatever we have
        if (isGameActive && PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.FinishCurrentIndicator();
            PsychometricReportManager.Instance.UploadCurrentGameResult();
        }
    }

    public void ShowNextButton()
    {
        if (nextButton != null) nextButton.SetActive(true);
    }
    public void OnNextButtonClicked()
    {
        if (nextButton != null) nextButton.SetActive(false);

        isGameActive = false;
        currentPhase++;

        if (currentPhase == 2)
        {
            StartCoroutine(GoToGame2());
        }
        else if (currentPhase == 3)
        {
            StartCoroutine(GoToGame3());
        }
        else if (currentPhase == 4)
        {
            StartCoroutine(GoToGame4());
        }
    }

    IEnumerator GoToGame2()
    {

        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.FinishCurrentIndicator();
        }

        float timer = 0;
        if (overlayCG != null)
        {
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                overlayCG.alpha = Mathf.Lerp(gameplayOverlayAlpha, 0f, timer / 0.5f);
                yield return null;
            }
        }

        timer = 0;
        while (timer < 1f)
        {
            timer += Time.deltaTime;
            if (game1UI_CG != null) game1UI_CG.alpha = 1f - (timer / 1f);
            yield return null;
        }

        if (game1Elements != null) game1Elements.SetActive(false);
        if (floorJarsGroup != null) floorJarsGroup.SetActive(false);

        if (shelfJarsFake != null)
        {
            shelfJarsFake.SetActive(true);
            CanvasGroup cg = shelfJarsFake.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }
        if (lubnaGame2 != null)
        {
            lubnaGame2.SetActive(true);
            if (lubnaAudio2 != null) lubnaAudio2.Play();

            float waitTime = (lubnaAudio2 != null && lubnaAudio2.clip != null) ? lubnaAudio2.clip.length : 3f;
            yield return new WaitForSeconds(waitTime);

            if (lubnaAnimator2 != null) lubnaAnimator2.speed = 0f;
            lubnaGame2.SetActive(false);
        }

        Vector2 currentPos = environmentPanel.anchoredPosition;
        Vector3 currentScale = environmentPanel.localScale;
        Vector2 targetPos = currentPos * zoomScale;

        timer = 0;
        while (timer < zoomDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, timer / zoomDuration);
            environmentPanel.localScale = Vector3.Lerp(currentScale, new Vector3(zoomScale, zoomScale, 1f), progress);
            environmentPanel.anchoredPosition = Vector2.Lerp(currentPos, targetPos, progress);
            yield return null;
        }

        timer = 0;
        if (overlayCG != null)
        {
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                overlayCG.alpha = Mathf.Lerp(0f, gameplayOverlayAlpha, timer / 1f);
                yield return null;
            }
        }

        if (game2Elements != null) game2Elements.SetActive(true);

        StartBackendTracking("مطابقة العناصر البصرية وفق الشكل");


        if (Game2Spawner.Instance != null) Game2Spawner.Instance.LoadLevel(0);
    }

    IEnumerator GoToGame3()
    {

        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.FinishCurrentIndicator();
        }

        float timer = 0;
        if (overlayCG != null)
        {
            while (timer < 0.5f)
            {
                timer += Time.deltaTime;
                overlayCG.alpha = Mathf.Lerp(gameplayOverlayAlpha, 0f, timer / 0.5f);
                yield return null;
            }
        }

        if (game2Elements != null) game2Elements.SetActive(false);

        Vector2 currentPos = environmentPanel.anchoredPosition;
        Vector3 currentScale = environmentPanel.localScale;
        Vector2 targetPos = currentPos / zoomScale;

        timer = 0;
        while (timer < zoomDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, timer / zoomDuration);
            environmentPanel.localScale = Vector3.Lerp(currentScale, Vector3.one, progress);
            environmentPanel.anchoredPosition = Vector2.Lerp(currentPos, targetPos, progress);
            yield return null;
        }

        if (lubnaGame3 != null)
        {
            lubnaGame3.SetActive(true);
            if (lubnaAudio3 != null) lubnaAudio3.Play();

            float waitTime = (lubnaAudio3 != null && lubnaAudio3.clip != null) ? lubnaAudio3.clip.length : 3f;
            yield return new WaitForSeconds(waitTime);

            if (lubnaAnimator3 != null) lubnaAnimator3.speed = 0f;
            lubnaGame3.SetActive(false);
        }

        if (game3Elements != null) game3Elements.SetActive(true);

        if (animatedCandyBoxRT != null)
        {
            Vector2 startPos = animatedCandyBoxRT.anchoredPosition;
            float autoCenterX = (environmentPanel != null) ? -environmentPanel.anchoredPosition.x : 0f;
            Vector2 dynamicTargetPos = new Vector2(autoCenterX, boxCenterPos.y);

            if (boxClosedImg != null) boxClosedImg.SetActive(true);
            if (boxOpenImg != null) boxOpenImg.SetActive(false);
            if (finalCandiesCG != null) finalCandiesCG.alpha = 0;
            if (lightGlowCG != null) lightGlowCG.alpha = 0;
            if (game3OddOneOutCG != null) game3OddOneOutCG.alpha = 1;

            timer = 0;
            Vector3 startScale = animatedCandyBoxRT.localScale;
            Vector3 endScale = new Vector3(boxEndScale, boxEndScale, 1f);

            while (timer < boxMoveDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.SmoothStep(0, 1, timer / boxMoveDuration);
                animatedCandyBoxRT.anchoredPosition = Vector2.Lerp(startPos, dynamicTargetPos, progress);
                animatedCandyBoxRT.localScale = Vector3.Lerp(startScale, endScale, progress);

                if (overlayCG != null) overlayCG.alpha = progress * backgroundDarknessTarget;
                yield return null;
            }

            animatedCandyBoxRT.anchoredPosition = dynamicTargetPos;
            animatedCandyBoxRT.localScale = endScale;
            if (overlayCG != null) overlayCG.alpha = backgroundDarknessTarget;

            if (boxClosedImg != null) boxClosedImg.SetActive(false);
            if (boxOpenImg != null) boxOpenImg.SetActive(true);

            timer = 0;
            if (lightGlowCG != null)
            {
                while (timer < 0.5f)
                {
                    timer += Time.deltaTime;
                    lightGlowCG.alpha = timer / 0.5f;
                    yield return null;
                }
            }

            timer = 0;
            if (finalCandiesCG != null)
            {
                RectTransform candiesRT = finalCandiesCG.GetComponent<RectTransform>();
                Vector2 finalCandiesPos = candiesRT.anchoredPosition;
                Vector2 startCandiesPos = finalCandiesPos + candiesStartOffset;

                candiesRT.anchoredPosition = startCandiesPos;
                candiesRT.localScale = Vector3.zero;

                while (timer < popOutDuration)
                {
                    timer += Time.deltaTime;
                    float progress = timer / popOutDuration;
                    float popProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);

                    finalCandiesCG.alpha = progress;
                    candiesRT.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, popProgress);
                    candiesRT.anchoredPosition = Vector2.Lerp(startCandiesPos, finalCandiesPos, popProgress);
                    yield return null;
                }

                finalCandiesCG.alpha = 1;
                candiesRT.localScale = Vector3.one;
                candiesRT.anchoredPosition = finalCandiesPos;
                if (game3OddOneOutCG != null) game3OddOneOutCG.blocksRaycasts = true;
            }
        }

        StartBackendTracking("إكمال الأنماط البصرية");

    }

    IEnumerator GoToGame4()
    {
        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.FinishCurrentIndicator();
        }

        if (MasterManager.Instance != null)
        {
            MasterManager.Instance.currentStageIndex = 1;
        }

        if (animatedCandyBoxRT != null)
        {
            if (finalCandiesCG != null) finalCandiesCG.alpha = 0f;
            if (game3OddOneOutCG != null) game3OddOneOutCG.alpha = 0f;

            if (boxOpenImg != null) boxOpenImg.SetActive(false);
            if (boxClosedImg != null) boxClosedImg.SetActive(true);

            float timer = 0;
            Vector2 currentBoxPos = animatedCandyBoxRT.anchoredPosition;
            Vector3 currentBoxScale = animatedCandyBoxRT.localScale;

            while (timer < 1f)
            {
                timer += Time.deltaTime;
                float progress = Mathf.SmoothStep(0, 1, timer / 1f);
                animatedCandyBoxRT.anchoredPosition = Vector2.Lerp(currentBoxPos, boxOriginalPos, progress);
                animatedCandyBoxRT.localScale = Vector3.Lerp(currentBoxScale, Vector3.one, progress);

                if (overlayCG != null) overlayCG.alpha = Mathf.Lerp(backgroundDarknessTarget, 0f, progress);

                yield return null;
            }
        }

        if (game3Elements != null) game3Elements.SetActive(false);

        Vector2 startEnvPos = environmentPanel.anchoredPosition;
        Vector2 endEnvPos = startEnvPos - new Vector2(game4PanDistance, 0);

        float panTimer = 0;
        while (panTimer < panDuration)
        {
            panTimer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, panTimer / panDuration);
            environmentPanel.anchoredPosition = Vector2.Lerp(startEnvPos, endEnvPos, progress);
            yield return null;
        }
        environmentPanel.anchoredPosition = endEnvPos;

        if (lubnaGame4 != null)
        {
            lubnaGame4.SetActive(true);
            if (lubnaAudio4 != null) lubnaAudio4.Stop();
            if (lubnaAnimator4 != null) lubnaAnimator4.speed = 0f;
        }

        if (blueTableRT != null)
        {
            blueTableRT.gameObject.SetActive(true);
            Vector2 finalTablePos = blueTableRT.anchoredPosition;
            Vector2 startTablePos = new Vector2(finalTablePos.x, finalTablePos.y - 1500f);
            blueTableRT.anchoredPosition = startTablePos;

            float tableTimer = 0;
            while (tableTimer < tableMoveDuration)
            {
                tableTimer += Time.deltaTime;
                float progress = Mathf.SmoothStep(0, 1, tableTimer / tableMoveDuration);
                blueTableRT.anchoredPosition = Vector2.Lerp(startTablePos, finalTablePos, progress);
                yield return null;
            }
            blueTableRT.anchoredPosition = finalTablePos;
        }

        if (lubnaGame4 != null)
        {
            if (lubnaAnimator4 != null) lubnaAnimator4.speed = 1f;
            if (lubnaAudio4 != null) lubnaAudio4.Play();

            float waitTime = (lubnaAudio4 != null && lubnaAudio4.clip != null) ? lubnaAudio4.clip.length : 3f;
            yield return new WaitForSeconds(waitTime);

            if (lubnaAnimator4 != null) lubnaAnimator4.speed = 0f;
        }

        float overlayFadeTimer = 0;
        if (overlayCG != null)
        {
            while (overlayFadeTimer < 1f)
            {
                overlayFadeTimer += Time.deltaTime;
                overlayCG.alpha = Mathf.Lerp(0f, gameplayOverlayAlpha, overlayFadeTimer / 1f);
                yield return null;
            }
        }

        if (game4Elements != null) game4Elements.SetActive(true);

    }
}