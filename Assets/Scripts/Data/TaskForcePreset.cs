using System;
using UnityEngine;

public struct TaskForcePreset
{
    public Action onUpdate;

    public int power;
    public int metalsPrice;
    public int crysalsPrice;
    public int maintenancePrice;
    public int health;
    public int size;
    public float travelSpeed;
    public float range;
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
    int battleshipsCount,
    Action onUpdateCallabck = null)
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
        range = 0;
        health = 0;
        size = 0;
        travelSpeed = 0;

        onUpdate = onUpdateCallabck;

        UpdateValues();
    }

    public void UpdateValues()
    {
        power = 0;
        metalsPrice = 0;
        crysalsPrice = 0;
        maintenancePrice = 0;
        range = 0;
        health = 0;
        size = 0;
        travelSpeed = float.PositiveInfinity;

        AiController member;

        if (battleshipsCount > 0 && battleshipPrefab)
        {
            member = battleshipPrefab.GetComponent<AiController>();
            power += member.Values.power * battleshipsCount;
            metalsPrice += member.Values.metalPrice * battleshipsCount;
            crysalsPrice += member.Values.crystalPrice * battleshipsCount;
            maintenancePrice += member.Values.maintenancePrice * battleshipsCount;
            health += member.Values.health * battleshipsCount;
            size += member.Values.size * battleshipsCount;
            range += member.Values.spotDistance * battleshipsCount;

            if (member.Values.unitSpeed < travelSpeed)
                travelSpeed = member.Values.unitSpeed;
        }
        else battleshipsCount = 0;

        if (cruisersCount > 0 && cruiserPrefab)
        {
            member = cruiserPrefab.GetComponent<AiController>();
            power += member.Values.power * cruisersCount;
            metalsPrice += member.Values.metalPrice * cruisersCount;
            crysalsPrice += member.Values.crystalPrice * cruisersCount;
            maintenancePrice += member.Values.maintenancePrice * cruisersCount;
            health += member.Values.health * cruisersCount;
            size += member.Values.size * cruisersCount;
            range += member.Values.spotDistance * cruisersCount;

            if (member.Values.unitSpeed < travelSpeed)
                travelSpeed = member.Values.unitSpeed;
        }
        else cruisersCount = 0;

        if (destroyersCount > 0 && destroyerPrefab)
        {
            member = destroyerPrefab.GetComponent<AiController>();
            power += member.Values.power * destroyersCount;
            metalsPrice += member.Values.metalPrice * destroyersCount;
            crysalsPrice += member.Values.crystalPrice * destroyersCount;
            maintenancePrice += member.Values.maintenancePrice * destroyersCount;
            health += member.Values.health * destroyersCount;
            size += member.Values.size * destroyersCount;
            range += member.Values.spotDistance * destroyersCount;

            if (member.Values.unitSpeed < travelSpeed)
                travelSpeed = member.Values.unitSpeed;
        }
        else destroyersCount = 0;

        if (frigatesCount > 0 && frigatePrefab)
        {
            member = frigatePrefab.GetComponent<AiController>();
            power += member.Values.power * frigatesCount;
            metalsPrice += member.Values.metalPrice * frigatesCount;
            crysalsPrice += member.Values.crystalPrice * frigatesCount;
            maintenancePrice += member.Values.maintenancePrice * frigatesCount;
            health += member.Values.health * frigatesCount;
            size += member.Values.size * frigatesCount;
            range += member.Values.spotDistance * frigatesCount;

            if (member.Values.unitSpeed < travelSpeed)
                travelSpeed = member.Values.unitSpeed;
        }
        else frigatesCount = 0;

        // avarage value
        range /= frigatesCount + destroyersCount + cruisersCount + battleshipsCount;

        if (onUpdate != null)
            onUpdate.Invoke();
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
