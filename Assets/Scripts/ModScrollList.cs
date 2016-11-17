using UnityEngine;
using UnityEngine.UI;

public class ModScrollList : MonoBehaviour
{
    public ModToggle modTogglePrefab = null;

    public void CreateToggle(ModSelectorService modSelectorService, ModSelectorService.Module module)
    {
        ModToggle modToggle = Instantiate<ModToggle>(modTogglePrefab);
        modToggle.modSelectorService = modSelectorService;
        modToggle.module = module;
        modToggle.transform.SetParent(GetComponent<ScrollRect>().content, false);
        modToggle.gameObject.SetActive(true);
    }
}
