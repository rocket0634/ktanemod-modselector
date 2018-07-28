using System;
using System.Reflection;
using UnityEngine;

public static class KMHighlightableExtensions
{
    static KMHighlightableExtensions()
    {
        HighlightableType = ReflectionHelper.FindType("Highlightable");
        HighlightField = HighlightableType.GetField("highlight", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public static GameObject GetInternalHighlightableObject(this KMHighlightable highlightable)
    {
        Component modHighlightable = highlightable.GetComponent(HighlightableType);
        if (modHighlightable != null)
        {
            return (GameObject)HighlightField.GetValue(modHighlightable);
        }

        return null;
    }

    private static readonly Type HighlightableType;
    private static readonly FieldInfo HighlightField;
}
