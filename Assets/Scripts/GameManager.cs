using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool logExterminationCamp = false;

    private Stack<GameObject> exterminationCamp = new();
    private Stack<GameObject> temporaryCamp = new();

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

            if (temporaryCamp.TryPeek(out obj))
            {
                if (obj)
                {
                    if (logExterminationCamp)
                        Debug.Log("destroying: " + obj.name);

                    Destroy(obj);
                    temporaryCamp.Pop();
                }
                else if (obj == null)
                {
                    if (logExterminationCamp)
                        Debug.Log("object is null");

                    temporaryCamp.Pop();
                }
                else if (obj.activeSelf)
                {
                    if (logExterminationCamp)
                        Debug.Log("object is active in temporary camp. Skipping iteration");

                    yield return null;
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

    public void AddToExterminationCamp(GameObject obj)
    {
        exterminationCamp.Push(obj);
    }

    public void AddToTemporaryCamp(GameObject obj)
    {
        temporaryCamp.Push(obj);
    }
}
