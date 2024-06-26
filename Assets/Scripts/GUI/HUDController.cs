using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    private RectTransform rectTransform;
    public MonoBehaviour owner;

    public Image healthBar;
    public Image strengthBar;

    public GameObject additionalInfoContainer;
    public TMP_Text unitsCount;
    public TMP_Text healthText;

    private enum DisplayMode { TaskForce, Outpost }
    private DisplayMode displayMode;
    public Vector3 offset = new(0, 20, 0);

    public UnityEvent onSelect = new();

    public static HUDController Create(MonoBehaviour owner, GameObject prefab, GameManager gameManager)
    {
        

        HUDController instance = Instantiate(prefab, gameManager.worldSpaceCanvas.transform).GetComponent<HUDController>();

        if (owner is TaskForceController taskForce)
        {
            instance.displayMode = DisplayMode.TaskForce;
            instance.owner = taskForce;
            taskForce.onStrengthChanged.AddListener(instance.UpdateStrengthBar);
            taskForce.onHealthChanged.AddListener((newHealth) => instance.UpdateHealthBar(newHealth, taskForce.InitialHealth));
            taskForce.onTaskForceDestroyed.AddListener((_) => Destroy(instance.gameObject));
            taskForce.onHealthChanged.AddListener((_) => instance.unitsCount.text = taskForce.Units.Count.ToString());
            instance.healthText?.gameObject.SetActive(false);
        }

        else if (owner is Outpost outpost)
        {
            instance.displayMode = DisplayMode.Outpost;
            instance.owner = outpost;
            outpost.onOutpostDestroy.AddListener(() => Destroy(instance.gameObject));
            outpost.onHealthChanged.AddListener((newHealth) =>
            {
                instance.UpdateHealthBar(newHealth, outpost.values.health);
                instance.healthText.text = outpost.values.health.ToString();
            });
            instance.unitsCount?.gameObject.SetActive(false);
        }

        else return null;

        instance.rectTransform = instance.gameObject.GetComponent<RectTransform>();
        instance.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        instance.transform.localScale = new Vector3(0.25f, 0.25f, 25f);
        return instance;
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);

        float cameraDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        rectTransform.localScale = new Vector3(1, 1, 1) * cameraDistance * 0.0005f;

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

    public void ShowAdditionalInfo(BaseEventData data)
    {
        additionalInfoContainer.SetActive(true);
    }

    public void HideAdditionalInfo(BaseEventData data)
    {
        additionalInfoContainer.SetActive(false);
    }

    public void OnPointerClick(BaseEventData data)
    {
        Debug.Log("click");
    }
}
