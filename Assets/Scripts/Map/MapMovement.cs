using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MapMovement : MonoBehaviour
{
    public float zoomSpeed = 30000.0f;
    public float moveSpeed = 100.0f;
    public float dragSpeed = 20.0f;
    public KeyCode modifierKey = KeyCode.LeftControl; //trzeba r�wnie� zmieni� w UnitSelector.cs
    public float distanceToTarget = 10;


    private bool isDragging = false;
    private Vector3 lastPosition;
    private KeyCode modifierMouseKey = KeyCode.Mouse1;
    private Vector3 previousPosition;

    
    void Update()
    {
        mouseMovement();
        WASDMovement();
        ZoomMovement();
        Roatato();
        // RotateMovement();
    }

    void mouseMovement()
    {
        // Sprawd�, czy naci�ni�ty jest klawisz modyfikatora (Ctrl)
        bool isModifierKeyPressed = Input.GetKey(modifierKey);

        // Je�li naci�ni�ty jest prawy przycisk myszy i klawisz modyfikatora, w��cz tryb przeci�gania
        if (Input.GetMouseButtonDown(0) && isModifierKeyPressed)
        {
            isDragging = true;
            lastPosition = Input.mousePosition;
        }

        // Je�li przycisk myszy zosta� puszczony, wy��cz tryb przeci�gania
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // Je�li jeste�my w trybie przeci�gania, przesu� map�
        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastPosition;
            transform.Translate(-delta.x * dragSpeed * Time.deltaTime, -delta.y * dragSpeed * Time.deltaTime, 0);
            lastPosition = Input.mousePosition;
        }

    }

    void WASDMovement()
    {
        //Input do poruszania si� w p�aszczy�nie pionu(W,S)
        float horizontalInput = Input.GetAxis("Horizontal");
        //Input do poruszania si� w p�aszczy�nie poziomu(A,D)
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
    }

    void ZoomMovement()
    {
        // Input Scroll'a
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");

        Vector3 movement = new Vector3(0, 0, zoomInput) * zoomSpeed * Time.deltaTime;
        transform.Translate(movement);
    }
    void Roatato()
    {
        bool isModifierKeyPressed = Input.GetKey(modifierKey);
        bool dragInput = Input.GetKey(modifierMouseKey);


        if (dragInput)
        {
            if (Input.GetMouseButtonDown(1) && !isModifierKeyPressed)
            {
                previousPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            }
            else if (Input.GetMouseButton(1) && !isModifierKeyPressed)
            {
                Vector3 newPosition = Camera.main.ScreenToViewportPoint(Input.mousePosition);
                Vector3 direction = previousPosition - newPosition;

                float rotationAroundYAxis = -direction.x * 150; //pr�dko�� poruszania Y
                float rotationAroundXAxis = direction.y * 150;  //pr�dko�� poruszania X
                
                float currentRotationX = transform.eulerAngles.x;
                // Oblicz nowy k�t rotacji, uwzgl�dniaj�c ograniczenia
                float newRotationX = currentRotationX + rotationAroundXAxis;

                newRotationX = Mathf.Clamp(newRotationX, 0.0f, 90.0f);

                // Ustaw now� rotacj�, ale z ograniczonym k�tem wok� osi X
                transform.eulerAngles = new Vector3(newRotationX, transform.eulerAngles.y + rotationAroundYAxis, transform.eulerAngles.z);

                previousPosition = newPosition;
            }
        }
    }
}