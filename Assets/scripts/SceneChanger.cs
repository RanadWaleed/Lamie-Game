using UnityEngine;
using UnityEngine.SceneManagement; 

public class SceneChanger : MonoBehaviour
{
    public void GoToScene(string comingSoonScene)
    {
        SceneManager.LoadScene(comingSoonScene);
    }
}