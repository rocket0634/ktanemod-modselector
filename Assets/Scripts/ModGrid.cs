using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ModGrid : MonoBehaviour
{
    public ModToggle modTogglePrefab = null;

    public List<ModToggle> modToggleList = new List<ModToggle>();

    public IEnumerable<string> DisabledModuleTypeNames
    {
        get
        {
            return modToggleList.Where((x) => !x.IsActive).Select((y) => y.module.ModuleType);
        }
    }

    public void CreateToggle(ModSelectorService modSelectorService, ModSelectorService.Module module, Sprite emojiSprite)
    {
        ModToggle modToggle = Instantiate<ModToggle>(modTogglePrefab);
        modToggle.modSelectorService = modSelectorService;
        modToggle.module = module;
        if (emojiSprite != null)
        {
            modToggle.emojiImage.sprite = emojiSprite;
        }

        modToggle.gameObject.SetActive(true);
        modToggle.transform.SetParent(transform, false);

        modToggleList.Add(modToggle);
    }
}
