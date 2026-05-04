using System.Collections;
using UnityEngine;

public class TaskManager : MonoBehaviour
{

    public static TaskManager Instance { get; private set; }

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

    [Header("References")]
    public QuestionManager questionManager;

    [Header("SFX")]
    public AudioClip clickSound;

    private void Awake()
    {
        Instance = this;
    }
    public void StartTaskMode()
    {
        taskRoot.SetActive(true);
        ShowStep(frameChooseStep);
        PlayVoice(frameChoiceVoice);
    }

    public void PlayClickSound()
    {
        if (audioSource && clickSound)
        {
            audioSource.PlayOneShot(clickSound);
        }
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
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void GoToDesign()
    {
        ShowStep(designTaskStep);
        PlayVoice(designVoice);
        ArtAssessmentManager.Instance?.StartTracking();
    }

    public void GoToAlignment()
    {
        ArtAssessmentManager.Instance?.StopTracking();
        StartCoroutine(GoToAlignmentSequence());
    }

    IEnumerator GoToAlignmentSequence()
    {
        BoardManager.Instance.Deselect();
        BoardManager.Instance.CaptureBoard();
        ShowStep(paintAlignmentStep);
        yield return null;
        PlayVoice(paintVoice);
        yield return null;
    }

    public void GoToQuestion()
    {

        ShowStep(questionStep);
        PlayVoice(questionVoice);

        float voiceDuration = (questionVoice != null) ? questionVoice.length : 3f;
        questionManager?.EnableButtonsAfterDelay(voiceDuration);
    }

    public void GoToNextScene()
    {
        Debug.Log("انتقال للسين التالي");
    }
}