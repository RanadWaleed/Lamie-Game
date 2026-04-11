using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;


public class PaintAlignmentManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public RectTransform paintingRect;
    public RectTransform hangPoint;
    public RectTransform parentCanvas;

    [Header("Settings")]
    public float snapDistance = 150f;

    private bool isHanged = false;

    [Header("Lights")]
    public RectTransform rLight;
    public RectTransform lLight;
    public AudioSource audioSource;
    public AudioClip hangSound;
    public float lightDropFrom = 1200f;
    public float lightDropDuration = 1f;


    [Header("Next Button")]
    public CanvasGroup nextButton;
    void OnEnable()
    {
        isHanged = false;
        paintingRect.anchoredPosition = new Vector2(0, -300f);
        nextButton.alpha = 0;
        nextButton.interactable = false;
        nextButton.blocksRaycasts = false;

        rLight.anchoredPosition = new Vector2(534, lightDropFrom);
        lLight.anchoredPosition = new Vector2(-559, lightDropFrom);


    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isHanged) return;

        float dist = Vector2.Distance(paintingRect.position, hangPoint.position);

        if (dist < snapDistance)
        {
            paintingRect.position = hangPoint.position;
            isHanged = true;

            if (audioSource && hangSound) audioSource.PlayOneShot(hangSound);
            StartCoroutine(DropLights());
        }
    }

    IEnumerator DropLights()
    {
        float t = 0;
        Vector2 rStart = rLight.anchoredPosition;
        Vector2 lStart = lLight.anchoredPosition;
        Vector2 rEnd = new Vector2(534, 389.1598f);
        Vector2 lEnd = new Vector2(-559, 389.1598f);

        while (t < 1)
        {
            t += Time.deltaTime / lightDropDuration;
            rLight.anchoredPosition = Vector2.Lerp(rStart, rEnd, Mathf.SmoothStep(0, 1, t));
            lLight.anchoredPosition = Vector2.Lerp(lStart, lEnd, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        rLight.anchoredPosition = rEnd;
        lLight.anchoredPosition = lEnd;

        if (audioSource && hangSound)
            yield return new WaitForSeconds(hangSound.length);

        yield return StartCoroutine(FadeIn(nextButton, 1f));
    }

    IEnumerator FadeIn(CanvasGroup cg, float duration)
    {
        cg.alpha = 0;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            cg.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    public void OnBeginDrag(PointerEventData eventData) { if (isHanged) return; }

    public void OnDrag(PointerEventData eventData)
    {
        if (isHanged) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas, eventData.position, eventData.pressEventCamera, out localPoint);

        paintingRect.anchoredPosition = localPoint;
    }


}