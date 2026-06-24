// MY CODE START

using UnityEngine;

public class FishAgent : MonoBehaviour
{
    
    Vector3 localPosition;
    Vector3 localVelocity;

    [Header("Tank")]
    FishTank tank;

    [Header("Turning")]
    [SerializeField] float maxTurnForce = 8f;  // how sharply heading can change per step

    [Header("Swim speed")]
    [SerializeField] float minSpeed = 0.05f;
    [SerializeField] float maxSpeed = 0.4f;
    [SerializeField] float speedJitter = 8f;     // how quickly the fish's individual speed changes over time
    [SerializeField] float speedMatchRate = 1.5f;  // how strongly neighbors pull speed toward their average
    float currentSpeed;       // this fish's actual moving speed
    float personalSpeedGoal;  // preferred speed, when not influenced by flock

    [Header("Perception")]
    [SerializeField] float neighborRadius = 0.2f;

    [Header("Behavior weights")]
    [SerializeField] float separationWeight = 4f; 
    [SerializeField] float alignmentWeight = 3f; 
    [SerializeField] float cohesionWeight = 4f; 
    [SerializeField] float wanderWeight = 1f;
    [SerializeField] float containmentWeight = 1f; 

    [Header("Wander")] 
    [SerializeField] float wanderRadius = 0.3f; // size of the wander circle, higher value means fish meanders more 
    [SerializeField] float wanderDistance = 0.1f;
    [SerializeField] float wanderJitter = 20f; // how rapidly wanderAngle changes, ie. how often fish picks new direction
    float wanderAngle; // current angle of the wander target around the wander circle

    [Header("Wander Debug")]
    [SerializeField] bool drawGizmos = false;
    Vector3 debugCircleCenter;
    Vector3 debugWanderPoint;
    Quaternion debugWanderRotation;

    [Header("Orientation control")]
    [SerializeField] Transform pitchNode;
    float currentYaw;
    float currentPitch;

    // Entry point to initialize a fish
    public void Initialize(FishTank tank, Vector3 localPos)
    {
        this.tank = tank;

        localPosition = localPos;
        localVelocity = Random.onUnitSphere;

        personalSpeedGoal = Random.Range(minSpeed, maxSpeed);
        currentSpeed = personalSpeedGoal;

        transform.localPosition = localPosition;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Start fresh and recalculate main driving vector each frame 
        Vector3 steering = Vector3.zero;

        // Calculate vectors from each behavior separately
        Vector3 separation = Separation();
        Vector3 alignment = Alignment();
        Vector3 cohesion = Cohesion();
        Vector3 wander = Wander();
        Vector3 containment = tank.GetContainmentSteering(localPosition);


        // Sum up the behavior results, considering their weights
        steering += separation * separationWeight;
        steering += alignment * alignmentWeight;
        steering += cohesion * cohesionWeight;
        steering += wander * wanderWeight;
        steering += containment * containmentWeight;

        // limit the maximum heading change per frame to avoid abrupt turns
        steering = Vector3.ClampMagnitude(steering, maxTurnForce); 
    
        // Separate direction from speed through normalization
        // Steering only changes direction, and not magnitude
        // If the vector is nearly 0, then keep moving in whatever direction the fish is currently facing
        localVelocity += steering * dt;
        localVelocity = localVelocity.sqrMagnitude > 0.0001f
            ? localVelocity.normalized
            : transform.localRotation * Vector3.forward;

        UpdateSpeed(dt);

        // Apply speed to a normalized velocity
        localPosition += localVelocity * currentSpeed * dt;
        
        // Move the fish
        transform.localPosition = localPosition;

        // Control orientation without spinning
        Vector3 localDir = localVelocity.normalized;
        currentYaw = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
        currentPitch =
            -Mathf.Atan2(
                localDir.y,
                new Vector2(localDir.x, localDir.z).magnitude
            ) * Mathf.Rad2Deg;
        currentPitch = Mathf.Clamp(currentPitch, -85f, 85f);
        transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
        pitchNode.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

    }

