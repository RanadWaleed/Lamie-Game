using UnityEngine;
using UnityEngine.Video;

public class IntroController : MonoBehaviour
{
    public VideoPlayer introVideo;

    void Start()
    {
        if (introVideo != null)
        {
            introVideo.loopPointReached += OnVideoFinished;
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        GameFlowManager.Instance.GoToState(GameFlowState.Login);
    }
}