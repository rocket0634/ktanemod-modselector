using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class LoadProfileWindow : MonoBehaviour
{
    public ProfileOption profileOptionPrefab = null;
    public ToggleGroup toggleGroup = null;

    private ModSelectorService _service = null;
    private Animator _animator = null;
    private float _closeWait = float.NaN;

    public void SetupService(ModSelectorService service)
    {
        _service = service;
    }

    public void OnOK()
    {
        Toggle activeToggle = toggleGroup.ActiveToggles().First();
        ProfileOption activeProfile = activeToggle.GetComponent<ProfileOption>();
        _service.LoadProfile(activeProfile.ProfileName);

        Close();
    }

    public void OnCancel()
    {
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
        eventSystem.SetSelectedGameObject(GetComponentInChildren<ProfileOption>(true).gameObject);

        _animator.Play("Fade In");

        foreach (string profile in _service.AvailableProfiles)
        {
            ProfileOption profileOption = Instantiate<ProfileOption>(profileOptionPrefab);
            profileOption.gameObject.SetActive(true);
            profileOption.transform.SetParent(profileOptionPrefab.transform.parent, false);
            profileOption.ProfileName = profile;
        }
    }

    private void OnDisable()
    {
        _animator.Stop();

        InputInterceptor.EnableControls();

        ProfileOption[] profileOptions = toggleGroup.GetComponentsInChildren<ProfileOption>();
        foreach(ProfileOption profileOption in profileOptions)
        {
            Destroy(profileOption.gameObject);
        }
    }
}
