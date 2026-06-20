using UnityEngine;

public class FishBody : MonoBehaviour
{
    private Transform[] segments;
    private float minSpeed = 0f;
    private float maxSpeed = 10f;
    private Vector3[] basePositions;
    [SerializeField] [Range(0f, 10f)] private float speed;

    [SerializeField] private  float minFrequency = 2f;
    [SerializeField] private float maxFrequency = 30f;

    [SerializeField] private  float maxAmplitude = 0.3f;
    [SerializeField] private  float minAmplitude = 0.1f;
    [SerializeField] private  float phaseOffset = 0.6f;

    
    void Awake()
    {
        segments = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            segments[i] = transform.GetChild(i);
        }
    }

    public float Speed
    {
        get => speed;
        set => speed = Mathf.Clamp(value, minSpeed, maxSpeed);
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
        float frequency = Mathf.Lerp(
            minFrequency,
            maxFrequency,
            speed / maxSpeed
        );

        float amplitude = Mathf.Lerp(
            minAmplitude,
            maxAmplitude,
            speed / maxSpeed
        );
        
        float time = Time.time;
        float weight;
        for (int i = 0; i < segments.Length; i++)
        {
            weight = (i + 1f) / segments.Length;
            Vector3 pos = basePositions[i];

            pos.x += Mathf.Sin(
                time * frequency +
                i * phaseOffset
            ) * amplitude
            * weight;

            segments[i].localPosition = pos;
        }
    }
}