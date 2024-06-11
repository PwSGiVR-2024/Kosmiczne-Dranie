using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TaskForceHUD : MonoBehaviour
{
    public MonoBehaviour owner;

    public Image healthBar;
    public Image strengthBar;

    private enum DisplayMode { TaskForce, Outpost }
    private DisplayMode displayMode;
    public Vector3 offset = new(0, 20, 0);

    public UnityEvent onSelect = new();

    public static TaskForceHUD Create(MonoBehaviour owner, GameObject prefab, GameManager gameManager)
    {
        TaskForceHUD instance = Instantiate(prefab, gameManager.worldSpaceCanvas.transform).GetComponent<TaskForceHUD>();

        if (owner is TaskForceController taskForce)
        {
            instance.displayMode = DisplayMode.TaskForce;
            instance.owner = taskForce;
            taskForce.onStrengthChanged.AddListener(instance.UpdateStrengthBar);
            taskForce.onHealthChanged.AddListener((newHealth) => instance.UpdateHealthBar(newHealth, taskForce.InitialHealth));
            taskForce.onTaskForceDestroyed.AddListener((_) => Destroy(instance.gameObject));
        }

        else if (owner is Outpost outpost)
        {
            instance.displayMode = DisplayMode.Outpost;
            instance.owner = outpost;
            outpost.onHealthChanged.AddListener((newHealth) => instance.UpdateHealthBar(newHealth, outpost.values.health));
            outpost.onOutpostDestroy.AddListener(() => Destroy(instance.gameObject));
        }

        else return null;

        
        instance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        instance.transform.localScale = new Vector3(0.25f, 0.25f, 25f);
        return instance;
    }

    void Update()
    {
        switch (displayMode)
        {
            case DisplayMode.TaskForce:
                TaskForceUpdate();
                break;

            case DisplayMode.Outpost:
                OutpostUpdate();
                break;
        }
    }

    private void TaskForceUpdate()
    {
        TaskForceController taskForce = owner as TaskForceController;

        if (taskForce.Commander == null)
            return;

        //transform.LookAt(Camera.main.transform, Vector3.up);
        transform.position = taskForce.Commander.transform.position + offset;
    }

    private void OutpostUpdate()
    {
        Outpost outpost = owner as Outpost;

        //transform.LookAt(Camera.main.transform, Vector3.up);
        transform.position = outpost.transform.position + offset;
    }

    private void UpdateHealthBar(int newHealth, int originalHealth)
    {
        healthBar.fillAmount = (float)newHealth / originalHealth;
    }

    private void UpdateStrengthBar(float value)
    {
        strengthBar.fillAmount = value;
    }
}
