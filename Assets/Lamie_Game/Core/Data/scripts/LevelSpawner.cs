using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelSpawner : MonoBehaviour
{
    public static LevelSpawner Instance;

    public List<StageData> levels;
    private int currentLevelIndex = 0;

    public GameObject candyPrefab;
    public Transform candiesContainer;

    public Image[] prePlacedJars;

    private int candiesNeededToPlace = 0;
    private int candiesCurrentlyPlaced = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void LoadLevel(int index)
    {
        if (index >= levels.Count)
        {
            Debug.Log("All Levels Completed!");

            if (MasterManager.Instance != null) MasterManager.Instance.ShowNextButton();

            return;
        }

        currentLevelIndex = index;
        StageData currentStage = levels[index];

        candiesContainer.name = "OldCandiesGroup_Destroying";

        foreach (Transform child in candiesContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }

        candiesContainer.DetachChildren();
        candiesContainer.name = "AllCandiesGroup";

        candiesNeededToPlace = currentStage.matchPairs.Count;
        candiesCurrentlyPlaced = 0;

        // --- Setup Level Data in Backend ---
        if (MasterManager.Instance != null)
        {
            MasterManager.Instance.totalRequiredMatches = candiesNeededToPlace;
            MasterManager.Instance.maxTimeForCurrentLevel = currentStage.standardTime;
        }

        SpawnLevel(currentStage);
    }

    void SpawnLevel(StageData stage)
    {
        List<RectTransform> activeJars = new List<RectTransform>();
        List<string> createdJarIDs = new List<string>();

        foreach (var jar in prePlacedJars)
        {
            jar.gameObject.SetActive(false);
        }

        int jarIndex = 0;
        foreach (var pair in stage.matchPairs)
        {
            if (!createdJarIDs.Contains(pair.pairID))
            {
                if (jarIndex < prePlacedJars.Length)
                {
                    Image currentJar = prePlacedJars[jarIndex];
                    currentJar.gameObject.SetActive(true);
                    currentJar.gameObject.name = pair.pairID;
                    currentJar.sprite = pair.targetSprite;

                    activeJars.Add(currentJar.GetComponent<RectTransform>());
                    createdJarIDs.Add(pair.pairID);
                    jarIndex++;
                }
            }
        }

        if (candiesContainer.GetComponent<LayoutGroup>() != null)
            candiesContainer.GetComponent<LayoutGroup>().enabled = true;

        foreach (var pair in stage.matchPairs)
        {
            SpawnCandy(pair.draggableSprite, pair.pairID, activeJars.ToArray());
        }

        foreach (var distractorSprite in stage.distractors)
        {
            SpawnCandy(distractorSprite, "Distractor", activeJars.ToArray());
        }

        RandomizeChildren(candiesContainer);

        Invoke("UnlockCandiesForDragging", 0.5f);
    }

    void SpawnCandy(Sprite sprite, string id, RectTransform[] jars)
    {
        GameObject newCandy = Instantiate(candyPrefab, candiesContainer);
        newCandy.transform.localScale = Vector3.one;
        newCandy.GetComponent<Image>().sprite = sprite;

        DragDrop dragScript = newCandy.GetComponent<DragDrop>();
        if (dragScript != null)
        {
            dragScript.candyID = id;
            dragScript.allMolds = jars;
        }
    }

    void UnlockCandiesForDragging()
    {
        if (candiesContainer.GetComponent<LayoutGroup>() != null)
            candiesContainer.GetComponent<LayoutGroup>().enabled = false;
    }

    void RandomizeChildren(Transform container)
    {
        int childCount = container.childCount;
        for (int i = 0; i < childCount; i++)
        {
            int randomIndex = Random.Range(0, childCount);
            container.GetChild(i).SetSiblingIndex(randomIndex);
        }
    }

    public void CandyPlacedCorrectly()
    {
        candiesCurrentlyPlaced++;
        if (candiesCurrentlyPlaced >= candiesNeededToPlace)
        {
            // --- Submit Stage Data to Backend ---
            if (MasterManager.Instance != null)
            {
                MasterManager.Instance.SubmitStageData();
            }

            Invoke("LoadNextLevel", 2f);
        }
    }

    void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }
}