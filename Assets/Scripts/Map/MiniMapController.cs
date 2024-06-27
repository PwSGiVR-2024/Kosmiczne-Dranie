using UnityEngine;


public class MiniMapController : MonoBehaviour
{
    public Transform target; // Cel kamery (zazwyczaj zaznaczona jednostka)
    public float smoothSpeed = 0.125f; // Szybko�� �ledzenia kamery
    public Vector3 offset; // Przesuni�cie kamery wzgl�dem celu

    private bool isFollowing = true; // Flaga �ledzenia celu

    void LateUpdate()
    {
        if (isFollowing && target != null)
        {
            // Oblicz docelow� pozycj� kamery
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Zwr�� kamery w stron� celu (zak�adaj�c, �e kamera patrzy w d� -90 stopni w osi Y)
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
        }
    }

    public void ToggleFollow()
    {
        isFollowing = !isFollowing;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
