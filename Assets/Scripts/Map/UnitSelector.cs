using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectionBox : MonoBehaviour
{
    public Image selectionBoxImage;
    private RectTransform selectionBoxRectTransform;
    private Vector2 startPosition;
    private bool isDragging = false;
    private bool isSelecting = false;
    private Camera mainCamera;
    private List<HUDController> selectedObjects = new List<HUDController>();
    public UnityEvent<TaskForceContainer> OnSelect = new();
    public KeyCode modifierKey = KeyCode.LeftControl;  //trzeba r�wnie� zmieni� w MapMovement.cs
    private ScrollableList ScrollableListInstance;

    public GameObject HUDContainer;

    void Start()
    {
        ScrollableListInstance = FindAnyObjectByType<ScrollableList>();
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
        Rect selectionRect = new Rect(
            selectionBoxRectTransform.anchoredPosition - (selectionBoxRectTransform.sizeDelta / 2),
            selectionBoxRectTransform.sizeDelta);

        selectedObjects.Clear(); // Czyścimy listę zaznaczonych obiektów

        // to lepsze
        HUDController[] taskForceHUDs = HUDContainer.GetComponentsInChildren<HUDController>();

        foreach (var hud in taskForceHUDs)
        {
            Vector3 screenPos = mainCamera.WorldToScreenPoint(hud.transform.position);
            if (selectionRect.Contains(screenPos) && hud.owner is TaskForceController taskForce && taskForce.Affiliation == Affiliation.Blue)
            {
                // Obiekt jest w zaznaczonym obszarze
                selectedObjects.Add(hud);
            }
            
        }
        foreach(var cos in selectedObjects)
        {
            if (cos.owner is TaskForceController tf && !tf.isSelectedInUI)
            {
                tf.onSelect.Invoke();
            }
        }
    }
}
