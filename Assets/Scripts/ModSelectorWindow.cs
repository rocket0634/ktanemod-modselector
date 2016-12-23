using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModSelectorWindow : MonoBehaviour
{
    public ModuleGrid normalModuleGrid = null;
    public ModuleGrid needyModuleGrid = null;
    public ServiceGrid serviceGrid = null;

    public IEnumerable<string> DisabledModNames
    {
        get
        {
            return normalModuleGrid.DisabledModuleTypeNames.Concat(needyModuleGrid.DisabledModuleTypeNames).Concat(serviceGrid.DisabledServiceNames);
        }
    }

    private ModSelectorService _service = null;
    private Animator _animator = null;
    private float _closeWait = float.NaN;

    public void SetupService(ModSelectorService service)
    {
        _service = service;
    }

    public void SetupNormalModules(IEnumerable<ModSelectorService.SolvableModule> normalModules)
    {
        foreach (ModSelectorService.SolvableModule module in normalModules)
        {
            normalModuleGrid.CreateModuleToggle(_service, module, _service.GetEmojiSprite(module.ModuleType));
        }
    }

    public void SetupNeedyModules(IEnumerable<ModSelectorService.NeedyModule> needyModules)
    {
        foreach (ModSelectorService.NeedyModule module in needyModules)
        {
            needyModuleGrid.CreateModuleToggle(_service, module, _service.GetEmojiSprite(module.ModuleType));
        }
    }

    public void SetupServices(IEnumerable<ModSelectorService.Service> services)
    {
        foreach (ModSelectorService.Service service in services)
        {
            serviceGrid.CreateServiceToggle(_service, service);
        }
    }

    public void OnOK()
    {
        _service.EnableAllModules();
        _service.EnableAllServices();

        foreach (string modName in DisabledModNames)
        {
            if (!_service.DisableModule(modName))
            {
                _service.DisableService(modName);
            }
        }

        _service.SaveDefaults();

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
        eventSystem.SetSelectedGameObject(GetComponentInChildren<ModuleToggle>(true).gameObject);

        _animator.Play("Fade In");
    }

    private void OnDisable()
    {
        _animator.Stop();

        InputInterceptor.EnableControls();
    }
}
