using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    private Transform focus = default;

    [Header("Position")]
    [SerializeField, Range(1f, 30f)]
    private float distance = 5f;

    [SerializeField, Min(0f)]
    private float focusRadius = 1f;

    [SerializeField, Range(0f, 1f)]
    private float focusCentering = 0.5f;

    [Header("Rotation")]
    [SerializeField, Range(1f, 360f)]
    private float rotationSpeed = 90f;

    [SerializeField, Range(-89f, 89f)]
    private float minVerticalAngle = -30f, maxVerticalAngle = 60f;

    [SerializeField, Min(0f)]
    private float alignDelay = 5f;

    [SerializeField, Range(0f, 90f)]
    private float alignSmoothRange = 45f;

    private Vector3 focusPoint, previousFocusPoint;
    private Vector2 orbitAngles = new Vector2(45f, 0f);
    private float lastManualRotationTime;

    private void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle) maxVerticalAngle = minVerticalAngle;
    }

    private void Awake()
    {
        focusPoint = focus.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }

    private void Update()
    {
    }

    private void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion lookRotation;
        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            lookRotation = Quaternion.Euler(orbitAngles);
        }
        else
        {
            lookRotation = transform.localRotation;
        }
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        transform.SetPositionAndRotation(lookPosition, lookRotation);

        void UpdateFocusPoint()
        {
            previousFocusPoint = focusPoint;
            Vector3 targetPoint = focus.position;
            if (focusRadius > 0f)
            {
                float distance = Vector3.Distance(targetPoint, focusPoint);
                float t = 1f;
                if (distance > 0.01f && focusCentering > 0f)
                {
                    t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
                }
                if (distance > focusRadius)
                {
                    t = Mathf.Min(t, focusRadius / distance);
                }
                focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
            }
            else
            {
                focusPoint = targetPoint;
            }
        }
        bool ManualRotation()
        {
            if (mouseRotated) { mouseRotated = false; lastManualRotationTime = Time.unscaledTime; return true; }
            if (lastStickInput.magnitude < 0.01f) return false;
            lastManualRotationTime = Time.unscaledTime;
            orbitAngles.x -= rotationSpeed * Time.unscaledDeltaTime * lastStickInput.y;
            orbitAngles.y += rotationSpeed * Time.unscaledDeltaTime * lastStickInput.x;
            return true;
        }
        bool AutomaticRotation()
        {
            if (Time.unscaledTime - lastManualRotationTime < alignDelay)
                return false;

            Vector2 movement = new(focusPoint.x - previousFocusPoint.x, focusPoint.z - previousFocusPoint.z);
            float movementDeltaSqr = movement.sqrMagnitude;
            if (movementDeltaSqr < 0.0001f) return false;

            float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
            float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
            float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
            if (deltaAbs < alignSmoothRange) rotationChange *= deltaAbs / alignSmoothRange;
            else if (180f - deltaAbs < alignSmoothRange) rotationChange *= (180f - deltaAbs) / alignSmoothRange;
            orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
            return true;

            static float GetAngle(Vector2 direction)
            {
                float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
                return direction.x < 0f ? 360f - angle : angle;
            }
        }
        void ConstrainAngles()
        {
            orbitAngles.x =
                Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

            if (orbitAngles.y < 0f)
            {
                orbitAngles.y += 360f;
            }
            else if (orbitAngles.y >= 360f)
            {
                orbitAngles.y -= 360f;
            }
        }
    }

    #region InputSystem

#pragma warning disable IDE0051

    private Vector2 lastStickInput;
    private bool mouseRotated = false;

    private void OnCameraStick(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        lastStickInput = input;
    }

    private void OnCameraMouse(InputValue value)
    {
        lastStickInput = Vector2.zero;
        mouseRotated = true;
        Vector2 input = value.Get<Vector2>();
        const float e = 0.001f;
        if (input.x >= -e && input.x <= e && input.y >= -e && input.y <= e) return;
        orbitAngles.x -= rotationSpeed * 0.01f * input.y;
        orbitAngles.y += rotationSpeed * 0.01f * input.x;
    }

#pragma warning restore IDE0051

    #endregion InputSystem
}