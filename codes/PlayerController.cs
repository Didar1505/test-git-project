
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    public float rotationSpeed = 12f;

    [Header("Jump")]
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && UnityEngine.Camera.main != null)
        {
            cameraTransform = UnityEngine.Camera.main.transform;
        }
    }

    void Update()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 inputDirection = new Vector3(moveX, 0f, moveZ);
        inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

        if (cameraTransform != null && inputDirection.sqrMagnitude > 0.01f)
        {
            // Movement relative to camera direction
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            Vector3 moveDirection =
                forward * inputDirection.z +
                right * inputDirection.x;

            moveDirection.Normalize();

            // Smoothly rotate character toward movement
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            controller.Move(
                moveDirection * speed * Time.deltaTime
            );
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
