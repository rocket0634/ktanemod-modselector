using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
[ExecuteInEditMode]
public class Page : MonoBehaviour
{
    [Multiline(lines: 2)]
    public string HeaderText = null;

    public TextMesh HeaderRenderer = null;

    private KMSelectable _selectable = null;
    private PageManager _pageManager = null;
    private PageNavigation _pageNavigation = null;

    private void Awake()
    {
        _selectable = GetComponent<KMSelectable>();
        _pageManager = GetComponentInParent<PageManager>();
        _pageNavigation = GetComponentInParent<PageNavigation>();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (_selectable == null)
        {
            _selectable = GetComponent<KMSelectable>();
        }
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

    public void GoToPage<T>(T pagePrefab) where T : MonoBehaviour
    {
        _pageNavigation.GoToPage(pagePrefab.name);
    }

    public T GetPageWithComponent<T>(T pagePrefab) where T : MonoBehaviour
    {
        return _pageManager[pagePrefab.name].GetComponent<T>();
    }
}
