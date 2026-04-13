using UnityEngine;
using System.Collections;

public class Game1Manager : MonoBehaviour
{
    public Animator mahmoodAnimator;
    public AudioSource mahmoodAudio;
    public float mahmoodAnimSpeed = 1.5f;

    public RectTransform environmentPanel;
    public float panDistance = 1000f;
    public float panDuration = 2f;

    public GameObject lubnahObject;
    public Animator lubnahAnimator;
    public AudioSource lubnahAudio;
    public float lubnahAnimSpeed = 1.2f;

    public CanvasGroup game1UI_CG;
    public RectTransform game1UI_RT;

    public CanvasGroup shelfJarsCanvasGroup;
    public float fadeOutDuration = 1.5f;

    public GameObject nextButton;

    void Start()
    {
        if (shelfJarsCanvasGroup != null) shelfJarsCanvasGroup.alpha = 1f;

        if (game1UI_CG != null) game1UI_CG.alpha = 0;
        if (game1UI_RT != null) game1UI_RT.localScale = Vector3.zero;

        if (lubnahObject != null) lubnahObject.SetActive(false);

        StartCoroutine(MainSequence());
    }

    IEnumerator MainSequence()
    {
        if (mahmoodAnimator != null) mahmoodAnimator.speed = mahmoodAnimSpeed;

        yield return new WaitForSeconds(1f);

        if (mahmoodAudio != null) mahmoodAudio.Play();

        float mahmoodWaitTime = (mahmoodAudio != null && mahmoodAudio.clip != null) ? mahmoodAudio.clip.length : 3f;
        yield return new WaitForSeconds(mahmoodWaitTime);

        if (mahmoodAnimator != null) mahmoodAnimator.speed = 0f;

        if (environmentPanel != null)
        {
            float timer = 0;
            Vector2 startPos = environmentPanel.anchoredPosition;
            Vector2 endPos = startPos - new Vector2(panDistance, 0);

            while (timer < panDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.SmoothStep(0, 1, timer / panDuration);
                environmentPanel.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
                yield return null;
            }
            environmentPanel.anchoredPosition = endPos;
        }

        if (lubnahObject != null) lubnahObject.SetActive(true);
        if (lubnahAnimator != null) lubnahAnimator.speed = lubnahAnimSpeed;
        if (lubnahAudio != null) lubnahAudio.Play();

        float lubnahWaitTime = (lubnahAudio != null && lubnahAudio.clip != null) ? lubnahAudio.clip.length : 3f;
        yield return new WaitForSeconds(lubnahWaitTime);

        if (lubnahAnimator != null) lubnahAnimator.speed = 0f;

        float uiTimer = 0;
        while (uiTimer < 1f)
        {
            uiTimer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, uiTimer / 1f);

            if (game1UI_CG != null) game1UI_CG.alpha = progress;
            if (game1UI_RT != null) game1UI_RT.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress);

            yield return null;
        }

        if (shelfJarsCanvasGroup != null)
        {
            float fadeTimer = 0;
            while (fadeTimer < fadeOutDuration)
            {
                fadeTimer += Time.deltaTime;
                shelfJarsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutDuration);
                yield return null;
            }
            shelfJarsCanvasGroup.alpha = 0f;
            shelfJarsCanvasGroup.gameObject.SetActive(false);
        }

        if (LevelSpawner.Instance != null)
        {
            LevelSpawner.Instance.LoadLevel(0);
        }

        if (nextButton != null) nextButton.SetActive(true);
    }
}