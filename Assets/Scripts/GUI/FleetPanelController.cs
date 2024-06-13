using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static InputManager;
using static FleetManager;


// skrypt odpowiada za panel UI, odpowiadaj�cy za sterowanie flot� (w projekcie ScreenSpaceCanvas -> FleetPanel)
public class FleetPanelController : MonoBehaviour
{
    // enum z InputManagera, bo ten skrypt te� oferuje alternatywne sterowanie
    public InputControl currentState = InputControl.Normal;

    // skrypty do komunikacji
    public PlayerFleetManager fleetManager;
    public InputManager input;
    public Spawner spawner;

    // elementy ui
    public ScrollableList taskForceList; // customowy skrypt

    public Button buttonMerge;

    public List<TaskForceContainer> allElements = new(); // lista wszystkich element�w scroll listy
    public List<TaskForceContainer> selectedTaskForceContainers = new(); // lista wybranych element�w w scroll li�cie
    public List<TaskForceController> selectedTaskForces = new(); // lista task forc�w przypadaj�cych elementom ui

    public SelectUnitButton buttonSelectFrigate;
    public SelectUnitButton buttonSelectDestroyer;
    public SelectUnitButton buttonSelectCruiser;
    public SelectUnitButton buttonSelectBattleship;
    public SelectUnitButton buttonSelectOutpost;

    public SelectUnitButton currentSelection;

    void Start()
    {
        input.OnStateChange.AddListener(ChangeCurrentState); // zmiana stanu na podstawie inputManagera
        buttonMerge.onClick.AddListener(MergeWrapper);
        spawner.onTaskForceSpawned.AddListener(AddTaskForceToList); // je�li task force jest spawnowany, to pojawia sie na listach i w ui

        buttonSelectFrigate.onSelect.AddListener(() => SelectButton(buttonSelectFrigate));
        buttonSelectDestroyer.onSelect.AddListener(() => SelectButton(buttonSelectDestroyer));
        buttonSelectCruiser.onSelect.AddListener(() => SelectButton(buttonSelectCruiser));
        buttonSelectBattleship.onSelect.AddListener(() => SelectButton(buttonSelectBattleship));
        buttonSelectOutpost.onSelect.AddListener(() => SelectButton(buttonSelectOutpost));
    }

    public void SelectButton(SelectUnitButton button)
    {
        DeselectAllOtherButtons(button);
        currentSelection = button;
        //fleetManager.SetUnitToProcure(button.unitPrefab, button.selectedCount);
    }

    private void ChangeCurrentState(InputControl state)
    {
        currentState = state;
    }

    private char ValidateCountInput(string text, int charIndex, char addedChar)
    {
        if (!char.IsNumber(addedChar) && addedChar != '\b') // Allow backspace for deletion
        {
            return '\0'; // Return null character to prevent invalid input
        }
        return addedChar;
    }

    public void AddTaskForceToList(TaskForceController taskForce)
    {
        // w tym przypadku instance to fizyczny obiekt na scenie (prefab z do��czonym skryptem TaskForceContainer). taskForceList to skrypt odpowiedzialny za list� ze scrollem i inicjalizacj� prefab�w
        GameObject instance = taskForceList.CreateAndAddElement();
        TaskForceContainer container = instance.GetComponent<TaskForceContainer>(); // wydzielony skrypt �eby nie u�ywa� GetComponent kilka razy niepotrzebnie
        allElements.Add(container); // element dodawany do listy dla wygodniejszego dost�pu
        container.Init(taskForce); // inicjalizacja skryptu musi zosta� wykonana oddzielnie
        container.OnSelect.AddListener(AddTaskForceToSelection); // skrypt posiada eventy informuj�ce czy element zosta� wybrany, czy nie
        container.OnDeselect.AddListener(RemoveTaskForceFromSelection);
        container.OnDestroy.AddListener(RemoveElement);
    }

    
    private void AddTaskForceToSelection(TaskForceContainer container)
    {
        // normal -> selectuje tylko jeden
        // je�li false, to dodaje do selekcji
        if (currentState == InputControl.Normal)
            ClearSelectionExcluding(container);

        // dwie oddzielne listy �eby u�atwi� dost�p przez inne skrypty. Jedna odpowiada za elementy w UI, a druga za odpowiadaj�ce im taskForcy
        // przyk�adowo FleetManager nie potrzebuje �adnych informacji o wybranych elementach UI, ale list� wybranych taskForce ju� tak
        // mo�na wykorzysta� jedn� list�, ale tak jest szybciej, bo jesli co� potrzebowa�oby listy taskForc�w to musia�oby sobie to od nowa tworzy� w p�tli, a tak ma od razu
        selectedTaskForceContainers.Add(container);
        selectedTaskForces.Add(container.taskForce);
    }

