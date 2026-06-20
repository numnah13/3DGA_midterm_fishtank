using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupDrop : MonoBehaviour
{
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform objectGrabPoint;
    [SerializeField] private LayerMask pickupLayer;

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
                    Debug.Log("Hit");
                    if (hit.transform.TryGetComponent<ObjectGrabbable>(out var grab))
                    {
                        Debug.Log("grab: " + grab);
                        grabbable = grab;
                        grabbable.Grab(objectGrabPoint);
                    }
                    // ObjectGrabbable grab = hit.transform.GetComponentInParent<ObjectGrabbable>();
                    // Debug.Log("grab: " + grab);
                    // if (grab != null)
                    // {
                    //     grabbable = grab;
                    //     grabbable.Grab(objectGrabPoint);
                    // }
                }
            }
            else
            {
                grabbable.Drop();
                grabbable = null;
            }
        }
    }
}
