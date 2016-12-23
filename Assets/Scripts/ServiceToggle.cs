using UnityEngine;
using UnityEngine.UI;

public class ServiceToggle : MonoBehaviour
{
    public ModSelectorService.Service service = null;
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
        private set
        {
            _toggle.isOn = value;
        }
    }

    private Toggle _toggle = null;

    private void Awake()
    {
        modNameText.text = service.ServiceName;

        _toggle = GetComponent<Toggle>();
        IsActive = modSelectorService.IsServiceActive(service.ServiceName);
        OnToggleChanged(IsActive);
    }

    private void OnEnable()
    {
        IsActive = modSelectorService.IsServiceActive(service.ServiceName);
        OnToggleChanged(IsActive);
    }

    public void OnToggleChanged(bool state)
    {
        backgroundImage.color = state ? onColor : offColor;
        modNameText.color = state ? textOnColor : textOffColor;
    }
}
