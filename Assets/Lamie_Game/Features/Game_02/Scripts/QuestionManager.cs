using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class QuestionManager : MonoBehaviour
{
    [Header("Title Buttons")]
    public Button[] titleButtons;

    [Header("Confirm Button")]
    public Button confirmButton;

    [Header("Option Frames")]
    public Image[] optionFrames;

    [Header("Option Voices")]
    public AudioClip[] optionVoices;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Fade Settings")]
    public float fadeDuration = 0.3f;

    private int selectedIndex = -1;
    private Coroutine sequenceCoroutine;

    void Start()
    {
        SetButtonsInteractable(false);
        HideAllFrames();
    }


    public void EnableButtonsAfterDelay(float voiceDuration)
    {
        sequenceCoroutine = StartCoroutine(PlayOptionsSequence(voiceDuration));
    }

    private IEnumerator PlayOptionsSequence(float initialDelay)
    {
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < optionVoices.Length; i++)
        {
            yield return StartCoroutine(FadeAllOut());

            yield return StartCoroutine(FadeIn(optionFrames[i]));

            if (audioSource && optionVoices[i] != null)
            {
                audioSource.Stop();
                audioSource.clip = optionVoices[i];
                audioSource.Play();
                yield return new WaitForSeconds(optionVoices[i].length);
            }
        }

        yield return StartCoroutine(FadeAllOut());

        SetButtonsInteractable(true);
        ArtAssessmentManager.Instance?.StartTitleTimer();

        Debug.Log("[Question] الأزرار مفعّلة — بدأ توقيت المؤشر 7");
    }

    public void OnTitleSelected(string titleId)
    {

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
        if (audioSource != null) audioSource.Stop();

        HideAllFrames();

        for (int i = 0; i < buttonTitles.Length; i++)
        {
            if (buttonTitles[i] == titleId && i < optionFrames.Length)
            {
                selectedIndex = i;
                StartCoroutine(FadeIn(optionFrames[i]));
                break;
            }
        }

        ArtAssessmentManager.Instance?.OnTitleSelected(titleId);
    }


    private string GetButtonTitle(int index)
    {
        if (index < buttonTitles.Length)
            return buttonTitles[index];
        return "";
    }

    [Header("Button Titles")]
    public string[] buttonTitles;

    public void OnConfirm()
    {
        ArtAssessmentManager.Instance?.OnConfirm();
    }


    private void HideAllFrames()
    {
        foreach (var frame in optionFrames)
            if (frame != null)
            {
                var c = frame.color;
                c.a = 0f;
                frame.color = c;
                frame.gameObject.SetActive(false);
            }
    }

    private IEnumerator FadeIn(Image img)
    {
        if (img == null) yield break;
        img.gameObject.SetActive(true);

        float elapsed = 0f;
        Color c = img.color;
        c.a = 0f;
        img.color = c;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / fadeDuration);
            img.color = c;
            yield return null;
        }
        c.a = 1f;
        img.color = c;
    }

    private IEnumerator FadeOut(Image img)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        Color c = img.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            img.color = c;
            yield return null;
        }
        c.a = 0f;
        img.color = c;
        img.gameObject.SetActive(false);
    }

    private IEnumerator FadeAllOut()
    {
        foreach (var frame in optionFrames)
            if (frame != null && frame.gameObject.activeSelf)
                yield return StartCoroutine(FadeOut(frame));
    }

    private void SetButtonsInteractable(bool state)
    {
        foreach (var btn in titleButtons)
            if (btn != null) btn.interactable = state;
        if (confirmButton != null) confirmButton.interactable = state;
    }
}