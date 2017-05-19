using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TabletPage))]
public class TabletPageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Update Page"))
        {
            UpdatePage((TabletPage)target);
        }

        if (GUILayout.Button("Update Links"))
        {
            UpdateLinks((TabletPage)target);
        }
    }

    private static void UpdatePage(TabletPage page)
    {
        page.UpdatePage();
    }

    private static void UpdateLinks(TabletPage page)
    {
        EditorUtility.SetDirty(page);

        TabletSelectablePageLink[] pageLinks = page.transform.root.GetComponentsInChildren<TabletSelectablePageLink>(true);

        foreach (TabletSelectablePageLink pageLink in pageLinks)
        {
            if (pageLink.linkPage == page)
            {
                page.UpdateSelectablePageLink(pageLink);
                EditorUtility.SetDirty(pageLink);
            }
        }
    }    
}
