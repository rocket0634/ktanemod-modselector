using System.Text;
using UnityEngine;

public class ModBundleInfoPage : MonoBehaviour
{
    public ModSelectorService.ModWrapper ModWrapper = null;
    public TextMesh DetailText = null;
    public int MinimumSize = 12;
    public int MaximumSize = 24;
    public int PathCharacterCutOff = 90;
    public int ModCharacterCutOff = 100;

    private Page _page = null;

    private const string FormattingString = "<b>Title</b>:  {0}\n<b>ID</b>:  {1}\n<b>Version</b>:  {2}\n<b>Path</b>:  <size={3}>{4}</size>\n<b>Mod Object Prefabs</b>:\n{5}\n<b>Mod Module IDs</b>:\n{6}";

    private void Awake()
    {
        _page = GetComponent<Page>();
    }

    public void OnEnable()
    {
        _page.HeaderText = string.Format("<b>{0}</b>\n<size=16>Mod Info</size>", ModWrapper == null ? "**NULL**" : ModWrapper.ModTitle);

        if (DetailText == null)
        {
            return;
        }

        if (ModWrapper == null)
        {
            DetailText.text = string.Format(FormattingString, "**NULL**", "**NULL**", "**NULL**", 24, "**NULL**", "**NULL**", "**NULL**");
            return;
        }

        int pathFontSize = Mathf.Clamp((MaximumSize * PathCharacterCutOff) / ModWrapper.ModDirectory.Length, MinimumSize, MaximumSize);

        StringBuilder modStringBuilder = new StringBuilder();
        StringBuilder modLineStringBuilder = new StringBuilder();

        StringBuilder moduleIDStringBuilder = new StringBuilder();
        StringBuilder moduleIDLineStringBuilder = new StringBuilder();
        foreach(GameObject gameObject in ModWrapper.ModObjects)
        {
            if (modLineStringBuilder.Length > 0)
            {
                modLineStringBuilder.Append(", ");
            }

            string addition = gameObject.name.Replace("(Clone)", "");
            if (modLineStringBuilder.Length + addition.Length > ModCharacterCutOff)
            {
                modStringBuilder.Append(modLineStringBuilder.ToString());
                modStringBuilder.Append("\n");
                modLineStringBuilder = new StringBuilder();
            }

            modLineStringBuilder.Append(addition);

            KMBombModule[] regularModules = gameObject.GetComponentsInChildren<KMBombModule>(true);
            foreach(KMBombModule regularModule in regularModules)
            {
                if (moduleIDLineStringBuilder.Length > 0)
                {
                    moduleIDLineStringBuilder.Append(", ");
                }

                if (moduleIDLineStringBuilder.Length + regularModule.ModuleType.Length > ModCharacterCutOff)
                {
                    moduleIDStringBuilder.Append(moduleIDLineStringBuilder.ToString());
                    moduleIDStringBuilder.Append("\n");
                    moduleIDLineStringBuilder = new StringBuilder();
                }

                moduleIDLineStringBuilder.Append(regularModule.ModuleType);
            }

            KMNeedyModule[] needyModules = gameObject.GetComponentsInChildren<KMNeedyModule>(true);
            foreach(KMNeedyModule needyModule in needyModules)
            {
                if (moduleIDLineStringBuilder.Length > 0)
                {
                    moduleIDLineStringBuilder.Append(", ");
                }

                if (moduleIDLineStringBuilder.Length + needyModule.ModuleType.Length > ModCharacterCutOff)
                {
                    moduleIDStringBuilder.Append(moduleIDLineStringBuilder.ToString());
                    moduleIDStringBuilder.Append("\n");
                    moduleIDLineStringBuilder = new StringBuilder();
                }

                moduleIDLineStringBuilder.Append(needyModule.ModuleType);
            }
        }

        modStringBuilder.Append(modLineStringBuilder);
        moduleIDStringBuilder.Append(moduleIDLineStringBuilder);

        string modStringText = modStringBuilder.ToString();
        string moduleIDStringText = moduleIDStringBuilder.ToString();

        DetailText.text = string.Format(FormattingString, ModWrapper.ModTitle, ModWrapper.ModName, ModWrapper.ModVersion, pathFontSize, ModWrapper.ModDirectory, modStringText, moduleIDStringText);
    }   
    
    public void OpenPath()
    {
        PlatformActions.ShowPath(ModWrapper.ModDirectory);
    }
}
