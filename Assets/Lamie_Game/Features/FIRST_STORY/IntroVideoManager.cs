using UnityEngine;
using UnityEngine.Video;

public class IntroVideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        string childId = PlayerPrefs.GetString("CurrentChildID", "default");
        PlayerPrefs.SetInt("HasSeenStory_" + childId, 1);
        PlayerPrefs.Save();

        videoPlayer.loopPointReached += EndReached;
        videoPlayer.Play();
    }

    void EndReached(VideoPlayer vp)
    {
        if (GameFlowManager.Instance != null)
        {
            if (!ChildLocalProfile.HasCharacter)
            {
                GameFlowManager.Instance.GoToState(GameFlowState.CharacterSelection);
            }
            else
            {
                GameFlowManager.Instance.GoToState(GameFlowState.Home);
            }
        }
    }
}