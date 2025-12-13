using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using System.Collections;

public class IntroController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "LoginScene";
    public float pauseDuration = 1.0f;

    [Header("Transition")]
    public Image fadeImage; 
    public float fadeSpeed = 2.0f;

    void Start()
    {
        videoPlayer.loopPointReached += OnVideoFinished;
        if (fadeImage != null) fadeImage.color = new Color(0, 0, 0, 0); 
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        vp.Pause();
        StartCoroutine(WaitAndTransition());
    }

    IEnumerator WaitAndTransition()
    {
      
        yield return new WaitForSeconds(pauseDuration);

        if (fadeImage != null)
        {
            float alpha = 0;
            while (alpha < 1)
            {
                alpha += Time.deltaTime * fadeSpeed;
                fadeImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        }

        SceneManager.LoadScene(nextSceneName);
    }
}