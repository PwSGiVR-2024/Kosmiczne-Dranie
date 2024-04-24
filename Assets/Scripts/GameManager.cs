using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool logExterminationCamp = false;

    private Stack<GameObject> exterminationCamp = new();

    private IEnumerator ExterminationChamber()
    {
        if (logExterminationCamp)
            Debug.Log("chamber activated");

        GameObject obj;
        while (true)
        {
            if (exterminationCamp.TryPeek(out obj))
            {
                if (obj)
                {
                    if (logExterminationCamp)
                        Debug.Log("destroying: " + obj.name);

                    Destroy(obj);
                    exterminationCamp.Pop();
                }
                else
                {
                    if (logExterminationCamp)
                        Debug.Log("object is null");

                    exterminationCamp.Pop();
                }
            }
            yield return null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ExterminationChamber());
    }

    public void AddToExterminationCamp(GameObject unit)
    {
        exterminationCamp.Push(unit);
    }
}
