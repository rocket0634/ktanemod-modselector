using UnityEngine;
using UnityEngine.UI;

public class ModToggle : MonoBehaviour
{
    public ModSelectorService.Module module = null;
    public ModSelectorService modSelectorService = null;

    public Image backgroundImage = null;
    public Image emojiImage = null;
    public Text modNameText = null;
    public Text modIDText = null;

    public Color onColor = Color.white;
    public Color offColor = Color.white;

    public Color textOnColor = Color.black;
    public Color textOffColor = Color.black;

    public Color emojiOnColor = Color.white;
    public Color emojiOffColor = Color.white;

    public float transitionDuration = 0.5f;

    public bool IsActive
    {
        get
        {
            return _toggle.isOn;
        }
        private set
        {
            _toggle.isOn = value;
        }
    }

    private Toggle _toggle = null;

	private void Awake()
    {
        modNameText.text = module.ModuleName;
        modIDText.text = module.ModuleType;

        _toggle = GetComponent<Toggle>();
        IsActive = modSelectorService.IsModuleActive(module.ModuleType);
        OnToggleChanged(IsActive);
    }

    private void OnEnable()
    {
        IsActive = modSelectorService.IsModuleActive(module.ModuleType);
        OnToggleChanged(IsActive);
    }

    public void OnToggleChanged(bool state)
    {
        backgroundImage.color = state ? onColor : offColor;
        modNameText.color = state ? textOnColor : textOffColor;
        modIDText.color = state ? textOnColor : textOffColor;
        emojiImage.color = state ? emojiOnColor : emojiOffColor;
    }
}
