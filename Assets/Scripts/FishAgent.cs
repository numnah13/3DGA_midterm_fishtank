using UnityEngine;
using System.Collections.Generic;

public class FishAgent : MonoBehaviour
{
    FishTank tank;

    Vector3 localPosition;
    Vector3 localVelocity;

    Vector3 debugCircleCenter;
    Vector3 debugWanderPoint;
    Vector3 debugTarget;

    [SerializeField] private bool drawGizmos = false;

    public float maxSpeed = 1f;
    public float maxForce = 2f;

    public float neighborRadius = 0.2f;

    public float separationWeight = 2f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 2f;
    public float wanderWeight = 0.5f;  // 1, 0.5

    float wanderAngle;
    [SerializeField] float wanderRadius = 0.3f;
    [SerializeField] float wanderDistance = 0.5f;
    [SerializeField] float wanderJitter = 30f;

    void Start()
    {

    }

    public void Initialize(FishTank tank, Vector3 localPos)
    {
        this.tank = tank;

        localPosition = localPos;

        localVelocity =
            Random.onUnitSphere *
            Random.Range(1f, maxSpeed);

        transform.localPosition = localPosition;
    }

    void Update()
    {
        float dt = Time.deltaTime;

        Vector3 steering = Vector3.zero;

        steering += Separation() * separationWeight;
        steering += Alignment() * alignmentWeight;
        steering += Cohesion() * cohesionWeight;
        steering += Wander() * wanderWeight;

        steering = Vector3.ClampMagnitude(
            steering,
            maxForce
        );

        localVelocity += steering * dt;
        localVelocity = Vector3.ClampMagnitude(
            localVelocity,
            maxSpeed
        );

        localPosition += localVelocity * dt;

        ContainInsideTank();

        transform.localPosition = localPosition;

        if (localVelocity.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    tank.transform.TransformDirection(
                        localVelocity.normalized
                    )
                );
        }
    }

    // void ContainInsideTank()
    // {
    //     Vector3 half = tank.tankSize * 0.5f;

    //     localPosition.x =
    //         Mathf.Clamp(localPosition.x, -half.x, half.x);

    //     localPosition.y =
    //         Mathf.Clamp(localPosition.y, -half.y, half.y);

    //     localPosition.z =
    //         Mathf.Clamp(localPosition.z, -half.z, half.z);
    // }

    void ContainInsideTank()
    {
        Vector3 half = tank.tankSize * 0.5f;

        Vector3 target = localPosition;
        bool outside = false;

        if (localPosition.x > half.x)
        {
            target.x = half.x - 1f;
            outside = true;
        }
        else if (localPosition.x < -half.x)
        {
            target.x = -half.x + 1f;
            outside = true;
        }

        if (localPosition.y > half.y)
        {
            target.y = half.y - 1f;
            outside = true;
        }
        else if (localPosition.y < -half.y)
        {
            target.y = -half.y + 1f;
            outside = true;
        }

        if (localPosition.z > half.z)
        {
            target.z = half.z - 1f;
            outside = true;
        }
        else if (localPosition.z < -half.z)
        {
            target.z = -half.z + 1f;
            outside = true;
        }

        if (outside)
        {
            localVelocity +=
                Seek(target) * Time.deltaTime * 5f;
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
                Vector3.Distance(
                    localPosition,
                    other.localPosition
                );

            if (dist < neighborRadius &&
                dist > 0.001f)
            {
                force +=
                    (localPosition -
                     other.localPosition).normalized
                    / dist;

                count++;
            }
        }

        if (count > 0)
            force /= count;

        return SeekVelocity(force.normalized * maxSpeed);
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
                Vector3.Distance(
                    localPosition,
                    other.localPosition
                );

            if (dist < neighborRadius)
            {
                avgVel += other.localVelocity;
                count++;
            }
        }

        if (count == 0)
            return Vector3.zero;

        avgVel /= count;

        return SeekVelocity(
            avgVel.normalized * maxSpeed
        );
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
                Vector3.Distance(
                    localPosition,
                    other.localPosition
                );

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

    // Vector3 Wander()
    // {
    //     wanderAngle +=
    //         Random.Range(-30f, 30f)
    //         * Time.deltaTime;

    //     Vector3 circleCenter =
    //         localVelocity.normalized * 2f;

    //     Vector3 displacement =
    //         Quaternion.AngleAxis(
    //             wanderAngle,
    //             Vector3.up
    //         ) * Vector3.forward;

    //     return circleCenter + displacement;
    // }

    // Vector3 Wander()
    // {
    //     // Randomly move the wander target over time
    //     wanderAngle += Random.Range(-wanderJitter, wanderJitter)
    //                 * Time.deltaTime;

    //     // Direction the fish is currently moving
    //     Vector3 forward = localVelocity.normalized;

    //     if (forward.sqrMagnitude < 0.001f)
    //         forward = Vector3.forward;

    //     // Point on a circle in local space
    //     Vector3 circleOffset =
    //         new Vector3(
    //             Mathf.Cos(wanderAngle),
    //             Mathf.Sin(wanderAngle),
    //             0
    //         ) * wanderRadius;


    //     // Create a rotation where Z points along velocity
    //     Quaternion rotation =
    //         Quaternion.LookRotation(forward);

    //     // Rotate circle point into movement direction
    //     Vector3 displacement =
    //         rotation * circleOffset;


    //     // Project circle ahead of the fish
    //     Vector3 target =
    //         localPosition +
    //         forward * wanderDistance +
    //         displacement;


    //     return Seek(target);
    // }

    Vector3 Wander()
    {
        wanderAngle += Random.Range(-wanderJitter, wanderJitter)
                        * Time.deltaTime;

        Vector3 forward = localVelocity.normalized;

        if (forward.sqrMagnitude < 0.001f)
            forward = Vector3.forward;


        Vector3 circleOffset =
            new Vector3(
                Mathf.Cos(wanderAngle),
                Mathf.Sin(wanderAngle),
                0
            ) * wanderRadius;


        Quaternion rotation =
            Quaternion.LookRotation(forward);


        Vector3 displacement =
            rotation * circleOffset;


        Vector3 circleCenter =
            localPosition +
            forward * wanderDistance;


        Vector3 target =
            circleCenter +
            displacement;


        // Debug values
        debugCircleCenter = circleCenter;
        debugWanderPoint = target;
        debugTarget = target;


        return Seek(target);
    }

    Vector3 Seek(Vector3 target)
    {
        Vector3 desired =
            (target - localPosition).normalized
            * maxSpeed;

        return desired - localVelocity;
    }

    Vector3 SeekVelocity(Vector3 desired)
    {
        return desired - localVelocity;
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


        // Wander circle
        Gizmos.color = Color.yellow;

        DrawCircle(
            worldCircleCenter,
            rotation,
            wanderRadius
        );


        // Wander distance
        Gizmos.color = Color.red;

        Gizmos.DrawLine(
            transform.position,
            worldCircleCenter
        );


        // Wander target
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


        // Neighbour radius
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            neighborRadius
        );
    }


    // void OnDrawGizmos()
    // {
    //     if (localVelocity.sqrMagnitude < 0.001f)
    //         return;


    //     Vector3 forward = localVelocity.normalized;

    //     Quaternion rotation =
    //         Quaternion.LookRotation(forward);


    //     Vector3 worldCircleCenter =
    //         transform.position +
    //         tank.transform.TransformDirection(
    //             debugCircleCenter - localPosition
    //         );


    //     Vector3 worldWanderPoint =
    //         transform.position +
    //         tank.transform.TransformDirection(
    //             debugWanderPoint - localPosition
    //         );


    //     Gizmos.color = Color.yellow;

    //     DrawCircle(
    //         worldCircleCenter,
    //         rotation,
    //         wanderRadius
    //     );


    //     // Fish -> circle center
    //     Gizmos.color = Color.red;

    //     Gizmos.DrawLine(
    //         transform.position,
    //         worldCircleCenter
    //     );


    //     // Circle center -> jitter point
    //     Gizmos.color = Color.green;

    //     Gizmos.DrawLine(
    //         worldCircleCenter,
    //         worldWanderPoint
    //     );


    //     // Actual seek target
    //     Gizmos.color = Color.blue;

    //     Gizmos.DrawSphere(
    //         worldWanderPoint,
    //         0.05f
    //     );
    // }

    void DrawCircle(
        Vector3 center,
        Quaternion rotation,
        float radius
    )
    {
        int segments = 32;

        Vector3 previous =
            center +
            rotation *
            new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle =
                i * Mathf.PI * 2f / segments;

            Vector3 next =
                center +
                rotation *
                new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius,
                    0
                );

            Gizmos.DrawLine(
                previous,
                next
            );

            previous = next;
        }
    }

}