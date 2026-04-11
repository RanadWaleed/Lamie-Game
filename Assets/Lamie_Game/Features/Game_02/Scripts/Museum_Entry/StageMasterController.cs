using UnityEngine;
using System.Collections;

public enum SceneState { Intro, Story, Customization }

public class StageMasterController : MonoBehaviour
{
    public SceneState currentState;

    [Header("1. Dark Purple Side Curtains")]
    public RectTransform darkPurpleLeft;
    public RectTransform darkPurpleRight;

    [Header("2. Top Decorations")]
    public RectTransform[] topDecorations;

    [Header("3. Intro Objects (Lebnah)")]
    public CanvasGroup bulbsGroup;
    public CanvasGroup characterCG;
    public AudioSource characterVoice;
    public Animator characterAnimator;

    [Header("4. Story Objects")]
    public CanvasGroup storyPanel;

    [Header("5. Navigation Buttons")]
    public CanvasGroup buttonsGroup;

    [Header("6. Cinematic Zoom")]
    public RectTransform sceneContainer;
    public float targetZoomScale = 1.2f;
    public float zoomInDuration = 4.0f;
    public float zoomOutDuration = 2.0f;

    [Header("7. Curtain Sequencer")]
    public RectTransform leftSideGroup;
    public RectTransform rightSideGroup;
    public RectTransform leftStars;
    public RectTransform rightStars;
    public float curtainDuration = 2.0f;
    public float curtainHorizontalDist = 1000f;
    public float starsUpDist = 600f;

    [Header("8. Story Manager")]
    public StoryTellingManager storyTellingManager;

    [Header("9. Intro Root")]
    public GameObject introRoot;

    [Header("Settings")]
    public float moveSpeed = 1.2f;
    public float dropDist = 800f;
    public float horizontalDist = 1500f;

    void Start()
    {
        PrepareStage();
        SetState(SceneState.Intro);
    }

    void PrepareStage()
    {
        if (sceneContainer != null) sceneContainer.localScale = Vector3.one;

        foreach (var item in topDecorations)
            item.anchoredPosition += new Vector2(0, dropDist);

        bulbsGroup.alpha = 0;
        characterCG.alpha = 0;
        storyPanel.alpha = 0;
        storyPanel.interactable = false;
        storyPanel.blocksRaycasts = false;

        buttonsGroup.alpha = 0;
        buttonsGroup.interactable = false;
        buttonsGroup.blocksRaycasts = false;

        if (characterAnimator != null) characterAnimator.enabled = false;
    }

