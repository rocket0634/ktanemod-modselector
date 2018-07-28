using System.Collections.Generic;
using UnityEngine;

public class PageManager : MonoBehaviour
{
    public Transform RootTransform = null;

    public KMSelectable this[string pageName]
    {
        get
        {
            KMSelectable page = null;
            if (Pages.TryGetValue(pageName, out page))
            {
                return page;
            }

            KMSelectable pagePrefab = null;
            if (PagePrefabs.TryGetValue(pageName, out pagePrefab))
            {
                page = Instantiate<KMSelectable>(pagePrefab, RootTransform, false);
                page.transform.localPosition = Vector3.zero;
                page.gameObject.SetActive(false);
                Pages[pageName] = page;
                return page;
            }

            return null;
        }
    }

    private static readonly Dictionary<string, KMSelectable> PagePrefabs = new Dictionary<string, KMSelectable>();
    private readonly Dictionary<string, KMSelectable> Pages = new Dictionary<string, KMSelectable>();

    public static void AddPagePrefabs(KMSelectable[] pageSelectables)
    {
        foreach (KMSelectable pageSelectable in pageSelectables)
        {
            AddPagePrefab(pageSelectable);
        }
    }

    public static void AddPagePrefab(KMSelectable pageSelectable)
    {
        AddPagePrefab(pageSelectable.name, pageSelectable);
    }

    public static void AddPagePrefab(string name, KMSelectable pageSelectable)
    {
        if (PagePrefabs.ContainsKey(name))
        {
            return;
        }

        PagePrefabs[name] = pageSelectable;
        pageSelectable.EnsureModSelectable();
        pageSelectable.Reproxy();
    }

    public void DestroyCachedPages()
    {
        foreach (KMSelectable page in Pages.Values)
        {
            DestroyObject(page.gameObject);
        }

        Pages.Clear();
    }
}
