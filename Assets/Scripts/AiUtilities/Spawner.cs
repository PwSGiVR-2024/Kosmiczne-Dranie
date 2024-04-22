using UnityEngine;
using UnityEngine.Events;
using static AiController;


// klasa odpowiada za instancjonowanie ka¿dej jednostki oraz Task Forca w œwiecie gry
public class Spawner : MonoBehaviour
{
    public GameObject enemyContainer; // kontenery ¿eby by³ porz¹dek na scenie. Tylko przechowuj¹ gameObjecty, nic wiêcej nie robi¹
    public GameObject allyContainer;
    public GameObject enemyProjectileContainer;
    public GameObject allyProjectileContainer;
    public GameObject allyTaskForceContainer;
    public GameObject enemyTaskForceContainer;

    public UnityEvent<GameObject> onAllySpawned = new();
    public UnityEvent<GameObject> onEnemySpawned = new();
    public UnityEvent<TaskForceController> onAllyTaskForceSpawned = new();
    public UnityEvent<TaskForceController> onEnemyTaskForceSpawned = new();

    private int allyCount = 1;
    private int enemyCount = 1;

    public GameObject unitPrefab;
    public int unitsToSpawn = 0;

    private AiController CreateUnit(string name, GameObject prefab, TaskForceController taskForce, Vector3 pos, GameObject projectileContainer, GameObject allyContainer)
    {
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity, allyContainer.transform);
        instance.name = name;

        Side unitSide = Side.Neutral;
        if (instance.CompareTag("Ally"))
        {
            unitSide = Side.Ally;
            instance.layer = LayerMask.NameToLayer("Allies");
        }

        else if (instance.CompareTag("Enemy"))
        {
            unitSide = Side.Enemy;
            instance.layer = LayerMask.NameToLayer("Enemies");
        }

        AiController controller = instance.GetComponent<AiController>();
        controller.Init(unitSide, taskForce, projectileContainer);
        return controller;
    }


    private AiController SpawnAlly(GameObject prefab, Vector3 pos, TaskForceController taskForce)
    {
        AiController unit = CreateUnit("Ally_" + allyCount, prefab, taskForce, pos, allyProjectileContainer, allyContainer);

        onAllySpawned?.Invoke(unit.gameObject);
        allyCount++;

        return unit;
    }

    private AiController SpawnEnemy(GameObject prefab, Vector3 pos, TaskForceController taskForce)
    {
        AiController unit = CreateUnit("Enemy_" + enemyCount, prefab, taskForce, pos, enemyProjectileContainer, enemyContainer);
        onAllySpawned?.Invoke(unit.gameObject);
        enemyCount++;

        return unit;
    }

    private TaskForceController SpawnAllyTaskForce(Vector3 pos, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        if (pos == null || unitPrefab == null || unitsToSpawn < 1 || iconPrefab == null || renderingCanvas == null || unitPrefab.CompareTag("Enemy") || !unitPrefab.CompareTag("Ally"))
            return null;

        GameObject icon = Instantiate(iconPrefab, renderingCanvas.transform); // ikona jest kopiowana, a canvas jest ustawiany jako parent bo musi byæ

        TaskForceController taskForce = TaskForceController.Create(name, unitsToSpawn, false, icon, iconOffset, allyTaskForceContainer);

        AiController commander = SpawnAlly(unitPrefab, pos, taskForce);
        taskForce.AddUnit(commander.gameObject);

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
            taskForce.AddUnit(ally.gameObject);
        }

        onAllyTaskForceSpawned?.Invoke(taskForce);

        return taskForce;
    }

    private TaskForceController SpawnEnemyTaskForce(Vector3 pos, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        if (pos == null || unitPrefab == null || unitsToSpawn < 1 || iconPrefab == null || renderingCanvas == null || unitPrefab.CompareTag("Ally") || !unitPrefab.CompareTag("Enemy"))
            return null;

        GameObject icon = Instantiate(iconPrefab, renderingCanvas.transform); // ikona jest kopiowana, a canvas jest ustawiany jako parent bo musi byæ

        TaskForceController taskForce = TaskForceController.Create(name, unitsToSpawn, false, icon, iconOffset, enemyTaskForceContainer);

        AiController commander = SpawnEnemy(unitPrefab, pos, taskForce);
        taskForce.AddUnit(commander.gameObject);

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
            taskForce.AddUnit(enemy.gameObject);
        }

        onEnemyTaskForceSpawned?.Invoke(taskForce);

        return taskForce;
    }

    public TaskForceController SpawnTaskForce(Vector3 pos, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        if (unitPrefab.CompareTag("Enemy"))
            return SpawnEnemyTaskForce(pos, name, iconPrefab, renderingCanvas, iconOffset);

        else if (unitPrefab.CompareTag("Ally"))
            return SpawnAllyTaskForce(pos, name, iconPrefab, renderingCanvas, iconOffset);

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
}
