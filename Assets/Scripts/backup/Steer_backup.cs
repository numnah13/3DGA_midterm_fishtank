using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class Steer_backup : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] private float flySpeed = 5.0f;

    public bool isGrounded;

    private Vector3 velocity;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float maxForce = 10f;
   //private float verticalVelocity = 0.0f;
    private Collider droneCollider;
    private float droneBottom;

    [SerializeField] private float avoidanceSensorRadius = 0.5f;
    [SerializeField] private float avoidanceLookahead = 0.3f;

    private float upInput;
    private float forwardInput;
    private float rightInput;

    

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        droneCollider = GetComponent<Collider>();

        isGrounded = true;
    }


    private void FixedUpdate()
    {
        isGrounded = false;

        ReadInput();

        Vector3 desired = new Vector3(rightInput * flySpeed, upInput * flySpeed, forwardInput * flySpeed);
        
        Vector3 steering = (desired - velocity);
        steering = Vector3.ClampMagnitude(steering, maxForce);

        steering += AvoidObstacles();
        velocity += steering * Time.fixedDeltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        Vector3 newPos = transform.position + velocity * Time.fixedDeltaTime;

        ApplyGround(ref newPos); // should be: (ref newPos)

        

        // Vector3 steering = 
        //     Seek(target) +
        //     Separation() +
        //     Alignment() +
        //     Cohesion() +
        //     AvoidObstacles();



        rb.MovePosition(newPos);

        Debug.Log("Drone position: " + rb.position);

    }

    private void ReadInput()
    {
        upInput = 0f;
        forwardInput = 0f;
        rightInput = 0f;

        if (Keyboard.current.qKey.isPressed)
            upInput += 1f;

        if (Keyboard.current.eKey.isPressed)
            upInput -= 1f;

        if (Keyboard.current.dKey.isPressed)
            rightInput += 1f;

        if (Keyboard.current.aKey.isPressed)
            rightInput -= 1f;

        if (Keyboard.current.wKey.isPressed)
            forwardInput += 1f;

        if (Keyboard.current.sKey.isPressed)
            forwardInput -= 1f;
    }

    private void ApplyGround(ref Vector3 pos)
    {
        droneBottom = transform.position.y
                                 - droneCollider.bounds.min.y;
        
        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, droneBottom + 0.02f))
        {
            if (velocity.y < 0f)
                velocity.y = 0f;

            pos.y = hit.point.y + droneBottom;
            isGrounded = true;
        }
    }



    private Vector3 AvoidObstacles()
    {
        Vector3 dir = velocity.normalized;

        if (dir.sqrMagnitude < 0.001f)
            return Vector3.zero;

        float lookaheadDistance = velocity.magnitude * avoidanceLookahead;
        
        DebugSphereCast(transform.position, dir, avoidanceSensorRadius, lookaheadDistance);

        if (Physics.SphereCast(transform.position, avoidanceSensorRadius, dir, out RaycastHit hit, lookaheadDistance))
        {
            Vector3 avoidDir = Vector3.ProjectOnPlane(dir, hit.normal);
            float strength = 1f - hit.distance / lookaheadDistance;
            return (avoidDir - dir) * maxForce * strength;
        }

        return Vector3.zero;
    }


    private void DebugSphereCast(Vector3 origin, Vector3 direction, float radius, float distance)
    {
        Vector3 end = origin + direction * distance;

        // draw start and end spheres
        DrawWireSphere(origin, radius, Color.cyan);
        DrawWireSphere(end, radius, Color.cyan);

        // connect centers
        Debug.DrawLine(origin, end, Color.cyan);

        // optional: show movement direction ray
        Debug.DrawRay(origin, direction * distance, Color.red);
    }
    
    private void DrawWireSphere(Vector3 center, float radius, Color color)
    {   
        int segments = 16;

        Vector3 prevX = center + new Vector3(radius, 0, 0);
        Vector3 prevY = center + new Vector3(0, radius, 0);
        Vector3 prevZ = center + new Vector3(0, 0, radius);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector3 nextX = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );

            Vector3 nextY = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );

            Vector3 nextZ = center + new Vector3(
                0,
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );

            Debug.DrawLine(prevX, nextX, color);
            Debug.DrawLine(prevY, nextY, color);
            Debug.DrawLine(prevZ, nextZ, color);

            prevX = nextX;
            prevY = nextY;
            prevZ = nextZ;
        }
    }
}
