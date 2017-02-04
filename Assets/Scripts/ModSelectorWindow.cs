using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class ModSelectorWindow : MonoBehaviour
{
    public ModuleGrid normalModuleGrid = null;
    public ModuleGrid needyModuleGrid = null;
    public ServiceGrid serviceGrid = null;
    public ModGrid bombGrid = null;
    public ModGrid gameplayRoomGrid = null;
    public ModGrid widgetGrid = null;

    public IEnumerable<string> DisabledModNames
    {
        get
        {
            return normalModuleGrid.DisabledModuleTypeNames.Concat(needyModuleGrid.DisabledModuleTypeNames)
                                                           .Concat(serviceGrid.DisabledServiceNames)
                                                           .Concat(bombGrid.DisabledModNames)
                                                           .Concat(gameplayRoomGrid.DisabledModNames)
                                                           .Concat(widgetGrid.DisabledModNames);
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

    public void SetupMods(ModGrid modGrid, IEnumerable<ModSelectorService.ModWrapper> mods, Type modObjectType)
    {
        foreach (ModSelectorService.ModWrapper modWrapper in mods)
        {
            foreach (KeyValuePair<GameObject, bool> modObject in modWrapper.GetModObjects(modObjectType))
            {
                modGrid.CreateModToggle(_service, modWrapper, modObjectType, modObject.Key.name);
            }
        }
    }

    public void OnOK()
    {
        _service.EnableAllModules();
        _service.EnableAllServices();
        _service.EnableAllMods();

        foreach (string modName in DisabledModNames)
        {
            if (_service.DisableModule(modName))
            {
                continue;                
            }

            if (_service.DisableService(modName))
            {
                continue;
            }

            if (_service.DisableMod(ModSelectorService.ModWrapper.BombType, modName))
            {
                continue;
            }

            if (_service.DisableMod(ModSelectorService.ModWrapper.GameplayRoomType, modName))
            {
                continue;
            }

            if (_service.DisableMod(ModSelectorService.ModWrapper.WidgetType, modName))
            {
                continue;
            }
        }

        _service.SaveDefaults();

        Close();
    }

    public void OnTemporaryOK()
    {
        _service.EnableAllModules();
        _service.EnableAllServices();

        foreach (string modName in DisabledModNames)
        {
            if (_service.DisableModule(modName))
            {
                continue;                
            }

            if (_service.DisableService(modName))
            {
                continue;
            }

            if (_service.DisableMod(ModSelectorService.ModWrapper.BombType, modName))
            {
                continue;
            }

            if (_service.DisableMod(ModSelectorService.ModWrapper.GameplayRoomType, modName))
            {
                continue;
            }

            if (_service.DisableMod(ModSelectorService.ModWrapper.WidgetType, modName))
            {
                continue;
            }
        }

        Close();
    }

    public void OnCancel()
    {
        _service.LoadDefaults();

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
