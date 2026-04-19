using UnityEngine;
using UnityEngine.UI;

public class setting : MonoBehaviour
{
    public Slider volumeSlider;

    [Header("UI Panels")]
    public GameObject settingsPanel;

    [Header("Character Display")]
    public Image characterPreview;
    public Sprite[] boySprites;
    public Sprite[] girlSprites;

    void Awake()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    void Start()
    {
        if (volumeSlider != null)
            volumeSlider.value = AudioListener.volume;

        LoadSelectedCharacter();
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            LoadSelectedCharacter();
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void LoadSelectedCharacter()
    {
        if (characterPreview == null)
        {
            Debug.LogError("CharacterPreview Image is NOT assigned in the Inspector!");
            return;
        }

        string gender = PlayerPrefs.GetString("UserGender", "أنثى").Trim();
        int savedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);


        Sprite[] activeArray = (gender == "ذكر" || gender == "ركذ") ? boySprites : girlSprites;

        if (activeArray != null && activeArray.Length > 0)
        {
            if (savedIndex >= activeArray.Length) savedIndex = 0;

            characterPreview.sprite = activeArray[savedIndex];
            Debug.Log("Home Scene Image Updated to: " + activeArray[savedIndex].name);

            characterPreview.SetAllDirty();
        }
        else
        {
            Debug.LogError("Sprite arrays are empty in the setting script!");
        }
    }

    public void ChangeCharacterScene()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.GoToState(GameFlowState.CharacterSelection);
    }

    public void ChangeVolume(float sliderValue)
    {
        AudioListener.volume = sliderValue;
    }

    public void LogoutScene()
    {
        if (GameFlowManager.Instance != null)
            GameFlowManager.Instance.GoToState(GameFlowState.Login);
    }
}