using UnityEngine;
using System.Collections;

public class Game1Manager : MonoBehaviour
{
    public CanvasGroup game1UI_CG;
    public RectTransform game1UI_RT;

    public CanvasGroup shelfJarsCanvasGroup;
    public float fadeOutDuration = 1.5f;

    public GameObject nextButton;

    public GameObject actualFloorJars;

    void Start()
    {
        if (shelfJarsCanvasGroup != null) shelfJarsCanvasGroup.alpha = 1f;

        if (game1UI_CG != null) game1UI_CG.alpha = 0;
        if (game1UI_RT != null) game1UI_RT.localScale = Vector3.zero;

        if (actualFloorJars != null) actualFloorJars.SetActive(false);

        StartCoroutine(MainSequence());
    }

    IEnumerator MainSequence()
    {
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

        if (actualFloorJars != null) actualFloorJars.SetActive(true);

        if (LevelSpawner.Instance != null)
        {
            LevelSpawner.Instance.LoadLevel(0);
        }

        if (nextButton != null) nextButton.SetActive(true);
    }
}