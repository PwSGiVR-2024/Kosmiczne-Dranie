using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// skrypt generalnego przeznaczenia
// jeszce nie wiadomo czy w ogóle potrzebny
// kontroluje prefab UI o nazwie List
public class ScrollableList : MonoBehaviour
{

    
    public GameObject container; // gameObject przechowuj¹cy elementy listy
    public GameObject elementPrefab; // element listy
    public List<GameObject> elementsList = new List<GameObject>();

    public GameObject CreateAndAddElement()
    {
        GameObject element = Instantiate(elementPrefab, container.transform);
        elementsList.Add(element);
        return element;
    }
    public void CheckElementsList()
    {
        foreach (GameObject element in elementsList)
        {
            Debug.Log("Nazwa elementu: " + element.name);
        }
    }

    public void RemoveElement(GameObject element)
    {
        elementsList.Remove(element);
        Destroy(element);
    }

    public void HideElement(GameObject element)
    {
        element.SetActive(false);

    }

    public void ToggleSelectForElement(GameObject element)
    {
        var selectScript = element.GetComponent<TaskForceContainer>();

        if (selectScript != null)
        {
            selectScript.ToggleSelect();
        }
    }
    public List<GameObject> GetElementsList()
    {
        return elementsList;
    }
}
