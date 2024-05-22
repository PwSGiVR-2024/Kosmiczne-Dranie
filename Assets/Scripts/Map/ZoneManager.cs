using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public GameObject zonePrefab; // Prefab strefy
    private int numberOfZones = 200; // Liczba stref
    private Vector2 areaSize = new Vector2(2000, 2000); // Rozmiar obszaru do losowego rozmieszczenia stref
    public int minValue = 1; // Minimalna wartoœæ int
    public int maxValue = 100; // Maksymalna wartoœæ int

    private List<Zone> zones = new List<Zone>();

    void Start()
    {
        GenerateRandomZones();
    }

    void GenerateRandomZones()
    {
        for (int i = 0; i < numberOfZones; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(-areaSize.x , areaSize.x ),
                0, // Ustaw wysokoœæ na 0
                Random.Range(-areaSize.y , areaSize.y )
            );

            GameObject zoneObject = Instantiate(zonePrefab, randomPosition, Quaternion.identity);
            Zone zone = zoneObject.GetComponent<Zone>();

            if (zone != null)
            {
                zone.resources = new int[4];
                for (int j = 0; j < 4; j++)
                {
                    zone.resources[j] = Random.Range(minValue, maxValue);
                }
                zones.Add(zone);
            }
        }
    }
}

