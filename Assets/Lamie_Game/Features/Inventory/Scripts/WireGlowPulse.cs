using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WireGlowPulse : MonoBehaviour
{
    [Header("Glow Wires")]
    public Image[] glowWires;

    [Range(0f, 1f)] public float minAlpha = 0.05f;
    [Range(0f, 1f)] public float maxAlpha = 1f;
    public float glowOnTime = 1.2f;
    public float glowOffTime = 0.8f;
    public float fadeDuration = 0.4f;

    private void Start() => StartCoroutine(PulseLoop());

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return Fade(minAlpha, maxAlpha);
            yield return new WaitForSeconds(glowOnTime);
            yield return Fade(maxAlpha, minAlpha);
            yield return new WaitForSeconds(glowOffTime);
        }
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / fadeDuration);
            foreach (var w in glowWires)
            {
                if (w == null) continue;
                Color c = w.color; c.a = a; w.color = c;
            }
            yield return null;
        }
    }
}
