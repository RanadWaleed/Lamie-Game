using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CandySet
{
    public Sprite[] candyImages = new Sprite[6];
    public int correctSlotIndex;
    public float standardTime = 10f;
    public GameObject distractorObject;
}

public class Game3Manager : MonoBehaviour
{
    public MasterManager masterManager;
    public Image[] candySlots;
    public CandySet[] allSets;

    private int currentSetIndex = 0;
    private bool isLevelCompleted = false;
    private int clicksThisStage = 0;

    void Start()
    {
        LoadSet(0);
    }

    public void LoadSet(int index)
    {
        if (index < allSets.Length)
        {
            isLevelCompleted = false;
            clicksThisStage = 0;
            CandySet currentSet = allSets[index];

            foreach (var set in allSets)
            {
                if (set.distractorObject != null)
                {
                    set.distractorObject.SetActive(false);
                }
            }

            if (currentSet.distractorObject != null)
            {
                currentSet.distractorObject.SetActive(true);
            }

            for (int i = 0; i < candySlots.Length; i++)
            {
                if (candySlots[i] != null)
                {
                    if (currentSet.candyImages != null && i < currentSet.candyImages.Length && currentSet.candyImages[i] != null)
                    {
                        candySlots[i].sprite = currentSet.candyImages[i];
                        candySlots[i].gameObject.SetActive(true);

                        Button btn = candySlots[i].GetComponent<Button>();
                        if (btn != null) btn.interactable = true;
                    }
                    else
                    {
                        candySlots[i].gameObject.SetActive(false);
                    }
                }
            }

            if (MasterManager.Instance != null)
            {
                MasterManager.Instance.totalRequiredMatches = 1;
                MasterManager.Instance.maxTimeForCurrentLevel = currentSet.standardTime;
            }
        }
        else
        {
            if (masterManager != null) masterManager.ShowNextButton();
        }
    }

    public void OnCandyClicked(int slotIndex)
    {
        if (isLevelCompleted) return;

        clicksThisStage++;
        bool isCorrect = (slotIndex == allSets[currentSetIndex].correctSlotIndex);

        if (MasterManager.Instance != null)
        {
            MasterManager.Instance.RegisterAttempt(isCorrect, clicksThisStage == 1);
        }

        if (isCorrect)
        {
            isLevelCompleted = true;

            foreach (var slot in candySlots)
            {
                if (slot != null && slot.GetComponent<Button>() != null)
                    slot.GetComponent<Button>().interactable = false;
            }

            if (MasterManager.Instance != null)
            {
                MasterManager.Instance.SubmitStageData();
            }

            Invoke("LoadNextSet", 0.8f);
        }
    }

    void LoadNextSet()
    {
        currentSetIndex++;
        LoadSet(currentSetIndex);
    }
}