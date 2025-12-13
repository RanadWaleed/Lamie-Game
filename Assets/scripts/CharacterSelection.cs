using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 0.5f;

    [Header("Auto-Detected Values (Don't Edit)")]
    public float sideScale;
    public float centerScale;
    public float centerPos;
    public float rightPos;
    public float leftPos;
    public float hiddenPos;

    public float centerPosY;
    public float sidePosY;

    [Header("UI Objects (Order is Important!)")]
    public Image[] slots;

    [Header("Data")]
    public List<Sprite> allCharacters;

    private int centerIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        centerScale = slots[1].rectTransform.localScale.x;
        sideScale = slots[0].rectTransform.localScale.x;

        leftPos = slots[0].rectTransform.anchoredPosition.x;
        centerPos = slots[1].rectTransform.anchoredPosition.x;
        rightPos = slots[2].rectTransform.anchoredPosition.x;

        centerPosY = slots[1].rectTransform.anchoredPosition.y;
        sidePosY = slots[0].rectTransform.anchoredPosition.y;

        float gap = Mathf.Abs(rightPos - centerPos);
        hiddenPos = (rightPos > 0) ? rightPos + gap : rightPos - gap;

        UpdateSpritesContent();
        ResetLayerPriorities();

        slots[3].rectTransform.anchoredPosition = new Vector2(hiddenPos, sidePosY);
        slots[3].color = new Color(1, 1, 1, 0);
        slots[3].rectTransform.localScale = new Vector3(sideScale, sideScale, 1);
    }

    public void NextCharacter()
    {
        if (isMoving) return;
        centerIndex++;
        if (centerIndex >= allCharacters.Count) centerIndex = 0;
        StartCoroutine(AnimateMove(true));
    }

    public void PreviousCharacter()
    {
        if (isMoving) return;
        centerIndex--;
        if (centerIndex < 0) centerIndex = allCharacters.Count - 1;
        StartCoroutine(AnimateMove(false));
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("SelectedCharacter", centerIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator AnimateMove(bool moveLeft)
    {
        isMoving = true;
        float timer = 0f;

        Image temp;
        if (moveLeft)
        {
            temp = slots[0];
            for (int i = 0; i < 3; i++) slots[i] = slots[i + 1];
            slots[3] = temp;

            slots[3].rectTransform.anchoredPosition = new Vector2(hiddenPos, sidePosY);
            AssignSpriteToSlot(slots[3], centerIndex + 1);
        }
        else
        {
            temp = slots[3];
            for (int i = 3; i > 0; i--) slots[i] = slots[i - 1];
            slots[0] = temp;

            slots[0].rectTransform.anchoredPosition = new Vector2(-hiddenPos, sidePosY);
            AssignSpriteToSlot(slots[0], centerIndex - 1);
        }

        ResetLayerPriorities();

        while (timer < 1f)
        {
            timer += Time.deltaTime / moveSpeed;
            if (timer > 1f) timer = 1f;

            LerpSlot(slots[0], leftPos, sidePosY, sideScale, 0.5f, timer);
            LerpSlot(slots[1], centerPos, centerPosY, centerScale, 1.0f, timer);
            LerpSlot(slots[2], rightPos, sidePosY, sideScale, 0.5f, timer);

            if (moveLeft) LerpSlot(slots[3], rightPos + (Mathf.Abs(rightPos - centerPos)), sidePosY, sideScale, 0f, timer);
            else LerpSlot(slots[3], leftPos - (Mathf.Abs(rightPos - centerPos)), sidePosY, sideScale, 0f, timer);

            yield return null;
        }
        slots[0].rectTransform.localScale = new Vector3(sideScale, sideScale, 1);
        slots[1].rectTransform.localScale = new Vector3(centerScale, centerScale, 1);
        slots[2].rectTransform.localScale = new Vector3(sideScale, sideScale, 1);

        UpdateSpritesContent();
        isMoving = false;
    }

    void LerpSlot(Image img, float targetX, float targetY, float targetScale, float targetAlpha, float time)
    {
        Vector2 currentPos = img.rectTransform.anchoredPosition;

        Vector2 targetPos = new Vector2(targetX, targetY);
        img.rectTransform.anchoredPosition = Vector2.Lerp(currentPos, targetPos, time);

        img.rectTransform.localScale = Vector3.Lerp(img.rectTransform.localScale, new Vector3(targetScale, targetScale, 1), time);

        Color c = img.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, time);
        img.color = c;
    }

    void UpdateSpritesContent()
    {
        AssignSpriteToSlot(slots[1], centerIndex);
        AssignSpriteToSlot(slots[0], centerIndex - 1);
        AssignSpriteToSlot(slots[2], centerIndex + 1);
        AssignSpriteToSlot(slots[3], centerIndex + 2);
    }

    void AssignSpriteToSlot(Image slot, int index)
    {
        int realIndex = index % allCharacters.Count;
        if (realIndex < 0) realIndex += allCharacters.Count;
        slot.sprite = allCharacters[realIndex];
    }

    void ResetLayerPriorities()
    {
        slots[1].transform.SetAsLastSibling();
        slots[0].transform.SetSiblingIndex(0);
        slots[2].transform.SetSiblingIndex(1);
        slots[3].transform.SetAsFirstSibling();
    }
}