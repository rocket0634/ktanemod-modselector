using UnityEngine;
using UnityEditor;

public static class KMSelectableFixes
{
    [MenuItem("Keep Talking ModKit/Fix KMSelectable Colliders")]
    public static void FixSelectableColliders()
    {
        KMSelectable[] selectables = Resources.FindObjectsOfTypeAll<KMSelectable>();
        foreach (KMSelectable selectable in selectables)
        {
            BoxCollider collider = selectable.GetComponent<BoxCollider>();
            if (collider != null)
            {
                selectable.SelectableColliders = new Collider[1];
                selectable.SelectableColliders[0] = collider;
                EditorUtility.SetDirty(selectable.gameObject);
            }
        }
    }
}
