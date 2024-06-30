using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ResourceManager : MonoBehaviour
{
    public bool infiniteResources = false;
    public float multiplier = 1.0f;


    public float gatherInterval = 3.0f;
    public ResourceHolder[] allResources;
    public List<ResourceHolder> currentResources = new();

    public int totalMetal = 0;
    public int totalCrystal = 0;
    public int totalCredits;
    public int totalMaintenance = 0;

    public UnityEvent onUpdate;

    public int Metals { get => totalMetal; set => totalMetal = value; }
    public int Crystals { get => totalCrystal; set => totalCrystal = value; }
    public int Credits { get => totalCredits; set => totalCredits = value; }
    public int Maintenance { get => totalMaintenance; set => totalMaintenance = value; }

    void Start()
    {
        allResources = FindObjectsOfType<ResourceHolder>();

        foreach (var resource in allResources)
        {
            resource.onCapture.AddListener((capturer) => OnResourceCapture(resource, capturer));
        }

        StartCoroutine(GatherResources());
    }

    public abstract void OnResourceCapture(ResourceHolder resource, Affiliation capturer);

    IEnumerator GatherResources()
    {
        WaitForSeconds interval = new(gatherInterval);
        while (true)
        {
            foreach (var resource in currentResources)
            {
                switch (resource.resourceType)
                {
                    case ResourceHolder.ResourceType.Crystals:
                        Crystals += (int)(resource.value * multiplier);
                        break;

                    case ResourceHolder.ResourceType.Metals:
                        Metals += (int)(resource.value * multiplier);
                        break;

                    case ResourceHolder.ResourceType.Credits:
                        Credits += (int)(resource.value * multiplier);
                        break;
                }
            }

            Credits -= Maintenance;

            onUpdate.Invoke();
            yield return interval;
        }
    }

    public void RemoveResources(GameObject unitPrefab, int count)
    {
        if (infiniteResources)
            return;

        AiController controller = unitPrefab.GetComponent<AiController>();
        UnitValues values = controller.Values;

        Crystals -= values.crystalPrice * count;
        Metals -= values.metalPrice * count;
        Maintenance += values.maintenancePrice * count;

        controller.onUnitNeutralized.AddListener((unit) => RemoveMaintenance(unit.Values.maintenancePrice));
        onUpdate.Invoke();
    }

    public void RemoveResources(GameObject outpostPrefab)
    {
        if (infiniteResources)
            return;

        Outpost outpost = outpostPrefab.GetComponent<Outpost>();
        OutpostValues values = outpost.values;

        Crystals -= values.crystalPrice;
        Metals -= values.metalPrice;
        Maintenance += values.maintenancePrice;

        outpost.onOutpostDestroy.AddListener(() => RemoveMaintenance(outpost.values.maintenancePrice));
        onUpdate.Invoke();
    }

    public void RemoveMaintenance(int value)
    {
        if (infiniteResources)
            return;

        Maintenance -= value;
        onUpdate.Invoke();
    }

    public void RemoveResources(TaskForcePreset preset)
    {
        if (infiniteResources)
            return;

        Crystals -= preset.crysalsPrice;
        Metals -= preset.metalsPrice;
        Maintenance += preset.maintenancePrice;
        onUpdate.Invoke();
    }

    public bool CheckIfHavingResources(TaskForcePreset preset)
    {
        if (infiniteResources)
            return true;

        if (totalCredits <= 0)
            return false;

        if (preset.metalsPrice > Metals || preset.crysalsPrice > Crystals)
            return false;

        return true;
    }

    public bool CheckIfHavingResources(GameObject prefab)
    {
        if (infiniteResources)
            return true;

        if (totalCredits <= 0)
            return false;

        if (prefab.TryGetComponent(out AiController controller))
        {
            UnitValues values = controller.Values;
            if (values.metalPrice > Metals || values.crystalPrice > Crystals)
                return false;

            else return true;
        }

        else if (prefab.TryGetComponent(out Outpost outpst))
        {
            OutpostValues values = outpst.values;
            if (values.metalPrice > Metals || values.crystalPrice > Crystals)
                return false;

            else return true;
        }

        return false;
    }

    public bool CheckIfHavingResources(GameObject prefab, int number)
    {
        if (infiniteResources)
            return true;

        if (totalCredits <= 0)
            return false;

        if (prefab.TryGetComponent(out AiController controller))
        {
            UnitValues values = controller.Values;

            int metalTotalPrice = values.metalPrice * number;
            int crystalTotalPrice = values.crystalPrice * number;

            if (crystalTotalPrice > Crystals || metalTotalPrice > Metals)
                return false;

            else return true;
        }

        else if (prefab.TryGetComponent(out Outpost outpst))
        {
            OutpostValues values = outpst.values;

            int metalTotalPrice = values.metalPrice * number;
            int crystalTotalPrice = values.crystalPrice * number;

            if (crystalTotalPrice > Crystals || metalTotalPrice > Metals)
                return false;

            else return true;
        }

        return false;
    }
}

