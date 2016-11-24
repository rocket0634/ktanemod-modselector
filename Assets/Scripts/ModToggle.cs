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

    private Toggle _toggle = null;

	private void Awake()
    {
        modNameText.text = module.ModuleName;
        modIDText.text = module.ModuleType;

        _toggle = GetComponent<Toggle>();
        _toggle.isOn = modSelectorService.IsModuleActive(module.ModuleType);
        backgroundImage.color = _toggle.isOn ? onColor : offColor;
        modNameText.color = _toggle.isOn ? textOnColor : textOffColor;
        modIDText.color = _toggle.isOn ? textOnColor : textOffColor;
        emojiImage.color = _toggle.isOn ? emojiOnColor : emojiOffColor;
    }

    public void OnToggleChanged(bool state)
    {
        if (state)
        {
            modSelectorService.EnableModule(module.ModuleType);
        }
        else
        {
            modSelectorService.DisableModule(module.ModuleType);
        }

        backgroundImage.color = state ? onColor : offColor;
        modNameText.color = state ? textOnColor : textOffColor;
        modIDText.color = state ? textOnColor : textOffColor;
        emojiImage.color = state ? emojiOnColor : emojiOffColor;
    }
}
