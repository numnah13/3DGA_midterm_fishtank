using UnityEngine;
using System.Collections.Generic;

public class FishAgent : MonoBehaviour
{
    FishTank tank;

    Vector3 localPosition;
    Vector3 localVelocity;

    public float maxSpeed = 4f;
    public float maxForce = 2f;

    public float neighborRadius = 3f;

    public float separationWeight = 2f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float wanderWeight = 0.5f;

    float wanderAngle;

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

        Vector3 Wander()
    {
        wanderAngle +=
            Random.Range(-30f, 30f)
            * Time.deltaTime;

        Vector3 circleCenter =
            localVelocity.normalized * 2f;

        Vector3 displacement =
            Quaternion.AngleAxis(
                wanderAngle,
                Vector3.up
            ) * Vector3.forward;

        return circleCenter + displacement;
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

}