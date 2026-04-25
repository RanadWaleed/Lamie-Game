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
    public float barFillDuration = 1.5f;

    private float currentFill = 0f;
    private AudioSource loopAudioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        loopAudioSource = gameObject.AddComponent<AudioSource>();
        loopAudioSource.playOnAwake = false;

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

    public void ShowCompleteScreen(int levelNumber)
    {
        StartCoroutine(CompleteScreenRoutine(levelNumber));
    }

    IEnumerator CompleteScreenRoutine(int levelNumber)
    {
        float startFill = (levelNumber - 1) / 3f;
        float targetFill = levelNumber / 3f;
        currentFill = startFill;

        if (progressBarFill != null) progressBarFill.fillAmount = startFill;

        for (int i = 0; i < levelNumber - 1; i++)
        {
            if (i < nodeGlows.Length && nodeGlows[i] != null)
            {
                nodeGlows[i].color = new Color(1, 1, 1, 1);
            }
        }

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

        if (audioSource != null && successVoiceClip != null)
            audioSource.PlayOneShot(successVoiceClip);

        if (loopAudioSource != null && barFillSoundEffect != null)
        {
            loopAudioSource.clip = barFillSoundEffect;
            loopAudioSource.loop = true;
            loopAudioSource.Play();
        }

        float fillTimer = 0f;

        while (fillTimer < barFillDuration)
        {
            fillTimer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, fillTimer / barFillDuration);
            currentFill = Mathf.Lerp(startFill, targetFill, progress);

            if (progressBarFill != null) progressBarFill.fillAmount = currentFill;

            CheckAndAnimateNodes(currentFill);
            yield return null;
        }

        if (progressBarFill != null) progressBarFill.fillAmount = targetFill;
        currentFill = targetFill;
        CheckAndAnimateNodes(currentFill);

        if (loopAudioSource != null)
        {
            loopAudioSource.loop = false;
            loopAudioSource.Stop();
        }

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

    void CheckAndAnimateNodes(float fill)
    {
        if (fill >= 0.3f && nodeGlows.Length > 0 && nodeGlows[0].color.a == 0) StartCoroutine(AnimateNode(nodeGlows[0]));
        if (fill >= 0.65f && nodeGlows.Length > 1 && nodeGlows[1].color.a == 0) StartCoroutine(AnimateNode(nodeGlows[1]));
        if (fill >= 0.99f && nodeGlows.Length > 2 && nodeGlows[2].color.a == 0) StartCoroutine(AnimateNode(nodeGlows[2]));
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