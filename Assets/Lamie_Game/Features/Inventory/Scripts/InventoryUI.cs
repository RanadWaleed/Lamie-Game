using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    [Header("Border")]
    public CanvasGroup borderGroup;
    public float borderFadeDuration = 1.0f;
    public float borderFadeDelay = 0.2f;

    [Header("Pipes")]
    public RectTransform pipesRect;
    public float pipesSlideDistance = 300f;
    public float pipesSlideDuration = 0.7f;

    [Header("Spaceship")]
    public Image spaceshipImage;
    public Sprite[] spaceshipSprites;
    public Button leftButton;
    public Button rightButton;
    public float spaceshipFadeDuration = 0.25f;

    private int _shipIndex;

    private void Start()
    {
        if (borderGroup != null) { borderGroup.alpha = 0f; StartCoroutine(FadeBorder()); }
        if (pipesRect != null) StartCoroutine(SlidePipes());
        if (leftButton != null) leftButton.onClick.AddListener(() => ChangeShip(-1));
        if (rightButton != null) rightButton.onClick.AddListener(() => ChangeShip(+1));
    }

    private IEnumerator FadeBorder()
    {
        yield return new WaitForSeconds(borderFadeDelay);
        float t = 0f;
        while (t < borderFadeDuration)
        {
            t += Time.deltaTime;
            borderGroup.alpha = Mathf.Lerp(0f, 1f, t / borderFadeDuration);
            yield return null;
        }
        borderGroup.alpha = 1f;
    }

    private IEnumerator SlidePipes()
    {
        Vector2 end = pipesRect.anchoredPosition;
        Vector2 start = end + Vector2.up * pipesSlideDistance;
        pipesRect.anchoredPosition = start;

        float t = 0f;
        while (t < pipesSlideDuration)
        {
            t += Time.deltaTime;
            pipesRect.anchoredPosition = Vector2.Lerp(start, end,
                Mathf.SmoothStep(0f, 1f, t / pipesSlideDuration));
            yield return null;
        }
        pipesRect.anchoredPosition = end;
    }

    private void ChangeShip(int dir)
    {
        if (spaceshipSprites == null || spaceshipSprites.Length < 2) return;
        _shipIndex = (_shipIndex + dir + spaceshipSprites.Length) % spaceshipSprites.Length;
        StartCoroutine(SwitchShip());
    }

    private IEnumerator SwitchShip()
    {
        yield return FadeImage(spaceshipImage, 1f, 0f, spaceshipFadeDuration);
        spaceshipImage.sprite = spaceshipSprites[_shipIndex];
        yield return FadeImage(spaceshipImage, 0f, 1f, spaceshipFadeDuration);
    }
    public void OnCloseButton() =>
        GameFlowManager.Instance?.GoToState(GameFlowState.Home);

    public void OnStartButton() =>
        GameFlowManager.Instance?.GoToState(GameFlowState.Game01);

    private IEnumerator FadeImage(Image img, float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            Color c = img.color; c.a = Mathf.Lerp(from, to, t / dur); img.color = c;
            yield return null;
        }
        Color fc = img.color; fc.a = to; img.color = fc;
    }
}