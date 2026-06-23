using UnityEngine;
using System.Collections.Generic;

public class FishTank : MonoBehaviour
{
    [Header("Nodes involved")]
    [SerializeField] private FishAgent fishPrefab;
    [SerializeField] private Transform visualTank;
    [Header("Aquarium parameters")]
    [SerializeField] private int fishCount = 20;
    //[SerializeField] private float wallBuffer = 0.2f;

    public Vector3 tankSize => visualTank.localScale;

    public List<FishAgent> Fish { get; private set; }
        = new List<FishAgent>();

    void Start()
    {
        for (int i = 0; i < fishCount; i++)
        {
            FishAgent fish = Instantiate(fishPrefab, transform);

            Vector3 localPos = new Vector3(
                Random.Range(-tankSize.x * 0.5f, tankSize.x * 0.5f),
                Random.Range(-tankSize.y * 0.5f, tankSize.y * 0.5f),
                Random.Range(-tankSize.z * 0.5f, tankSize.z * 0.5f)
            );

            fish.Initialize(this, localPos);

            Fish.Add(fish);
        }
    }

    // public Vector3 GetContainmentSteering(Vector3 localPosition)
    // {
    //     Vector3 half = tankSize * 0.5f;

    //     Vector3 target = localPosition;
    //     bool outside = false;

    //     if (localPosition.x > half.x - wallBuffer)
    //     {
    //         target.x = half.x - 1f;
    //         outside = true;
    //     }
    //     else if (localPosition.x < -half.x + wallBuffer)
    //     {
    //         target.x = -half.x + 1f;
    //         outside = true;
    //     }

    //     if (localPosition.y > half.y)
    //     {
    //         target.y = half.y - 1f;
    //         outside = true;
    //     }
    //     else if (localPosition.y < -half.y + wallBuffer)
    //     {
    //         target.y = -half.y + 1f;
    //         outside = true;
    //     }

    //     if (localPosition.z > half.z - wallBuffer)
    //     {
    //         target.z = half.z - 1f;
    //         outside = true;
    //     }
    //     else if (localPosition.z < -half.z + wallBuffer)
    //     {
    //         target.z = -half.z + 1f;
    //         outside = true;
    //     }

    //     if (!outside)
    //         return Vector3.zero;

    //     return (target - localPosition).normalized;
    // }

}