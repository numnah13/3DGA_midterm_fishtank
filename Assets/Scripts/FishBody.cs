// SOURCE: adapted from https://discussions.unity.com/t/snake-like-trail-of-gameobjects-following-a-leader/43513/2

using UnityEngine;
using System.Collections.Generic;

public class FishBody : MonoBehaviour
{
    // Nodes involved
    [Header("Nodes involved")]
    [SerializeField] Transform head;
    [SerializeField] private Transform[] tailSegments;
    private Transform referenceFrame;

    // Animation properties
    [Header("Animation properties")]
    [SerializeField] private int frameDelay = 2;
    [SerializeField] private float followSpeed = 22f;
    [SerializeField] private float rotationSpeed = 180f;

    // History of positions
    private Vector3[] basePositions;
    private List<Frame>[] formerPositions;



    public class Frame
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
    }


    void Awake()
    {
        // MY CODE START
        referenceFrame = GameObject.FindWithTag("Tank").transform; 
        // MY CODE END

        formerPositions = new List<Frame>[tailSegments.Length + 1];

        formerPositions[0] = new List<Frame>();

        for (int i = 1; i < formerPositions.Length; i++)
        {
            formerPositions[i] = new List<Frame>();
        }
    }


    void Start()
    {
        basePositions = new Vector3[tailSegments.Length];

        for (int i = 0; i < tailSegments.Length; i++)
        {
            basePositions[i] = tailSegments[i].position;
        }
    }

    void Update()
    {
        StorePositions();
        MoveTail();
    }

    void StorePositions()
    {
        AddFrame(formerPositions[0], head.position, head.rotation);

        for (int i = 0; i < tailSegments.Length; i++)
        {
            AddFrame(
                formerPositions[i + 1],
                tailSegments[i].position,
                tailSegments[i].rotation
            );
        }
    }

    // MY CODE START
    void AddFrame(List<Frame> history, Vector3 worldPosition, Quaternion worldRotation)
    {
        Vector3 localPos = referenceFrame.InverseTransformPoint(worldPosition); 
        Quaternion localRot = Quaternion.Inverse(referenceFrame.rotation) * worldRotation; 

        if (history.Count == 0 || localPos != history[0].localPosition) 
        {
            history.Insert(
                0,
                new Frame
                {
                    localPosition = localPos,
                    localRotation = localRot
                }
            );

            if (history.Count > frameDelay + 2)
            {
                history.RemoveAt(history.Count - 1);
            }
        }
    }
    // MY CODE END



    void MoveTail()
    {
        for (int i = 0; i < tailSegments.Length; i++)
        {
            int leaderIndex = i;

            if (formerPositions[leaderIndex].Count > frameDelay)
            {
                Frame frame = formerPositions[leaderIndex][frameDelay];

                // MY CODE START
                Vector3 targetWorldPos = referenceFrame.TransformPoint(frame.localPosition); // MY CODE
                Quaternion targetWorldRot = referenceFrame.rotation * frame.localRotation; // MY CODE

                tailSegments[i].position = Vector3.Lerp( // MY CODE
                        tailSegments[i].position,
                        targetWorldPos,
                        followSpeed * Time.deltaTime
                    );
                // MY CODE END
                tailSegments[i].rotation = Quaternion.RotateTowards(
                        tailSegments[i].rotation,
                        targetWorldRot,
                        rotationSpeed * Time.deltaTime
                    );
            }
        }
    }
}