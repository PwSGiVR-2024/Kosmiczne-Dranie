using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Resources;
using TMPro;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ZoneManager : MonoBehaviour
{
    public GameObject zonePrefab; // Prefab strefy
    private int numberOfZones = 200; // Liczba stref
    private Vector2 areaSize = new Vector2(2000, 2000); // Rozmiar obszaru do losowego rozmieszczenia stref
    public int metalValue = 0;
    public int crystalValue = 0;
    public int playerTotalMetal = 0;
    public int playerTotalCrystal = 0;
    public Spawner spawner;
    public TMP_Text resourceText;
    public int money;

    void Start()
    {
        spawner.onAllyOutpostSpawned.AddListener(Subscribe);
        spawner.onEnemyOutpostSpawned.AddListener(Subscribe);
        GenerateRandomZones();
        StartCoroutine(ResourceCalculator());
        UpdateResourceText();
    }

    public void Subscribe(Outpost outpost)
    {
        outpost.onZoneCaptured.AddListener(GatherResources);
        outpost.onZoneRelease.AddListener(UngatherResources);


    }

    private void Update()
    {
        UpdateResourceText();
    }
    void UngatherResources(Zone zone)
    {
        switch (zone.zoneResource)
        {
            case Zone.ResourceType.Crystals:
                crystalValue -= zone.value;
                break;

            case Zone.ResourceType.Metals:
                metalValue -= zone.value;
                break;
        }

    }
    void GatherResources(Zone zone)
    {
        switch (zone.zoneResource)
        {
            case Zone.ResourceType.Crystals:
                crystalValue += zone.value;
            break;

            case Zone.ResourceType.Metals:
                metalValue += zone.value;
            break;
        }

    }
    IEnumerator ResourceCalculator()
    {
        WaitForSeconds interval = new(3);
        while (true)
        {
            playerTotalCrystal += crystalValue;
            playerTotalMetal += metalValue;
            money += crystalValue + metalValue / 10;
            yield return interval;
        }
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
            zone.Init();
        }
    }
    private void UpdateResourceText()
    {
        resourceText.text = $"Space Credits: {money} Crystals: {playerTotalCrystal} Metals: {playerTotalMetal}";
    }
}

