using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Game2Spawner : MonoBehaviour
{
    public static Game2Spawner Instance;

    public List<StageData> levels;
    private int currentLevelIndex = 0;

    public GameObject shapePrefab;
    public Transform shapesContainer;
    public Image[] prePlacedMolds;

    public Vector3 customShapeRotation = Vector3.zero;
    public float defaultShapeScale = 1f;

    private int shapesNeededToPlace = 0;
    private int shapesCurrentlyPlaced = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void LoadLevel(int index)
    {
        if (index >= levels.Count)
        {
            if (MasterManager.Instance != null) MasterManager.Instance.ShowNextButton();
            return;
        }

        currentLevelIndex = index;
        StageData currentStage = levels[index];

        shapesContainer.name = "OldShapesGroup_Destroying";
        foreach (Transform child in shapesContainer)
        {
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        shapesContainer.DetachChildren();
        shapesContainer.name = "AllShapesGroup";

        shapesNeededToPlace = currentStage.matchPairs.Count;
        shapesCurrentlyPlaced = 0;

        if (MasterManager.Instance != null)
        {
            MasterManager.Instance.totalRequiredMatches = shapesNeededToPlace;
            MasterManager.Instance.maxTimeForCurrentLevel = currentStage.standardTime;
        }

        SpawnLevel(currentStage);
    }

    void SpawnLevel(StageData stage)
    {
        List<RectTransform> activeMolds = new List<RectTransform>();
        List<string> createdMoldIDs = new List<string>();

        foreach (var mold in prePlacedMolds)
        {
            mold.gameObject.SetActive(false);
        }

        int moldIndex = 0;
        foreach (var pair in stage.matchPairs)
        {
            if (!createdMoldIDs.Contains(pair.pairID))
            {
                if (moldIndex < prePlacedMolds.Length)
                {
                    Image currentMold = prePlacedMolds[moldIndex];
                    currentMold.gameObject.SetActive(true);
                    currentMold.gameObject.name = pair.pairID;
                    currentMold.sprite = pair.targetSprite;
                    currentMold.color = Color.white;

                    activeMolds.Add(currentMold.GetComponent<RectTransform>());
                    createdMoldIDs.Add(pair.pairID);
                    moldIndex++;
                }
            }
        }

        if (shapesContainer.GetComponent<LayoutGroup>() != null)
            shapesContainer.GetComponent<LayoutGroup>().enabled = true;

        bool isStage3 = (currentLevelIndex == 2);

        foreach (var pair in stage.matchPairs)
        {
            SpawnShape(pair.draggableSprite, pair.pairID, activeMolds.ToArray(), isStage3);
        }

        foreach (var distractor in stage.distractors)
        {
            SpawnShape(distractor, "Distractor", activeMolds.ToArray(), isStage3);
        }

        RandomizeChildren(shapesContainer);
        Invoke("UnlockShapesForDragging", 0.5f);
    }

    void SpawnShape(Sprite sprite, string id, RectTransform[] molds, bool isStage3 = false)
    {
        GameObject newShape = Instantiate(shapePrefab, shapesContainer);

        newShape.transform.localRotation = Quaternion.Euler(customShapeRotation);
        newShape.transform.localScale = new Vector3(defaultShapeScale, defaultShapeScale, 1f);

        RectTransform rt = newShape.GetComponent<RectTransform>();
        if (rt != null)
        {
            if (isStage3)
            {
                rt.pivot = new Vector2(0.5f, 0.8f);
                rt.sizeDelta = new Vector2(rt.sizeDelta.x + 20f, rt.sizeDelta.y + 20f);
            }
        }

        newShape.GetComponent<Image>().sprite = sprite;

        DragDrop dragScript = newShape.GetComponent<DragDrop>();
        if (dragScript != null)
        {
            dragScript.candyID = id;
            dragScript.allMolds = molds;
        }
    }

    void UnlockShapesForDragging()
    {
        if (shapesContainer.GetComponent<LayoutGroup>() != null)
            shapesContainer.GetComponent<LayoutGroup>().enabled = false;

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

    public void ShapePlacedCorrectly()
    {
        shapesCurrentlyPlaced++;
        if (shapesCurrentlyPlaced >= shapesNeededToPlace)
        {
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