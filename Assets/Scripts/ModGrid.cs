using UnityEngine;

public class ModGrid : MonoBehaviour
{
    public ModToggle modTogglePrefab = null;

    public void CreateToggle(ModSelectorService modSelectorService, ModSelectorService.Module module, Sprite emojiSprite)
    {
        Debug.Log("Trying to add toggle for " + module.ModuleName);
        ModToggle modToggle = Instantiate<ModToggle>(modTogglePrefab);
        modToggle.modSelectorService = modSelectorService;
        modToggle.module = module;
        if (emojiSprite != null)
        {
            modToggle.emojiImage.sprite = emojiSprite;
        }

        modToggle.gameObject.SetActive(true);
        modToggle.transform.SetParent(transform, false);
    }
}
