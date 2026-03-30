using UnityEngine;
using RTLTMPro; 

public class CharacterManager : MonoBehaviour
{
    public GameObject[] allCharacters;
    public GameObject[] allGlows;

    public RTLTextMeshPro mainTitleText;

    public void SelectCharacter(int index)
    {
        for (int i = 0; i < allCharacters.Length; i++)
        {
            if (allCharacters[i] != null) allCharacters[i].SetActive(false);
            if (allGlows[i] != null) allGlows[i].SetActive(false);
        }

        if (allCharacters[index] != null) allCharacters[index].SetActive(true);
        if (allGlows[index] != null) allGlows[index].SetActive(true);

        if (mainTitleText != null)
        {
            if (index == 0) mainTitleText.text = "الجنوبية";
            else if (index == 1) mainTitleText.text = "الشمالية";
            else if (index == 2) mainTitleText.text = "الغربية";
            else if (index == 3) mainTitleText.text = "الوسطى";

            mainTitleText.UpdateText();
        }
    }
}