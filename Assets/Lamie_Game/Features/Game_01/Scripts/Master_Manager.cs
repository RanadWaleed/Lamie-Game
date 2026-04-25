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

    [Header("Lubna Cinematic Swap")]
    public AudioSource mahmoodIntroAudio;
    public float introPanDistance = 1500f;
    public float introPanDuration = 2f;
    public GameObject lubnaWalkerObj;
    public Animator lubnaIntroAnimator;
    public Transform lubnaIntroRT;
    public float lubnaWalkDistance = 500f;
    public GameObject lubnaTalkerObj;
    public AudioSource lubnaMouthAudio;
    public AudioClip lubnaFirstClip;
    public AudioClip lubnaSecondClip;

    [Header("Game 1 Zoom Settings")]
    public float game1ZoomScale = 1.15f;
    public float game1ZoomDuration = 1.2f;
    private Vector2 postIntroEnvPos;

    [Header("Game 2 Transition Settings")]
    public RectTransform game2BaseRT;
    public Vector2 game2BaseTargetPos = new Vector2(0f, -200f);
    public float game2BaseScaleMultiplier = 1.2f;
    public float game2BaseSlideDuration = 1.5f;
    private Vector2 game2BaseOriginalPos;
    private Vector3 game2BaseOriginalScale;

    [Header("Game 3 Transition Settings")]
    public float game3PanDistance = 1500f;
    public float game3PanDuration = 2f;
    public float lubnaWalkDistanceGame3 = 500f;
    public RectTransform boxTargetRT;

    [Header("Game 4 Transition Settings")]
    public float lubnaWalkDistanceGame4 = 800f;

    [Header("Original UI Variables")]
    public GameObject lubnaChefHat;
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

    public CanvasGroup overlayCG;
    public float gameplayOverlayAlpha = 0.6f;

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
    public AudioClip game2Clip;
    public AudioClip game3Clip;
    public AudioClip game4Clip;

    private Vector2 boxOriginalPos;
    private Vector2 tableOriginalPos;

    public GameObject shelfJarsFake;
    public GameObject floorJarsGroup;

    [Header("SFX & Titles")]
    public AudioSource sfxSource;
    public AudioClip dropSound;
    public AudioClip clickSound;
    public AudioClip successSound;
    public GameObject game2TitlePanel;
    public GameObject game3TitlePanel;
    public GameObject game4TitlePanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
        }
    }

    void Start()
    {
        if (lubnaChefHat != null) lubnaChefHat.SetActive(false);
        if (animatedCandyBoxRT != null) boxOriginalPos = animatedCandyBoxRT.anchoredPosition;
        if (blueTableRT != null) tableOriginalPos = blueTableRT.anchoredPosition;

        if (game2BaseRT != null)
        {
            game2BaseOriginalPos = game2BaseRT.anchoredPosition;
            game2BaseOriginalScale = game2BaseRT.localScale;
        }

        if (nextButton != null) nextButton.SetActive(false);

        if (game1Elements != null) game1Elements.SetActive(false);
        if (game2Elements != null) game2Elements.SetActive(false);
        if (game3Elements != null) game3Elements.SetActive(false);
        if (game4Elements != null) game4Elements.SetActive(false);

        if (blueTableRT != null) blueTableRT.gameObject.SetActive(false);
        if (overlayCG != null) overlayCG.alpha = 0f;
        if (game3OddOneOutCG != null) game3OddOneOutCG.blocksRaycasts = false;
        if (lightGlowCG != null) lightGlowCG.alpha = 0f;

        if (game3TitlePanel != null) game3TitlePanel.SetActive(false);
        if (game4TitlePanel != null) game4TitlePanel.SetActive(false);

        if (PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.SetupNewAspect("التصور", "Game_1");
        }

        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        if (isGameActive) timeTaken += Time.deltaTime;
    }

    public void PlayDropSound()
    {
        if (sfxSource != null && dropSound != null) sfxSource.PlayOneShot(dropSound);
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null) sfxSource.PlayOneShot(clickSound);
    }

    public void PlaySuccessSound()
    {
        if (sfxSource != null && successSound != null) sfxSource.PlayOneShot(successSound);
    }

    IEnumerator IntroSequence()
    {
        if (lubnaWalkerObj != null) lubnaWalkerObj.SetActive(false);
        if (lubnaTalkerObj != null) lubnaTalkerObj.SetActive(true);

        if (mahmoodIntroAudio != null)
        {
            mahmoodIntroAudio.Play();
            yield return new WaitForSeconds(mahmoodIntroAudio.clip.length);
        }

        if (lubnaMouthAudio != null && lubnaFirstClip != null)
        {
            lubnaMouthAudio.clip = lubnaFirstClip;
            lubnaMouthAudio.Play();
            yield return new WaitForSeconds(lubnaFirstClip.length);
        }

        if (lubnaTalkerObj != null) lubnaTalkerObj.SetActive(false);
        if (lubnaWalkerObj != null)
        {
            lubnaWalkerObj.SetActive(true);
            if (lubnaTalkerObj != null)
                lubnaWalkerObj.transform.localPosition = lubnaTalkerObj.transform.localPosition;
        }

        bool shouldWalk = lubnaWalkDistance > 0.1f;
        if (lubnaIntroAnimator != null) lubnaIntroAnimator.SetBool("isWalking", shouldWalk);

        Vector2 screenStart = environmentPanel.anchoredPosition;
        Vector2 screenTarget = screenStart - new Vector2(introPanDistance, 0f);
        Vector3 lubnaStart = (lubnaWalkerObj != null) ? lubnaWalkerObj.transform.localPosition : Vector3.zero;
        Vector3 lubnaTarget = lubnaStart + new Vector3(lubnaWalkDistance, 0f, 0f);

        float timer = 0f;
        while (timer < introPanDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, timer / introPanDuration);
            environmentPanel.anchoredPosition = Vector2.Lerp(screenStart, screenTarget, progress);

            if (shouldWalk && lubnaWalkerObj != null)
                lubnaWalkerObj.transform.localPosition = Vector3.Lerp(lubnaStart, lubnaTarget, progress);

            yield return null;
        }

        environmentPanel.anchoredPosition = screenTarget;
        postIntroEnvPos = screenTarget;

        if (lubnaIntroAnimator != null) lubnaIntroAnimator.SetBool("isWalking", false);

        if (lubnaWalkerObj != null) lubnaWalkerObj.SetActive(false);
        if (lubnaTalkerObj != null)
        {
            lubnaTalkerObj.SetActive(true);
            if (lubnaWalkerObj != null)
                lubnaTalkerObj.transform.localPosition = lubnaWalkerObj.transform.localPosition;
        }

        if (lubnaMouthAudio != null && lubnaSecondClip != null)
        {
            lubnaMouthAudio.clip = lubnaSecondClip;
            lubnaMouthAudio.Play();
            yield return new WaitForSeconds(lubnaSecondClip.length);
        }

        float zoomTimer = 0f;
        Vector3 initialScale = environmentPanel.localScale;
        Vector3 targetZoomScale = new Vector3(game1ZoomScale, game1ZoomScale, 1f);
        Vector2 initialPos = environmentPanel.anchoredPosition;
        Vector2 targetZoomPos = initialPos * game1ZoomScale;

        while (zoomTimer < game1ZoomDuration)
        {
            zoomTimer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, zoomTimer / game1ZoomDuration);
            if (environmentPanel != null)
            {
                environmentPanel.localScale = Vector3.Lerp(initialScale, targetZoomScale, progress);
                environmentPanel.anchoredPosition = Vector2.Lerp(initialPos, targetZoomPos, progress);
            }
            if (overlayCG != null) overlayCG.alpha = Mathf.Lerp(0f, gameplayOverlayAlpha, progress);
            yield return null;
        }

        if (game1Elements != null) game1Elements.SetActive(true);
        StartBackendTracking("مطابقة العناصر البصرية وفق اللون");
    }

    public void StartBackendTracking(string gameName)
    {
        currentIndicatorName = gameName;
        currentStageIndex = 1;
        timeTaken = 0f;
        Score = 0;
        Attempts = 0;
        scoreFirstAttempt = 0;
        isGameActive = false;

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
        isGameActive = false;
    }

    public void StartTimer()
    {
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
        if (isGameActive && PsychometricReportManager.Instance != null)
        {
            PsychometricReportManager.Instance.FinishCurrentIndicator();
            PsychometricReportManager.Instance.UploadCurrentGameResult();
        }
    }

    public void ShowNextButton()
    {
        if (currentPhase == 4)
        {
            if (LevelCompleteManager.Instance != null)
            {
                LevelCompleteManager.Instance.ShowCompleteScreen(1); 
            }
        }
        else
        {
            if (nextButton != null) nextButton.SetActive(true);
        }
    }

    public void OnNextButtonClicked()
    {
        if (nextButton != null) nextButton.SetActive(false);
        isGameActive = false;
        currentPhase++;

        if (currentPhase == 2) StartCoroutine(GoToGame2());
        else if (currentPhase == 3) StartCoroutine(GoToGame3());
        else if (currentPhase == 4) StartCoroutine(GoToGame4());
    }

    IEnumerator GoToGame2()
    {
        if (PsychometricReportManager.Instance != null)
            PsychometricReportManager.Instance.FinishCurrentIndicator();

        if (game1Elements != null) game1Elements.SetActive(false);
        if (floorJarsGroup != null) floorJarsGroup.SetActive(false);

        if (shelfJarsFake != null)
        {
            shelfJarsFake.SetActive(true);
            CanvasGroup cg = shelfJarsFake.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }

        if (game2TitlePanel != null) game2TitlePanel.SetActive(true);
        if (lubnaTalkerObj != null) lubnaTalkerObj.SetActive(true);

        if (lubnaMouthAudio != null && game2Clip != null)
        {
            lubnaMouthAudio.clip = game2Clip;
            lubnaMouthAudio.Play();
            yield return new WaitForSeconds(game2Clip.length);
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

        if (game2BaseRT != null)
        {
            game2BaseRT.gameObject.SetActive(true);
            game2BaseRT.SetAsLastSibling();
            timer = 0;

            Vector2 stayInPlacePos = game2BaseRT.anchoredPosition;
            Vector3 startScale = game2BaseRT.localScale;
            Vector3 targetScale = game2BaseOriginalScale * game2BaseScaleMultiplier;

            while (timer < game2BaseSlideDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.SmoothStep(0, 1, timer / game2BaseSlideDuration);
                game2BaseRT.anchoredPosition = stayInPlacePos;
                game2BaseRT.localScale = Vector3.Lerp(startScale, targetScale, progress);
                yield return null;
            }
            game2BaseRT.anchoredPosition = stayInPlacePos;
            game2BaseRT.localScale = targetScale;
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
            PsychometricReportManager.Instance.FinishCurrentIndicator();

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
        if (game2TitlePanel != null) game2TitlePanel.SetActive(false);

        if (game2BaseRT != null)
        {
            game2BaseRT.anchoredPosition = game2BaseOriginalPos;
            game2BaseRT.localScale = game2BaseOriginalScale;
            game2BaseRT.gameObject.SetActive(true);
        }

        if (lubnaTalkerObj != null) lubnaTalkerObj.SetActive(false);
        if (lubnaWalkerObj != null)
        {
            lubnaWalkerObj.SetActive(true);
            if (lubnaTalkerObj != null)
                lubnaWalkerObj.transform.localPosition = lubnaTalkerObj.transform.localPosition;
        }

        bool shouldWalk = lubnaWalkDistanceGame3 > 0.1f;
        if (lubnaIntroAnimator != null) lubnaIntroAnimator.SetBool("isWalking", shouldWalk);

        Vector2 screenStart = environmentPanel.anchoredPosition;
        Vector2 screenTarget = screenStart - new Vector2(game3PanDistance, 0f);
        Vector3 lubnaStart = (lubnaWalkerObj != null) ? lubnaWalkerObj.transform.localPosition : Vector3.zero;
        Vector3 lubnaTarget = lubnaStart + new Vector3(lubnaWalkDistanceGame3, 0f, 0f);
        timer = 0f;
        while (timer < game3PanDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, timer / game3PanDuration);
            environmentPanel.anchoredPosition = Vector2.Lerp(screenStart, screenTarget, progress);

            if (shouldWalk && lubnaWalkerObj != null)
                lubnaWalkerObj.transform.localPosition = Vector3.Lerp(lubnaStart, lubnaTarget, progress);

            yield return null;
        }

        if (lubnaIntroAnimator != null) lubnaIntroAnimator.SetBool("isWalking", false);
        if (lubnaWalkerObj != null) lubnaWalkerObj.SetActive(false);

        if (lubnaTalkerObj != null)
        {
            lubnaTalkerObj.SetActive(true);
            if (lubnaWalkerObj != null)
                lubnaTalkerObj.transform.localPosition = lubnaWalkerObj.transform.localPosition;
        }

        if (lubnaMouthAudio != null && game3Clip != null)
        {
            lubnaMouthAudio.clip = game3Clip;
            lubnaMouthAudio.Play();
            yield return new WaitForSeconds(game3Clip.length);
        }

        if (animatedCandyBoxRT != null && boxTargetRT != null)
        {
            if (animatedCandyBoxRT.GetComponent<Animator>() != null)
            {
                Destroy(animatedCandyBoxRT.GetComponent<Animator>());
            }

            Vector3 startBoxPos = animatedCandyBoxRT.localPosition;
            Vector3 targetBoxPos = boxTargetRT.localPosition;

            Vector3 startBoxScale = animatedCandyBoxRT.localScale;
            Vector3 targetBoxScale = new Vector3(boxEndScale, boxEndScale, 1f);

            timer = 0;
            while (timer < boxMoveDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, timer / boxMoveDuration);
                animatedCandyBoxRT.localPosition = Vector3.Lerp(startBoxPos, targetBoxPos, progress);
                animatedCandyBoxRT.localScale = Vector3.Lerp(startBoxScale, targetBoxScale, progress);
                yield return null;
            }
            animatedCandyBoxRT.localPosition = targetBoxPos;
            animatedCandyBoxRT.localScale = targetBoxScale;

            if (boxClosedImg != null) boxClosedImg.SetActive(false);
            if (boxOpenImg != null) boxOpenImg.SetActive(true);
        }

        if (game3TitlePanel != null) game3TitlePanel.SetActive(true);

        timer = 0;
        if (overlayCG != null)
        {
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                overlayCG.alpha = Mathf.Lerp(0f, backgroundDarknessTarget, timer / 1f);
                yield return null;
            }
        }

        if (game3Elements != null) game3Elements.SetActive(true);

        if (game3OddOneOutCG != null)
        {
            game3OddOneOutCG.alpha = 1f;
            game3OddOneOutCG.blocksRaycasts = true;
            game3OddOneOutCG.interactable = true;
        }

        if (GetComponent<Game3Manager>() != null) GetComponent<Game3Manager>().LoadSet(0);
        StartBackendTracking("إكمال الأنماط البصرية");
    }

    IEnumerator GoToGame4()
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
                overlayCG.alpha = Mathf.Lerp(backgroundDarknessTarget, 0f, timer / 0.5f);
                yield return null;
            }
        }

        if (game3Elements != null) game3Elements.SetActive(false);
        if (game3TitlePanel != null) game3TitlePanel.SetActive(false);
        if (game3OddOneOutCG != null) game3OddOneOutCG.interactable = false;

        if (animatedCandyBoxRT != null)
        {
            if (boxOpenImg != null) boxOpenImg.SetActive(false);
            if (boxClosedImg != null) boxClosedImg.SetActive(true);

            Vector2 currentBoxPos = animatedCandyBoxRT.anchoredPosition;
            Vector3 currentBoxScale = animatedCandyBoxRT.localScale;

            timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                float progress = Mathf.SmoothStep(0, 1, timer / 1f);
                animatedCandyBoxRT.anchoredPosition = Vector2.Lerp(currentBoxPos, boxOriginalPos, progress);
                animatedCandyBoxRT.localScale = Vector3.Lerp(currentBoxScale, Vector3.one, progress);
                yield return null;
            }
        }

        if (lubnaTalkerObj != null) lubnaTalkerObj.SetActive(false);
        if (lubnaWalkerObj != null)
        {
            lubnaWalkerObj.SetActive(true);
            if (lubnaTalkerObj != null)
                lubnaWalkerObj.transform.localPosition = lubnaTalkerObj.transform.localPosition;
        }
        if (lubnaChefHat != null) lubnaChefHat.SetActive(true);
        if (lubnaIntroAnimator != null) lubnaIntroAnimator.SetBool("isWalking", true);

        Vector2 startEnvPos = environmentPanel.anchoredPosition;
        Vector2 endEnvPos = startEnvPos - new Vector2(game4PanDistance, 0);

        Vector3 lubnaStart = (lubnaWalkerObj != null) ? lubnaWalkerObj.transform.localPosition : Vector3.zero;
        Vector3 lubnaTarget = lubnaStart + new Vector3(lubnaWalkDistanceGame4, 0f, 0f);

        float panTimer = 0;
        while (panTimer < panDuration)
        {
            panTimer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, panTimer / panDuration);

            environmentPanel.anchoredPosition = Vector2.Lerp(startEnvPos, endEnvPos, progress);

            if (lubnaWalkerObj != null)
                lubnaWalkerObj.transform.localPosition = Vector3.Lerp(lubnaStart, lubnaTarget, progress);

            yield return null;
        }
        environmentPanel.anchoredPosition = endEnvPos;

        if (lubnaIntroAnimator != null) lubnaIntroAnimator.SetBool("isWalking", false);
        if (lubnaWalkerObj != null) lubnaWalkerObj.SetActive(false);

        if (lubnaTalkerObj != null)
        {
            lubnaTalkerObj.SetActive(true);
            if (lubnaWalkerObj != null)
                lubnaTalkerObj.transform.localPosition = lubnaWalkerObj.transform.localPosition;
        }

        if (lubnaMouthAudio != null && game4Clip != null)
        {
            lubnaMouthAudio.clip = game4Clip;
            lubnaMouthAudio.Play();
            yield return new WaitForSeconds(game4Clip.length);
        }

        if (blueTableRT != null)
        {
            blueTableRT.gameObject.SetActive(true);
            Vector2 finalTablePos = tableOriginalPos;
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

        if (game4TitlePanel != null) game4TitlePanel.SetActive(true);

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