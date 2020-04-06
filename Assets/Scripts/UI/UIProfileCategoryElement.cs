using UnityEngine;

[ExecuteInEditMode]
public class UIProfileCategoryElement : UIElement
{
    public string DisabledText;
    public string EnabledText;
    public string TotalText;

    public TextMesh DisabledTextRenderer;
    public TextMesh EnabledTextRenderer;
    public TextMesh TotalTextRenderer;

    protected override void Update()
    {
        base.Update();
        if (DisabledTextRenderer != null)
        {
            DisabledTextRenderer.text = DisabledText;
            DisabledTextRenderer.color = CanSelect ? EnabledTextColor : DisabledTextColor;
        }
        if (EnabledTextRenderer != null)
        {
            EnabledTextRenderer.text = EnabledText;
            EnabledTextRenderer.color = CanSelect ? EnabledTextColor : DisabledTextColor;
        }
        if (TotalTextRenderer != null)
        {
            TotalTextRenderer.text = TotalText;
            TotalTextRenderer.color = CanSelect ? EnabledTextColor : DisabledTextColor;
        }
    }
}
