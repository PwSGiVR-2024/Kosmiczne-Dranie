using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// skrypt steruj�cy prefabem UI TaskForceContainer. Prefab zosta� stworzony jako element listy i jest odpowiedzialny za wy�wietlanie informacji o danym taskForce
public class TaskForceContainer : MonoBehaviour
{
    

    public Image image; // t�o zmienia kolor w zale�no�ci od stanu

    public UnityEvent<TaskForceContainer> OnDestroy = new();
    public UnityEvent<TaskForceContainer> OnSelect = new(); // eventy subskrybowane przez FleetPanelController pozwalaj� na przechowanie informacji kt�re elementy s� wybrane
    public UnityEvent<TaskForceContainer> OnDeselect = new();
    public bool selected = false;
    public UnityEngine.Color selectedColor;
    public UnityEngine.Color idleColor;
    public UnityEngine.Color combatColor;
    public UnityEngine.Color movingColor;
    public UnityEngine.Color retreatColor;
   


    public TaskForceController taskForce; // wszystkie informacje o taskForce
    public TMP_Text taskForceName; // TMP_Text wy�iwtla info UI
    public TMP_Text size;
    public TMP_Text strength;
    public TMP_Text power; // work in progress
    public TMP_Text status; // work in progress

    // Inicjalizacja musi zosta� wykonana oddzielnie, po stworzeniu instancji skryptu, poniewa� Start() nie bierze parametr�w
    public void Init(TaskForceController tf)
    {
        taskForce = tf;

        taskForceName.text = taskForce.gameObject.name;
        size.text = taskForce.Units.Count.ToString();
        strength.text = 100 + "%";
        power.text = taskForce.CurrentPower.ToString();
        status.text = "Idle";

        taskForce.onTaskForceDestroyed.AddListener(RemoveSelf); // taskForce posiada eventy sygnalizuj�ce zmian� jakich� atrybut�w/parametr�w. Element UI powinien to odzwierciedla�
        taskForce.onSizeChanged.AddListener(UpdateSize);
        taskForce.onStrengthChanged.AddListener(UpdateStrength);
        taskForce.onPowerChanged.AddListener((newPower) => power.text = newPower.ToString());
        taskForce.onStateChanged.AddListener(UpdateState);
        taskForce.onSelect.AddListener(() => ToggleSelect());
    }

    private void UpdateState(State state)
    {
        switch (state)
        {
            case State.Idle:
                status.text = "Idle";
                break;

            case State.Combat:
                status.text = "Combat";
                break;

            case State.Retreat:
                status.text = "Retreat";
                break;

            case State.Moving:
                status.text = "Moving";
                break;
        }
    }

    // Selekcja elementu poprzez button (prafab jest jednocze�nie buttonem, trzeba przypisa� ToggleSelect() do buttona w inspektorze)
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

    public void OnPointerClick()
    {
        taskForce.onSelect.Invoke();
    }

    // Powinno zosta� przypisane do jakiego� buttona w prefabie podobnie jak ToggleSelect()
    // Skupia kamer� na danym taskForce, work in progress
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
