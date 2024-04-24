using UnityEngine;

public class MapMovement : MonoBehaviour
{

    public float minZoom = 20.0f; //Minimalna wartoœæ Zooma
    public float maxZoom = 100.0f; //Maksymalna wartoœæ Zooma
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
        // Ustawienia wartoœci minFOV i maxFOV na podstawie ustawieñ pocz¹tkowych kamery
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
        // SprawdŸ, czy naciœniêty jest klawisz modyfikatora (Ctrl)
        bool isModifierKeyPressed = Input.GetKey(modifierKey);

        // Jeœli naciœniêty jest prawy przycisk myszy i klawisz modyfikatora, w³¹cz tryb przeci¹gania
        if (Input.GetMouseButtonDown(1) && isModifierKeyPressed)
        {
            isDragging = true;
            lastPosition = Input.mousePosition;
        }

        // Jeœli przycisk myszy zosta³ puszczony, wy³¹cz tryb przeci¹gania
        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        // Jeœli jesteœmy w trybie przeci¹gania, przesuñ mapê
        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastPosition;
            transform.Translate(-delta.x * dragSpeed * Time.deltaTime, -delta.y * dragSpeed * Time.deltaTime, 0);
            lastPosition = Input.mousePosition;
        }

    }

    void WASDMovement()
    {
        //Input do poruszania siê w p³aszczyŸnie pionu(W,S)
        float horizontalInput = Input.GetAxis("Horizontal");
        //Input do poruszania siê w p³aszczyŸnie poziomu(A,D)
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
    }

    void zoomMovement()
    {
        // Input Scroll'a
        float zoomInput = Input.GetAxis("Mouse ScrollWheel");

        // Zmniejsz lub zwiêksz pole widzenia (FOV) kamery w zale¿noœci od wartoœci wejœcia
        Camera.main.fieldOfView += zoomInput * zoomSpeed;

        // Ogranicz wartoœæ pola widzenia, aby nie wykracza³o poza okreœlone zakresy (np. 20 - 100 stopni) -- Ustawiaæ za pomoc¹ mix-max wartoœci zooma
        Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView, minFOV, maxFOV);

    }
}