using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// klasa odpowiedzialna za interakcje gracza ze �rodowiskiem
// na razie odpowiada tylko za clicki na plane
// w przysz�o�ci mo�e odpowiada� za wybieranie task forc�w w �wiecie gry etc...
public class InputManager : MonoBehaviour
{
    // plane kt�ry registruje clicki
    public MeshCollider plane;

    // dwa stany zmieniane w Update, w zale�no�ci czy jest wciskany Ctrl
    public enum InputControl { Normal, Ctrl }
    public InputControl currentState = InputControl.Normal;
    public UnityEvent<InputControl> OnStateChange = new();

    public UnityEvent<RaycastHit> onPlaneLeftClick = new();
    public UnityEvent<RaycastHit> onPlaneLeftClickCtrl = new();
    public UnityEvent<RaycastHit> onPlaneRightClick = new();
    public UnityEvent<RaycastHit> onPlaneRightClickCtrl = new();

    




    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            currentState = InputControl.Ctrl;
            OnStateChange?.Invoke(currentState);
        }
            

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            currentState = InputControl.Normal;
            OnStateChange?.Invoke(currentState);
        }


        // zwraca false je�li kursor jest nad elementem ui
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        switch (currentState)
        {
            case InputControl.Normal:
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        onPlaneLeftClick?.Invoke(hit);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        onPlaneRightClick?.Invoke(hit);
                }

                break;


            case InputControl.Ctrl:
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        onPlaneLeftClickCtrl?.Invoke(hit);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        onPlaneRightClickCtrl?.Invoke(hit);
                }

                break;
        }
    }
}
