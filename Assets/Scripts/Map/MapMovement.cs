using UnityEngine;

public class MapMovement : MonoBehaviour
{

    public float minZoom = 20.0f; //Minimalna warto�� Zooma
    public float maxZoom = 100.0f; //Maksymalna warto�� Zooma
    public float zoomSpeed = 10.0f; 
    public float moveSpeed = 10.0f;
    public float dragSpeed = 20.0f;
    public KeyCode modifierKey = KeyCode.LeftControl;
    private float minFOV;
    private float maxFOV;

    private bool isDragging = false;
    private Vector3 lastPosition;

    void Start()
    {
        minFOV = minZoom;
        maxFOV = maxZoom;
        // Ustawienia warto�ci minFOV i maxFOV na podstawie ustawie� pocz�tkowych kamery
        //minFOV = Camera.main.fieldOfView / 2f;
        //maxFOV = Camera.main.fieldOfView * 2f;
    }

    void Update()
    {
        mouseMovement();
        WASDMovement();
        zoomMovement();
    }

    void mouseMovement()
    {
        // Sprawd�, czy naci�ni�ty jest klawisz modyfikatora (Ctrl)
        bool isModifierKeyPressed = Input.GetKey(modifierKey);

        // Je�li naci�ni�ty jest prawy przycisk myszy i klawisz modyfikatora, w��cz tryb przeci�gania
        if (Input.GetMouseButtonDown(1) && isModifierKeyPressed)
        {
            isDragging = true;
            lastPosition = Input.mousePosition;
        }

        // Je�li przycisk myszy zosta� puszczony, wy��cz tryb przeci�gania
        if (Input.GetMouseButtonUp(1))
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

    void zoomMovement()
    {
        // Input Scroll'a
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");

        // Zmniejsz lub zwi�ksz pole widzenia (FOV) kamery w zale�no�ci od warto�ci wej�cia
        Camera.main.fieldOfView += zoomInput * zoomSpeed;

        // Ogranicz warto�� pola widzenia, aby nie wykracza�o poza okre�lone zakresy (np. 20 - 100 stopni) -- Ustawia� za pomoc� mix-max warto�ci zooma
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, minFOV, maxFOV);

    }
}