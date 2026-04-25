using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LubnaRepeater : MonoBehaviour, IPointerClickHandler
{
    [Header("Lubna References")]
    public AudioSource lubnaAudioSource;
    public AudioClip instructionClip;
    public Animator lubnaAnimator;

    public string talkingParameter = "isTalking";

    public void OnPointerClick(PointerEventData eventData)
    {
        if (lubnaAudioSource != null && lubnaAudioSource.isPlaying) return;

        StartCoroutine(TalkRoutine());
    }

    IEnumerator TalkRoutine()
    {
        if (lubnaAudioSource != null && instructionClip != null)
        {
            lubnaAudioSource.clip = instructionClip;
            lubnaAudioSource.Play();
        }

        if (lubnaAnimator != null)
        {
            lubnaAnimator.SetBool(talkingParameter, true);
        }

        yield return new WaitForSeconds(instructionClip != null ? instructionClip.length : 2f);

        if (lubnaAnimator != null)
        {
            lubnaAnimator.SetBool(talkingParameter, false);
        }
    }
}