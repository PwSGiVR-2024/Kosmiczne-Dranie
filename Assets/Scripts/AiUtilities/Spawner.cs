using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AiController;
using static TaskForceController;


// klasa odpowiada za instancjonowanie ka¿dej jednostki oraz Task Forca w œwiecie gry
public class Spawner : MonoBehaviour
{
    public TaskForceHUD basicHUDVariant;

    public GameManager gameManager;

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

    public GameObject unitPrefab;
    public GameObject outpostPrefab;
    public int unitsToSpawn = 0;
    public bool spawnOutpost = false;

    public OutpostNames outpostNames;
    public TaskForceNames taskForceNames;

    private TaskForceController SpawnBlueTaskForce(Vector3 pos, string name)
    {
        if (pos == null || unitPrefab == null || unitsToSpawn < 1 || unitPrefab.CompareTag("Enemy") || !unitPrefab.CompareTag("Ally"))
            return null;

        TaskForceController taskForce = TaskForceController.Create(name, unitsToSpawn, gameManager, Affiliation.Blue);
        taskForce.transform.SetParent(blueTaskForceContainer.transform);
        taskForce.gameObject.AddComponent<HUDController>().Init(taskForce, basicHUDVariant, gameManager);

        AiController commander = AiController.Create(pos, unitPrefab, taskForce, blueProjectileContainer, Affiliation.Blue);
        commander.transform.SetParent(blueUnitsContainer.transform, true);
        commander.gameObject.name += blueUnitsCount;
        taskForce.AddUnit(commander);
        onAllySpawned.Invoke(commander);
        blueUnitsCount++;

        // randomowa pozycja jednostki jest ustawiana w zale¿noœci od ilosci spawnowanych jednostek
        float range = 0;
        if (unitsToSpawn > 1)
        {
            range = unitsToSpawn;
            range = Mathf.Sqrt(range);
            range *= commander.Volume;
        }

        for (int i = 1; i < unitsToSpawn; i++)
        {
            AiController member = AiController.Create(GameUtils.RandomPlanePositionCircle(pos, range), unitPrefab, taskForce, blueProjectileContainer, Affiliation.Blue);
            member.transform.SetParent(blueUnitsContainer.transform, true);
            member.gameObject.name += blueUnitsCount;
            taskForce.AddUnit(member);
            onAllySpawned.Invoke(member);
            blueUnitsCount++;
        }

        onAllyTaskForceSpawned?.Invoke(taskForce);

        return taskForce;
    }

    private TaskForceController SpawnRedTaskForce(Vector3 pos, string name)
    {
        if (unitPrefab == null || unitsToSpawn < 1 || unitPrefab.CompareTag("Ally") || !unitPrefab.CompareTag("Enemy"))
            return null;

        TaskForceController taskForce = TaskForceController.Create(name, unitsToSpawn, gameManager, Affiliation.Red);
        taskForce.transform.SetParent(redTaskForceContainer.transform);
        taskForce.gameObject.AddComponent<HUDController>().Init(taskForce, basicHUDVariant, gameManager);

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
            range *= commander.Volume;
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

        return taskForce;
    }

    private Outpost SpawnBlueOutpost(Vector3 pos, string name)
    {
        Outpost outpost = Outpost.Create(pos, outpostPrefab, Affiliation.Blue, gameManager).GetComponent<Outpost>();
        outpost.gameObject.name = name + blueOutpostsCount;
        outpost.transform.SetParent(blueOutpostsContainer.transform, true);
        outpost.gameObject.AddComponent<HUDController>().Init(outpost, basicHUDVariant, gameManager);

        return outpost;
    }

    private Outpost SpawnRedOutpost(Vector3 pos, string name)
    {
        Outpost outpost = Outpost.Create(pos, outpostPrefab, Affiliation.Red, gameManager).GetComponent<Outpost>();
        outpost.gameObject.name = name + redOutpostsCount;
        outpost.transform.SetParent(redOutpostsContainer.transform, true);
        outpost.gameObject.AddComponent<HUDController>().Init(outpost, basicHUDVariant, gameManager);

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
}
