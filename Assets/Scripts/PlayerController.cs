// SOURCE: adapted from https://www.youtube.com/watch?v=_QajrabyTJc

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSensitivity = 5f;
    
    private float gravity = -9.81f;
    private float verticalVelocity;
    private float yaw;
    private float pitch;

    private CharacterController characterController;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        Look();
        Move();
    }

    void Look()
    {
        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            yaw += delta.x * rotateSensitivity * Time.deltaTime;
            pitch -= delta.y * rotateSensitivity * Time.deltaTime;
        }

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Move()
    {
        // Gravity
        if (!characterController.isGrounded)
            verticalVelocity += gravity * Time.deltaTime;

        // Movement directions on horizontal plane only
        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = transform.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
            move += forward;

        if (Keyboard.current.sKey.isPressed)
            move -= forward;

        if (Keyboard.current.aKey.isPressed)
            move -= right;

        if (Keyboard.current.dKey.isPressed)
            move += right;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }
}