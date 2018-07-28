using UnityEngine;

public class TabletSelectableDisableable : MonoBehaviour
{
    public TextMesh text = null;
    public Color enabledTextColor = Color.black;
    public Color disabledTextColor = Color.grey;

    public MeshRenderer image = null;
    public Texture enabledTexture = null;
    public Texture disabledTexture = null;

    private KMSelectable _selectable = null;

    private void Awake()
    {
        _selectable = GetComponent<KMSelectable>();
    }

    public void SetEnable(bool enabled)
    { 
        if (text != null)
        {
            text.color = enabled ? enabledTextColor : disabledTextColor;
        }  
        
        if (image != null)
        {
            Material material = image.material;
            material.mainTexture = enabled ? enabledTexture : disabledTexture;
            image.material = material;
        }

        if (_selectable != null)
        {
            _selectable.enabled = enabled;
        }
    }
}
