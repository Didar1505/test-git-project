
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Settings")]
    public float distance = 4.5f;
    public float height = 1.5f;
    public float smoothSpeed = 10f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2f;
    public float minPitch = -20f;
    public float maxPitch = 65f;

    private float yaw;
    private float pitch = 20f;

    void Start()
    {
        if (target == null) return;

        yaw = target.eulerAngles.y;

        // Position camera immediately
        UpdateCameraPosition(true);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Mouse controls camera rotation
        yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        UpdateCameraPosition(false);
    }

    void UpdateCameraPosition(bool instant)
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 lookTarget = target.position + Vector3.up * height;

        Vector3 desiredPosition =
            lookTarget - rotation * Vector3.forward * distance;

        if (instant)
        {
            transform.position = desiredPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                1f - Mathf.Exp(-smoothSpeed * Time.deltaTime)
            );
        }

        // Always look at the character
        transform.LookAt(lookTarget);
    }
}
