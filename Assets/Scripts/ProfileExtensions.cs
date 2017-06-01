using UnityEngine;

public static class ProfileExtensions
{
    public static Color GetColor(this Profile.SetOperation value)
    {
        return value.GetAttributeOfType<ColorAttribute>().Color;
    }
}
