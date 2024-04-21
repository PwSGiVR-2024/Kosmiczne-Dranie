using UnityEngine;

public class MapMovement : MonoBehaviour
{
    public float moveSpeed = 1.0f;
    public float dragSpeed = 2.0f;
    public KeyCode modifierKey = KeyCode.LeftControl;

    private bool isDragging = false;
    private Vector3 lastPosition;

    void Update()
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
}