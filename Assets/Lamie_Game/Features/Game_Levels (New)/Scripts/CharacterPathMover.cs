using System.Collections;
using UnityEngine;

public class CharacterPathMover : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private RectTransform character;
    [Header("Path Points - In Order")]
    [SerializeField] private RectTransform[] pathPoints;
    [Header("Movement Settings")]
    [SerializeField, Min(0.1f)] private float speed = 300f;
    private Coroutine movementRoutine;
    private int reachedPointIndex;
    private bool initialized;

    public bool IsMoving { get; private set; }
    public int PathPointCount => pathPoints == null ? 0 : pathPoints.Length;
    public int CurrentSegmentIndex { get; private set; }
    public float SegmentProgress { get; private set; }
    public RectTransform GetPathPoint(int index) => pathPoints[index];

    private void Start()
    {
        if (!initialized) SetStationImmediate(0);
    }

    private bool HasValidPath()
    {
        if (character == null || PathPointCount < 2) return false;
        for (int i = 0; i < PathPointCount; i++)
            if (pathPoints[i] == null) return false;
        return true;
    }

    // Restores both the movement anchor and the reveal progress without replaying movement.
    public bool SetStationImmediate(int stationIndex)
    {
        if (!HasValidPath() || stationIndex < 0 || stationIndex > (PathPointCount - 1) / 2) return false;
        if (movementRoutine != null) StopCoroutine(movementRoutine);
        movementRoutine = null;
        IsMoving = false;
        reachedPointIndex = stationIndex * 2;
        character.position = pathPoints[reachedPointIndex].position;
        CurrentSegmentIndex = reachedPointIndex > 0 ? reachedPointIndex - 1 : 0;
        SegmentProgress = reachedPointIndex > 0 ? 1f : 0f;
        initialized = true;
        return true;
    }

    public void MoveToStation(int stationIndex)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || IsMoving || !HasValidPath()) return;
        // Preserve the existing three-station mapping: 0, 2, 4.
        if (stationIndex < 0 || stationIndex > (PathPointCount - 1) / 2) return;
        int targetIndex = stationIndex * 2;
        if (targetIndex <= reachedPointIndex) return;
        IsMoving = true;
        Coroutine routine = StartCoroutine(MoveAlongPath(targetIndex));
        // A path of coincident points can finish before StartCoroutine returns.
        movementRoutine = IsMoving ? routine : null;
    }

    private IEnumerator MoveAlongPath(int targetIndex)
    {
        for (int i = reachedPointIndex + 1; i <= targetIndex; i++)
        {
            CurrentSegmentIndex = i - 1;
            SegmentProgress = 0f;
            while (Vector3.Distance(character.position, pathPoints[i].position) > 0.5f)
            {
                character.position = Vector3.MoveTowards(character.position,
                    pathPoints[i].position, Mathf.Max(0.1f, speed) * Time.deltaTime);
                Vector3 delta = pathPoints[i].position - pathPoints[i - 1].position;
                SegmentProgress = delta.sqrMagnitude > 0.000001f
                    ? Mathf.Clamp01(Vector3.Dot(character.position - pathPoints[i - 1].position, delta) / delta.sqrMagnitude)
                    : 1f;
                yield return null;
            }
            character.position = pathPoints[i].position;
            SegmentProgress = 1f;
            reachedPointIndex = i;
        }
        IsMoving = false;
        movementRoutine = null;
    }

    private void OnDisable()
    {
        if (movementRoutine != null) StopCoroutine(movementRoutine);
        movementRoutine = null;
        IsMoving = false;
    }

    [ContextMenu("Test Move To Station 2")]
    private void TestMoveToStation2() => MoveToStation(1);
    [ContextMenu("Test Move To Station 3")]
    private void TestMoveToStation3() => MoveToStation(2);
}
