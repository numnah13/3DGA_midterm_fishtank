// SOURCE: adapted from https://www.youtube.com/watch?v=2IhzPTS4av4

using UnityEngine;

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

    public void RotateY(float amount, float speed) // MY CODE
        {
            if (amount == 0f)
                return;

            transform.Rotate(
                Vector3.up,
                amount * speed * Time.deltaTime,
                Space.World
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

