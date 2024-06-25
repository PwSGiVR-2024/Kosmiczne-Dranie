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
    public KeyCode modifierKey = KeyCode.LeftControl;  //trzeba równie¿ zmieniæ w MapMovement.cs
    private ScrollableList ScrollableListInstance;

    void Start()
    {
        ScrollableListInstance = FindAnyObjectByType<ScrollableList>();
        if (selectionBoxImage != null)
        {
            selectionBoxRectTransform = selectionBoxImage.GetComponent<RectTransform>();
            selectionBoxImage.gameObject.SetActive(false);
        }

        mainCamera = Camera.main; // Zak³adamy, ¿e g³ówna kamera to ta, której u¿ywamy
    }

    void Update()
    {
        dziala();
    }

    void dziala()
    {
        // SprawdŸ, czy naciœniêty jest klawisz modyfikatora (Ctrl)
        bool isModifierKeyPressed = Input.GetKey(modifierKey);

        if (Input.GetMouseButtonDown(1) && isModifierKeyPressed)
        {
            // Rozpoczêcie przeci¹gania
            startPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(1) && isModifierKeyPressed)
        {
            // Zakoñczenie zaznaczania lub przeci¹gania
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
            if (!isSelecting && Vector2.Distance(startPosition, currentMousePosition) > 10) // Próg przeci¹gniêcia, aby rozpocz¹æ zaznaczanie
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
                selectionBoxRectTransform.anchoredPosition = startPosition + size / 2;  //Ustawiæ anchor w image na lewy dó³
            }
        }


    }
    void SelectObjectsInArea()
    {
        Rect selectionRect = new Rect(
            selectionBoxRectTransform.anchoredPosition - (selectionBoxRectTransform.sizeDelta / 2),
            selectionBoxRectTransform.sizeDelta);

        selectedObjects.Clear(); // Czyœcimy listê zaznaczonych obiektów

        //var element;

        TaskForceHUD[] taskForceHUDs = FindObjectsOfType<TaskForceHUD>();

        foreach (var hud in taskForceHUDs)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(hud.transform.position);
            if (selectionRect.Contains(screenPos))
            {
                // Obiekt jest w zaznaczonym obszarze
                selectedObjects.Add(hud);
                hud.onSelect.Invoke(); // Wywo³aj UnityEvent onSelect


                //ScrollableListInstance.ToggleSelectForElement(element);
                foreach (var element in ScrollableListInstance.elementsList)
                {
                    ScrollableListInstance.ToggleSelectForElement(element);
                }
            }
        }
    }
}
