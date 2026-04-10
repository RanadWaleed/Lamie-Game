using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CharacterSelection : MonoBehaviour
{
    public float moveSpeed = 0.5f;

    public float centerScale;
    public float centerPos;
    public float centerPosY;

    public Image[] slots;

    public List<Sprite> allCharacters;

    private int centerIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        centerScale = slots[1].rectTransform.localScale.x;

        centerPos = slots[1].rectTransform.anchoredPosition.x;
        centerPosY = slots[1].rectTransform.anchoredPosition.y;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i == 1)
            {
                slots[i].color = new Color(1, 1, 1, 1);
                slots[i].rectTransform.localScale = new Vector3(centerScale, centerScale, 1);
            }
            else
            {
                slots[i].color = new Color(1, 1, 1, 0);
            }
        }

        UpdateCenterSprite();
    }

    public void NextCharacter()
    {
        if (isMoving) return;
        centerIndex++;
        if (centerIndex >= allCharacters.Count) centerIndex = 0;
        StartCoroutine(AnimateCenter());
    }

    public void PreviousCharacter()
    {
        if (isMoving) return;
        centerIndex--;
        if (centerIndex < 0) centerIndex = allCharacters.Count - 1;
        StartCoroutine(AnimateCenter());
    }

    public void StartGame()
    {
        PlayerPrefs.SetInt("SelectedCharacter", centerIndex);
        PlayerPrefs.Save();
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator AnimateCenter()
    {
        isMoving = true;
        float timer = 0f;

        Image centerSlot = slots[1];
        AssignSpriteToSlot(centerSlot, centerIndex);

        while (timer < 1f)
        {
            timer += Time.deltaTime / moveSpeed;
            if (timer > 1f) timer = 1f;

            centerSlot.rectTransform.localScale = Vector3.Lerp(
                new Vector3(centerScale * 0.8f, centerScale * 0.8f, 1),
                new Vector3(centerScale, centerScale, 1),
                timer
            );

            yield return null;
        }

        isMoving = false;
    }

    void UpdateCenterSprite()
    {
        AssignSpriteToSlot(slots[1], centerIndex);
    }

    void AssignSpriteToSlot(Image slot, int index)
    {
        int realIndex = index % allCharacters.Count;
        if (realIndex < 0) realIndex += allCharacters.Count;
        slot.sprite = allCharacters[realIndex];
    }
}