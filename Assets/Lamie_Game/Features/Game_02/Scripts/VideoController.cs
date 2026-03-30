using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoController : MonoBehaviour
{
    public VideoClip introVideo;
    public VideoClip story1Video;
    public VideoClip currentVideoPlay;

    public GameObject buttonsPanel;
    public Button replayVideoButton;
    public Button nextButton;
    public VideoPlayer videoPlayer;
    public RawImage currentVideoImage;

    private int currentStep = 0;

    void Start()
    {
        if (buttonsPanel) buttonsPanel.SetActive(false);

        if (replayVideoButton) replayVideoButton.onClick.AddListener(ReplayVideo);
        if (nextButton) nextButton.onClick.AddListener(NextVideo);

        if (videoPlayer) videoPlayer.loopPointReached += OnVideoEnd;

        PlayVideo(introVideo);
    }

    void PlayVideo(VideoClip clip)
    {
        if (clip == null || videoPlayer == null) return;

        currentVideoPlay = clip;
        videoPlayer.clip = clip;

        if (currentVideoImage != null)
        {
            RectTransform rt = currentVideoImage.GetComponent<RectTransform>();
            float clipWidth = (float)clip.width;
            float clipHeight = (float)clip.height;
            float aspectRatio = clipWidth / clipHeight;

            float currentHeight = rt.sizeDelta.y;
            if (currentHeight == 0) currentHeight = 1080f;

            rt.sizeDelta = new Vector2(currentHeight * aspectRatio, currentHeight);
        }

        if (buttonsPanel) buttonsPanel.SetActive(false);
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (buttonsPanel) buttonsPanel.SetActive(true);
    }

    void ReplayVideo()
    {
        if (buttonsPanel) buttonsPanel.SetActive(false);
        if (videoPlayer)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
        }
    }

    void NextVideo()
    {
        if (currentStep == 0)
        {
            currentStep = 1;
            PlayVideo(story1Video);
        }
        else if (currentStep == 1)
        {
            if (buttonsPanel) buttonsPanel.SetActive(false);
            this.gameObject.SetActive(false);

            FrameSelectionManager fsm = Object.FindFirstObjectByType<FrameSelectionManager>(FindObjectsInactive.Include);
            if (fsm != null) fsm.gameObject.SetActive(true);
        }
    }
}