    void UpdateSpeed(float dt)
    {
        personalSpeedGoal += Random.Range(-speedJitter, speedJitter) * dt;
        personalSpeedGoal = Mathf.Clamp(personalSpeedGoal, minSpeed, maxSpeed);

        float neighborSpeedSum = 0f;
        int neighborCount = 0;

        foreach (var other in tank.Fish)
        {
            if (other == this)
                continue;

            float dist = Vector3.Distance(localPosition, other.localPosition);

            if (dist < neighborRadius)
            {
                neighborSpeedSum += other.currentSpeed;
                neighborCount++;
            }
        }

        float targetSpeed = personalSpeedGoal;

        if (neighborCount > 0)
        {
            float neighborAvgSpeed = neighborSpeedSum / neighborCount;
            targetSpeed = Mathf.Lerp(personalSpeedGoal, neighborAvgSpeed, 0.6f);
        }

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            speedMatchRate * dt
        );

    }

    Vector3 Separation()
    {
        Vector3 force = Vector3.zero;
        int count = 0;

        foreach (var other in tank.Fish)
        {
            if (other == this)
                continue;

            float dist = Vector3.Distance(localPosition, other.localPosition);

            if (dist < neighborRadius && dist > 0.001f)
            {
                force += (localPosition - other.localPosition).normalized / dist;
                count++;
            }
        }

        if (count > 0)
            force /= count;

        return SeekDirection(force.normalized);
    }

    Vector3 Alignment()
    {
        Vector3 avgVel = Vector3.zero;
        int count = 0;

        foreach (var other in tank.Fish)
        {
            if (other == this)
                continue;

            float dist = Vector3.Distance(localPosition, other.localPosition);

            if (dist < neighborRadius)
            {
                avgVel += other.localVelocity;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        avgVel /= count;

        return SeekDirection(avgVel.normalized);
    }

    Vector3 Cohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (var other in tank.Fish)
        {
            if (other == this)
                continue;

            float dist = Vector3.Distance(localPosition, other.localPosition);

            if (dist < neighborRadius)
            {
                center += other.localPosition;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        center /= count;

        return Seek(center);
    }

    Vector3 Wander()
    {
        wanderAngle += Random.Range(-wanderJitter, wanderJitter) * Time.deltaTime;

        Vector3 forward = localVelocity;

        Vector3 circleOffset =
            new Vector3(
                Mathf.Cos(wanderAngle),
                Mathf.Sin(wanderAngle),
                0
            ) * wanderRadius;

        Quaternion rotation = transform.localRotation;

        Vector3 displacement = rotation * circleOffset;

        Vector3 circleCenter = localPosition + forward * wanderDistance;

        Vector3 target = circleCenter + displacement;

        debugCircleCenter = circleCenter;
        debugWanderPoint = target;
        debugWanderRotation = rotation;

        return Seek(target);
    }

    Vector3 Seek(Vector3 target)
    {
        Vector3 desiredDirection = (target - localPosition).normalized;

        return desiredDirection - localVelocity;
    }

    Vector3 SeekDirection(Vector3 desiredDirection)
    {
        return desiredDirection - localVelocity;
    }

 
    ////////////////////////////////////////
    // Debug drawing
    ////////////////////////////////////////
    
    void OnDrawGizmos()
    {
        if (!drawGizmos || tank == null)
            return;

        if (localVelocity.sqrMagnitude < 0.001f)
            return;

        // Convert local-space debug points into world space
        Vector3 worldCircleCenter = tank.transform.TransformPoint(debugCircleCenter);

        Vector3 worldWanderPoint = tank.transform.TransformPoint(debugWanderPoint);

        Gizmos.color = Color.yellow;
        DrawCircle(worldCircleCenter, debugWanderRotation, wanderRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            worldCircleCenter
        );

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            worldCircleCenter,
            worldWanderPoint
        );

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(
            worldWanderPoint,
            0.05f
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            neighborRadius
        );
    }

    void DrawCircle(Vector3 center, Quaternion rotation, float radius)
    {
        int segments = 32;

        Vector3 previous = center + rotation * new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector3 next =
                center +
                rotation *
                new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );

            Gizmos.DrawLine(previous, next);

            previous = next;
        }
    }
}

// MY CODE END