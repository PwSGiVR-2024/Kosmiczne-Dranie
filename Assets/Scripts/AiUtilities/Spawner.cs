using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.Rendering.CameraUI;


// klasa odpowiada za instancjonowanie ka¿dej jednostki oraz Task Forca w œwiecie gry
public class Spawner : MonoBehaviour
{
    //public TaskForceHUDScriptable basicHUDVariant;
    public GameObject allyHUD;
    public GameObject enemyHUD;

    public GameManager gameManager;
    public ResourceManager resourceManager;
    public Headquarters playerHeadquarters;

    public GameObject redUnitsContainer; // kontenery ¿eby by³ porz¹dek na scenie. Tylko przechowuj¹ gameObjecty, nic wiêcej nie robi¹
    public GameObject redProjectileContainer;
    public GameObject redTaskForceContainer;
    public GameObject redOutpostsContainer;

    public GameObject blueUnitsContainer;
    public GameObject blueProjectileContainer;
    public GameObject blueTaskForceContainer;
    public GameObject blueOutpostsContainer;

    private int blueUnitsCount = 1;
    private int blueOutpostsCount = 1;

    private int redUnitsCount = 1;
    private int redOutpostsCount = 1;

    public UnityEvent<AiController> onAllySpawned = new();
    public UnityEvent<AiController> onEnemySpawned = new();
    public UnityEvent<TaskForceController> onAllyTaskForceSpawned = new();
    public UnityEvent<TaskForceController> onEnemyTaskForceSpawned = new();
    public UnityEvent<Outpost> onAllyOutpostSpawned = new();
    public UnityEvent<Outpost> onEnemyOutpostSpawned = new();

    public GameObject unitPrefab;
    public GameObject outpostPrefab;
    public int unitsToSpawn = 0;
    public bool spawnOutpost = false;

    public OutpostNames outpostNames;
    public TaskForceNames taskForceNames;

    private TaskForceController SpawnBlueTaskForce(Vector3 pos, string name)
    {
        if (!CheckIfPositionViable(pos))
            return null;

        if (!CheckIfHavingResources(unitPrefab, unitsToSpawn))
            return null;

        if (pos == null || unitPrefab == null || unitsToSpawn < 1 || unitPrefab.CompareTag("Enemy") || !unitPrefab.CompareTag("Ally"))
            return null;

        TaskForceController taskForce = TaskForceController.Create(name, gameManager, Affiliation.Blue);
        taskForce.transform.SetParent(blueTaskForceContainer.transform);
        // taskForce.gameObject.AddComponent<HUDController>().Init(taskForce, basicHUDVariant, gameManager);

        AiController commander = AiController.Create(pos, unitPrefab, taskForce, blueProjectileContainer, Affiliation.Blue);
        commander.transform.SetParent(blueUnitsContainer.transform, true);
        commander.gameObject.name += blueUnitsCount;
        taskForce.AddUnit(commander);
        RemovePlayerResources(commander);
        onAllySpawned.Invoke(commander);
        blueUnitsCount++;

        // randomowa pozycja jednostki jest ustawiana w zale¿noœci od ilosci spawnowanych jednostek
        float range = 0;
        if (unitsToSpawn > 1)
        {
            range = unitsToSpawn;
            range = Mathf.Sqrt(range);
            range *= commander.Values.size;
        }

        for (int i = 1; i < unitsToSpawn; i++)
        {
            AiController member = AiController.Create(GameUtils.RandomPlanePositionCircle(pos, range), unitPrefab, taskForce, blueProjectileContainer, Affiliation.Blue);
            member.transform.SetParent(blueUnitsContainer.transform, true);
            member.gameObject.name += blueUnitsCount;
            taskForce.AddUnit(member);
            RemovePlayerResources(member);
            onAllySpawned.Invoke(member);
            blueUnitsCount++;
        }

        onAllyTaskForceSpawned?.Invoke(taskForce);

        TaskForceHUD.Create(taskForce, allyHUD, gameManager);
        return taskForce;
    }

