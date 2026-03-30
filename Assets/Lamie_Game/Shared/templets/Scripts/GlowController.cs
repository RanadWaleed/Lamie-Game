using UnityEngine;

using UnityEngine.UI;



public class GlowController : MonoBehaviour

{

    public GameObject myGlowEffect;



    public GameObject[] otherGlows;



    public void OnButtonClick()

    {

        foreach (GameObject glow in otherGlows)

        {

            if (glow != null)

                glow.SetActive(false);

        }



        if (myGlowEffect != null)

            myGlowEffect.SetActive(true);

    }

}