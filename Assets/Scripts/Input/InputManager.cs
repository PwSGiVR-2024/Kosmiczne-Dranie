using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// klasa odpowiedzialna za interakcje gracza ze œrodowiskiem
// na razie odpowiada tylko za clicki na plane
// w przysz³oœci mo¿e odpowiadaæ za wybieranie task forców w œwiecie gry etc...
public class InputManager : MonoBehaviour
{
    public GraphicRaycaster graphicRaycaster;
    public EventSystem eventSystem;
    public LayerMask ignoreLayer;

    // plane który registruje clicki
    public MeshCollider plane;

    // dwa stany zmieniane w Update, w zale¿noœci czy jest wciskany Ctrl
    public enum InputControl { Normal, Ctrl }
    public InputControl currentState = InputControl.Normal;
    public UnityEvent<InputControl> OnStateChange = new();

    public UnityEvent<RaycastHit> onPlaneLeftClick = new();
    public UnityEvent<RaycastHit> onPlaneLeftClickCtrl = new();
    public UnityEvent<RaycastHit> onPlaneRightClick = new();
    public UnityEvent<RaycastHit> onPlaneRightClickCtrl = new();


    private bool IsPointerOverUIElement(LayerMask layerMask)
    {
        PointerEventData pointerEventData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        graphicRaycaster.Raycast(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if ((layerMask.value & (1 << result.gameObject.layer)) > 0)
            {
                return true;
            }
        }

        return false;
    }



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
        //if (EventSystem.current.IsPointerOverGameObject())
        //    return;

        switch (currentState)
        {
            case InputControl.Normal:
                if (Input.GetMouseButtonDown(0))
                {
                    if (IsPointerOverUIElement(ignoreLayer))
                        return;

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        onPlaneLeftClick?.Invoke(hit);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (IsPointerOverUIElement(ignoreLayer))
                        return;

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        onPlaneRightClick?.Invoke(hit);
                }

                break;


            case InputControl.Ctrl:
                if (Input.GetMouseButtonDown(0))
                {
                    if (IsPointerOverUIElement(ignoreLayer))
                        return;

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        onPlaneLeftClickCtrl?.Invoke(hit);
                }

                if (Input.GetMouseButtonDown(1))
                {
                    if (IsPointerOverUIElement(ignoreLayer))
                        return;

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (plane.Raycast(ray, out hit, Mathf.Infinity))
                        onPlaneRightClickCtrl?.Invoke(hit);
                }

                break;
        }
    }
}
