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
    private bool stageClosed = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void LoadLevel(int index)
    {
        stageClosed = true;
        SetStageInteraction(false);
        CancelInvoke(nameof(UnlockCandiesForDragging));
        if (index >= levels.Count)
        {
            if (MasterManager.Instance != null) MasterManager.Instance.ShowNextButton();
            return;
        }

        currentLevelIndex = index;
        StageData currentStage = levels[index];
        if (!ValidateTargets(currentStage)) return;

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

        if (MasterManager.Instance != null)
        {
            MasterManager.Instance.totalRequiredMatches = candiesNeededToPlace;
            MasterManager.Instance.maxTimeForCurrentLevel = currentStage.standardTime;
        }

        SpawnLevel(currentStage);
    }

    private bool ValidateTargets(StageData stage)
    {
        if (stage == null || stage.matchPairs == null || prePlacedJars == null)
        {
            Debug.LogError("Color stage target references are missing.", this);
            return false;
        }
        var ids = new HashSet<string>();
        foreach (MatchPair pair in stage.matchPairs) ids.Add(pair.pairID);
        int count = ids.Count + (stage.distractorTargets == null ? 0 : stage.distractorTargets.Count);
        if (count > prePlacedJars.Length)
        {
            Debug.LogError("Color stage needs " + count + " target containers, but only " +
                prePlacedJars.Length + " are assigned in Pre Placed Jars.", this);
            return false;
        }
        for (int i = 0; i < count; i++)
            if (prePlacedJars[i] == null)
            {
                Debug.LogError("A required target container is missing from Pre Placed Jars.", this);
                return false;
            }
        if (stage.distractorTargets != null)
            foreach (Sprite sprite in stage.distractorTargets)
                if (sprite == null)
                {
                    Debug.LogError("Assign every Distractor Targets sprite or remove its empty entry.", this);
                    return false;
                }
        return true;
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

        // Wrong containers are drop candidates, but never add required candy matches.
        if (stage.distractorTargets != null)
        {
            for (int i = 0; i < stage.distractorTargets.Count; i++)
            {
                Image jar = prePlacedJars[jarIndex++];
                string id = "__DistractorTarget_" + i;
                while (createdJarIDs.Contains(id)) id += "_";
                createdJarIDs.Add(id);
                jar.gameObject.name = id;
                jar.sprite = stage.distractorTargets[i];
                jar.gameObject.SetActive(true);
                activeJars.Add(jar.GetComponent<RectTransform>());
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

        RectTransform rt = newCandy.GetComponent<RectTransform>();
        if (rt != null) rt.pivot = new Vector2(0.5f, 0.7f);

        newCandy.GetComponent<Image>().sprite = sprite;

        DragDrop dragScript = newCandy.GetComponent<DragDrop>();
        if (dragScript != null)
        {
            dragScript.candyID = id;
            dragScript.allMolds = jars;
            dragScript.SetInteractionEnabled(false);
        }
    }

    void UnlockCandiesForDragging()
    {
        stageClosed = false;
        SetStageInteraction(true);
        if (candiesContainer.GetComponent<LayoutGroup>() != null)
            candiesContainer.GetComponent<LayoutGroup>().enabled = false;

        if (MasterManager.Instance != null) MasterManager.Instance.StartTimer();
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
        if (stageClosed) return;
        candiesCurrentlyPlaced++;
        if (candiesCurrentlyPlaced >= candiesNeededToPlace)
        {
            // Close every item before resetting counters, including distractors and active drags.
            stageClosed = true;
            SetStageInteraction(false);
            if (MasterManager.Instance != null)
            {
                MasterManager.Instance.SubmitStageData();
            }

            Invoke("LoadNextLevel", 2f);
        }
    }

    private void SetStageInteraction(bool value)
    {
        if (candiesContainer == null) return;
        foreach (DragDrop item in candiesContainer.GetComponentsInChildren<DragDrop>(true))
            item.SetInteractionEnabled(value);
    }

    void LoadNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);
    }
}