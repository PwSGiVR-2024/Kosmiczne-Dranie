using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

[CreateAssetMenu(fileName = "OutpostNamesCollection", menuName = "Scriptable Objects/Other/OutpostNamesCollection", order = 1)]
public class OutpostNames : ScriptableObject
{
    public string[] names;

    public string GetRandomName()
    {
        return names[Random.Range(0, names.Length)];
    }
}
