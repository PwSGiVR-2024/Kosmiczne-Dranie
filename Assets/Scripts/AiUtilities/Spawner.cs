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


        //GameObject currentEntity = Instantiate(prefab, pos, Quaternion.identity, allyContainer.transform);
        //currentEntity.layer = 7; // warstwa Allies
        //currentEntity.transform.SetParent(allyContainer.transform, true);

        //AiController aiController = currentEntity.GetComponent<AiController>();

        //currentEntity.name = aiController.unitValues.prefabName + allyCount;
        //aiController.Init(taskForce, aiManager, allyProjectileContainer);

        onAllySpawned?.Invoke(unit.gameObject);
        allyCount++;

        return unit;
    }

    private AiController SpawnEnemy(GameObject prefab, Vector3 pos, TaskForceController taskForce)
    {
        //GameObject currentEntity = Instantiate(prefab, pos, Quaternion.identity, enemyContainer.transform);
        //currentEntity.layer = 6; // warstwa Enemies
        ////currentEntity.transform.SetParent(enemyContainer.transform, true);

        //AiController aiController = currentEntity.GetComponent<AiController>();

        //currentEntity.name = aiController.unitValues.prefabName + enemyCount;
        //aiController.Init(taskForce, aiManager, enemyProjectileContainer);

        //onEnemySpawned?.Invoke(currentEntity);
        //enemyCount++;
        //return currentEntity;

        AiController unit = CreateUnit("Enemy_" + enemyCount, prefab, taskForce, pos, enemyProjectileContainer, enemyContainer);
        onAllySpawned?.Invoke(unit.gameObject);
        enemyCount++;

        return unit;
    }

    public TaskForceController SpawnAllyTaskForce(Vector3 pos, GameObject unit, int unitsToSpawn, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        if (pos == null || unit == null || unitsToSpawn < 1 || iconPrefab == null || renderingCanvas == null || unit.CompareTag("Enemy") || !unit.CompareTag("Ally"))
            return null;

        GameObject icon = Instantiate(iconPrefab, renderingCanvas.transform); // ikona jest kopiowana, a canvas jest ustawiany jako parent bo musi byæ

        TaskForceController taskForce = TaskForceController.Create(name, unitsToSpawn, false, icon, iconOffset, allyTaskForceContainer);

        // randomowa pozycja jednostki jest ustawiana w zale¿noœci od ilosci spawnowanych jednostek
        float range = 0;
        if (unitsToSpawn > 1)
        {
            range = unitsToSpawn;
            range = Mathf.Sqrt(range);
            range *= 2;
        }

        AiController commander = SpawnAlly(unit, pos, taskForce);
        taskForce.AddUnit(commander.gameObject);

        for (int i = 1; i < unitsToSpawn; i++)
        {
            AiController ally = SpawnAlly(unit, GameUtils.RandomPlanePositionCircle(pos, range), taskForce);
            taskForce.AddUnit(ally.gameObject);
        }

        onAllyTaskForceSpawned?.Invoke(taskForce);

        return taskForce;
    }

    public TaskForceController SpawnEnemyTaskForce(Vector3 pos, GameObject unit, int unitsToSpawn, string name, GameObject iconPrefab, Canvas renderingCanvas, Vector3 iconOffset)
    {
        if (pos == null || unit == null || unitsToSpawn < 1 || iconPrefab == null || renderingCanvas == null || unit.CompareTag("Ally") || !unit.CompareTag("Enemy"))
            return null;

        GameObject icon = Instantiate(iconPrefab, renderingCanvas.transform); // ikona jest kopiowana, a canvas jest ustawiany jako parent bo musi byæ

        TaskForceController taskForce = TaskForceController.Create(name, unitsToSpawn, false, icon, iconOffset, enemyTaskForceContainer);


        // randomowa pozycja jednostki jest ustawiana w zale¿noœci od ilosci spawnowanych jednostek
        float range = 0;
        if (unitsToSpawn > 1)
        {
            range = unitsToSpawn;
            range = Mathf.Sqrt(range);
            range *= 2;
        }

        AiController commander = SpawnEnemy(unit, pos, taskForce);
        taskForce.AddUnit(commander.gameObject);

        for (int i = 1; i < unitsToSpawn; i++)
        {
            AiController enemy = SpawnEnemy(unit, GameUtils.RandomPlanePositionCircle(pos, range), taskForce);
            taskForce.AddUnit(enemy.gameObject);
        }

        onEnemyTaskForceSpawned?.Invoke(taskForce);

        return taskForce;
    }
}
