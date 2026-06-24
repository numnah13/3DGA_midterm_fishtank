// MY CODE START

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{   
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSensitivity = 5f;
    
    private float yaw;
    private float pitch;


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Look();
        Move();
    }

    void Look()
    {
        // Map mouse screen coordinates onto pitch and yaw
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            yaw += mouseDelta.x * rotateSensitivity * Time.deltaTime; 
            pitch -= mouseDelta.y * rotateSensitivity * Time.deltaTime; 
        }
 
        // Rebuild new orientation fresh from the current pitch and yaw values
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f); 

    }


    void Move()
    {
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

        Vector3 deltaPosition = move * moveSpeed * Time.deltaTime;
        transform.position += deltaPosition;


        // Clamp position to contain within the room
        Vector3 position = transform.position;

        position.x = Mathf.Clamp(position.x, -8.5f, 2f);
        position.z = Mathf.Clamp(position.z, 0.5f, 6f);

        transform.position = position;
    }
}

// MY CODE END
