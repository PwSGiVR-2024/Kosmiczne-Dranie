using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


// nieaktualne
// w przysz³oœci mo¿e mieæ cel, ale to niejasne
public class TaskForceManager : MonoBehaviour
{
    public Spawner spawner;
    public LayerMask allyMask;
    public LayerMask enemyMask;

    public List<TaskForceController> allyTaskForces = new();
    public List<TaskForceController> enemyTaskForces = new();


    void Start()
    {
        spawner.onAllyTaskForceSpawned.AddListener(AddAlliedTaskForce);
        spawner.onEnemyTaskForceSpawned.AddListener(AddEnemyTaskForce);

        

    }

    private void AddAlliedTaskForce(TaskForceController taskForce)
    {
        allyTaskForces.Add(taskForce);
        taskForce.onEnemyTaskForceSpotted.AddListener(EngageTaskForce);
    }

    private void AddEnemyTaskForce(TaskForceController taskForce)
    {
        enemyTaskForces.Add(taskForce);
        taskForce.onEnemyTaskForceSpotted.AddListener(EngageTaskForce);
    }

    private void EngageTaskForce(TaskForceController attacker, TaskForceController defender)
    {
        //Debug.Log("engaging");
        //StartCoroutine(RefreshDestination(attacker, defender));
    }






}
