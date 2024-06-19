using UnityEngine;


public class MiniMap : MonoBehaviour
{
    public Transform mainCamera;
    public bool anchored;
    float m_MyX, m_MyY, m_MyZ, m_MyW;

    private void Start()
    {
        anchored = true;
        m_MyX = 0;
        m_MyY = 0;
        m_MyZ = 0;
        m_MyW = 0;
    }
    void Update()
    {
        transform.rotation = Change(m_MyX, m_MyY, m_MyZ, m_MyW);
    }

    void LateUpdate()
    {
        Vector3 newPosition = mainCamera.position;
        newPosition.y = transform.position.y;  // Upewnij siê, ¿e wysokoœæ minimapy pozostaje sta³a
        transform.position = newPosition;
        transform.rotation = Quaternion.Euler(90f, mainCamera.eulerAngles.y, 0f);  // Obróæ minimapê wzglêdem kierunku g³ównej kamery
    }
    private static Quaternion Change(float x, float y, float z, float w)
    {
        Quaternion newQuaternion = new Quaternion();
        newQuaternion.Set(0, 0, 0, 1);
        //Return the new Quaternion
        return newQuaternion;
    }
}
