using System;
using UnityEngine;

public class ColorAttribute : Attribute
{
    public ColorAttribute(float r, float g, float b, float a = 1.0f)
    {
        Color = new Color(r, g, b, a);
    }

    public readonly Color Color;
}
