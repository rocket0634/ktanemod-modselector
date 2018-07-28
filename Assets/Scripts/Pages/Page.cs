using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
[ExecuteInEditMode]
public class Page : MonoBehaviour
{
    [Multiline(lines: 2)]
    public string HeaderText = null;

    public TextMesh HeaderRenderer = null;

    private KMSelectable _selectable = null;

    private void Awake()
    {
        _selectable = GetComponent<KMSelectable>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        foreach (KMSelectable child in _selectable.Children)
        {
            if (child != null)
            {
                child.Parent = _selectable;
            }
        }
#endif

        if (HeaderRenderer != null)
        {
            HeaderRenderer.text = HeaderText;
        }
    }
}
