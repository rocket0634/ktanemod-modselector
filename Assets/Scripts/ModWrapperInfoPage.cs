using System.Text;
using UnityEngine;

[RequireComponent(typeof(TabletPage))]
public class ModWrapperInfoPage : MonoBehaviour
{
    public ModSelectorService.ModWrapper modWrapper = null;
    public TextMesh detailText = null;
    public int minimumSize = 12;
    public int maximumSize = 24;
    public int pathCharacterCutOff = 50;
    public int modCharacterCutOff = 60;

    private TabletPage _tabletPage = null;

    private const string FormattingString = "<b>Title</b>:\t{0}\n<b>ID</b>:\t{1}\n<b>Version</b>:\t{2}\n<b>Path</b>:\t<size={3}>{4}</size>\n<b>Mod Object Prefabs</b>:\n{5}\n<b>Mod Module IDs</b>:\n{6}";

    private void Awake()
    {
        _tabletPage = GetComponent<TabletPage>();
    }

    public void OnEnable()
    {
        if (_tabletPage != null && _tabletPage.header != null)
        {
            _tabletPage.header.text = string.Format("<b>{0}</b>\n<size=16>Mod Info</size>", modWrapper == null ? "**NULL**" : modWrapper.ModTitle);
        }

        if (detailText == null)
        {
            return;
        }

        if (modWrapper == null)
        {
            detailText.text = string.Format(FormattingString, "**NULL**", "**NULL**", "**NULL**", 24, "**NULL**", "**NULL**", "**NULL**");
            return;
        }

        int pathFontSize = Mathf.Clamp((maximumSize * pathCharacterCutOff) / modWrapper.ModDirectory.Length, minimumSize, maximumSize);

        StringBuilder modStringBuilder = new StringBuilder();
        StringBuilder modLineStringBuilder = new StringBuilder();

        StringBuilder moduleIDStringBuilder = new StringBuilder();
        StringBuilder moduleIDLineStringBuilder = new StringBuilder();
        foreach(GameObject gameObject in modWrapper.ModObjects)
        {
            if (modLineStringBuilder.Length > 0)
            {
                modLineStringBuilder.Append(", ");
            }

            string addition = gameObject.name.Replace("(Clone)", "");
            if (modLineStringBuilder.Length + addition.Length > modCharacterCutOff)
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

                if (moduleIDLineStringBuilder.Length + regularModule.ModuleType.Length > modCharacterCutOff)
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

                if (moduleIDLineStringBuilder.Length + needyModule.ModuleType.Length > modCharacterCutOff)
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

        detailText.text = string.Format(FormattingString, modWrapper.ModTitle, modWrapper.ModName, modWrapper.ModVersion, pathFontSize, modWrapper.ModDirectory, modStringText, moduleIDStringText);
    }   
    
    public void OpenPath()
    {
        Application.OpenURL(string.Format("file://{0}", modWrapper.ModDirectory));
    }
}
