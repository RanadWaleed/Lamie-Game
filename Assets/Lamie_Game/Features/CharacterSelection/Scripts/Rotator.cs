using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Rotator : MonoBehaviour
{
    public float autoRotationSpeed = 30f;
    public float manualRotationSpeed = 0.5f;
    private bool isDragging = false;

    void Update()
    {
        bool pointerDown = false;
        bool pointerUp = false;
        float dragX = 0f;

        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.touches[0];
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began) pointerDown = true;
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled) pointerUp = true;
            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved) dragX = touch.delta.x.ReadValue();
        }
        else if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame) pointerDown = true;
            if (Mouse.current.leftButton.wasReleasedThisFrame) pointerUp = true;
            dragX = Mouse.current.delta.x.ReadValue();
        }

        if (pointerDown)
        {
            if (EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject())
            {
                isDragging = true;
            }
        }

        if (pointerUp)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            transform.Rotate(0, -dragX * manualRotationSpeed, 0, Space.World);
        }
        else
        {
            transform.Rotate(0, autoRotationSpeed * Time.deltaTime, 0, Space.World);
        }
    }

    void OnEnable()
    {
        isDragging = false;
    }
}