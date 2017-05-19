using System;
using System.Reflection;
using UnityEngine;

public static class InputHelper
{
    static InputHelper()
    {
        SelectableManagerType = ReflectionHelper.FindType("SelectableManager");
        SelectableType = ReflectionHelper.FindType("Selectable");
        HandleCancelMethod = SelectableManagerType.GetMethod("HandleCancel", BindingFlags.Public | BindingFlags.Instance);
        ConfigureSelectableAreasMethod = SelectableManagerType.GetMethod("ConfigureSelectableAreas", BindingFlags.Public | BindingFlags.Instance);
        SelectMethod = SelectableManagerType.GetMethod("Select", BindingFlags.Public | BindingFlags.Instance);
        HandleInteractMethod = SelectableManagerType.GetMethod("HandleInteract", BindingFlags.Public | BindingFlags.Instance);
    }

    public static void InvokeConfigureSelectableAreas(Component parentSelectableComponent)
    {
        Component internalSelectable = parentSelectableComponent.GetComponent(SelectableType);
        foreach (UnityEngine.Object selectableManager in SelectableManagers)
        {
            ConfigureSelectableAreasMethod.Invoke(selectableManager, new object[] { internalSelectable });
        }        
    }

    public static void InvokeSelect(this KMSelectable selectable, bool playSound, bool andInteract = false)
    {
        InputInvoker.Instance.Enqueue(delegate()
        {
            Component internalSelectable = selectable.GetComponent(SelectableType);
            foreach (UnityEngine.Object selectableManager in SelectableManagers)
            {
                SelectMethod.Invoke(selectableManager, new object[] { internalSelectable, playSound });
            }
        });

        if (andInteract)
        {
            InvokeInteract();
        }
    }

    public static void InvokeInteract()
    {
        InputInvoker.Instance.Enqueue(delegate()
        {
            foreach (UnityEngine.Object selectableManager in SelectableManagers)
            {
                HandleInteractMethod.Invoke(selectableManager, null);
            }
        });
    }

    public static void InvokeCancel()
    {
        InputInvoker.Instance.Enqueue(delegate()
        {
            foreach (UnityEngine.Object selectableManager in SelectableManagers)
            {
                HandleCancelMethod.Invoke(selectableManager, null);
            }
        });
    }

    private static readonly Type SelectableManagerType;
    private static readonly Type SelectableType;
    private static readonly MethodInfo HandleCancelMethod;
    private static readonly MethodInfo ConfigureSelectableAreasMethod;
    private static readonly MethodInfo SelectMethod;
    private static readonly MethodInfo HandleInteractMethod;

    private static UnityEngine.Object[] SelectableManagers
    {
        get
        {
            if (_selectableManagers == null)
            {
                _selectableManagers = Resources.FindObjectsOfTypeAll(SelectableManagerType);
            }

            return _selectableManagers;
        }
    }
    private static UnityEngine.Object[] _selectableManagers = null;
}

