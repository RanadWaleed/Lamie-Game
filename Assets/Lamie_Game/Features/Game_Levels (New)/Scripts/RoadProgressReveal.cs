using UnityEngine;
using UnityEngine.UI;

/// <summary>Reveals existing artwork; movement and station rules remain independent.</summary>
[DisallowMultipleComponent]
public sealed class RoadProgressReveal : MonoBehaviour
{
    private const int MaxPoints = 64;
    [SerializeField] private CharacterPathMover movementSource;
    [SerializeField] private Image roadGreen;
    [SerializeField] private Shader revealShader;
    [SerializeField, Min(0.01f)] private float edgeSoftness = 2f;
    private readonly Vector4[] points = new Vector4[MaxPoints];
    private Material runtimeMaterial;
    private Material previousMaterial;
    private bool previousActive;

    private void OnEnable()
    {
        if (movementSource == null || roadGreen == null || revealShader == null ||
            !revealShader.isSupported || movementSource.PathPointCount < 2 ||
            movementSource.PathPointCount > MaxPoints)
        {
            Debug.LogError("Road reveal needs a mover, green Image, supported shader and 2-64 path points.", this);
            enabled = false;
            return;
        }
        for (int i = 0; i < movementSource.PathPointCount; i++)
            if (movementSource.GetPathPoint(i) == null)
            {
                Debug.LogError("Road reveal: a path point is missing.", this);
                enabled = false;
                return;
            }
        previousMaterial = roadGreen.material;
        previousActive = roadGreen.gameObject.activeSelf;
        runtimeMaterial = new Material(revealShader) { name = "Road Reveal (Runtime)" };
        roadGreen.material = runtimeMaterial;
        ApplyProgress(); // Hide before activating the overlay.
        roadGreen.gameObject.SetActive(true);
        Canvas.willRenderCanvases += ApplyProgress;
    }

    private void ApplyProgress()
    {
        if (runtimeMaterial == null || roadGreen == null || movementSource == null) return;
        int count = movementSource.PathPointCount;
        if (count < 2 || count > MaxPoints) return;
        float total = 0f;
        for (int i = 0; i < count; i++)
        {
            RectTransform point = movementSource.GetPathPoint(i);
            if (point == null) return;
            Vector3 local = roadGreen.rectTransform.InverseTransformPoint(point.position);
            if (i > 0) total += Vector2.Distance(new Vector2(points[i - 1].x, points[i - 1].y), local);
            points[i] = new Vector4(local.x, local.y, total, 0f);
        }
        int segment = Mathf.Clamp(movementSource.CurrentSegmentIndex, 0, count - 2);
        float distance = Mathf.Lerp(points[segment].z, points[segment + 1].z, movementSource.SegmentProgress);
        SetProperties(runtimeMaterial, count, distance, total);
        // Mask may create a stencil material; update the actual rendering copy too.
        Material rendered = roadGreen.materialForRendering;
        if (rendered != null && rendered != runtimeMaterial) SetProperties(rendered, count, distance, total);
    }

    private void SetProperties(Material material, int count, float distance, float total)
    {
        material.SetVectorArray("_PathPoints", points);
        material.SetInt("_PointCount", count);
        material.SetFloat("_ProgressDistance", distance);
        material.SetFloat("_TotalDistance", total);
        material.SetFloat("_EdgeSoftness", Mathf.Max(0.01f, edgeSoftness));
    }

    private void OnDisable()
    {
        Canvas.willRenderCanvases -= ApplyProgress;
        if (runtimeMaterial == null) return;
        if (roadGreen != null)
        {
            roadGreen.material = previousMaterial;
            roadGreen.gameObject.SetActive(previousActive);
        }
        Destroy(runtimeMaterial);
        runtimeMaterial = null;
    }
}
