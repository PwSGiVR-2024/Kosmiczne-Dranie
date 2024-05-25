using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    private enum DisplayMode { TaskForceDynamic, Static }
    private DisplayMode displayMode;

    private TaskForceController taskForce = null;
    public Transform staticOwner = null;

    public GameObject icon;
    TaskForceHUD hud;

    public void Init(MonoBehaviour owner, TaskForceHUD hud, GameManager gameManager)
    {
        if (owner is TaskForceController taskForce)
        {
            displayMode = DisplayMode.TaskForceDynamic;
            this.taskForce = taskForce;
            staticOwner = null;
        }

        else
        {
            displayMode = DisplayMode.Static;
            this.taskForce = null;
            staticOwner = owner.transform;
        }

        this.hud = hud;
        icon = Instantiate(hud.icon, gameManager.worldSpaceCanvas.transform);
    }

    void Update()
    {
        switch (displayMode) {
            case DisplayMode.TaskForceDynamic:
                DynamicUpdate();
                break;

            case DisplayMode.Static:
                StaticUpdate();
                break;
        }
    }

    private void DynamicUpdate()
    {
        if (taskForce.Commander == null)
            return;

        if (taskForce == null)
            Destroy(gameObject);

        icon.transform.LookAt(Camera.main.transform, Vector3.up);
        icon.transform.position = taskForce.Commander.transform.position + hud.offset;
    }

    private void StaticUpdate()
    {
        if (staticOwner == null)
            Destroy(gameObject);

        icon.transform.LookAt(Camera.main.transform, Vector3.up);
        icon.transform.position = staticOwner.transform.position + hud.offset;
    }

    private void OnDestroy()
    {
        Destroy(icon);
    }
}
