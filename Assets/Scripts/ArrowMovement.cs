// csharp
// File: Assets/Scripts/ArrowMovement.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowMovement : MonoBehaviour
{
    private RectTransform rectTransform;
    private Transform targetTransform;

    public float verticalOffset = 1.5f;
    public float rotationOffset = -90f;
    [Tooltip("Distance to project right-stick aim from the target")]
    public float aimDistance = 5f;
    [Tooltip("Right stick deadzone to consider as intentional aiming")]
    public float rightStickDeadzone = 0.15f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(Transform target)
    {
        targetTransform = target;
    }

    void Update()
    {
        if (targetTransform == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 worldTargetPosition = targetTransform.position + Vector3.up * verticalOffset;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldTargetPosition);
        rectTransform.position = screenPosition;

        // Prefer right stick if moved, otherwise use mouse world position
        Vector3 aimWorldPosition;
        Vector2 right = Vector2.zero;
        if (Gamepad.current != null)
        {
            right = Gamepad.current.rightStick.ReadValue();
        }

        if (right.sqrMagnitude > rightStickDeadzone * rightStickDeadzone)
        {
            Vector3 dir = new Vector3(right.x, right.y, 0f).normalized;
            aimWorldPosition = targetTransform.position + dir * aimDistance;
        }
        else
        {
            aimWorldPosition = GetMouseWorldPosition();
        }

        // Rotate arrow to point from the target to the aim position
        Vector3 direction = aimWorldPosition - targetTransform.position;
        direction.z = 0;
        if (direction.sqrMagnitude < 0.0001f) direction = Vector3.right; // default direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.localEulerAngles = new Vector3(0, 0, angle + rotationOffset);
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (Camera.main == null) return Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane gamePlane = new Plane(Vector3.forward, Vector3.zero);
        if (gamePlane.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            worldPosition.z = 0;
            return worldPosition;
        }
        return Vector3.zero;
    }
}
