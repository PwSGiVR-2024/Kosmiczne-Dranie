using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIInputReciver))]
public class UIButton : Button
{
    private InputReciver reciver;
    protected override void Awake()
    {
        base.Awake();
        reciver = GetComponent<UIInputReciver>();
        onClick.AddListener(() => reciver.OnInputRecieved());
    }
}
