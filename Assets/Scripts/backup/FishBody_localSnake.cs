// using UnityEngine;
// using System.Collections.Generic;

// public class FishBody : MonoBehaviour
// {
//     [SerializeField] Transform head;

//     // The frame everything is recorded/applied relative to.
//     // Assign this to the Tank transform in the inspector.
//     private Transform referenceFrame;

//     [SerializeField] private Transform[] tailSegments;


//     private Vector3[] basePositions;
//     private float minSpeed = 0f;
//     private float maxSpeed = 1f;

//     [SerializeField][Range(0f, 1f)] private float speed;



//     [SerializeField] private float minFrequency = 22f;
//     [SerializeField] private float maxFrequency = 22f;

//     [SerializeField] private float minAmplitude = 0.1f;
//     [SerializeField] private float maxAmplitude = 0.5f; //0.3f;

//     [SerializeField] private float phaseOffset = 0.6f;


//     [SerializeField] private int frameDelay = 2;
//     [SerializeField] private float followSpeed = 22f;
//     [SerializeField] private float rotationSpeed = 180f;

//     private List<Frame>[] formerPositions;


//     // Now stores position/rotation relative to referenceFrame (the Tank),
//     // NOT world space. This way tank movement never gets "baked into"
//     // old history - it's reapplied fresh every frame using the tank's
//     // CURRENT transform, while the swim wiggle (which only shows up as
//     // motion relative to the tank) is preserved correctly.
//     public class Frame
//     {
//         public Vector3 localPosition;
//         public Quaternion localRotation;
//     }


//     void Awake()
//     {
//         referenceFrame = GameObject.FindWithTag("Tank").transform;
//         if (referenceFrame == null)
//         {
//             // Fallback: walk up to the topmost parent if nothing assigned.
//             referenceFrame = transform.root;
//             Debug.LogWarning(
//                 $"{name}: referenceFrame not assigned, defaulting to root '{referenceFrame.name}'. " +
//                 "Assign the Tank transform explicitly in the inspector."
//             );
//         }

//         formerPositions = new List<Frame>[tailSegments.Length + 1];

//         formerPositions[0] = new List<Frame>();

//         for (int i = 1; i < formerPositions.Length; i++)
//         {
//             formerPositions[i] = new List<Frame>();
//         }
//     }

//     public float Speed
//     {
//         get => speed;
//         set => speed = Mathf.Clamp(value, minSpeed, maxSpeed);
//     }



//     void Start()
//     {
//         basePositions = new Vector3[tailSegments.Length];

//         for (int i = 0; i < tailSegments.Length; i++)
//         {
//             basePositions[i] = tailSegments[i].position;
//         }
//     }

//     void Update()
//     {
//         StorePositions();
//         MoveTail();
//     }

//     void StorePositions()
//     {
//         AddFrame(formerPositions[0], head.position, head.rotation);

//         for (int i = 0; i < tailSegments.Length; i++)
//         {
//             AddFrame(
//                 formerPositions[i + 1],
//                 tailSegments[i].position,
//                 tailSegments[i].rotation
//             );
//         }
//     }

//     void AddFrame(List<Frame> history, Vector3 worldPosition, Quaternion worldRotation)
//     {
//         // Convert world -> tank-relative space before storing.
//         Vector3 localPos = referenceFrame.InverseTransformPoint(worldPosition);
//         Quaternion localRot = Quaternion.Inverse(referenceFrame.rotation) * worldRotation;

//         if (history.Count == 0 || localPos != history[0].localPosition)
//         {
//             history.Insert(0,
//                 new Frame
//                 {
//                     localPosition = localPos,
//                     localRotation = localRot
//                 }
//             );

//             if (history.Count > frameDelay + 5)
//             {
//                 history.RemoveAt(history.Count - 1);
//             }
//         }
//     }



//     void MoveTail()
//     {
//         for (int i = 0; i < tailSegments.Length; i++)
//         {
//             int leaderIndex = i;

//             if (formerPositions[leaderIndex].Count > frameDelay)
//             {
//                 Frame frame = formerPositions[leaderIndex][frameDelay];

//                 // Convert the stored tank-relative frame back to world space
//                 // using the tank's CURRENT transform - this is what makes
//                 // tank movement "free" instead of dragging old history along.
//                 Vector3 targetWorldPos = referenceFrame.TransformPoint(frame.localPosition);
//                 Quaternion targetWorldRot = referenceFrame.rotation * frame.localRotation;

//                 tailSegments[i].position =
//                     Vector3.Lerp(
//                         tailSegments[i].position,
//                         targetWorldPos,
//                         followSpeed * Time.deltaTime
//                     );

//                 tailSegments[i].rotation =
//                     Quaternion.RotateTowards(
//                         tailSegments[i].rotation,
//                         targetWorldRot,
//                         rotationSpeed * Time.deltaTime
//                     );
//             }
//         }
//     }
// }