    public void SetState(SceneState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case SceneState.Intro:
                StartCoroutine(StartShow());
                break;
            case SceneState.Story:
                StartCoroutine(TransitionToStory());
                break;
        }
    }

    IEnumerator StartShow()
    {
        yield return new WaitForSeconds(0.8f);

        if (sceneContainer != null)
            StartCoroutine(AnimateZoom(Vector3.one, new Vector3(targetZoomScale, targetZoomScale, 1f), zoomInDuration));

        StartCoroutine(MoveSide(darkPurpleLeft, -horizontalDist));
        StartCoroutine(MoveSide(darkPurpleRight, horizontalDist));

        StartCoroutine(RunCurtainSequencer());

        yield return new WaitForSeconds(0.6f);

        foreach (var item in topDecorations)
        {
            StartCoroutine(DropItem(item));
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(0.5f);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            bulbsGroup.alpha = t;
            characterCG.alpha = t;
            yield return null;
        }

        if (characterVoice != null) characterVoice.Play();

        yield return new WaitForSeconds(1.0f);

        if (characterAnimator != null)
        {
            characterAnimator.enabled = true;
            characterAnimator.Play("LebnahTalk", 0, 0f);
        }

        bool zoomedOut = false;
        while (characterVoice != null && characterVoice.isPlaying)
        {
            float remainingTime = characterVoice.clip.length - characterVoice.time;
            if (!zoomedOut && remainingTime <= zoomOutDuration)
            {
                StartCoroutine(AnimateZoom(sceneContainer.localScale, Vector3.one, zoomOutDuration));
                zoomedOut = true;
            }
            yield return null;
        }

        if (characterAnimator != null) characterAnimator.enabled = false;

        yield return StartCoroutine(FadeCanvasGroup(buttonsGroup, 1f, 2f));
    }

    IEnumerator RunCurtainSequencer()
    {
        yield return new WaitForSeconds(0.5f);

        float elapsed = 0;

        Vector2 startL = leftSideGroup.anchoredPosition;
        Vector2 startR = rightSideGroup.anchoredPosition;
        Vector2 startLStars = leftStars.anchoredPosition;
        Vector2 startRStars = rightStars.anchoredPosition;

        Vector2 targetL = startL + new Vector2(-curtainHorizontalDist, 0);
        Vector2 targetR = startR + new Vector2(curtainHorizontalDist, 0);
        Vector2 targetLStars = startLStars + new Vector2(0, starsUpDist);
        Vector2 targetRStars = startRStars + new Vector2(0, starsUpDist);

        while (elapsed < curtainDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(0, 1, elapsed / curtainDuration);

            leftSideGroup.anchoredPosition = Vector2.Lerp(startL, targetL, t);
            rightSideGroup.anchoredPosition = Vector2.Lerp(startR, targetR, t);
            leftStars.anchoredPosition = Vector2.Lerp(startLStars, targetLStars, t);
            rightStars.anchoredPosition = Vector2.Lerp(startRStars, targetRStars, t);

            yield return null;
        }

        leftSideGroup.gameObject.SetActive(false);
        rightSideGroup.gameObject.SetActive(false);
    }

    IEnumerator TransitionToStory()
    {
        yield return StartCoroutine(FadeCanvasGroup(buttonsGroup, 0f, 2f));

        StartCoroutine(FadeCanvasGroup(characterCG, 0f, 1f));
        StartCoroutine(FadeCanvasGroup(bulbsGroup, 0f, 1f));

        yield return new WaitForSeconds(1f);

        if (introRoot != null) introRoot.SetActive(false);

        if (storyTellingManager != null)
            storyTellingManager.SetupAndStart();
    }
    IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float speed)
    {
        float t = 0;
        float startAlpha = cg.alpha;
        while (t < 1)
        {
            t += Time.deltaTime * speed;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        cg.alpha = targetAlpha;
        cg.interactable = targetAlpha > 0.5f;
        cg.blocksRaycasts = targetAlpha > 0.5f;
    }

    public void ReplayCharacter()
    {
        StopAllCoroutines();
        characterVoice.Stop();
        characterAnimator.enabled = false;
        buttonsGroup.alpha = 0;
        StartCoroutine(ReplaySequence());
    }

    IEnumerator ReplaySequence()
    {
        if (sceneContainer != null)
            StartCoroutine(AnimateZoom(sceneContainer.localScale, new Vector3(targetZoomScale, targetZoomScale, 1f), zoomInDuration));

        characterVoice.Play();
        yield return new WaitForSeconds(1.0f);
        characterAnimator.enabled = true;
        characterAnimator.Play("LebnahTalk", 0, 0f);

        bool zoomedOut = false;
        while (characterVoice.isPlaying)
        {
            float remainingTime = characterVoice.clip.length - characterVoice.time;
            if (!zoomedOut && remainingTime <= zoomOutDuration)
            {
                StartCoroutine(AnimateZoom(sceneContainer.localScale, Vector3.one, zoomOutDuration));
                zoomedOut = true;
            }
            yield return null;
        }

        characterAnimator.enabled = false;
        yield return StartCoroutine(FadeCanvasGroup(buttonsGroup, 1f, 2f));
    }

    public void GoToNextScene()
    {
        if (currentState == SceneState.Intro)
            SetState(SceneState.Story);
    }

    IEnumerator AnimateZoom(Vector3 start, Vector3 end, float duration)
    {
        float zt = 0;
        while (zt < 1)
        {
            zt += Time.deltaTime / duration;
            sceneContainer.localScale = Vector3.Lerp(start, end, Mathf.SmoothStep(0, 1, zt));
            yield return null;
        }
        sceneContainer.localScale = end;
    }

    IEnumerator MoveSide(RectTransform rt, float dist)
    {
        float t = 0;
        Vector2 start = rt.anchoredPosition;
        Vector2 target = start + new Vector2(dist, 0);
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            rt.anchoredPosition = Vector2.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
    }
    IEnumerator DropItem(RectTransform rt)
    {
        float t = 0;
        Vector2 start = rt.anchoredPosition;
        Vector2 target = start - new Vector2(0, dropDist);
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;
            rt.anchoredPosition = Vector2.Lerp(start, target, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
    }

}