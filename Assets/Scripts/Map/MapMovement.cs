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
}