using System.Collections.Generic;
using System.ComponentModel;

public class ModTogglePage : Pagination<KeyValuePair<string, string>, UIToggle>
{
    public Profile Profile = null;
    public ModSelectorService.ModType ModType = ModSelectorService.ModType.Unknown;
    public KeyValuePair<string, string>[] Entries = null;

    protected override KeyValuePair<string, string>[] ValueCollection
    {
        get
        {
            return Entries;
        }
    }

    protected override UIToggle[] ElementCollection
    {
        get
        {
            return _toggles;
        }
    }

    private UIToggle[] _toggles = null;

    protected override void Awake()
    {
        base.Awake();

        _toggles = GetComponentsInChildren<UIToggle>(true);
        for(int toggleIndex = 0; toggleIndex < _toggles.Length; ++toggleIndex)
        {
            int localToggleIndex = toggleIndex;
            _toggles[toggleIndex].OnToggleChange += delegate (bool toggled)
            {
                string entry = Entries[ValueOffset + localToggleIndex].Key;

                if (toggled)
                {
                    Profile.Disable(entry);
                }
                else
                {
                    Profile.Enable(entry);
                }
            };
        }
    }

    private void OnEnable()
    {
        SetPage(0);
    }

    public override void SetPage(int pageIndex, int pageOffset = 0)
    {
        if (Entries == null || Profile == null || _toggles == null)
        {
            return;
        }

        base.SetPage(pageIndex, pageOffset);

        Page.HeaderText = string.Format("<b>{0}</b>\n<size=16>{1}, {2}</size>", Profile.FriendlyName, ModType.GetAttributeOfType<DescriptionAttribute>().Description, PageName);
    }

    public void EnableAll()
    {
        Profile.EnableAllOfType(ModType);
        SetPage(PageIndex);
    }

    public void DisableAll()
    {
        Profile.DisableAllOfType(ModType);
        SetPage(PageIndex);
    }

    protected override void SetElement(KeyValuePair<string, string> value, UIToggle element)
    {
        switch (Profile.GetEnabledFlag(value.Key))
        {
            case Profile.EnableFlag.Disabled:
                element.IsOn = true;
                break;
            case Profile.EnableFlag.Enabled:
                element.IsOn = false;
                break;

            default:
                break;
        }

        element.Text = value.Value;
    }
}
