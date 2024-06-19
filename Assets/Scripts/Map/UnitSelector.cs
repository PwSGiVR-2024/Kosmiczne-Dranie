using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public class SelectionBox : MonoBehaviour
{
    public Image selectionBoxImage;
    private RectTransform selectionBoxRectTransform;
    private Vector2 startPosition;
    private bool isDragging = false;
    private bool isSelecting = false;
    private Camera mainCamera;
    private List<TaskForceHUD> selectedObjects = new List<TaskForceHUD>();
    public KeyCode modifierKey = KeyCode.LeftControl;  //trzeba r�wnie� zmieni� w MapMovement.cs

    void Start()
    {
        if (selectionBoxImage != null)
        {
            selectionBoxRectTransform = selectionBoxImage.GetComponent<RectTransform>();
            selectionBoxImage.gameObject.SetActive(false);
        }

        mainCamera = Camera.main; // Zak�adamy, �e g��wna kamera to ta, kt�rej u�ywamy
    }

    void Update()
    {
        dziala();
    }

    void dziala()
    {
        // Sprawd�, czy naci�ni�ty jest klawisz modyfikatora (Ctrl)
        bool isModifierKeyPressed = Input.GetKey(modifierKey);

        if (Input.GetMouseButtonDown(1) && isModifierKeyPressed)
        {
            // Rozpocz�cie przeci�gania
            startPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(1) && isModifierKeyPressed)
        {
            // Zako�czenie zaznaczania lub przeci�gania
            isDragging = false;
            if (isSelecting)
            {
                isSelecting = false;
                if (selectionBoxImage != null)
                {
                    selectionBoxImage.gameObject.SetActive(false);
                }
                SelectObjectsInArea();
            }
        }

        if (isDragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            if (!isSelecting && Vector2.Distance(startPosition, currentMousePosition) > 10) // Pr�g przeci�gni�cia, aby rozpocz�� zaznaczanie
            {
                isSelecting = true;
                if (selectionBoxImage != null)
                {
                    selectionBoxImage.gameObject.SetActive(true);
                }
            }

            if (isSelecting && selectionBoxImage != null)
            {
                Vector2 size = currentMousePosition - startPosition;

                selectionBoxRectTransform.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
                selectionBoxRectTransform.anchoredPosition = startPosition + size / 2;  //Ustawi� anchor w image na lewy d�
            }
        }


    }
    void SelectObjectsInArea()
    {
        Vector2 min = selectionBoxRectTransform.anchoredPosition - (selectionBoxRectTransform.sizeDelta / 2);
        Vector2 max = selectionBoxRectTransform.anchoredPosition + (selectionBoxRectTransform.sizeDelta / 2);

        selectedObjects.Clear(); // Czy�cimy list� zaznaczonych obiekt�w

        foreach (var hud in FindObjectsOfType<TaskForceHUD>())
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(hud.transform.position);
            if (screenPos.x > min.x && screenPos.x < max.x && screenPos.y > min.y && screenPos.y < max.y)
            {
                // Obiekt jest w zaznaczonym obszarze
                selectedObjects.Add(hud);
                hud.onSelect.Invoke(); // Wywo�aj UnityEvent onSelect
            }
        }
    }
}
