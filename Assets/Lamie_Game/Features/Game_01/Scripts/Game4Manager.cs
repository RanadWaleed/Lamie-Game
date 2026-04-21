using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class PatternLevel
{
    public Sprite traySprite;
    public Sprite[] optionSprites = new Sprite[3];
    public int correctOptionIndex;
    public float standardTime = 15f;
}

public class Game4Manager : MonoBehaviour
{
    public MasterManager masterManager;
    public RectTransform levelContainerRect;
    public Image trayImage;
    public Image targetSlotImage;
    public Image[] optionsImages;
    public Button[] optionsButtons;
    public Button backButton;

    public PatternLevel[] levels = new PatternLevel[6];
    private int currentLevel = 0;
    private bool isAnimating = false;
    private Vector2 originalContainerPos;
    private bool isLevelCompleted = false;
    private int clicksThisStage = 0;

    void Start()
    {
        if (levelContainerRect != null)
        {
            originalContainerPos = levelContainerRect.anchoredPosition;
        }

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        LoadLevel(0);
    }

    void LoadLevel(int index)
    {
        if (index < levels.Length)
        {
            isLevelCompleted = false;
            clicksThisStage = 0;

            if (trayImage != null) trayImage.sprite = levels[index].traySprite;

            if (targetSlotImage != null)
            {
                targetSlotImage.sprite = null;
                targetSlotImage.color = new Color(1, 1, 1, 0);
            }

            for (int i = 0; i < optionsImages.Length; i++)
            {
                if (optionsImages[i] != null)
                {
                    if (levels[index].optionSprites != null && i < levels[index].optionSprites.Length && levels[index].optionSprites[i] != null)
                    {
                        optionsImages[i].sprite = levels[index].optionSprites[i];
                        optionsImages[i].gameObject.SetActive(true);

                        if (optionsButtons != null && i < optionsButtons.Length && optionsButtons[i] != null)
                        {
                            optionsButtons[i].interactable = true;
                        }
                    }
                    else
                    {
                        optionsImages[i].gameObject.SetActive(false);
                    }
                }
            }

            if (levelContainerRect != null)
            {
                levelContainerRect.anchoredPosition = originalContainerPos;
            }

            if (backButton != null)
            {
                backButton.gameObject.SetActive(index > 0);
            }

            if (MasterManager.Instance != null)
            {
                MasterManager.Instance.totalRequiredMatches = 1;
                MasterManager.Instance.maxTimeForCurrentLevel = levels[index].standardTime;
                MasterManager.Instance.StartTimer();
            }
        }
        else
        {
            if (MasterManager.Instance != null)
            {
                MasterManager.Instance.FinalizeAndUploadReport();
            }

            if (masterManager != null)
            {
                masterManager.ShowNextButton();
            }
        }
    }

    public void OnOptionClicked(int index)
    {
        if (isAnimating || isLevelCompleted) return;

        clicksThisStage++;
        bool isCorrect = (index == levels[currentLevel].correctOptionIndex);

        if (MasterManager.Instance != null)
        {
            MasterManager.Instance.RegisterAttempt(isCorrect, clicksThisStage == 1);
        }

        if (isCorrect)
        {
            isLevelCompleted = true;

            if (MasterManager.Instance != null)
            {
                MasterManager.Instance.SubmitStageData();
            }

            StartCoroutine(OptionSelectedRoutine(index));
        }
    }

    public void OnBackClicked()
    {
        if (isAnimating || currentLevel <= 0) return;
        StartCoroutine(GoBackRoutine());
    }

    IEnumerator OptionSelectedRoutine(int index)
    {
        isAnimating = true;

        if (backButton != null) backButton.gameObject.SetActive(false);

        for (int i = 0; i < optionsButtons.Length; i++)
        {
            if (optionsButtons[i] != null) optionsButtons[i].interactable = false;
        }

        if (optionsImages[index] != null)
        {
            optionsImages[index].gameObject.SetActive(false);
        }

        if (targetSlotImage != null)
        {
            targetSlotImage.sprite = levels[currentLevel].optionSprites[index];
            targetSlotImage.color = new Color(1, 1, 1, 1);
        }

        yield return new WaitForSeconds(1.5f);

        if (levelContainerRect != null)
        {
            Vector2 startPos = levelContainerRect.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(-2500f, 0);

            float timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                levelContainerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer / 1f);
                yield return null;
            }
        }

        currentLevel++;

        if (currentLevel < levels.Length)
        {
            if (levelContainerRect != null)
            {
                levelContainerRect.anchoredPosition = originalContainerPos + new Vector2(2500f, 0);
                LoadLevel(currentLevel);

                Vector2 startPos = levelContainerRect.anchoredPosition;
                Vector2 endPos = originalContainerPos;

                float timer = 0;
                while (timer < 1f)
                {
                    timer += Time.deltaTime;
                    levelContainerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer / 1f);
                    yield return null;
                }
            }
            else
            {
                LoadLevel(currentLevel);
            }
        }
        else
        {
            LoadLevel(currentLevel);
        }

        isAnimating = false;
    }

    IEnumerator GoBackRoutine()
    {
        isAnimating = true;

        if (backButton != null) backButton.gameObject.SetActive(false);

        if (levelContainerRect != null)
        {
            Vector2 startPos = levelContainerRect.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(2500f, 0);

            float timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                levelContainerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer / 1f);
                yield return null;
            }
        }

        currentLevel--;

        if (levelContainerRect != null)
        {
            levelContainerRect.anchoredPosition = originalContainerPos + new Vector2(-2500f, 0);
            LoadLevel(currentLevel);

            Vector2 startPos = levelContainerRect.anchoredPosition;
            Vector2 endPos = originalContainerPos;

            float timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                levelContainerRect.anchoredPosition = Vector2.Lerp(startPos, endPos, timer / 1f);
                yield return null;
            }
        }
        else
        {
            LoadLevel(currentLevel);
        }

        isAnimating = false;
    }
}