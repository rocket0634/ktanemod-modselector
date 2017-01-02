using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SaveProfileWindow : MonoBehaviour
{
    public Button okButton = null;

    private ModSelectorService _service = null;
    private Animator _animator = null;
    private float _closeWait = float.NaN;

    public void SetupService(ModSelectorService service)
    {
        _service = service;
    }

    public void OnProfileNameChange(string profileName)
    {
        okButton.interactable = !string.IsNullOrEmpty(profileName);
    }

    public void OnOK()
    {
        InputField inputField = GetComponentInChildren<InputField>();
        _service.SaveProfile(inputField.text);

        Close();
    }

    public void OnCancel()
    {
        _service.LoadTemporary();

        Close();
    }

    private void Close()
    {
        _animator.SetTrigger("FadeOut");
        _closeWait = 0.5f;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!float.IsNaN(_closeWait))
        {
            _closeWait -= Time.deltaTime;
            if (_closeWait <= 0.0f)
            {
                gameObject.SetActive(false);
                _closeWait = float.NaN;
            }
        }
    }

    private void OnEnable()
    {
        InputInterceptor.DisableControls();

        EventSystem eventSystem = EventSystem.current;

        InputField inputField = GetComponentInChildren<InputField>();
        inputField.text = "";

        eventSystem.SetSelectedGameObject(inputField.gameObject);

        _animator.Play("Fade In");
    }

    private void OnDisable()
    {
        _animator.Stop();

        InputInterceptor.EnableControls();
    }
}
