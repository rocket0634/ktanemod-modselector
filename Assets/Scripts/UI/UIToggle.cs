using System;
using UnityEngine;

[ExecuteInEditMode]
public class UIToggle : UIElement
{
    public bool IsOn = false;
    public event Action<bool> OnToggleChange = null;

    protected override void Awake()
    {
        base.Awake();
        InteractAction.AddListener(OnInteract);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InteractAction.RemoveListener(OnInteract);
    }

    protected override void Update()
    {
        base.Update();

        if (IconMesh != null)
        {
            IconMesh.enabled = IsOn;
        }
    }

    private void OnInteract()
    {
        IsOn = !IsOn;

        if (OnToggleChange != null)
        {
            OnToggleChange(IsOn);
        }
    }
}
