using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CandySet
{
    public Sprite[] candyImages = new Sprite[4];
}

public class Game3Manager : MonoBehaviour
{
    public MasterManager masterManager;

    public Image[] candySlots;
    public CandySet[] allSets = new CandySet[4];

    private int currentSetIndex = 0;

    void Start()
    {
        LoadSet(0);
    }

    public void LoadSet(int index)
    {
        if (index < allSets.Length)
        {
            for (int i = 0; i < 4; i++)
            {
                if (candySlots[i] != null && allSets[index].candyImages[i] != null)
                {
                    candySlots[i].sprite = allSets[index].candyImages[i];
                }
            }
        }
        else
        {
            Debug.Log("[Backend Log] Game 3 Finished! All 4 sets completed.");
            if (masterManager != null)
            {
                masterManager.ShowNextButton();
            }
        }
    }

    public void OnCandyClicked(int slotIndex)
    {
        if (currentSetIndex >= allSets.Length) return;

        string clickedCandyName = candySlots[slotIndex].sprite.name;
        Debug.Log($"[Backend Log] Child clicked Candy [{clickedCandyName}] in Set [{currentSetIndex + 1}]");

        currentSetIndex++;
        LoadSet(currentSetIndex);
    }
}