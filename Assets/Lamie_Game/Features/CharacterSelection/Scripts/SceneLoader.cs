using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject game1UI; // اسحبي أوبجكت اللعبة الأولى هنا
    public GameObject game2UI; // اسحبي أوبجكت اللعبة الثانية هنا

    public void ShowGame1()
    {
        game1UI.SetActive(true);
        game2UI.SetActive(false);
    }

    public void ShowGame2()
    {
        game1UI.SetActive(false);
        game2UI.SetActive(true);
    }

    public void StartSelect()
    {
        SceneManager.LoadScene("characterSelectionFinal");
    }
}