using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InputManager;


// skrypt odpowiada za panel UI, odpowiadaj¹cy za sterowanie flot¹ (w projekcie ScreenSpaceCanvas -> FleetPanel)
public class FleetPanelController : MonoBehaviour
{
    public bool spawnOutpost = false;

    public TaskForcePreset currentPreset;
    public int presetPower;
    public int presetMetalsPrice;
    public int presetCrystalsPrice;
    public int presetMaintenancePrice;
    public int presetFrigates;
    public int presetDestroyers;
    public int presetCruisers;
    public int presetBattleships;
    public GameObject presetFrigte;
    public GameObject presetDestroyer;
    public GameObject presetCruiser;
    public GameObject presetBattleship;

    // enum z InputManagera, bo ten skrypt te¿ oferuje alternatywne sterowanie
    public InputControl currentState = InputControl.Normal;

    // skrypty do komunikacji
    public PlayerFleetManager fleetManager;
    public InputManager input;
    public Spawner spawner;

    // elementy ui
    public ScrollableList taskForceList; // customowy skrypt

    public Button buttonMerge;
    public Button buttonSplit;

    public List<TaskForceContainer> allElements = new(); // lista wszystkich elementów scroll listy
    public List<TaskForceContainer> selectedTaskForceContainers = new(); // lista wybranych elementów w scroll liœcie
    public List<TaskForceController> selectedTaskForces = new(); // lista task forców przypadaj¹cych elementom ui

    public SelectUnitButton buttonSelectFrigate;
    public SelectUnitButton buttonSelectDestroyer;
    public SelectUnitButton buttonSelectCruiser;
    public SelectUnitButton buttonSelectBattleship;
    public SelectUnitButton buttonSelectOutpost;

    //public SelectUnitButton currentSelection;

    void Start()
    {
        currentPreset = new TaskForcePreset(
            frigatePrefab: null,
            destroyerPrefab: null,
            cruiserPrefab: null,
            battleshipPrefab: null,
            frigatesCount: 0,
            destroyersCount: 0,
            cruisersCount: 0,
            battleshipsCount: 0
            );

        input.onPlaneLeftClick.AddListener((hit) => DeselectAllTaskForces());
        input.OnStateChange.AddListener(ChangeCurrentState); // zmiana stanu na podstawie inputManagera
        buttonMerge.onClick.AddListener(MergeWrapper);
        buttonSplit.onClick.AddListener(SplitTaskForces);
        spawner.onTaskForceSpawned.AddListener(AddTaskForceToList); // jeœli task force jest spawnowany, to pojawia sie na listach i w ui

        buttonSelectFrigate.onSelect.AddListener(() => SelectButton(buttonSelectFrigate));
        buttonSelectDestroyer.onSelect.AddListener(() => SelectButton(buttonSelectDestroyer));
        buttonSelectCruiser.onSelect.AddListener(() => SelectButton(buttonSelectCruiser));
        buttonSelectBattleship.onSelect.AddListener(() => SelectButton(buttonSelectBattleship));
        buttonSelectOutpost.onSelect.AddListener(() => SelectButton(buttonSelectOutpost));

        buttonSelectFrigate.onDeselect.AddListener(() => DeselectButton(buttonSelectFrigate));
        buttonSelectDestroyer.onDeselect.AddListener(() => DeselectButton(buttonSelectDestroyer));
        buttonSelectCruiser.onDeselect.AddListener(() => DeselectButton(buttonSelectCruiser));
        buttonSelectBattleship.onDeselect.AddListener(() => DeselectButton(buttonSelectBattleship));
        buttonSelectOutpost.onDeselect.AddListener(() => DeselectButton(buttonSelectOutpost));
    }

