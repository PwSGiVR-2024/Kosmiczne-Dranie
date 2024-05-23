using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

//[CreateAssetMenu(fileName ="New ResourceOutpost",menuName ="ResourceOutpost")]
public class ResourceHolder : MonoBehaviour
{
    public int crystals;
    public int metals;
    public int scrap;
    public int rareMaterials;

    public UnityEvent<Affiliation> onCaptured = new();
    public void Start()
    {
        
    }
    

}
