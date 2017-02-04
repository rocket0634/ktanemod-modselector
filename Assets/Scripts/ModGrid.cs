using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ModGrid : MonoBehaviour
{
    public ModToggle modTogglePrefab = null;

    public List<ModToggle> modToggleList = new List<ModToggle>();

    public IEnumerable<string> DisabledModNames
    {
        get
        {
            return modToggleList.Where((x) => !x.IsActive).Select((y) => y.modObjectName);
        }
    }

    public void CreateModToggle(ModSelectorService modSelectorService, ModSelectorService.ModWrapper modWrapper, Type modObjectType, string modObjectName)
    {
        ModToggle modToggle = Instantiate<ModToggle>(modTogglePrefab);
        modToggle.modSelectorService = modSelectorService;
        modToggle.modWrapper = modWrapper;
        modToggle.modObjectType = modObjectType;
        modToggle.modObjectName = modObjectName;

        modToggle.gameObject.SetActive(true);
        modToggle.transform.SetParent(transform, false);

        modToggleList.Add(modToggle);
    }

    public void EnableAll()
    {
        foreach (ModToggle modToggle in modToggleList)
        {
            modToggle.IsActive = true;
        }
    }

    public void DisableAll()
    {
        foreach (ModToggle modToggle in modToggleList)
        {
            modToggle.IsActive = false;
        }
    }
}
