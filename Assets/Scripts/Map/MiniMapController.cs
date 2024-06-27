using UnityEngine;


public class MiniMapController : MonoBehaviour
{
    public Transform target; // Cel kamery (zazwyczaj zaznaczona jednostka)
    public float smoothSpeed = 0.125f; // Szybkoœæ œledzenia kamery
    public Vector3 offset; // Przesuniêcie kamery wzglêdem celu

    private bool isFollowing = true; // Flaga œledzenia celu

    void LateUpdate()
    {
        if (isFollowing && target != null)
        {
            // Oblicz docelow¹ pozycjê kamery
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Zwróæ kamery w stronê celu (zak³adaj¹c, ¿e kamera patrzy w dó³ -90 stopni w osi Y)
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
