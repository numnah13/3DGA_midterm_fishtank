using UnityEngine;
using System.Collections.Generic;

public class FishTank : MonoBehaviour
{
    public FishAgent fishPrefab;
    public int fishCount = 50;

    public Vector3 tankSize = new Vector3(20, 10, 10);

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

    public bool IsInsideBounds(Vector3 localPos)
    {
        Vector3 half = tankSize * 0.5f;

        return
            Mathf.Abs(localPos.x) <= half.x &&
            Mathf.Abs(localPos.y) <= half.y &&
            Mathf.Abs(localPos.z) <= half.z;
    }
}