using UnityEngine;
using System.Collections;

public class StoryTellingManager : MonoBehaviour
{
    [Header("Flow Management")]
    public GameObject storyContent;
    public TaskManager taskManager;

    [Header("Main UI")]
    public RectTransform sceneContainer;
    public CanvasGroup storyCanvas;

    [Header("Curtains")]
    public RectTransform leftCurtain;
    public RectTransform rightCurtain;
    public RectTransform topCurtain;

    [Header("Clouds")]
    public RectTransform topCloud;
    public RectTransform bottomCloud;

    [Header("Bulbs")]
    public RectTransform[] bulbs;

    [Header("Audio & Character")]
    public AudioSource storyVoice;
    public Animator characterAnimator;
    public CanvasGroup character;
    public CanvasGroup lights;
    public CanvasGroup glow;

    [Header("Navigation")]
    public CanvasGroup navButtons;

    float curtainDistance = 1600f;
    Vector2 leftTarget, rightTarget, topTarget, topCloudTarget, bottomCloudTarget;
    Vector2[] bulbTargets;

    void Awake()
    {
        SaveOriginalPositions();
        navButtons.alpha = 0;
        navButtons.interactable = false;
        navButtons.blocksRaycasts = false;
        if (characterAnimator != null) characterAnimator.enabled = false;
    }

    void SaveOriginalPositions()
    {
        leftTarget = leftCurtain.anchoredPosition;
        rightTarget = rightCurtain.anchoredPosition;
        topTarget = topCurtain.anchoredPosition;
        topCloudTarget = topCloud.anchoredPosition;
        bottomCloudTarget = bottomCloud.anchoredPosition;

        bulbTargets = new Vector2[bulbs.Length];
        for (int i = 0; i < bulbs.Length; i++)
        {
            if (bulbs[i] != null)
                bulbTargets[i] = bulbs[i].anchoredPosition;
        }
    }

    public void SetupAndStart()
    {
        StopAllCoroutines();
        navButtons.alpha = 0;
        storyCanvas.interactable = true;
        storyCanvas.blocksRaycasts = true;
        if (storyVoice != null) storyVoice.Stop();
        if (characterAnimator != null) characterAnimator.enabled = false;

        leftCurtain.anchoredPosition = leftTarget + new Vector2(-curtainDistance, 0);
        rightCurtain.anchoredPosition = rightTarget + new Vector2(curtainDistance, 0);
        topCurtain.anchoredPosition = topTarget + new Vector2(0, 1000);
        topCloud.anchoredPosition = topCloudTarget + new Vector2(0, 800);
        bottomCloud.anchoredPosition = bottomCloudTarget + new Vector2(0, -800);

        for (int i = 0; i < bulbs.Length; i++)
        {
            if (bulbs[i] != null)
                bulbs[i].anchoredPosition = bulbTargets[i] + new Vector2(0, 1000);
        }

        lights.alpha = 0;
        glow.alpha = 0;
        character.alpha = 0;
        storyCanvas.alpha = 1;
        sceneContainer.localScale = new Vector3(1.5f, 1.5f, 1);
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        StartCoroutine(ZoomOut());
        StartCoroutine(Move(leftCurtain, leftTarget, 1.2f));
        StartCoroutine(Move(rightCurtain, rightTarget, 1.2f));
        StartCoroutine(Move(topCurtain, topTarget, 1f));
        StartCoroutine(Move(topCloud, topCloudTarget, 1.2f));
        StartCoroutine(Move(bottomCloud, bottomCloudTarget, 1.2f));

        for (int i = 0; i < bulbs.Length; i++)
        {
            if (bulbs[i] != null)
            {
                StartCoroutine(Move(bulbs[i], bulbTargets[i], 0.8f));
                yield return new WaitForSeconds(0.1f);
            }
        }

        StartCoroutine(Fade(lights, 1, 1.5f));
        StartCoroutine(Fade(glow, 1, 1.5f));
        yield return StartCoroutine(Fade(character, 1, 1.2f));

        if (storyVoice != null)
        {
            storyVoice.Play();
            yield return new WaitForSeconds(5f);
            if (characterAnimator != null)
            {
                characterAnimator.enabled = true;
                characterAnimator.Play("TalkState", 0, 0f);
            }
            while (storyVoice.isPlaying) yield return null;
            if (characterAnimator != null) characterAnimator.enabled = false;
        }
        yield return StartCoroutine(Fade(navButtons, 1, 1f));
    }

    public void SwitchToTask()
    {
        storyContent.SetActive(false);
        if (taskManager != null) taskManager.StartTaskMode();
    }

    IEnumerator Move(RectTransform obj, Vector2 target, float duration)
    {
        Vector2 start = obj.anchoredPosition;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            obj.anchoredPosition = Vector2.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        obj.anchoredPosition = target;
    }

    IEnumerator Fade(CanvasGroup cg, float target, float duration)
    {
        float start = cg.alpha;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        cg.alpha = target;
        cg.interactable = target > 0.5f;
        cg.blocksRaycasts = target > 0.5f;
    }

    IEnumerator ZoomOut()
    {
        Vector3 start = sceneContainer.localScale;
        Vector3 end = Vector3.one;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / 2.5f;
            sceneContainer.localScale = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        sceneContainer.localScale = end;
    }
}