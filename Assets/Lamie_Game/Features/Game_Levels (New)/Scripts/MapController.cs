using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [Header("Stations")]
    [SerializeField] private Button[] stations;
    [Header("Station Sprites")]
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite completedSprite;
    [Header("Station Scenes - In Order")]
    [SerializeField] private string[] levelScenes = { "Game01Scene", "Game02Scene", "Game_03" };

    private readonly LocalAssessmentStore assessmentStore =
        new LocalAssessmentStore(new AssessmentLocalPersistence());
    private UnityAction[] clickHandlers;
    private bool isLoading;
    private bool isPositioning;
    private CharacterPathMover movement;
    private Coroutine positioningRoutine;

    private void OnEnable()
    {
        isLoading = false;
        isPositioning = true;
        movement = GetComponent<CharacterPathMover>();
        if (stations == null || levelScenes == null || stations.Length != 3 || levelScenes.Length != 3)
        {
            Debug.LogError("Map requires three station buttons and three game scenes.", this);
            if (stations != null)
                foreach (Button button in stations)
                    if (button != null) button.interactable = false;
            return;
        }
        clickHandlers = new UnityAction[stations.Length];
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] == null) continue;
            int index = i;
            clickHandlers[i] = () => OpenStation(index);
            stations[i].onClick.AddListener(clickHandlers[i]);
        }
        UpdateStations();
        positioningRoutine = StartCoroutine(RestoreMapPosition());
    }

    private bool IsComplete(int index)
    {
        string child = ChildLocalProfile.CurrentChildId;
        if (string.IsNullOrWhiteSpace(child) || string.IsNullOrWhiteSpace(levelScenes[index])) return false;
        return PlayerPrefs.GetInt("lamie_" + child + "_game_" + levelScenes[index], 0) == 1 ||
            assessmentStore.HasBaseline(child, levelScenes[index]);
    }

    private int FirstIncompleteStation()
    {
        int index = 0;
        while (index < levelScenes.Length && IsComplete(index)) index++;
        return index;
    }

    public void UpdateStations()
    {
        if (stations == null || levelScenes == null || stations.Length != levelScenes.Length) return;
        int current = FirstIncompleteStation();
        for (int i = 0; i < stations.Length; i++)
        {
            Button button = stations[i];
            if (button == null) continue;
            Image image = button.GetComponent<Image>();
            bool completed = IsComplete(i);
            if (image != null)
                image.sprite = completed ? completedSprite :
                    ChildLocalProfile.HasIdentity && i == current ? currentSprite : lockedSprite;
            button.interactable = ChildLocalProfile.HasIdentity && !isLoading && !isPositioning && i <= current;
        }
    }

    public void OpenStation(int stationIndex)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || isLoading || isPositioning || !ChildLocalProfile.HasIdentity ||
            levelScenes == null || stationIndex < 0 || stationIndex >= levelScenes.Length ||
            stationIndex > FirstIncompleteStation()) return;
        string sceneName = levelScenes[stationIndex];
        GameFlowState state;
        switch (sceneName)
        {
            case "Game01Scene": state = GameFlowState.Game01; break;
            case "Game02Scene": state = GameFlowState.Game02; break;
            case "Game_03": state = GameFlowState.Game03; break;
            default: Debug.LogError("Unknown station scene: " + sceneName, this); return;
        }
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Station scene is missing from the build scene list: " + sceneName, this);
            return;
        }
        isLoading = true;
        UpdateStations();
        if (GameFlowManager.Instance != null) GameFlowManager.Instance.GoToState(state);
        else SceneManager.LoadScene(sceneName);
    }

    public void ReturnHome()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || isLoading) return;
        const string homeScene = "HomeScene";
        if (!Application.CanStreamedLevelBeLoaded(homeScene))
        {
            Debug.LogError("Home scene is missing from the build scene list: " + homeScene, this);
            return;
        }
        isLoading = true;
        UpdateStations();
        if (GameFlowManager.Instance != null) GameFlowManager.Instance.GoToState(GameFlowState.Home);
        else SceneManager.LoadScene(homeScene);
    }

    private IEnumerator RestoreMapPosition()
    {
        // Wait for the mover's Start before restoring the last visited station.
        yield return null;
        if (movement == null || levelScenes.Length * 2 - 1 > movement.PathPointCount)
        {
            Debug.LogError("Map movement references are incomplete.", this);
            positioningRoutine = null;
            yield break;
        }
        if (!ChildLocalProfile.HasIdentity)
        {
            movement.SetStationImmediate(0);
            isPositioning = false;
            UpdateStations();
            positioningRoutine = null;
            yield break;
        }
        string child = ChildLocalProfile.CurrentChildId;
        string positionKey = ChildLocalProfile.Key(child, "MapStation");
        int target = Mathf.Min(FirstIncompleteStation(), levelScenes.Length - 1);
        int previous = Mathf.Clamp(PlayerPrefs.GetInt(positionKey, target), 0, target);
        if (!movement.SetStationImmediate(previous))
        {
            Debug.LogError("Map character could not be positioned.", this);
            positioningRoutine = null;
            yield break;
        }
        if (previous < target)
        {
            movement.MoveToStation(target);
            while (movement.IsMoving) yield return null;
        }
        if (child == ChildLocalProfile.CurrentChildId)
        {
            PlayerPrefs.SetInt(positionKey, target);
            PlayerPrefs.Save();
        }
        isPositioning = false;
        UpdateStations();
        positioningRoutine = null;
    }

    private void OnDisable()
    {
        if (positioningRoutine != null) StopCoroutine(positioningRoutine);
        positioningRoutine = null;
        if (clickHandlers == null || stations == null) return;
        for (int i = 0; i < clickHandlers.Length && i < stations.Length; i++)
            if (stations[i] != null && clickHandlers[i] != null)
                stations[i].onClick.RemoveListener(clickHandlers[i]);
        clickHandlers = null;
    }

    [ContextMenu("Refresh Stations")]
    private void RefreshStations() => UpdateStations();
}
