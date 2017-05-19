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

    private const string FormattingString = "<b>Title</b>:\t{0}\n<b>ID</b>:\t{1}\n<b>Version</b>:\t{2}\n<b>Path</b>:\t<size={3}>{4}</size>\n\n<b>Included Mod Objects</b>:\n{5}";

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
            detailText.text = string.Format(FormattingString, "**NULL**", "**NULL**", "**NULL**", 24, "**NULL**", "**NULL**");
            return;
        }

        int pathFontSize = Mathf.Clamp((maximumSize * pathCharacterCutOff) / modWrapper.ModDirectory.Length, minimumSize, maximumSize);

        StringBuilder modStringBuilder = new StringBuilder();
        StringBuilder lineStringBuilder = new StringBuilder();
        foreach(GameObject gameObject in modWrapper.ModObjects)
        {
            if (lineStringBuilder.Length > 0)
            {
                lineStringBuilder.Append(", ");
            }

            string addition = gameObject.name.Replace("(Clone)", "");
            if (lineStringBuilder.Length + addition.Length > modCharacterCutOff)
            {
                modStringBuilder.Append(lineStringBuilder.ToString());
                modStringBuilder.Append("\n");
                lineStringBuilder = new StringBuilder();
            }

            lineStringBuilder.Append(addition);
        }

        modStringBuilder.Append(lineStringBuilder);
        string modStringText = modStringBuilder.ToString();

        detailText.text = string.Format(FormattingString, modWrapper.ModTitle, modWrapper.ModName, modWrapper.ModVersion, pathFontSize, modWrapper.ModDirectory, modStringText);
    }   
    
    public void OpenPath()
    {
        Application.OpenURL(string.Format("file://{0}", modWrapper.ModDirectory));
    }
}
