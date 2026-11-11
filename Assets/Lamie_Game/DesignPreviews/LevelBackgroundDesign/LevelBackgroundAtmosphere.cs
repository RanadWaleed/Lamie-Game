using UnityEngine;

namespace Lamie.DesignPreview
{
    // This component belongs exclusively to the independent design scene.
    public sealed class LevelBackgroundAtmosphere : MonoBehaviour
    {
        public Transform[] clouds;
        public SpriteRenderer[] sparkles;
        public SpriteRenderer[] lanternGlows;
        Vector3[] origins;

        void OnEnable()
        {
            origins = new Vector3[clouds.Length];
            for (int i = 0; i < clouds.Length; i++)
                if (clouds[i]) origins[i] = clouds[i].localPosition;
        }

        void Update()
        {
            float t = Time.timeSinceLevelLoad;
            for (int i = 0; i < clouds.Length; i++)
            {
                if (!clouds[i]) continue;
                Vector3 p = origins[i];
                p.x += Mathf.Sin(t * (0.055f + i * 0.009f) + i) * 0.6f;
                p.y += Mathf.Sin(t * 0.16f + i) * 0.045f;
                clouds[i].localPosition = p;
            }
            for (int i = 0; i < sparkles.Length; i++)
            {
                if (!sparkles[i]) continue;
                Color c = sparkles[i].color;
                c.a = Mathf.Pow((Mathf.Sin(t * 0.9f + i * 2.4f) + 1f) * 0.5f, 3f) * 0.65f;
                sparkles[i].color = c;
            }
            for (int i = 0; i < lanternGlows.Length; i++)
            {
                if (!lanternGlows[i]) continue;
                Color c = lanternGlows[i].color;
                c.a = 0.12f + Mathf.Sin(t * 1.2f + i) * 0.025f;
                lanternGlows[i].color = c;
            }
        }
    }
}
