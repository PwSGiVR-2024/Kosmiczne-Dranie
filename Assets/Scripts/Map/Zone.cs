using UnityEngine;

public class Zone : MonoBehaviour
{
    public int[] resources = new int[4];

    void Start()
    {
        for (int i = 0; i < resources.Length; i++)
        {
            resources[i] = Random.Range(1, 101); // Inicjalizacja zasobów losowymi wartoœciami
        }
    }

    public int[] GetResources()
    {
        return resources;
    }
}

public class ZoneSpawner : MonoBehaviour
{
    public GameObject ZonePrefab;
    public int numberOfZones = 10;
    public float spawnRadius = 100f;

    void Start()
    {
        for (int i = 0; i < numberOfZones; i++)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                Random.Range(-spawnRadius, spawnRadius)
            );

            Instantiate(ZonePrefab, spawnPosition, Quaternion.identity);
        }
    }
}