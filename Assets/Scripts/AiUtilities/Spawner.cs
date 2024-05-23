using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static AiController;
using static TaskForceController;


// klasa odpowiada za instancjonowanie ka¿dej jednostki oraz Task Forca w œwiecie gry
public class Spawner : MonoBehaviour
{
    public GameManager gameManager;

    public GameObject enemyContainer; // kontenery ¿eby by³ porz¹dek na scenie. Tylko przechowuj¹ gameObjecty, nic wiêcej nie robi¹
    public GameObject allyContainer;
    public GameObject enemyProjectileContainer;
    public GameObject allyProjectileContainer;
    public GameObject allyTaskForceContainer;
    public GameObject enemyTaskForceContainer;
    public GameObject blueOutpostsContainer;

    public UnityEvent<GameObject> onAllySpawned = new();
    public UnityEvent<GameObject> onEnemySpawned = new();
    public UnityEvent<TaskForceController> onAllyTaskForceSpawned = new();
    public UnityEvent<TaskForceController> onEnemyTaskForceSpawned = new();

    private int allyCount = 1;
    private int enemyCount = 1;

    public GameObject unitPrefab;
    public GameObject outpostPrefab;
    public int unitsToSpawn = 0;
    public bool spawnOutpost = false;





    private TaskForceController CreateTaskForce(string name, int maxSize, GameObject icon, Vector3 iconOffset, GameObject container, GameManager gameManager, Affiliation side)
    {
        TaskForceController instance = new GameObject(name).AddComponent<TaskForceController>();
        instance.Init(name, maxSize, icon, iconOffset, gameManager, side);
        instance.gameObject.transform.SetParent(container.transform);

        return instance;
    }



    private AiController CreateUnit(string name, GameObject prefab, TaskForceController taskForce, Vector3 pos, GameObject projectileContainer, GameObject allyContainer, Affiliation side)
    {
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity, allyContainer.transform);
        instance.name = name;

        AiController controller = instance.GetComponent<AiController>();
        controller.Init(side, taskForce, projectileContainer);
        return controller;
    }


    private AiController SpawnAlly(GameObject prefab, Vector3 pos, TaskForceController taskForce)
    {
        AiController unit = CreateUnit("Ally_" + allyCount, prefab, taskForce, pos, allyProjectileContainer, allyContainer, Affiliation.Blue);

        onAllySpawned?.Invoke(unit.gameObject);
        allyCount++;

        return unit;
    }

    private AiController SpawnEnemy(GameObject prefab, Vector3 pos, TaskForceController taskForce)
    {
        AiController unit = CreateUnit("Enemy_" + enemyCount, prefab, taskForce, pos, enemyProjectileContainer, enemyContainer, Affiliation.Red);
        onAllySpawned?.Invoke(unit.gameObject);
        enemyCount++;

        return unit;
    }

    private TaskForceController SpawnBlueTaskForce(Vector3 pos, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        if (pos == null || unitPrefab == null || unitsToSpawn < 1 || iconPrefab == null || renderingCanvas == null || unitPrefab.CompareTag("Enemy") || !unitPrefab.CompareTag("Ally"))
            return null;

        GameObject icon = Instantiate(iconPrefab, renderingCanvas.transform); // ikona jest kopiowana, a canvas jest ustawiany jako parent bo musi byæ

        TaskForceController taskForce = CreateTaskForce(name, unitsToSpawn, icon, iconOffset, allyTaskForceContainer, gameManager, Affiliation.Blue);

        AiController commander = SpawnAlly(unitPrefab, pos, taskForce);
        taskForce.AddUnit(commander);

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
            AiController ally = SpawnAlly(unitPrefab, GameUtils.RandomPlanePositionCircle(pos, range), taskForce);
            taskForce.AddUnit(ally);
        }

        onAllyTaskForceSpawned?.Invoke(taskForce);

        return taskForce;
    }

    private TaskForceController SpawnRedTaskForce(Vector3 pos, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        if (pos == null || unitPrefab == null || unitsToSpawn < 1 || iconPrefab == null || renderingCanvas == null || unitPrefab.CompareTag("Ally") || !unitPrefab.CompareTag("Enemy"))
            return null;

        GameObject icon = Instantiate(iconPrefab, renderingCanvas.transform); // ikona jest kopiowana, a canvas jest ustawiany jako parent bo musi byæ

        TaskForceController taskForce = CreateTaskForce(name, unitsToSpawn, icon, iconOffset, enemyTaskForceContainer, gameManager, Affiliation.Red);

        AiController commander = SpawnEnemy(unitPrefab, pos, taskForce);
        taskForce.AddUnit(commander);

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
            AiController enemy = SpawnEnemy(unitPrefab, GameUtils.RandomPlanePositionCircle(pos, range), taskForce);
            taskForce.AddUnit(enemy);
        }

        onEnemyTaskForceSpawned?.Invoke(taskForce);

        return taskForce;
    }

    public TaskForceController SpawnTaskForce(Vector3 pos, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        if (unitPrefab.CompareTag("Enemy"))
            return SpawnRedTaskForce(pos, name, iconPrefab, renderingCanvas, iconOffset);

        else if (unitPrefab.CompareTag("Ally"))
            return SpawnBlueTaskForce(pos, name, iconPrefab, renderingCanvas, iconOffset);

        else return null;
    }


    public void SetUnitToSpawn(GameObject unit)
    {
        unitPrefab = unit;
    }

    public void SetSpawnCount(int count)
    {
        unitsToSpawn = count;
    }

    public void SetOutpostToSpawn(GameObject prefab)
    {
        spawnOutpost = true;
        outpostPrefab = prefab;
    }

    public Outpost SpawnOutpost(Vector3 pos, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        spawnOutpost = false;

        if (outpostPrefab.CompareTag("Ally"))
            return SpawnBlueOutpost(pos, name, iconPrefab, renderingCanvas, iconOffset);

        else
            return null;
    }

    private Outpost SpawnBlueOutpost(Vector3 pos, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        GameObject icon = Instantiate(iconPrefab, renderingCanvas.transform); // ikona jest kopiowana, a canvas jest ustawiany jako parent bo musi byæ

        Outpost outpost = Instantiate(outpostPrefab, pos, Quaternion.identity, blueOutpostsContainer.transform).GetComponent<Outpost>();

        outpost.gameObject.name = name;

        outpost.Init(icon, iconOffset, gameManager, Affiliation.Blue);

        return outpost;
    }


}
