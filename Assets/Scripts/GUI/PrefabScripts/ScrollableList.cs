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

    public GameObject CreateAndAddElement()
    {
        GameObject element = Instantiate(elementPrefab, container.transform);
        return element;
    }

    public void RemoveElement(GameObject element)
    {
        Destroy(element);
    }

    public void HideElement(GameObject element)
    {
        element.SetActive(false);

    }

    public void ShowElement(GameObject element)
    {
        element.SetActive(true);
    }
}
