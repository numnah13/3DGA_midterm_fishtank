using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotateSensitivity = 10f;
    private float yaw;
    private float pitch;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Look around
        Vector2 delta = Mouse.current.delta.ReadValue();
        yaw += delta.x * rotateSensitivity * Time.deltaTime;
        pitch -= delta.y * rotateSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);


        // Move 
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

        if (move.sqrMagnitude > 0)
        {
            move.Normalize();

            transform.position += move * moveSpeed * Time.deltaTime;
        }

    }
}