using System;
using System.Collections.Generic;
using UnityEngine;

public static class InputInterceptor
{
    static InputInterceptor()
    {
        TryAddControls("MouseControls");
        TryAddControls("GamepadControls");
    }

    private static List<MonoBehaviour> _ktaneControlsManaged = new List<MonoBehaviour>();
    private static List<MonoBehaviour> _ktaneControlsDisabled = new List<MonoBehaviour>();

    private static int _disableCount = 0;

    private static void TryAddControls(string typeName)
    {
        Type typeToFind = ReflectionHelper.FindType(typeName);
        if (typeToFind == null)
        {
            return;
        }

        UnityEngine.Object objectToFind = UnityEngine.Object.FindObjectOfType(typeToFind);
        if (objectToFind == null)
        {
            return;
        }

        _ktaneControlsManaged.Add((MonoBehaviour)objectToFind);
    }

    public static void EnableControls()
    {
        _disableCount--;

        if (_disableCount == 0)
        {
            foreach (MonoBehaviour controls in _ktaneControlsDisabled)
            {
                controls.gameObject.SetActive(true);
            }

            _ktaneControlsDisabled.Clear();
        }
    }

    public static void DisableControls()
    {
        if (_disableCount == 0)
        {
            foreach (MonoBehaviour controls in _ktaneControlsManaged)
            {
                if (controls.gameObject.activeSelf)
                {
                    _ktaneControlsDisabled.Add(controls);
                    controls.gameObject.SetActive(false);
                }
            }
        }

        _disableCount++;
    }
}
