using UnityEngine;
using UnityEngine.UI;

public class ModToggle : MonoBehaviour
{
    public ModSelectorService.Module module = null;
    public ModSelectorService modSelectorService = null;
    public float offOffset = 50.0f;
    public float animationDuration = 0.5f;

    public Transform offsetTransform = null;
    public Image backgroundImage = null;
    public Color offColor = Color.white;

    private float _delta = 0.0f;
    private float _direction = 0.0f;

	private void Awake()
    {
        GetComponentInChildren<Text>().text = module.ModuleName;
        GetComponent<Toggle>().isOn = modSelectorService.IsModuleActive(module.ModuleType);
	}

    private void Update()
    {
        if (_direction != 0.0f)
        {
            float newDelta = Mathf.Clamp(_delta + (_direction * Time.deltaTime) / animationDuration, 0.0f, 1.0f);

            float oldOffset = Mathf.SmoothStep(0.0f, offOffset, _delta);
            float newOffset = Mathf.SmoothStep(0.0f, offOffset, newDelta);

            offsetTransform.Translate(newOffset - oldOffset, 0.0f, 0.0f, Space.Self);

            _delta = newDelta;
            if (_delta <= 0.0f || _delta >= 1.0f)
            {
                _direction = 0.0f;
            }

            backgroundImage.color = Color.Lerp(Color.white, offColor, _delta);
        }
    }

    public void OnToggleChanged(bool state)
    {
        if (state)
        {
            modSelectorService.EnableModule(module.ModuleType);
            _direction = -1.0f;
        }
        else
        {
            modSelectorService.DisableModule(module.ModuleType);
            _direction = 1.0f;
        }
    }
}
