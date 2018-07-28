using System;
using System.Collections.Generic;
using UnityEngine;

public class InputInvoker : MonoBehaviour
{
    private static InputInvoker _instance = null;
    public static InputInvoker Instance
    {
        get
        {
            return _instance;
        }
    }

    private Queue<Action> _actions = new Queue<Action>();

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        if (_actions.Count > 0)
        {
            _actions.Dequeue()();
        }
    }

    public void Enqueue(Action action)
    {
        _actions.Enqueue(action);
    }
}

