using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TaskForceNamesCollection", menuName = "Scriptable Objects/Other/TaskForceNamesCollection", order = 1)]
public class TaskForceNames : ScriptableObject
{
    public string[] names;

    public string GetRandomName()
    {
        return names[Random.Range(0, names.Length)];
    }
}
