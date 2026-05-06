using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BadgeUnlockEffect : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform badgeImage;
    public Image raysImageComponent;
    public Button backgroundButton;

    [Header("Audio")]
    public AudioSource unlockSound;

    [Header("Pop-up & Spin Settings")]
    public float popUpSpeed = 5f;
    public float spinDuration = 1.5f;
    public float badgeRotationSpeedY = 1000f;

    [Header("Glow Settings (Pulse)")]
    public float glowSpeed = 2f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.8f;

    [Header("Scene Transition")]
    public string badgesSceneName = "BadgesScene";

    private Vector3 targetScale = Vector3.one;
    private float currentSpinTime = 0f;

    void Start()
    {
        badgeImage.localScale = Vector3.zero;
        if (raysImageComponent != null)
        {
            raysImageComponent.rectTransform.localScale = Vector3.zero;
        }

        backgroundButton.onClick.AddListener(GoToBadgesPage);

        if (unlockSound != null)
        {
            unlockSound.Play();
        }
    }

    void Update()
    {
        badgeImage.localScale = Vector3.Lerp(badgeImage.localScale, targetScale, Time.deltaTime * popUpSpeed);

        if (raysImageComponent != null)
        {
            raysImageComponent.rectTransform.localScale = Vector3.Lerp(raysImageComponent.rectTransform.localScale, targetScale, Time.deltaTime * popUpSpeed);
        }

        if (currentSpinTime < spinDuration)
        {
            badgeImage.Rotate(0f, badgeRotationSpeedY * Time.deltaTime, 0f);
            currentSpinTime += Time.deltaTime;
        }
        else
        {
            badgeImage.localRotation = Quaternion.Lerp(badgeImage.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }

        if (raysImageComponent != null)
        {
            Color c = raysImageComponent.color;
            c.a = Mathf.PingPong(Time.time * glowSpeed, maxAlpha - minAlpha) + minAlpha;
            raysImageComponent.color = c;
        }
    }

    void GoToBadgesPage()
    {
        SceneManager.LoadScene(badgesSceneName);
    }
}