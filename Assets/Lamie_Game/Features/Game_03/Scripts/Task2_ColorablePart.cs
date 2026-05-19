using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Task2_ColorablePart : MonoBehaviour, IPointerClickHandler
{
    public string requiredColorID;
    public Color paintColor = Color.white;
    public Image partImage;
    public float fillSpeed = 2.0f;

    private bool isCorrectlyColored = false;
    private bool hasBeenAttempted = false;

    void Start()
    {
        if (partImage == null) partImage = GetComponent<Image>();

        if (partImage != null && partImage.sprite != null)
        {
            try
            {
                partImage.alphaHitTestMinimumThreshold = 0.1f;
            }
            catch (System.Exception)
            {
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isCorrectlyColored) return;

        string currentSelectedColor = Task2_Manager.Instance.GetActiveColor();
        Color colorToPaint = Task2_Manager.Instance.GetActiveColorValue();

        if (string.IsNullOrEmpty(currentSelectedColor)) return;

        partImage.color = colorToPaint;

        Task2_Manager.Instance.PlayColorSound();

        if (partImage.fillAmount < 1f)
        {
            StartCoroutine(FillColorAnimation());
        }

        if (currentSelectedColor == requiredColorID)
        {
            isCorrectlyColored = true;
            bool isFirstTry = !hasBeenAttempted;
            Task2_Manager.Instance.RegisterColorAttempt(true, isFirstTry);
        }
        else
        {
            hasBeenAttempted = true;
            Task2_Manager.Instance.RegisterColorAttempt(false, false);
        }
    }

    private IEnumerator FillColorAnimation()
    {
        if (partImage == null) yield break;

        float elapsed = 0f;
        while (elapsed < 1f)
        {
            partImage.fillAmount = Mathf.Lerp(0f, 1f, elapsed);
            elapsed += Time.deltaTime * fillSpeed;
            yield return null;
        }
        partImage.fillAmount = 1f;
    }
}