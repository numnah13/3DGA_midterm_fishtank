// using UnityEngine;
// using System.Collections.Generic;

// public class FishBody : MonoBehaviour
// {
//     [SerializeField] Transform head;
//     //[SerializeField] Transform tailRoot;
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


//     [SerializeField] private int frameDelay = 15;
//     [SerializeField] private float followSpeed = 10f;
//     [SerializeField] private float rotationSpeed = 180f;

//     private List<Frame>[] formerPositions;
//     private Vector3 formerPosition;


//     public class Frame
//     {
//         public Vector3 position;
//         public Quaternion rotation;
//     }


//     void Awake()
//     {
//         // Create history for every segment
//         formerPositions = new List<Frame>[tailSegments.Length + 1];


//         // Index 0 = head
//         formerPositions[0] = new List<Frame>();


//         // Remaining indexes = tail segments
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
//         //SineAnimation();
//         StorePositions();
//         MoveTail();

//     }

//     void StorePositions()
//     {
//         // Store head position
//         AddFrame(
//             formerPositions[0],
//             head.position,
//             head.rotation
//         );


//         // Store each tail segment position
//         for (int i = 0; i < tailSegments.Length; i++)
//         {
//             AddFrame(
//                 formerPositions[i + 1],
//                 tailSegments[i].position,
//                 tailSegments[i].rotation
//             );
//         }
//     }

//     void AddFrame(List<Frame> history, Vector3 position, Quaternion rotation)
//     {
//         if (history.Count == 0 || position != history[0].position)
//         {
//             history.Insert(0,
//                 new Frame
//                 {
//                     position = position,
//                     rotation = rotation
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

//                 Frame frame =
//                     formerPositions[leaderIndex][frameDelay];


//                 tailSegments[i].position =
//                     Vector3.Lerp(
//                         tailSegments[i].position,
//                         frame.position,
//                         followSpeed * Time.deltaTime
//                     );


//                 tailSegments[i].rotation =
//                     Quaternion.RotateTowards(
//                         tailSegments[i].rotation,
//                         frame.rotation,
//                         rotationSpeed * Time.deltaTime
//                     );
//             }
//         }

//     }



//     // void SineAnimation()
//     // {
//     //     float time = Time.time;

//     //     float frequency = Mathf.Lerp(
//     //         minFrequency,
//     //         maxFrequency,
//     //         speed / maxSpeed
//     //     );

//     //     float amplitude = Mathf.Lerp(
//     //         minAmplitude,
//     //         maxAmplitude,
//     //         speed / maxSpeed
//     //     );

//     //     float sineWeight;
//     //     for (int i = 0; i < tailSegments.Length; i++)
//     //     {
//     //         sineWeight = (i + 1f) / tailSegments.Length;
//     //         Vector3 pos = basePositions[i];

//     //         pos.x += Mathf.Sin(
//     //             time * frequency +
//     //             i * phaseOffset
//     //         ) * amplitude
//     //         * sineWeight;

//     //         tailSegments[i].position = pos;
//     //     }
//     // }

// }

