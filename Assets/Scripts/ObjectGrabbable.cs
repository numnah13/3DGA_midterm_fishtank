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
        rb.useGravity = false;
        rb.freezeRotation = true;
    }

    public void Drop()
    {
        this.objectGrabPoint = null;
        rb.useGravity = true;
        rb.freezeRotation = false;
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

