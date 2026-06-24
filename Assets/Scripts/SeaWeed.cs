// MY CODE START

using UnityEngine;

public class SeaWeed : MonoBehaviour
{
    private Transform[] segments;
    private Vector3[] basePositions;

    [Header("Animation properties")]
    [SerializeField] [Range(0f, 1f)] private float speed;
    [SerializeField] private  float minFrequency = 0f;
    [SerializeField] private float maxFrequency = 22f;
    [SerializeField] private  float maxAmplitude = 0.5f; 
    [SerializeField] private  float minAmplitude = 0f;
    [SerializeField] private  float phaseOffset = 0.6f;

    
    void Awake()
    {
        segments = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            segments[i] = transform.GetChild(i);
        }
    }


    void Start()
    {
        basePositions = new Vector3[segments.Length];

        for (int i = 0; i < segments.Length; i++)
        {
            basePositions[i] = segments[i].localPosition;
        }
    }

    void Update()
    {
        float time = Time.time;
        
        // Higher speed produces larger and faster oscillations.
        float frequency = Mathf.Lerp(
            minFrequency,
            maxFrequency,
            speed
        );

        float amplitude = Mathf.Lerp(
            minAmplitude,
            maxAmplitude,
            speed
        );
        
        // Distribute sine wave offset gradually increasing from weed's base to its tip
        float sineWeight;
        for (int i = 0; i < segments.Length; i++)
        {
            sineWeight = (i + 1f) / segments.Length; // increasing sine weight
            Vector3 pos = basePositions[i];

            pos.x += Mathf.Sin(
                time * frequency +
                i * phaseOffset
            ) * amplitude
            * sineWeight;

            segments[i].localPosition = pos;
        }
    }

}

// MY CODE END