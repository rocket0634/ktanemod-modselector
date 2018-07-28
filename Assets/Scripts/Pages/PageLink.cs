using UnityEngine;

public class PageLink : MonoBehaviour
{
    public KMSelectable PagePrefab = null;

    private UIElement _uiElement = null;
    private PageNavigation _pageNavigation = null;

    private void Awake()
    {
        _pageNavigation = GetComponentInParent<PageNavigation>();
        _uiElement = GetComponent<UIElement>();
        _uiElement.InteractAction.AddListener(GoToPage);
    }

    private void OnDestroy()
    {
        _uiElement.InteractAction.RemoveListener(GoToPage);
    }

    public void GoToPage()
    {
        _pageNavigation.GoToPage(PagePrefab.name);
    }
}
