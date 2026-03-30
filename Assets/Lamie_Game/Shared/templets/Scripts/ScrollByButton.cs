using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonScroller : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float smoothTime = 0.2f;

    public void MoveRight()
    {
        ExecuteMove(1);
    }

    public void MoveLeft()
    {
        ExecuteMove(-1);
    }

    private void ExecuteMove(int direction)
    {
        int childCount = scrollRect.content.childCount;

        if (childCount <= 1) return;

        StopAllCoroutines();

        float step = 1f / (childCount - 1f);
        float targetPos = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition + (step * direction));

        StartCoroutine(SmoothScroll(targetPos));
    }

    IEnumerator SmoothScroll(float target)
    {
        float startPos = scrollRect.horizontalNormalizedPosition;
        float time = 0;
        while (time < smoothTime)
        {
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startPos, target, time / smoothTime);
            time += Time.deltaTime;
            yield return null;
        }
        scrollRect.horizontalNormalizedPosition = target;
    }
}