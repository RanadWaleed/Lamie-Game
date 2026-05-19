using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using RTLTMPro;

public class BadgeManager : MonoBehaviour
{
    public static BadgeManager Instance { get; private set; }

    [Header("Badges")]
    public List<BadgeSlot> UI_Slots;

    [Header("UI References")]
    public RTLTextMeshPro descriptionText;
    public Image expandedBadgeImage;

    [Header("Star & Mission Label")]
    public GameObject starBG;
    public GameObject starObject;
    public RTLTextMeshPro missionLabel;

    private BadgeSlot _selected;
    private AudioSource _audio;
    private bool _starHidden;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _audio = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        SetAlpha(expandedBadgeImage, 0f);
        LoadBadgesFromLocal();
    }

    private void LoadBadgesFromLocal()
    {
        if (LocalProgressManager.Instance == null)
        {
            Debug.LogWarning("BadgeManager: LocalProgressManager not found!");
            return;
        }

        foreach (var slot in UI_Slots)
            slot.Setup(slot.intelligenceId, false);

        var allIntelligences = LocalProgressManager.Instance.allIntelligences;
        int entranceIndex = 0;

        foreach (var intel in allIntelligences)
        {
            var slot = UI_Slots.Find(s =>
                string.Equals(s.intelligenceId, intel.intelligenceId, StringComparison.OrdinalIgnoreCase));

            if (slot == null) continue;

            bool unlocked = LocalProgressManager.Instance.IsBadgeUnlocked(intel.intelligenceId);

            slot.Setup(
                id: intel.intelligenceId,
                unlocked: unlocked
                );

            if (unlocked)
            {
                slot.PlayEntranceFade(0.2f + entranceIndex * 0.15f);
                entranceIndex++;
            }
        }
    }

    public void OnSlotClicked(BadgeSlot slot)
    {
        if (!slot.IsUnlocked) return;
        _selected = slot;

        if (missionLabel != null)
        {
            missionLabel.text = slot.missionText;
            Color c = missionLabel.color; c.a = 1f; missionLabel.color = c;
        }

        if (descriptionText != null)
            descriptionText.text = slot.description;

        if (_audio.isPlaying) _audio.Stop();
        if (slot.badgeAudio != null)
        {
            _audio.clip = slot.badgeAudio;
            _audio.Play();
        }

        if (expandedBadgeImage != null)
        {
            expandedBadgeImage.sprite = slot.badgeSprite;
            expandedBadgeImage.SetNativeSize();
            StartCoroutine(FadeAlpha(expandedBadgeImage, 0f, 1f, 0.3f));
        }

        if (!_starHidden) StartCoroutine(HideStar());
    }

    private IEnumerator HideStar()
    {
        _starHidden = true;
        const float dur = 0.3f;
        yield return StartCoroutine(FadeOutGroup(starBG, dur));
        yield return StartCoroutine(FadeOutGroup(starObject, dur));
    }

    private IEnumerator FadeAlpha(Image img, float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            SetAlpha(img, Mathf.Lerp(from, to, t / dur));
            yield return null;
        }
        SetAlpha(img, to);
    }

    private IEnumerator FadeOutGroup(GameObject go, float dur)
    {
        if (go == null) yield break;
        var imgs = go.GetComponentsInChildren<Image>();
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = 1f - (t / dur);
            foreach (var img in imgs) SetAlpha(img, a);
            yield return null;
        }
        go.SetActive(false);
    }

    private void SetAlpha(Image img, float a)
    {
        if (img == null) return;
        Color c = img.color; c.a = a; img.color = c;
    }
}