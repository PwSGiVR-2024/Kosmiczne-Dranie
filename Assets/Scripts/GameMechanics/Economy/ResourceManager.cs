using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Resources;
using TMPro;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ResourceManager : MonoBehaviour
{
    public GameObject resourceZonePrefab; // Prefab strefy
    private int numberOfZones = 200; // Liczba stref
    private Vector2 areaSize = new Vector2(2000, 2000); // Rozmiar obszaru do losowego rozmieszczenia stref
    public int metalValue = 0;
    public int crystalValue = 0;
    public int playerTotalMetal = 0;
    public int playerTotalCrystal = 0;
    public Spawner spawner;
    public TMP_Text resourceText;
    public int playerTotalMoney;
    public int totalMaintenance = 0;
    public int moneyValue = 0;

    public int PlayerMetal { get => playerTotalMetal; set => playerTotalMetal = value; }
    public int PlayerCrystal { get => playerTotalCrystal; set => playerTotalCrystal = value; }
    public int PlayerMoney { get => playerTotalMoney; set => playerTotalMoney = value; }
    public int PlayerMaintenance { get => totalMaintenance; set => totalMaintenance = value; }

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
    void UngatherResources(ResourceZone zone)
    {
        zone.captured = false;

        switch (zone.zoneResource)
        {
            case ResourceZone.ResourceType.Crystals:
                crystalValue -= zone.value;
                break;

            case ResourceZone.ResourceType.Metals:
                metalValue -= zone.value;
                break;
        }

        moneyValue -= (int)(zone.value * 0.1f);
    }
    void GatherResources(ResourceZone zone)
    {
        if (zone.captured)
            return;

        zone.captured = true;

        switch (zone.zoneResource)
        {
            case ResourceZone.ResourceType.Crystals:
                crystalValue += zone.value;
            break;

            case ResourceZone.ResourceType.Metals:
                metalValue += zone.value;
            break;
        }

        moneyValue += (int)(zone.value * 0.1f);
    }
    IEnumerator ResourceCalculator()
    {
        WaitForSeconds interval = new(3);
        while (true)
        {
            playerTotalCrystal += crystalValue;
            playerTotalMetal += metalValue;
            playerTotalMoney += moneyValue;
            playerTotalMoney -= totalMaintenance;
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

            GameObject zoneObject = Instantiate(resourceZonePrefab, randomPosition, Quaternion.identity);
            ResourceZone zone = zoneObject.GetComponent<ResourceZone>();
            zone.Init();
        }
    }
    private void UpdateResourceText()
    {
        resourceText.text = $"Space Credits: {playerTotalMoney} Crystals: {playerTotalCrystal} Metals: {playerTotalMetal}";
    }
}

