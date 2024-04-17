using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FleetManager;

// nieaktualne
// w przysz³oœci mo¿e znajdzie siê zastosowanie
public class HUDManager : MonoBehaviour
{
    public FleetManager fleetManager;
    public Vector3 iconOffset = new(0, 2, 0);

    public List<TaskForceController> allyTaskForceList = new();
    public List<TaskForceController> enemyTaskForceList = new();

    // Start is called before the first frame update
    void Start()
    {
        allyTaskForceList = fleetManager.allyTaskForceList;
        enemyTaskForceList = fleetManager.enemyTaskForceList;
    }

    // Update is called once per frame
    //void Update()
    //{


    //    if (allyTaskForceList.Count > 0)
    //    {
    //        for (int i = 0; i < allyTaskForceList.Count; i++)
    //        {
    //            allyTaskForceList[i].UpdateHUD();
    //        }
    //    }

    //    if (enemyTaskForceList.Count > 0)
    //    {
    //        for (int i = 0; i < enemyTaskForceList.Count; i++)
    //        {
    //            enemyTaskForceList[i].UpdateHUD();
    //        }
    //    }


    //}
}
