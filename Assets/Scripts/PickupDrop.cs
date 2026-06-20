using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

// SOURCE: https://www.youtube.com/watch?v=2IhzPTS4av4

public class PickupDrop : MonoBehaviour
{
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform objectGrabPoint;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] float rotateSpeed = 100f;

    private ObjectGrabbable grabbable;


    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (grabbable == null)
            {
                float pickUpDistance = 2f;
                if (Physics.Raycast(
                    playerCam.position,
                    playerCam.forward,
                    out RaycastHit hit,
                    pickUpDistance,
                    pickupLayer
                ))
                {
                    if (hit.transform.TryGetComponent<ObjectGrabbable>(out var grab))
                    {
                        grabbable = grab;
                        grabbable.Grab(objectGrabPoint);
                    }
                }
            }
            else
            {
                grabbable.Drop();
                grabbable = null;
            }
        }

        if (grabbable != null)
        {
            float scroll = Mouse.current.scroll.y.ReadValue();

            grabbable.transform.Rotate(
                Vector3.up,
                scroll * rotateSpeed * Time.deltaTime,
                Space.World
            );
        }
    }
}
