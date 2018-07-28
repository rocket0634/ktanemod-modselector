using System;
using System.Reflection;
using UnityEngine;

public static class KMSelectableExtensions
{
    private static readonly Type ModSelectableType;
    private static readonly Type ModHighlightableType;
    private static readonly MethodInfo CopySettingsFromProxyMethod;

    static KMSelectableExtensions()
    {
        ModSelectableType = ReflectionHelper.FindType("ModSelectable");
        ModHighlightableType = ReflectionHelper.FindType("ModHighlightable");
        CopySettingsFromProxyMethod = ModSelectableType.GetMethod("CopySettingsFromProxy", BindingFlags.Instance | BindingFlags.Public);
    }

    public static void EnsureModSelectable(this KMSelectable selectable)
    {
        Component modSelectable = selectable.GetComponent(ModSelectableType);
        if (modSelectable == null)
        {
            selectable.gameObject.AddComponent(ModSelectableType);
        }

        selectable.EnsureModHighlightable();
    }

    public static void EnsureModHighlightable(this KMSelectable selectable)
    {
        KMHighlightable highlightable = selectable.Highlight;
        if (highlightable != null)
        {
            Component modHighlightable = highlightable.GetComponent(ModHighlightableType);
            if (modHighlightable == null)
            {
                highlightable.gameObject.AddComponent(ModHighlightableType);
            }
        }
    }

    public static void Reproxy(this KMSelectable selectable)
    {
        object modSelectable = selectable.GetComponent(ModSelectableType);
        if (modSelectable != null)
        {
            CopySettingsFromProxyMethod.Invoke(modSelectable, null);
        }
    }
}
