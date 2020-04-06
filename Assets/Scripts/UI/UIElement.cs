using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class UIElement : MonoBehaviour
{
    public string Text = null;
    public Texture2D Icon = null;
    public Texture2D DisabledIcon = null;

    public TextMesh TextRenderer = null;
    public MeshRenderer IconMesh = null;

    public Color EnabledTextColor = Color.black;
    public Color DisabledTextColor = Color.grey;

    public BackgroundHighlight BackgroundHighlight = null;

    public UnityEvent InteractAction = new UnityEvent();

    public bool CanSelect
    {
        get
        {
            if (_selectable == null)
            {
                _selectable = GetComponent<KMSelectable>();
            }

            return _selectable.enabled;
        }
        set
        {
            if (_selectable == null)
            {
                _selectable = GetComponent<KMSelectable>();
            }

            _selectable.enabled = value;
        }
    }

    private KMSelectable _selectable = null;
    private MaterialPropertyBlock _iconPropertyBlock = null;

    private static int _mainTexShaderID = 0;

    protected virtual void Awake()
    {
        if (_mainTexShaderID == 0)
        {
            _mainTexShaderID = Shader.PropertyToID("_MainTex");
        }

        if (IconMesh != null && _iconPropertyBlock == null)
        {
            _iconPropertyBlock = new MaterialPropertyBlock();
            _iconPropertyBlock.SetTexture(_mainTexShaderID, Icon);
            IconMesh.SetPropertyBlock(_iconPropertyBlock);
        }

        if (_selectable == null)
        {
            _selectable = GetComponent<KMSelectable>();
        }

        _selectable.OnInteract += OnInteract;
    }

    protected virtual void OnDestroy()
    {
        _iconPropertyBlock = null;

        _selectable.OnInteract -= OnInteract;
        _selectable = null;
    }

    protected virtual void Update()
    {
#if UNITY_EDITOR
        Awake();
#endif

        name = Text;

        if (TextRenderer != null)
        {
            TextRenderer.text = Text;
            TextRenderer.color = _selectable.enabled ? EnabledTextColor : DisabledTextColor;
        }

        if (IconMesh != null && _iconPropertyBlock != null)
        {
            _iconPropertyBlock.SetTexture(_mainTexShaderID, _selectable.enabled ? Icon : (DisabledIcon != null ? DisabledIcon : Icon));
            IconMesh.SetPropertyBlock(_iconPropertyBlock);
        }
    }

    private bool OnInteract()
    {
        if (_selectable.enabled)
        {
            InteractAction.Invoke();
        }
        return false;
    }
}
