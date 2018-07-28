using System.Linq;
using UnityEngine;

public class HomePage : MonoBehaviour
{
    private Page _page = null;
    private PageLink[] _pageLinks = null;

    private PageManager.HomePageEntry[] _homePageEntries = null;

    private void Awake()
    {
        _page = GetComponent<Page>();
        _pageLinks = GetComponentsInChildren<PageLink>(true);
    }

    private void OnEnable()
    {
        _homePageEntries = PageManager.HomePageEntries.ToArray();

        for (int pageLinkIndex = 0; pageLinkIndex < _pageLinks.Length; ++pageLinkIndex)
        {
            PageLink pageLink = _pageLinks[pageLinkIndex];

            if (pageLinkIndex >= _homePageEntries.Length)
            {
                pageLink.gameObject.SetActive(false);
                continue;
            }

            PageManager.HomePageEntry homePageEntry = _homePageEntries[pageLinkIndex];

            pageLink.gameObject.SetActive(true);

            UIElement element = pageLink.GetComponent<UIElement>();
            element.Text = homePageEntry.DisplayName;
            element.Icon = homePageEntry.Icon;
            pageLink.PagePrefab = homePageEntry.PageSelectable;
        }
    }
}
