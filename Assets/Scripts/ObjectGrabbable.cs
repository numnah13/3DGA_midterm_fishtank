using UnityEngine;

// SOURCE: https://www.youtube.com/watch?v=2IhzPTS4av4

public class ObjectGrabbable : MonoBehaviour
{
    private Rigidbody rb;
    private Transform objectGrabPoint;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Grab(Transform objectGrabPoint)
    {
        this.objectGrabPoint = objectGrabPoint;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        rb.useGravity = false;
        rb.freezeRotation = true;
    }

    public void Drop()
    {
        this.objectGrabPoint = null;
        rb.useGravity = true;
        rb.freezeRotation = false;
    }

    public void RotateY(float amount, float speed)  // MY CODE
    {
        if (amount == 0)
            return;

        rb.MoveRotation(
            rb.rotation *
            Quaternion.Euler(
                0f,
                amount * speed * Time.fixedDeltaTime,
                0f
            )
        );
    }

    private void FixedUpdate()
    {
        if (objectGrabPoint != null)
        {
            float lerpSpeed = 10f;
            Vector3 newPosition = Vector3.Lerp(
                transform.position,
                objectGrabPoint.position,
                Time.deltaTime * lerpSpeed);
            rb.MovePosition(newPosition);

        }
    }

}