    private TaskForceController SpawnRedTaskForce(Vector3 pos, string name)
    {
        if (unitPrefab == null || unitsToSpawn < 1 || unitPrefab.CompareTag("Ally") || !unitPrefab.CompareTag("Enemy"))
            return null;

        TaskForceController taskForce = TaskForceController.Create(name, gameManager, Affiliation.Red);
        taskForce.transform.SetParent(redTaskForceContainer.transform);
        //taskForce.gameObject.AddComponent<HUDController>().Init(taskForce, basicHUDVariant, gameManager);
        

        AiController commander = AiController.Create(pos, unitPrefab, taskForce, redProjectileContainer, Affiliation.Red);
        commander.transform.SetParent(redUnitsContainer.transform, true);
        commander.gameObject.name += redUnitsCount;
        taskForce.AddUnit(commander);
        onEnemySpawned.Invoke(commander);
        redUnitsCount++;

        // randomowa pozycja jednostki jest ustawiana w zale¿noœci od ilosci spawnowanych jednostek
        float range = 0;
        if (unitsToSpawn > 1)
        {
            range = unitsToSpawn;
            range = Mathf.Sqrt(range);
            range *= commander.Values.size;
        }

        for (int i = 1; i < unitsToSpawn; i++)
        {
            AiController member = AiController.Create(GameUtils.RandomPlanePositionCircle(pos, range), unitPrefab, taskForce, redProjectileContainer, Affiliation.Red);
            member.transform.SetParent(redUnitsContainer.transform, true);
            member.gameObject.name += redUnitsCount;
            taskForce.AddUnit(member);
            onEnemySpawned.Invoke(member);
            redUnitsCount++;
        }

        onEnemyTaskForceSpawned?.Invoke(taskForce);

        TaskForceHUD.Create(taskForce, enemyHUD, gameManager);
        return taskForce;
    }

    private Outpost SpawnBlueOutpost(Vector3 pos, string name)
    {
        if (!CheckIfPositionViable(pos))
            return null;

        if (!CheckIfHavingResources(outpostPrefab))
            return null;

        Outpost outpost = Outpost.Create(pos, outpostPrefab, Affiliation.Blue, gameManager, playerHeadquarters).GetComponent<Outpost>();
        outpost.gameObject.name = name + blueOutpostsCount;
        outpost.transform.SetParent(blueOutpostsContainer.transform, true);
        //outpost.gameObject.AddComponent<HUDController>().Init(outpost, basicHUDVariant, gameManager);
        TaskForceHUD.Create(outpost, allyHUD, gameManager);

        RemovePlayerResources(outpost);
        onAllyOutpostSpawned?.Invoke(outpost);

        return outpost;
    }

    private Outpost SpawnRedOutpost(Vector3 pos, string name)
    {
        Outpost outpost = Outpost.Create(pos, outpostPrefab, Affiliation.Red, gameManager, null).GetComponent<Outpost>();
        outpost.gameObject.name = name + redOutpostsCount;
        outpost.transform.SetParent(redOutpostsContainer.transform, true);
        //outpost.gameObject.AddComponent<HUDController>().Init(outpost, basicHUDVariant, gameManager);
        TaskForceHUD.Create(outpost, enemyHUD, gameManager);

        onEnemyOutpostSpawned?.Invoke(outpost);

        return outpost;
    }

    public Object SpawnEntity(Vector3 position)
    {
        if (spawnOutpost && outpostPrefab.CompareTag("Ally"))
            return SpawnBlueOutpost(position, outpostNames.GetRandomName());

        else if (spawnOutpost && outpostPrefab.CompareTag("Enemy"))
            return SpawnRedOutpost(position, outpostNames.GetRandomName());

        else if (unitPrefab.CompareTag("Ally"))
            return SpawnBlueTaskForce(position, taskForceNames.GetRandomName());

        else if (unitPrefab.CompareTag("Enemy"))
            return SpawnRedTaskForce(position, taskForceNames.GetRandomName());

        else return null;
    }

