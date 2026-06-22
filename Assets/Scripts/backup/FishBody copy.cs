using UnityEngine;

public class FishBody_backup
 : MonoBehaviour
{
    private Transform[] segments;

    private Vector3[] basePositions;
    private float minSpeed = 0f;
    private float maxSpeed = 1f;

    [SerializeField] [Range(0f, 1f)] private float speed;
    // [SerializeField] [Range(-1f, 1f)] private float yaw;
    // [SerializeField] [Range(-1f, 1f)] private float pitch;
    // private float maxHorizontalOffset = 1f;
    // private float maxVerticalOffset = 0.5f;

    //public float speed;


    [SerializeField] private  float minFrequency = 22f;
    [SerializeField] private float maxFrequency = 22f;

    [SerializeField] private  float maxAmplitude = 0.5f; //0.3f;
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

    // public float Yaw
    // {
    //     get => yaw;
    //     set => yaw = Mathf.Clamp(value, -maxHorizontalOffset, maxHorizontalOffset);
    // }

    // public float Pitch
    // {
    //     get => pitch;
    //     set => pitch = Mathf.Clamp(value, -maxVerticalOffset, maxVerticalOffset);
    // }

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
        
        float sineWeight;
        for (int i = 0; i < segments.Length; i++)
        {
            sineWeight = (i + 1f) / segments.Length;
            Vector3 pos = basePositions[i];

            pos.x += Mathf.Sin(
                time * frequency +
                i * phaseOffset
            ) * amplitude
            * sineWeight;

            segments[i].localPosition = pos;
        }
        

        // for (int i = 0; i < segments.Length; i++)
        // {
        //     Vector3 pos = basePositions[i];

        //     float offsetX = (float)i / (segments.Length - 1);
        //     float curveX = offsetX * offsetX;
        //     pos.x += yaw * maxHorizontalOffset * curveX;

        //     float offsetY = (float)i / (segments.Length - 1);
        //     float curveY = offsetY * offsetY;
        //     pos.y += pitch * maxVerticalOffset * curveY;

        //     segments[i].localPosition = pos;
        // }

    }


}