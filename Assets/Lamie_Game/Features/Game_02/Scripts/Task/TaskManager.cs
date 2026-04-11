using System.Collections;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [Header("Main Sections")]
    public GameObject taskRoot;

    [Header("Steps")]
    public GameObject frameChooseStep;
    public GameObject designTaskStep;
    public GameObject paintAlignmentStep;
    public GameObject questionStep;

    [Header("Step Voice Overs")]
    public AudioSource audioSource;
    public AudioClip frameChoiceVoice;
    public AudioClip designVoice;
    public AudioClip paintVoice;
    public AudioClip questionVoice;

    public void StartTaskMode()
    {
        taskRoot.SetActive(true);
        ShowStep(frameChooseStep);
        PlayVoice(frameChoiceVoice);
    }

    void ShowStep(GameObject stepToShow)
    {
        frameChooseStep.SetActive(false);
        designTaskStep.SetActive(false);
        paintAlignmentStep.SetActive(false);
        questionStep.SetActive(false);

        stepToShow.SetActive(true);
    }

    void PlayVoice(AudioClip clip)
    {
        if (audioSource && clip)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(clip);
        }
    }

    public void GoToDesign()
    {
        ShowStep(designTaskStep);
        PlayVoice(designVoice);
    }

    public void GoToAlignment()
    {
        StartCoroutine(GoToAlignmentSequence());
    }
    IEnumerator GoToAlignmentSequence()
    {
        BoardManager.Instance.Deselect();
        BoardManager.Instance.CaptureBoard();
        ShowStep(paintAlignmentStep);
        PlayVoice(paintVoice);
        yield return null;
    }



    public void GoToQuestion()
    {
        ShowStep(questionStep);
        PlayVoice(questionVoice);
    }
}