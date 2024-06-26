using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TaskForcePreset
{
    public int power;
    public int metalsPrice;
    public int crysalsPrice;
    public int maintenancePrice;

    public int frigatesCount;
    public int destroyersCount;
    public int cruisersCount;
    public int battleshipsCount;

    public GameObject frigatePrefab;
    public GameObject destroyerPrefab;
    public GameObject cruiserPrefab;
    public GameObject battleshipPrefab;

    public TaskForcePreset(
        GameObject frigatePrefab,
        GameObject destroyerPrefab,
        GameObject cruiserPrefab,
        GameObject battleshipPrefab,
        int frigatesCount,
        int destroyersCount,
        int cruisersCount,
        int battleshipsCount)
    {
        this.frigatesCount = frigatesCount;
        this.destroyersCount = destroyersCount;
        this.cruisersCount = cruisersCount;
        this.battleshipsCount = battleshipsCount;

        this.frigatePrefab = frigatePrefab;
        this.destroyerPrefab = destroyerPrefab;
        this.cruiserPrefab = cruiserPrefab;
        this.battleshipPrefab = battleshipPrefab;

        power = 0;
        metalsPrice = 0;
        crysalsPrice = 0;
        maintenancePrice = 0;

        UpdateValues();
    }

    public void UpdateValues()
    {
        power = 0;
        metalsPrice = 0;
        crysalsPrice = 0;
        maintenancePrice = 0;

        AiController member;

        if (battleshipsCount > 0 && battleshipPrefab)
        {
            member = battleshipPrefab.GetComponent<AiController>();
            power += member.Values.power * battleshipsCount;
            metalsPrice += member.Values.metalPrice * battleshipsCount;
            crysalsPrice += member.Values.crystalPrice * battleshipsCount;
            maintenancePrice += member.Values.maintenancePrice * battleshipsCount;
        }
        else battleshipsCount = 0;

        if (cruisersCount > 0 && cruiserPrefab)
        {
            member = cruiserPrefab.GetComponent<AiController>();
            power += member.Values.power * cruisersCount;
            metalsPrice += member.Values.metalPrice * cruisersCount;
            crysalsPrice += member.Values.crystalPrice * cruisersCount;
            maintenancePrice += member.Values.maintenancePrice * cruisersCount;
        }
        else cruisersCount = 0;

        if (destroyersCount > 0 && destroyerPrefab)
        {
            member = destroyerPrefab.GetComponent<AiController>();
            power += member.Values.power * destroyersCount;
            metalsPrice += member.Values.metalPrice * destroyersCount;
            crysalsPrice += member.Values.crystalPrice * destroyersCount;
            maintenancePrice += member.Values.maintenancePrice * destroyersCount;
        }
        else destroyersCount = 0;

        if (frigatesCount > 0 && frigatePrefab)
        {
            member = frigatePrefab.GetComponent<AiController>();
            power += member.Values.power * frigatesCount;
            metalsPrice += member.Values.metalPrice * frigatesCount;
            crysalsPrice += member.Values.crystalPrice * frigatesCount;
            maintenancePrice += member.Values.maintenancePrice * frigatesCount;
        }
        else frigatesCount = 0;
    }

    public void MultiplyCounts(int value)
    {
        frigatesCount *= value;
        cruisersCount *= value;
        destroyersCount *= value;
        battleshipsCount *= value;

        UpdateValues();
    }

    public void DebugLogValues(string label)
    {
        Debug.Log($"{label}:\nPower: {power}\nMetals price: {metalsPrice}\nCrystals price: {crysalsPrice}\nMaintenance: {maintenancePrice}");
    }
}
