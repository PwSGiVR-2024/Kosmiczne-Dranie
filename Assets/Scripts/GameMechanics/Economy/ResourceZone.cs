using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

public class ResourceZone : MonoBehaviour
{
    public enum ResourceType { Crystals, Metals }
    public ResourceType zoneResource;
    public int value;
    public UnityEvent<ResourceZone> onCapture = new();
    public bool captured = false;

    
    public void Init()
    {
        int randomInt;
        value = Random.Range(50, 200);
        randomInt = Random.Range(0,2);
        //to jest g³upie nie patrz na chwile tylko
        if (randomInt==0)
        {
            zoneResource = ResourceType.Crystals;
        }
        else if (randomInt==1)
        {
            zoneResource = ResourceType.Metals;
        }
    }
}

/*public class ZoneSpawner : MonoBehaviour
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
}*/