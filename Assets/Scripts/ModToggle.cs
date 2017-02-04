using System;
using UnityEngine;
using UnityEngine.UI;

public class ModToggle : MonoBehaviour
{
    public Type modObjectType = null;

    public ModSelectorService.ModWrapper modWrapper = null;
    public string modObjectName = null;

    public ModSelectorService modSelectorService = null;

    public Image backgroundImage = null;
    public Text modNameText = null;

    public Color onColor = Color.white;
    public Color offColor = Color.white;

    public Color textOnColor = Color.black;
    public Color textOffColor = Color.black;

    public float transitionDuration = 0.5f;

    public bool IsActive
    {
        get
        {
            return _toggle.isOn;
        }
        set
        {
            _toggle.isOn = value;
        }
    }

    private Toggle _toggle = null;

    private void Awake()
    {
        modNameText.text = modWrapper.GetModObject(modObjectName).name;

        _toggle = GetComponent<Toggle>();
        IsActive = modSelectorService.IsModActive(modObjectType, modWrapper.ModName, modObjectName);
        OnToggleChanged(IsActive);
    }

    private void OnEnable()
    {
        IsActive = modSelectorService.IsModActive(modObjectType, modWrapper.ModName, modObjectName);
        OnToggleChanged(IsActive);
    }

    public void OnToggleChanged(bool state)
    {
        backgroundImage.color = state ? onColor : offColor;
        modNameText.color = state ? textOnColor : textOffColor;
    }
}
