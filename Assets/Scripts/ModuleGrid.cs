using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ModuleGrid : MonoBehaviour
{
    public ModuleToggle moduleTogglePrefab = null;

    public List<ModuleToggle> moduleToggleList = new List<ModuleToggle>();

    public IEnumerable<string> DisabledModuleTypeNames
    {
        get
        {
            return moduleToggleList.Where((x) => !x.IsActive).Select((y) => y.module.ModuleType);
        }
    }

    public void CreateModuleToggle(ModSelectorService modSelectorService, ModSelectorService.Module module, Sprite emojiSprite)
    {
        ModuleToggle modToggle = Instantiate<ModuleToggle>(moduleTogglePrefab);
        modToggle.modSelectorService = modSelectorService;
        modToggle.module = module;
        if (emojiSprite != null)
        {
            modToggle.emojiImage.sprite = emojiSprite;
        }

        modToggle.gameObject.SetActive(true);
        modToggle.transform.SetParent(transform, false);

        moduleToggleList.Add(modToggle);
    }
}
