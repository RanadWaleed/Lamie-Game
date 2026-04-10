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

    public RectTransform allJarsGroup;
    public float dropDuration = 2f;
    public Vector2 shelfPos = new Vector2(0, 0);
    public Vector2 floorPos = new Vector2(-2214f, -820.3f);
    public float floorScale = 2.576f;

    public GameObject nextButton;

    void Start()
    {
        if (allJarsGroup != null)
        {
            allJarsGroup.anchoredPosition = shelfPos;
            allJarsGroup.localScale = Vector3.one;
        }

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

        float dropTimer = 0;
        while (dropTimer < dropDuration)
        {
            dropTimer += Time.deltaTime;
            float progress = Mathf.SmoothStep(0, 1, dropTimer / dropDuration);

            if (allJarsGroup != null)
            {
                allJarsGroup.anchoredPosition = Vector2.Lerp(shelfPos, floorPos, progress);
                allJarsGroup.localScale = Vector3.Lerp(Vector3.one, new Vector3(floorScale, floorScale, floorScale), progress);
            }
            yield return null;
        }

        if (allJarsGroup != null)
        {
            allJarsGroup.anchoredPosition = floorPos;
            allJarsGroup.localScale = new Vector3(floorScale, floorScale, floorScale);
        }

        if (nextButton != null) nextButton.SetActive(true);
    }
}