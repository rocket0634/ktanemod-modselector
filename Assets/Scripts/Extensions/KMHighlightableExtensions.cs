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
        return (GameObject)HighlightField.GetValue(highlightable.GetComponent(HighlightableType));
    }

    private static readonly Type HighlightableType;
    private static readonly FieldInfo HighlightField;
}
