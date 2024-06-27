using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public Image indicator;
    public Image selectIndicator;

    private RectTransform rectTransform;
    public MonoBehaviour owner;

    public Image healthBar;
    public Image strengthBar;

    public Image defaultBackground;
    public Image selectBackground;

    public GameObject additionalInfoContainer;
    public TMP_Text unitsCount;
    public TMP_Text healthText;

    private enum DisplayMode { TaskForce, Outpost }
    private DisplayMode displayMode;
    public Vector3 offset = new();

    private bool selected = false;

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
            //instance.onSelect.AddListener(() => instance.ToggleSelect());
            taskForce.onSelect.AddListener(() => instance.ToggleSelect());
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
        //transform.position = taskForce.Commander.transform.position + offset;

        rectTransform.position = taskForce.Commander.transform.position + offset;

        if (indicator)
        {
            indicator.rectTransform.position = new Vector3(taskForce.Commander.transform.position.x, -20, taskForce.Commander.transform.position.z);
            indicator.rectTransform.rotation = Quaternion.Euler(90, 0, 0);
        }

        if (selectIndicator)
        {
            selectIndicator.rectTransform.position = new Vector3(taskForce.Commander.transform.position.x, -20, taskForce.Commander.transform.position.z);
            selectIndicator.rectTransform.rotation = Quaternion.Euler(90, 0, 0);
        }
            
            
    }

    private void OutpostUpdate()
    {
        Outpost outpost = owner as Outpost;

        //transform.LookAt(Camera.main.transform, Vector3.up);
        //transform.position = outpost.transform.position + offset;

        rectTransform.position = outpost.transform.position + offset;

        if (indicator)
        {
            indicator.rectTransform.position = new Vector3(outpost.transform.position.x, -20, outpost.transform.position.z);
            indicator.rectTransform.rotation = Quaternion.Euler(90, 0, 0);
        }

        if (selectIndicator)
        {
            selectIndicator.rectTransform.position = new Vector3(outpost.transform.position.x, -20, outpost.transform.position.z);
            selectIndicator.rectTransform.rotation = Quaternion.Euler(90, 0, 0);
        }
            
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
        if (owner is TaskForceController tf)
            tf.onSelect.Invoke();
    }


    public void ToggleSelect()
    {
        selected = !selected;

        if (selected)
        {
            defaultBackground?.gameObject.SetActive(false);
            selectBackground?.gameObject.SetActive(true);
            indicator?.gameObject.SetActive(false);
            selectIndicator?.gameObject.SetActive(true);
        }

        else
        {
            defaultBackground?.gameObject.SetActive(true);
            selectBackground?.gameObject.SetActive(false);
            indicator?.gameObject.SetActive(true);
            selectIndicator?.gameObject.SetActive(false);
        }
    }
}
