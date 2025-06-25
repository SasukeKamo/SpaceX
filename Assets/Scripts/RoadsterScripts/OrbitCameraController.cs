using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class OrbitCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform sun;

    [Header("Camera Settings")]
    [SerializeField] private float distance = 20f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 60f;
    [SerializeField] private float rotationSpeed = 0.5f;
    [SerializeField] private float pinchSpeed = 0.5f;

    [Header("Rotation Limits")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Input Sensitivity")]
    [SerializeField] private float mouseSensitivityMultiplier = 10f;
    [SerializeField] private float mouseScrollSensitivity = 50f;
    [SerializeField] private float mouseDeltaScale = 0.1f;
    [SerializeField] private float scrollWheelScale = 0.01f;
    [SerializeField] private float pinchDistanceScale = 0.1f;

    private float currentX = 0f;
    private float currentY = 20f;

    private void Start()
    {
        EnhancedTouchSupport.Enable();

        //set initial camera position
        UpdateCameraPosition();
    }

    private void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Update()
    {
        HandleTouchInput();
        HandleMouseInput();

        UpdateCameraPosition();
    }

    private void HandleTouchInput()
    {
        var touches = Touch.activeTouches;

        //single touch - rotation
        if (touches.Count == 1)
        {
            var touch = touches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector2 deltaPosition = touch.delta;

                currentX += deltaPosition.x * rotationSpeed;
                currentY -= deltaPosition.y * rotationSpeed;

                currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
            }
        }
        //two touch - pinch zoom
        else if (touches.Count == 2)
        {
            var touch0 = touches[0];
            var touch1 = touches[1];

            if (touch0.phase == UnityEngine.InputSystem.TouchPhase.Moved ||
                touch1.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                float currentDistance = Vector2.Distance(touch0.screenPosition, touch1.screenPosition);

                float lastDistance = Vector2.Distance(
                    touch0.screenPosition - touch0.delta,
                    touch1.screenPosition - touch1.delta
                );

                float deltaPinch = currentDistance - lastDistance;

                distance -= deltaPinch * pinchSpeed * pinchDistanceScale;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }
    }

    //mouse input for debug in editor
    private void HandleMouseInput()
    {
        var mouse = Mouse.current;

        if (mouse != null)
        {
            if (mouse.leftButton.isPressed)
            {
                Vector2 mouseDelta = mouse.delta.ReadValue() * mouseDeltaScale;

                currentX += mouseDelta.x * rotationSpeed * mouseSensitivityMultiplier;
                currentY -= mouseDelta.y * rotationSpeed * mouseSensitivityMultiplier;

                currentY = Mathf.Clamp(currentY, minVerticalAngle, maxVerticalAngle);
            }


            float scroll = mouse.scroll.y.ReadValue() * scrollWheelScale;

            if (scroll != 0)
            {
                distance -= scroll * mouseScrollSensitivity;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }
    }

    private void UpdateCameraPosition()
    {
        if (sun == null) return;

        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        Vector3 position = rotation * new Vector3(0, 0, -distance) + sun.position;

        transform.position = position;
        transform.LookAt(sun);
    }
}