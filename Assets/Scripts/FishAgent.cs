using UnityEngine;

public class FishAgent : MonoBehaviour
{
    FishTank tank;

    Vector3 localPosition;
    Vector3 localVelocity; 
    FishBody fishBody;
    float wanderAngle;
    Vector3 currentUp = Vector3.up;

    Vector3 debugCircleCenter;
    Vector3 debugWanderPoint;
    Vector3 debugTarget;

    [Header("Tank")]
    [SerializeField] float wallBuffer = 0.2f;

    [Header("Movement")]
    [SerializeField] float maxTurnForce = 8f;  // clamps how sharply heading can change per step

    [Header("Speed")]
    [SerializeField] float minSpeed = 0.05f;
    [SerializeField] float maxSpeed = 0.5f;
    [SerializeField] float speedJitter = 8f;     // how fast personal speed wanders per second
    [SerializeField] float speedMatchRate = 1.5f;  // how strongly neighbors pull speed toward their average

    float currentSpeed;       // this fish's actual moving speed, unit/sec
    float personalSpeedGoal;  // where this fish wants to be absent any flock influence

    [Header("Perception")]
    [SerializeField] float neighborRadius = 0.2f;

    [Header("Behavior weights")]
    [SerializeField] float separationWeight = 4f; //2.1f;
    [SerializeField] float alignmentWeight = 3f; //0.8f;
    [SerializeField] float cohesionWeight = 4f; //2f;
    [SerializeField] float wanderWeight = 1f; //0.5f;

    [Header("Wander")]
    [SerializeField] bool drawGizmos = false;
    [SerializeField] float wanderRadius = 0.3f;
    [SerializeField] float wanderDistance = 0.1f;
    [SerializeField] float wanderJitter = 10f;

    void Awake()
    {
        fishBody = GetComponentInChildren<FishBody>();
    }

    public void Initialize(FishTank tank, Vector3 localPos)
    {
        this.tank = tank;

        localPosition = localPos;

        // Velocity is purely a direction, magnitude always 1
        localVelocity = Random.onUnitSphere;

        personalSpeedGoal = Random.Range(minSpeed, maxSpeed);
        currentSpeed = personalSpeedGoal;

        transform.localPosition = localPosition;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        Vector3 steering = Vector3.zero;

        Vector3 separation = Separation();
        Vector3 alignment = Alignment();
        Vector3 cohesion = Cohesion();
        Vector3 wander = Wander();

        steering += separation * separationWeight;
        steering += alignment * alignmentWeight;
        steering += cohesion * cohesionWeight;
        steering += wander * wanderWeight;

        // Clamp how strongly heading can be pushed this frame
        steering = Vector3.ClampMagnitude(steering, maxTurnForce);

        // Steering only ever changes direction, never magnitude
        localVelocity += steering * dt;
        localVelocity = localVelocity.sqrMagnitude > 0.0001f
            ? localVelocity.normalized
            : transform.localRotation * Vector3.forward;

        // Move at a speed that drifts on its own but is pulled toward
        // the flock's average speed when neighbors are nearby
        UpdateSpeed(dt);
        localPosition += localVelocity * currentSpeed * dt;

        ContainInsideTank();

        transform.localPosition = localPosition;

        // Prevent spinning
        Vector3 worldVelocity =
            tank.transform.TransformDirection(localVelocity);

        Vector3 right = Vector3.Cross(currentUp, worldVelocity);
        currentUp = Vector3.Cross(worldVelocity, right).normalized;

        transform.rotation = Quaternion.LookRotation(
            worldVelocity,
            currentUp
        );
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

        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
        Debug.Log("currentSpeed: " + currentSpeed);
    }


    void ContainInsideTank()
    {
        Vector3 half = tank.tankSize * 0.5f;

        Vector3 target = localPosition;
        bool outside = false;

        if (localPosition.x > half.x - wallBuffer)
        {
            target.x = half.x - 1f;
            outside = true;
        }
        else if (localPosition.x < -half.x + wallBuffer)
        {
            target.x = -half.x + 1f;
            outside = true;
        }

        if (localPosition.y > half.y) // no buffer, allow jumping out of water
        {
            target.y = half.y - 1f;
            outside = true;
        }
        else if (localPosition.y < -half.y + wallBuffer)
        {
            target.y = -half.y + 1f;
            outside = true;
        }

        if (localPosition.z > half.z - wallBuffer)
        {
            target.z = half.z - 1f;
            outside = true;
        }
        else if (localPosition.z < -half.z + wallBuffer)
        {
            target.z = -half.z + 1f;
            outside = true;
        }

        if (outside)
        {
            localVelocity += Seek(target) * Time.deltaTime * 5f;
            localVelocity = localVelocity.normalized;
        }
    }

    Vector3 Separation()
    {
        Vector3 force = Vector3.zero;
        int count = 0;

        foreach (var other in tank.Fish)
        {
            if (other == this)
                continue;

            float dist =
                Vector3.Distance(localPosition, other.localPosition);

            if (dist < neighborRadius && dist > 0.001f)
            {
                force +=
                    (localPosition - other.localPosition).normalized
                    / dist;

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

            float dist =
                Vector3.Distance(localPosition, other.localPosition);

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

            float dist =
                Vector3.Distance(localPosition, other.localPosition);

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

        Vector3 forward = localVelocity; // always unit length already

        Vector3 circleOffset =
            new Vector3(
                Mathf.Cos(wanderAngle),
                Mathf.Sin(wanderAngle),
                0
            ) * wanderRadius;

        Quaternion rotation = transform.localRotation;

        Vector3 displacement = rotation * circleOffset;

        Vector3 circleCenter =
            localPosition + forward * wanderDistance;

        Vector3 target = circleCenter + displacement;

        debugCircleCenter = circleCenter;
        debugWanderPoint = target;
        debugTarget = target;

        return Seek(target);
    }

    // Returns a steering correction toward a target direction (unit vectors only)
    Vector3 Seek(Vector3 target)
    {
        Vector3 desiredDirection =
            (target - localPosition).normalized;

        return desiredDirection - localVelocity;
    }

    Vector3 SeekDirection(Vector3 desiredDirection)
    {
        return desiredDirection - localVelocity;
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        if (localVelocity.sqrMagnitude < 0.001f)
            return;

        Vector3 forward = localVelocity.normalized;

        Quaternion rotation =
            Quaternion.LookRotation(forward);

        Vector3 worldCircleCenter =
            transform.position +
            tank.transform.TransformDirection(
                debugCircleCenter - localPosition
            );

        Vector3 worldWanderPoint =
            transform.position +
            tank.transform.TransformDirection(
                debugWanderPoint - localPosition
            );

        Gizmos.color = Color.yellow;
        DrawCircle(worldCircleCenter, rotation, wanderRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, worldCircleCenter);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(worldCircleCenter, worldWanderPoint);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(worldWanderPoint, 0.05f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, neighborRadius);
    }

    void DrawCircle(Vector3 center, Quaternion rotation, float radius)
    {
        int segments = 32;

        Vector3 previous =
            center + rotation * new Vector3(radius, 0, 0);

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