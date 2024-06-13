using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Rendering.CameraUI;


// klasa odpowiada za instancjonowanie ka¿dej jednostki oraz Task Forca w œwiecie gry
public class Spawner : MonoBehaviour
{
    public Affiliation affiliation = Affiliation.None;
    public GameManager gameManager;

    public GameObject outpostsContainer;
    public GameObject taskForcesContainer;
    public GameObject unitsContainer;
    public GameObject projectilesContainer;

    public GameObject taskForceHUD;

    private int unitCount = 1;
    private int outpostCount = 1;
    private int taskForceCount = 1;

    public UnityEvent<AiController> onUnitSpawned = new();
    public UnityEvent<Outpost> onOutpostSpawned = new();
    public UnityEvent<TaskForceController> onTaskForceSpawned = new();

    public virtual TaskForceController SpawnTaskForce(GameObject unitPrefab, Vector3 position, int spawnCount, string name)
    {
        if (affiliation == Affiliation.None)
            return null;

        TaskForceController taskForce = TaskForceController.Create(name, gameManager, affiliation);
        taskForce.transform.SetParent(taskForcesContainer.transform);

        AiController commander = AiController.Create(position, unitPrefab, taskForce, projectilesContainer, affiliation);
        commander.transform.SetParent(unitsContainer.transform, true);
        commander.gameObject.name += unitCount;
        taskForce.AddUnit(commander);
        onUnitSpawned.Invoke(commander);
        unitCount++;

        // randomowa pozycja jednostki jest ustawiana w zale¿noœci od ilosci spawnowanych jednostek
        float range = 0;
        if (spawnCount > 1)
        {
            range = spawnCount;
            range = Mathf.Sqrt(range);
            range *= commander.Values.size;
        }

        for (int i = 1; i < spawnCount; i++)
        {
            AiController member = AiController.Create(GameUtils.RandomPlanePositionCircle(position, range), unitPrefab, taskForce, projectilesContainer, affiliation);
            member.transform.SetParent(unitsContainer.transform, true);
            member.gameObject.name += unitCount;
            taskForce.AddUnit(member);
            onUnitSpawned.Invoke(member);
            unitCount++;
        }

        onTaskForceSpawned.Invoke(taskForce);

        TaskForceHUD.Create(taskForce, taskForceHUD, gameManager);
        return taskForce;
    }

    public virtual Outpost SpawnOutpost(GameObject outpostPrefab, Vector3 position, string name)
    {
        if (affiliation == Affiliation.None)
            return null;

        Outpost outpost = Outpost.Create(position, outpostPrefab, affiliation, gameManager).GetComponent<Outpost>();
        outpost.gameObject.name = name + outpostCount;
        outpost.transform.SetParent(outpostsContainer.transform, true);
        TaskForceHUD.Create(outpost, taskForceHUD, gameManager);

        onOutpostSpawned.Invoke(outpost);

        return outpost;
    }
}
