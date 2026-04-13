using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class CharacterSelection : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public float centerScale = 1.0f;

    public Image[] slots;
    public TextMeshProUGUI characterNameDisplay;

    public List<Sprite> boySprites;
    public List<Sprite> girlSprites;

    private List<Sprite> activeCharacters;
    private int centerIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        string childName = PlayerPrefs.GetString("ChildName", "صديق لامع");
        if (characterNameDisplay != null) characterNameDisplay.text = childName;

        string gender = PlayerPrefs.GetString("UserGender", "أنثى").Trim();
        Debug.Log("Gender length: " + gender.Length); 

        if (gender == "ذكر" || gender == "ركذ")
        {
            activeCharacters = boySprites;
            Debug.Log("Boy list activated successfully.");
        }
        else
        {
            activeCharacters = girlSprites;
            Debug.Log("Girl list activated successfully.");
        }

        UpdateCenterSprite();
    }

    void UpdateCenterSprite()
    {
        if (activeCharacters != null && activeCharacters.Count > 0 && slots.Length > 1)
        {
            Debug.Log("Changing sprite to: " + activeCharacters[centerIndex].name);

            slots[1].sprite = activeCharacters[centerIndex];

            slots[1].enabled = false;
            slots[1].enabled = true;
        }
    }

    public void NextCharacter()
    {
        Debug.Log("Next Button Clicked");
        if (isMoving || activeCharacters == null || activeCharacters.Count == 0) return;

        centerIndex = (centerIndex + 1) % activeCharacters.Count;
        UpdateCenterSprite();
        StartCoroutine(AnimateCenter());
    }

    public void PreviousCharacter()
    {
        Debug.Log("Previous Button Clicked");
        if (isMoving || activeCharacters == null || activeCharacters.Count == 0) return;

        centerIndex--;
        if (centerIndex < 0) centerIndex = activeCharacters.Count - 1;

        UpdateCenterSprite();
        StartCoroutine(AnimateCenter());
    }

    public void ConfirmSelection()
    {
        Debug.Log("Start Button Clicked. Saving Index: " + centerIndex);
        PlayerPrefs.SetInt("SelectedCharacterIndex", centerIndex);
        PlayerPrefs.Save();

        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.GoToNextState();
    }

    IEnumerator AnimateCenter()
    {
        isMoving = true;
        float timer = 0f;
        Image centerSlot = slots[1];

        Vector3 startScale = new Vector3(centerScale * 0.8f, centerScale * 0.8f, 1);
        Vector3 endScale = new Vector3(centerScale, centerScale, 1);

        while (timer < 1f)
        {
            timer += Time.deltaTime / moveSpeed;
            centerSlot.rectTransform.localScale = Vector3.Lerp(startScale, endScale, timer);
            yield return null;
        }
        isMoving = false;
    }

   
}