using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class DeleteProfileWindow : MonoBehaviour
{
    public ProfileOption profileOptionPrefab = null;
    public ToggleGroup toggleGroup = null;
    public Button okButton = null;

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
        _service.DeleteProfile(activeProfile.ProfileName);
        okButton.interactable = false;

        RepopulateList();
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

        PopulateList();
    }

    private void OnDisable()
    {
        _animator.Stop();
        okButton.interactable = false;

        InputInterceptor.EnableControls();

        ClearList();
    }

    private void ClearList()
    {
        ProfileOption[] profileOptions = toggleGroup.GetComponentsInChildren<ProfileOption>();
        foreach (ProfileOption profileOption in profileOptions)
        {
            Destroy(profileOption.gameObject);
        }
    }

    private void PopulateList()
    {
        foreach (string profile in _service.AvailableProfiles)
        {
            ProfileOption profileOption = Instantiate<ProfileOption>(profileOptionPrefab);
            profileOption.gameObject.SetActive(true);
            profileOption.transform.SetParent(profileOptionPrefab.transform.parent, false);
            profileOption.ProfileName = profile;
        }
    }

    private void RepopulateList()
    {
        ClearList();
        PopulateList();
    }
}
