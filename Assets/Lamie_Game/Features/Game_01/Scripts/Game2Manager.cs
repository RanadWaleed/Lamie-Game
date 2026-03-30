using UnityEngine;

public class Game2Manager : MonoBehaviour
{
    public MasterManager masterManager;
    public DragDrop[] allCandies;
    public int requiredCandies = 4;

    public void CheckAllCandiesPlaced()
    {
        int placedCount = 0;

        foreach (DragDrop candy in allCandies)
        {
            if (candy != null && candy.isPlaced)
            {
                placedCount++;
            }
        }

        if (placedCount >= requiredCandies)
        {
            if (masterManager != null)
            {
                masterManager.ShowNextButton();
            }
        }
    }
}