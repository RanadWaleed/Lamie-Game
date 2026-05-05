using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class BadgeSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("Identity")]
    public string intelligenceId;
    public string badgeName;

    [Header("Visual Components")]
    public Image badgeImage;
    public Sprite badgeSprite;
    public AudioClip badgeAudio;
    [TextArea] public string description;
    [TextArea] public string missionText;

    [Header("Visual States")]
    public Color lockedColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
    public Color unlockedColor = Color.white;

    public bool IsUnlocked { get; private set; }

    public void Setup(string id, bool unlocked,
                      string desc = null, string mission = null,
                      Sprite sprite = null, AudioClip audio = null,
                      string name = null)
    {
        intelligenceId = id;
        IsUnlocked = unlocked;

        if (desc != null) description = desc;
        if (mission != null) missionText = mission;
        if (sprite != null) badgeSprite = sprite;
        if (audio != null) badgeAudio = audio;
        if (name != null) badgeName = name;

        UpdateVisuals(false);
    }

    public void UpdateVisuals(bool animate)
    {
        if (badgeImage == null) return;

        if (animate && IsUnlocked)
            StartCoroutine(RevealAnim());
        else
            badgeImage.color = IsUnlocked ? unlockedColor : lockedColor;
    }
    public void PlayEntranceFade(float delay)
    {
        if (!IsUnlocked) return;
        StartCoroutine(EntranceFade(delay));
    }

    private IEnumerator EntranceFade(float delay)
    {
        SetAlpha(0f);
        yield return new WaitForSeconds(delay);

        float dur = 0.5f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, t / dur));
            yield return null;
        }
        SetAlpha(1f);

        yield return StartCoroutine(PopAnim());
    }

    private IEnumerator PopAnim()
    {
        float dur = 0.25f;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            float scale = 1f + 0.12f * Mathf.Sin(p * Mathf.PI);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private void SetAlpha(float a)
    {
        if (badgeImage == null) return;
        Color c = badgeImage.color;
        c.a = a;
        badgeImage.color = c;
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (!IsUnlocked) return;
        BadgeManager.Instance.OnSlotClicked(this);
    }

    private IEnumerator RevealAnim()
    {
        float dur = 0.6f;
        float t = 0f;
        badgeImage.color = unlockedColor;

        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            float s = 1.70158f;
            float bounce = (p -= 1) * p * ((s + 1) * p + s) + 1;
            transform.localScale = Vector3.one * bounce;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }
}