    public void SelectButton(SelectUnitButton button)
    {
        switch (button.shipClass)
        {
            case ShipClass.Outpost:
                spawnOutpost = true;
                return;

            case ShipClass.Frigate:
                button.onCountChange.AddListener(() => {
                    currentPreset.frigatesCount = button.selectedCount;
                    currentPreset.UpdateValues();
                });
                currentPreset.frigatePrefab = button.unitPrefab;
                currentPreset.frigatesCount = button.selectedCount;
                currentPreset.UpdateValues();
                break;

            case ShipClass.Destroyer:
                button.onCountChange.AddListener(() => {
                    currentPreset.destroyersCount = button.selectedCount;
                    currentPreset.UpdateValues();
                });
                currentPreset.destroyerPrefab = button.unitPrefab;
                currentPreset.destroyersCount = button.selectedCount;
                currentPreset.UpdateValues();
                break;

            case ShipClass.Cruiser:
                button.onCountChange.AddListener(() => {
                    currentPreset.cruisersCount = button.selectedCount;
                    currentPreset.UpdateValues();
                });
                currentPreset.cruiserPrefab = button.unitPrefab;
                currentPreset.cruisersCount = button.selectedCount;
                currentPreset.UpdateValues();
                break;

            case ShipClass.Battleship:
                button.onCountChange.AddListener(() => {
                    currentPreset.battleshipsCount = button.selectedCount;
                    currentPreset.UpdateValues();
                });
                currentPreset.battleshipPrefab = button.unitPrefab;
                currentPreset.battleshipsCount = button.selectedCount;
                currentPreset.UpdateValues();
                break;
        }

        //DeselectAllOtherButtons(button);
        //currentSelection = button;
        //fleetManager.SetUnitToProcure(button.unitPrefab, button.selectedCount);
    }

    public void DeselectButton(SelectUnitButton button)
    {
        switch (button.shipClass)
        {
            case ShipClass.Outpost:
                spawnOutpost = false;
                return;

            case ShipClass.Frigate:
                currentPreset.frigatePrefab = null;
                currentPreset.UpdateValues();
                button.onCountChange.RemoveAllListeners();
                break;

            case ShipClass.Destroyer:
                currentPreset.destroyerPrefab = null;
                currentPreset.UpdateValues();
                button.onCountChange.RemoveAllListeners();
                break;

            case ShipClass.Cruiser:
                currentPreset.cruiserPrefab = null;
                currentPreset.UpdateValues();
                button.onCountChange.RemoveAllListeners();
                break;

            case ShipClass.Battleship:
                currentPreset.battleshipPrefab = null;
                currentPreset.UpdateValues();
                button.onCountChange.RemoveAllListeners();
                break;
        }

        //currentPreset.power -= button.prefabController.Values.power * button.selectedCount;
        //currentPreset.metalsPrice -= button.prefabController.Values.metalPrice * button.selectedCount;
        //currentPreset.crysalsPrice -= button.prefabController.Values.crystalPrice * button.selectedCount;
        //currentPreset.maintenancePrice -= button.prefabController.Values.maintenancePrice * button.selectedCount;

        //DeselectAllOtherButtons(button);
        //currentSelection = button;
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
        // w tym przypadku instance to fizyczny obiekt na scenie (prefab z do³¹czonym skryptem TaskForceContainer). taskForceList to skrypt odpowiedzialny za listê ze scrollem i inicjalizacjê prefabów
        GameObject instance = taskForceList.CreateAndAddElement();
        TaskForceContainer container = instance.GetComponent<TaskForceContainer>(); // wydzielony skrypt ¿eby nie u¿ywaæ GetComponent kilka razy niepotrzebnie
        allElements.Add(container); // element dodawany do listy dla wygodniejszego dostêpu
        container.Init(taskForce); // inicjalizacja skryptu musi zostaæ wykonana oddzielnie
        container.OnSelect.AddListener(AddTaskForceToSelection); // skrypt posiada eventy informuj¹ce czy element zosta³ wybrany, czy nie
        container.OnDeselect.AddListener(RemoveTaskForceFromSelection);
        container.OnDestroy.AddListener(RemoveElement);
    }

    
    private void AddTaskForceToSelection(TaskForceContainer container)
    {
        // normal -> selectuje tylko jeden
        // jeœli false, to dodaje do selekcji
        if (currentState == InputControl.Normal)
            ClearSelectionExcluding(container);

        // dwie oddzielne listy ¿eby u³atwiæ dostêp przez inne skrypty. Jedna odpowiada za elementy w UI, a druga za odpowiadaj¹ce im taskForcy
        // przyk³adowo FleetManager nie potrzebuje ¿adnych informacji o wybranych elementach UI, ale listê wybranych taskForce ju¿ tak
        // mo¿na wykorzystaæ jedn¹ listê, ale tak jest szybciej, bo jesli coœ potrzebowa³oby listy taskForców to musia³oby sobie to od nowa tworzyæ w pêtli, a tak ma od razu
        selectedTaskForceContainers.Add(container);
        selectedTaskForces.Add(container.taskForce);
    }

