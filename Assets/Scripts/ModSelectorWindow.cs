using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModSelectorWindow : MonoBehaviour
{
    public ModGrid normalModuleGrid = null;
    public ModGrid needyModuleGrid = null;

    public IEnumerable<string> DisabledModuleTypeNames
    {
        get
        {
            return normalModuleGrid.DisabledModuleTypeNames.Concat(needyModuleGrid.DisabledModuleTypeNames);
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
            normalModuleGrid.CreateToggle(_service, module, _service.GetEmojiSprite(module.ModuleType));
        }
    }

    public void SetupNeedyModules(IEnumerable<ModSelectorService.NeedyModule> needyModules)
    {
        foreach (ModSelectorService.NeedyModule module in needyModules)
        {
            needyModuleGrid.CreateToggle(_service, module, _service.GetEmojiSprite(module.ModuleType));
        }
    }

    public void OnOK()
    {
        _service.EnableAllModules();

        foreach (string moduleTypeName in DisabledModuleTypeNames)
        {
            _service.DisableModule(moduleTypeName);
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
        eventSystem.SetSelectedGameObject(GetComponentInChildren<ModToggle>(true).gameObject);

        _animator.Play("Fade In");
    }

    private void OnDisable()
    {
        _animator.Stop();

        InputInterceptor.EnableControls();
    }
}
