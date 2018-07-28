using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(KMSelectable))]
public class TabletSelectableToggle : MonoBehaviour
{
    public ToggleEvent onToggleEvent = new ToggleEvent();

    public GameObject toggleIndicator = null;
    public bool toggleState = false;

    private KMSelectable _selectable = null;

    private void Awake()
    {
        _selectable = GetComponent<KMSelectable>();
        _selectable.OnInteract += OnInteract;
    }

    private void OnDestroy()
    {
        if (_selectable != null)
        {
            _selectable.OnInteract -= OnInteract;
        }
    }

    private void Update()
    {
        toggleIndicator.SetActive(toggleState);
    }

    private bool OnInteract()
    {
        toggleState = !toggleState;

        if (onToggleEvent != null)
        {
            onToggleEvent.Invoke(toggleState);
        }

        return false;
    }
}

public class ToggleEvent : UnityEvent<bool>
{
}
