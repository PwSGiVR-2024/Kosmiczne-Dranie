using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static FleetManager;

// skrypt steruj¹cy prefabem UI TaskForceContainer. Prefab zosta³ stworzony jako element listy i jest odpowiedzialny za wyœwietlanie informacji o danym taskForce
public class TaskForceContainer : MonoBehaviour
{
    

    public Image image; // t³o zmienia kolor w zale¿noœci od stanu

    public UnityEvent<TaskForceContainer> OnDestroy = new();
    public UnityEvent<TaskForceContainer> OnSelect = new(); // eventy subskrybowane przez FleetPanelController pozwalaj¹ na przechowanie informacji które elementy s¹ wybrane
    public UnityEvent<TaskForceContainer> OnDeselect = new();
    public bool selected = false;
    public UnityEngine.Color selectedColor;
    public UnityEngine.Color idleColor;
    public UnityEngine.Color combatColor;
    public UnityEngine.Color movingColor;
    public UnityEngine.Color retreatColor;
   


    public TaskForceController taskForce; // wszystkie informacje o taskForce
    public TMP_Text taskForceName; // TMP_Text wyœiwtla info UI
    public TMP_Text size;
    public TMP_Text strength;
    public TMP_Text power; // work in progress
    public TMP_Text status; // work in progress

    // Inicjalizacja musi zostaæ wykonana oddzielnie, po stworzeniu instancji skryptu, poniewa¿ Start() nie bierze parametrów
    public void Init(TaskForceController tf)
    {
        taskForce = tf;

        taskForceName.text = taskForce.TaskForceName;
        size.text = taskForce.Units.Count.ToString();
        strength.text = taskForce.Strength * 100 + "%";

        taskForce.onTaskForceDestroyed.AddListener(RemoveSelf); // taskForce posiada eventy sygnalizuj¹ce zmianê jakichœ atrybutów/parametrów. Element UI powinien to odzwierciedlaæ
        taskForce.onSizeChanged.AddListener(UpdateSize);
        taskForce.onStrengthChanged.AddListener(UpdateStrength);

    }

    // Selekcja elementu poprzez button (prafab jest jednoczeœnie buttonem, trzeba przypisaæ ToggleSelect() do buttona w inspektorze)
    public void ToggleSelect()
    {
        if (selected)
        {
            selected = false;
            image.color = idleColor;
            OnDeselect.Invoke(this);
        }
        else
        {
            selected = true;
            image.color = selectedColor;
            OnSelect.Invoke(this);
        }
    }

    // Powinno zostaæ przypisane do jakiegoœ buttona w prefabie podobnie jak ToggleSelect()
    // Skupia kamerê na danym taskForce, work in progress
    public void FocusCamera()
    {
        if (taskForce.Commander != null && taskForce.Commander.gameObject.activeSelf)
            Camera.main.transform.LookAt(taskForce.Commander.transform.position);
    }


    // triggerowane przez event kiedy taskForce zostanie zniszczony. Niszczy element listy
    private void RemoveSelf(TaskForceController _)
    {
        OnDestroy.Invoke(this);
        Destroy(gameObject);
    }

    // triggerowane przez event taskForca
    private void UpdateSize(int newSize)
    {
        size.text = newSize.ToString();
    }

    // triggerowane przez event taskForca
    private void UpdateStrength(float newStrength)
    {
        strength.text = Mathf.Round(newStrength * 100 ) + "%";
    }
}
