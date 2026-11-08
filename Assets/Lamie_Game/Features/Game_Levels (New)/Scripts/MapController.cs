
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [Header("Stations")]
    [SerializeField] private Button[] stations;

    [Header("Station Sprites")]
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite completedSprite;

    [Header("Temporary Test")]
    [SerializeField, Range(0, 3)]
    private int completedStations = 0;

    private void Start()
    {
        UpdateStations();
    }

    public void UpdateStations()
    {
        for (int i = 0; i < stations.Length; i++)
        {
            Image image = stations[i].GetComponent<Image>();

            if (i < completedStations)
            {
                image.sprite = completedSprite;
                stations[i].interactable = true;
            }
            else if (i == completedStations)
            {
                image.sprite = currentSprite;
                stations[i].interactable = true;
            }
            else
            {
                image.sprite = lockedSprite;
                stations[i].interactable = false;
            }
        }
    }

    [ContextMenu("Refresh Stations")]
    private void RefreshStations()
    {
        UpdateStations();
    }
}