    private void RemoveTaskForceFromSelection(TaskForceContainer container)
    {
        selectedTaskForceContainers.Remove(container);
        selectedTaskForces.Remove(container.taskForce);
    }


    // to nie dzia�a ale zostawi�em jako pomnik, wi�cej info ni�ej
    public void MergeSelected()
    {
        if (selectedTaskForceContainers.Count == 0)
            return;

        foreach (var taskForce in selectedTaskForces)
        {
            selectedTaskForces[0].Merge(taskForce);
        }

        selectedTaskForceContainers[0].ToggleSelect();
        selectedTaskForceContainers.Clear();
        selectedTaskForces.Clear();
    }

    // to si� wydaje bez sensu ale tak musi by� je�li chcemy mergowa� wiele element�w z listy
    // wywo�anie Merge() niszczy task forca, a co za tym idzie r�wnie� referencje do aktualnej selekcji
    // to znaczy �e p�tla nie mo�e si� wykona� wiecej ni� jedn� iteracj�, bo d�ugo�� kolekcji (lista aktualnej selekcji) si� zmniejsza
    // tak po prostu jest, nie da si� przeskoczy�. D�ugo�� listy nie mo�e si� zmieni� w trakcie iteracji
    // metoda rekurencyjna nie korzysta z p�tli wi�c nie ma tego problemu
    // po zmniejszeniu d�ugo�ci kolekcji, metoda jest po prostu wykonywana ponownie, dla nast�pnego elementu
    public void MergeWrapper()
    {
        if (selectedTaskForceContainers.Count == 0)
            return;

        MergeSelectedRecursive(selectedTaskForces[0]); // Merge() jest wywo�ywane dla pierwszego wybranego elementu
    }

    public void MergeSelectedRecursive(TaskForceController master)
    {
        if (selectedTaskForces.Count > 1)
        {
            // drugi element w li�cie jest do��czany do pierwszego (podanego w parametrze)
            // nast�pnie ten drugi element jest niszczony (przez inny skrypt)
            // je�li element jest zniszczony to jest wywo�ywana metoda RemoveElement(), poprzez wcze�niej zasubskrybowany event
            // usuwa to ten drugi element z listy
            // trzeci element staje si� drugim elementem (pozosta�e elementy przesuwaj� si� w lewo o 1)
            // algorytm si� powtarza, do momentu kiedy wielko�� selekcji wyniesie 1
            // to znaczy �e wszystkie task forcy zosta�y do��czone do pierwszego i mo�na powr�ci�
            master.Merge(selectedTaskForces[1]);
            MergeSelectedRecursive(master);
            return;
        }
            
        return;
    }

    // usuwa wszystkie referencje je�li element zostanie zniszczony (task force zostanie zniszcony)
    private void RemoveElement(TaskForceContainer container)
    {
        allElements.Remove(container);
        selectedTaskForceContainers.Remove(container);
        selectedTaskForces.Remove(container.taskForce);
    }

    // wybiera tylko jeden element
    private void ClearSelectionExcluding(TaskForceContainer exclude)
    {
        foreach (var container in allElements)
        {
            if (container == exclude) 
                continue;

            if (container.selected)
                container.ToggleSelect();
        }
    }

    private void DeselectAllOtherButtons(SelectUnitButton exception)
    {
        if (buttonSelectFrigate != exception)
            buttonSelectFrigate.DeselectMain();

        if (buttonSelectDestroyer != exception)
            buttonSelectDestroyer.DeselectMain();

        if (buttonSelectCruiser != exception)
            buttonSelectCruiser.DeselectMain();

        if (buttonSelectBattleship != exception)
            buttonSelectBattleship.DeselectMain();

        if (buttonSelectOutpost != exception)
            buttonSelectOutpost.DeselectMain();
    }
}