    public void SetUnitToSpawn(GameObject unit)
    {
        spawnOutpost = false;
        unitPrefab = unit;
    }

    public void SetUnitsSpawnCount(int count)
    {
        unitsToSpawn = count;
    }

    public void SetOutpostToSpawn(GameObject prefab)
    {
        spawnOutpost = true;
        outpostPrefab = prefab;
    }

    // checks if clicked position is within the range of outpost network
    // player cant spawn entities outside of network
    // outpost must be connected with the rest of network, which is controlled by outpost script
    private bool CheckIfPositionViable(Vector3 pos)
    {
        // return true if in range of headquarters, network not needed
        if (Vector3.Distance(pos, playerHeadquarters.transform.position) <= playerHeadquarters.range)
            return true;

        // if not in range of headquarters:

        List<Outpost> nearbyOutposts = new();

        // adds outposts that have this position in their range
        foreach (Outpost outpost in playerHeadquarters.outpostNetwork)
        { 
            if (Vector3.Distance(pos, outpost.transform.position) <= outpost.range)
                nearbyOutposts.Add(outpost);
        }

        // if at least one of the outposts is connected, return true
        foreach (Outpost outpost in nearbyOutposts)
        {
            if (outpost.isConnected)
                return true;
        }

        // else return false
        return false;
    }

    private bool CheckIfHavingResources(GameObject prefabToSpawn)
    {
        if (resourceManager.playerTotalMoney <= 0)
            return false;

        if (prefabToSpawn.TryGetComponent(out AiController controller))
        {
            UnitValues values = controller.Values;
            if (values.metalPrice > resourceManager.PlayerCrystal || values.crystalPrice > resourceManager.PlayerMetal)
                return false;

            else return true;
        }

        else if (prefabToSpawn.TryGetComponent(out Outpost outpst))
        {
            OutpostValues values = outpst.values;
            if (values.metalPrice > resourceManager.PlayerCrystal || values.crystalPrice > resourceManager.PlayerMetal)
                return false;

            else return true;
        }

        return false;
    }

    private bool CheckIfHavingResources(GameObject prefabToSpawn, int number)
    {
        if (resourceManager.playerTotalMoney <= 0)
            return false;

        if (prefabToSpawn.TryGetComponent(out AiController controller))
        {
            UnitValues values = controller.Values;

            int metalTotalPrice = values.metalPrice * number;
            int crystalTotalPrice = values.crystalPrice * number;

            if (crystalTotalPrice > resourceManager.PlayerCrystal || metalTotalPrice > resourceManager.PlayerMetal)
                return false;

            else return true;
        }

        else if (prefabToSpawn.TryGetComponent(out Outpost outpst))
        {
            OutpostValues values = outpst.values;

            int metalTotalPrice = values.metalPrice * number;
            int crystalTotalPrice = values.crystalPrice * number;

            if (crystalTotalPrice > resourceManager.PlayerCrystal || metalTotalPrice > resourceManager.PlayerMetal)
                return false;

            else return true;
        }

        return false;
    }

    private void RemovePlayerResources(AiController unit)
    {
        UnitValues values = unit.Values;
        resourceManager.PlayerCrystal -= values.crystalPrice;
        resourceManager.PlayerMetal -= values.metalPrice;
        resourceManager.PlayerMaintenance += values.maintenancePrice;
    }

    private void RemovePlayerResources(Outpost outpost)
    {
        OutpostValues values = outpost.values;
        resourceManager.PlayerCrystal -= values.crystalPrice;
        resourceManager.PlayerMetal -= values.metalPrice;
        resourceManager.PlayerMaintenance += values.maintenancePrice;
    }
}
