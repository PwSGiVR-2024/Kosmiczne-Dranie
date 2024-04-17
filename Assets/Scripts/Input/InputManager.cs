using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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

    public UnityEvent<RaycastHit> OnPlaneLeftClick = new();
    public UnityEvent<RaycastHit> OnPlaneLeftClickCtrl = new();
    public UnityEvent<RaycastHit> OnPlaneRightClick = new();
    public UnityEvent<RaycastHit> OnPlaneRightClickCtrl = new();

    




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
                        OnPlaneLeftClick?.Invoke(hit);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        OnPlaneRightClick?.Invoke(hit);
                }

                break;


            case InputControl.Ctrl:
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        OnPlaneLeftClickCtrl?.Invoke(hit);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        OnPlaneRightClickCtrl?.Invoke(hit);
                }

                break;
        }
    }
}
