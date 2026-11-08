
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

    public bool IsMoving => movementRoutine != null;

    private void Start()
    {
        // Place the characters at the first station.
        if (character != null && pathPoints.Length > 0)
        {
            character.position = pathPoints[0].position;
        }
    }

    public void MoveToStation(int stationIndex)
    {
        if (IsMoving || character == null)
            return;

        // Station 1 = Point 01
        // Station 2 = Point 03
        // Station 3 = Point 05
        int targetPointIndex = stationIndex * 2;

        if (targetPointIndex < 0 ||
            targetPointIndex >= pathPoints.Length)
            return;

        movementRoutine = StartCoroutine(
            MoveAlongPath(targetPointIndex)
        );
    }

    private IEnumerator MoveAlongPath(int targetIndex)
    {
        int nearestIndex = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < pathPoints.Length; i++)
        {
            float distance = Vector3.Distance(
                character.position,
                pathPoints[i].position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        for (int i = nearestIndex + 1; i <= targetIndex; i++)
        {
            while (Vector3.Distance(
                character.position,
                pathPoints[i].position
            ) > 0.5f)
            {
                character.position = Vector3.MoveTowards(
                    character.position,
                    pathPoints[i].position,
                    speed * Time.deltaTime
                );

                yield return null;
            }

            character.position = pathPoints[i].position;
        }

        movementRoutine = null;
    }

    [ContextMenu("Test Move To Station 2")]
    private void TestMoveToStation2()
    {
        MoveToStation(1);
    }

    [ContextMenu("Test Move To Station 3")]
    private void TestMoveToStation3()
    {
        MoveToStation(2);
    }
}
