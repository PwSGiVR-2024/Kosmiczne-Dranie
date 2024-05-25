using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TaskForceHUD", menuName = "Scriptable Objects/Other/TaskForceHUD", order = 1)]
public class TaskForceHUD : ScriptableObject
{
    public GameObject icon;
    public Vector3 offset = new(0, 20, 0);
}
