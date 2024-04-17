using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// klasa odpowiedzialna za interakcje gracza ze œrodowiskiem
// na razie odpowiada tylko za clicki na plane
// w przysz³oœci mo¿e odpowiadaæ za wybieranie task forców w œwiecie gry etc...
public class InputManager : MonoBehaviour
{
    // plane który registruje clicki
    public MeshCollider plane;

    // dwa stany zmieniane w Update, w zale¿noœci czy jest wciskany Ctrl
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


        // zwraca false jeœli kursor jest nad elementem ui
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
