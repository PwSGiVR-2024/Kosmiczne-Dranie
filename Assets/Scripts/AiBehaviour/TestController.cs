using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : AiController
{

    [Header("Test controller attributes:")]
    public int value;

    new public static TestController Create(string name, GameObject prefab, TaskForceController taskForce, Vector3 pos, GameObject projectileContainer, GameObject allyContainer)
    {
        TestController controller = (TestController)AiController.Create(name, prefab, taskForce, pos, projectileContainer, allyContainer);

        // coœ


        return controller;
    }




    new public void SetIdleState()
    {
        base.SetIdleState();

        // coœ
    }

    public override void Test2()
    {
        base.Test2();
    }

    public override void Test()
    {
        throw new System.NotImplementedException();
    }

    private void Update()
    {

    }
}