    private void RemoveTaskForceFromSelection(TaskForceContainer container)
    {
        selectedTaskForceContainers.Remove(container);
        selectedTaskForces.Remove(container.taskForce);
    }


    // to nie dzia³a ale zostawi³em jako pomnik, wiêcej info ni¿ej
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

    // to siê wydaje bez sensu ale tak musi byæ jeœli chcemy mergowaæ wiele elementów z listy
    // wywo³anie Merge() niszczy task forca, a co za tym idzie równie¿ referencje do aktualnej selekcji
    // to znaczy ¿e pêtla nie mo¿e siê wykonaæ wiecej ni¿ jedn¹ iteracjê, bo d³ugoœæ kolekcji (lista aktualnej selekcji) siê zmniejsza
    // tak po prostu jest, nie da siê przeskoczyæ. D³ugoœæ listy nie mo¿e siê zmieniæ w trakcie iteracji
    // metoda rekurencyjna nie korzysta z pêtli wiêc nie ma tego problemu
    // po zmniejszeniu d³ugoœci kolekcji, metoda jest po prostu wykonywana ponownie, dla nastêpnego elementu
    public void MergeWrapper()
    {
        if (selectedTaskForceContainers.Count == 0)
            return;

        MergeSelectedRecursive(selectedTaskForces[0]); // Merge() jest wywo³ywane dla pierwszego wybranego elementu
    }

    public void MergeSelectedRecursive(TaskForceController master)
    {
        if (selectedTaskForces.Count > 1)
        {
            // drugi element w liœcie jest do³¹czany do pierwszego (podanego w parametrze)
            // nastêpnie ten drugi element jest niszczony (przez inny skrypt)
            // jeœli element jest zniszczony to jest wywo³ywana metoda RemoveElement(), poprzez wczeœniej zasubskrybowany event
            // usuwa to ten drugi element z listy
            // trzeci element staje siê drugim elementem (pozosta³e elementy przesuwaj¹ siê w lewo o 1)
            // algorytm siê powtarza, do momentu kiedy wielkoœæ selekcji wyniesie 1
            // to znaczy ¿e wszystkie task forcy zosta³y do³¹czone do pierwszego i mo¿na powróciæ
            master.Merge(selectedTaskForces[1]);
            MergeSelectedRecursive(master);
            return;
        }
            
        return;
    }

    // usuwa wszystkie referencje jeœli element zostanie zniszczony (task force zostanie zniszcony)
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
                //container.ToggleSelect();
                container.taskForce.onSelect.Invoke();
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

    private void Update()
    {
        presetPower = currentPreset.power;
        presetMetalsPrice = currentPreset.metalsPrice;
        presetCrystalsPrice = currentPreset.crysalsPrice;
        presetMaintenancePrice = currentPreset.maintenancePrice;

        presetFrigates = currentPreset.frigatesCount;
        presetDestroyers = currentPreset.destroyersCount;
        presetCruisers = currentPreset.cruisersCount;
        presetBattleships = currentPreset.battleshipsCount;

        presetFrigte = currentPreset.frigatePrefab;
        presetDestroyer = currentPreset.destroyerPrefab;
        presetCruiser = currentPreset.cruiserPrefab;
        presetBattleship = currentPreset.battleshipPrefab;
    }

    public void SplitTaskForces()
    {
        foreach (var taskForce in selectedTaskForces)
        {
            fleetManager.SplitTaskForce(taskForce, out TaskForceController newTaskForce);
            
        }
    }

    public void DeselectAllTaskForces()
    {
        DeselectAllTaskForcesRecursive();
    }

    public void DeselectAllTaskForcesRecursive()
    {
        if (selectedTaskForces.Count == 0)
            return;

        selectedTaskForces[selectedTaskForces.Count - 1].onSelect.Invoke();
        DeselectAllTaskForcesRecursive();
    }
}
