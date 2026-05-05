using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelCompleteManager : MonoBehaviour
{
    public static LevelCompleteManager Instance;

    public CanvasGroup completePanelCG;
    public Image progressBarFill;
    public GameObject nextButton;

    [Header("Scene Transition")]
    public string nextSceneName = "Game02Scene";

    [Header("Nodes (Circles)")]
    public Image[] nodeGlows;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successVoiceClip;
    public AudioClip barFillSoundEffect;
    public AudioClip nodePopSound;

    public float panelFadeDuration = 0.5f;
    public float barFillDuration = 1.5f; // الوقت الإجمالي للتعبئة

    private AudioSource loopAudioSource;
    private int totalNodes;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        loopAudioSource = gameObject.AddComponent<AudioSource>();
        loopAudioSource.playOnAwake = false;

        totalNodes = nodeGlows.Length; // يحسب النودز تلقائياً (4 نودز)

        if (completePanelCG != null)
        {
            completePanelCG.alpha = 0f;
            completePanelCG.blocksRaycasts = false;
            completePanelCG.interactable = false;
        }

        if (nextButton != null) nextButton.SetActive(false);

        foreach (var glow in nodeGlows)
        {
            if (glow != null) glow.color = new Color(1, 1, 1, 0);
        }
    }

    // حافظت على نفس شكل الدالة عشان ما يخرب السكربت الثاني اللي يستدعيها
    public void ShowCompleteScreen(int levelNumber = 0)
    {
        StartCoroutine(CompleteScreenRoutine());
    }

    IEnumerator CompleteScreenRoutine()
    {
        if (progressBarFill != null) progressBarFill.fillAmount = 0f;

        // 1. إظهار الشاشة (Fade In)
        if (completePanelCG != null)
        {
            completePanelCG.blocksRaycasts = true;
            completePanelCG.interactable = true;

            float timer = 0f;
            while (timer < panelFadeDuration)
            {
                timer += Time.deltaTime;
                completePanelCG.alpha = Mathf.Lerp(0f, 1f, timer / panelFadeDuration);
                yield return null;
            }
            completePanelCG.alpha = 1f;
        }

        // 2. تشغيل صوت تعبئة البار
        if (loopAudioSource != null && barFillSoundEffect != null)
        {
            loopAudioSource.clip = barFillSoundEffect;
            loopAudioSource.loop = true;
            loopAudioSource.Play();
        }

        // 3. تقسيم وقت التعبئة على عدد النودز (عشان يمشي ويوقف)
        float segmentDuration = barFillDuration / totalNodes;

        for (int i = 0; i < totalNodes; i++)
        {
            // حساب بداية ونهاية كل جزء
            float currentSegmentStart = (float)i / totalNodes;
            float targetSegmentFill = (float)(i + 1) / totalNodes;

            float timer = 0f;
            while (timer < segmentDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, timer / segmentDuration);
                if (progressBarFill != null)
                    progressBarFill.fillAmount = Mathf.Lerp(currentSegmentStart, targetSegmentFill, progress);

                yield return null;
            }

            if (progressBarFill != null) progressBarFill.fillAmount = targetSegmentFill;

            // === هنا السر: نوقف صوت التعبئة مؤقتاً عشان نسمع صوت النود ===
            if (loopAudioSource != null) loopAudioSource.Pause();

            // === ننتظر النود الحالية تنتفخ وتخلص حركتها قبل ما يكمل البار ===
            yield return StartCoroutine(AnimateNode(nodeGlows[i]));

            // نرجع نشغل صوت التعبئة للجزء اللي بعده (إلا لو كانت آخر نود)
            if (loopAudioSource != null && i < totalNodes - 1) loopAudioSource.UnPause();
        }

        if (loopAudioSource != null) loopAudioSource.Stop();

        // 4. تشغيل صوت النجاح النهائي بعد ما خلصنا كل النودز
        if (audioSource != null && successVoiceClip != null)
        {
            audioSource.PlayOneShot(successVoiceClip);
        }

        // 5. إظهار زر التالي (Next Button)
        if (nextButton != null)
        {
            nextButton.SetActive(true);
            Vector3 startScale = Vector3.zero;
            Vector3 endScale = Vector3.one;
            float btnTimer = 0f;

            while (btnTimer < 0.3f)
            {
                btnTimer += Time.deltaTime;
                float popProgress = Mathf.Sin((btnTimer / 0.3f) * Mathf.PI * 0.5f);
                nextButton.transform.localScale = Vector3.Lerp(startScale, endScale, popProgress);
                yield return null;
            }
            nextButton.transform.localScale = endScale;
        }
    }

    IEnumerator AnimateNode(Image glowImage)
    {
        if (audioSource != null && nodePopSound != null) audioSource.PlayOneShot(nodePopSound);

        glowImage.color = new Color(1, 1, 1, 1);

        Transform nodeTransform = glowImage.transform.parent;
        Vector3 startScale = Vector3.one;
        Vector3 peakScale = new Vector3(1.4f, 1.4f, 1f);

        float popTimer = 0;
        while (popTimer < 0.15f)
        {
            popTimer += Time.deltaTime;
            nodeTransform.localScale = Vector3.Lerp(startScale, peakScale, popTimer / 0.15f);
            yield return null;
        }
        popTimer = 0;
        while (popTimer < 0.15f)
        {
            popTimer += Time.deltaTime;
            nodeTransform.localScale = Vector3.Lerp(peakScale, startScale, popTimer / 0.15f);
            yield return null;
        }
        nodeTransform.localScale = startScale;
    }

    public void LoadNextLevelScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}