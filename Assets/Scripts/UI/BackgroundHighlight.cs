using UnityEngine;

public class BackgroundHighlight : MonoBehaviour
{
    public MeshRenderer Highlight = null;
    public Color UnselectedColor = Color.white;
    public Color SelectedColor = Color.black;

    private KMSelectable _selectable = null;
    private GameObject _internalHighlight = null;
    private MaterialPropertyBlock _propertyBlock = null;

    private static int _colorShaderID = 0;

    private void Awake()
    {
        if (_colorShaderID == 0)
        {
            _colorShaderID = Shader.PropertyToID("_Color");
        }

        _selectable = GetComponent<KMSelectable>();

        _propertyBlock = new MaterialPropertyBlock();
        _propertyBlock.SetColor(_colorShaderID, UnselectedColor);
        Highlight.SetPropertyBlock(_propertyBlock);
    }

    private void Update()
    {
        if (_internalHighlight == null)
        {
            _internalHighlight = _selectable.Highlight.GetInternalHighlightableObject();
            if (_internalHighlight == null)
            {
                return;
            }
        }

        _propertyBlock.SetColor(_colorShaderID, (_internalHighlight.activeSelf && _selectable.enabled) ? SelectedColor : UnselectedColor);
        
        Highlight.SetPropertyBlock(_propertyBlock);
    }
}
