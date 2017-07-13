using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(KMSelectable))]
public class TabletSelectable : MonoBehaviour
{
    public TextMesh textMesh = null;

    public Color deselectedColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    public Color selectedColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

    public UnityEvent onInteract = null;

    public bool drillOnInteract = true;

    private KMSelectable _selectable = null;
    private MeshRenderer _meshRenderer = null;
    private Material _material = null;
    private GameObject _highlightable = null;

	private void Awake()
    {
        if (textMesh == null)
        {
            textMesh = transform.parent.GetComponentInChildren<TextMesh>(true);
        }

        _meshRenderer = GetComponent<MeshRenderer>();
        _material = new Material(_meshRenderer.sharedMaterial);

        _selectable = GetComponent<KMSelectable>();

        _selectable.OnInteract += OnInteract;
        _selectable.OnInteractEnded += OnInteractEnded;
    }

    private void OnDestroy()
    {
        _selectable.OnInteract -= OnInteract;
        _selectable.OnInteractEnded -= OnInteractEnded;
    }

    private void Update()
    {
        if (_highlightable == null)
        {
            _highlightable = _selectable.Highlight.GetInternalHighlightableObject();
        }

        if (_highlightable != null)
        {
            _material.color = _highlightable.activeSelf ? selectedColor : deselectedColor;
        }

        _meshRenderer.material = _material;
    }

    private bool OnInteract()
    {        
        return drillOnInteract;
    }

    protected virtual void OnInteractEnded()
    {
        if (onInteract != null)
        {
            onInteract.Invoke();
        }
    }
}
