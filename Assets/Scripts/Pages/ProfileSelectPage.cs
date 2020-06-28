using System.Collections.Generic;
using System.Linq;

public class ProfileSelectPage : Pagination<Profile, UIElement>
{
    public ProfileSettingsPage SettingsPagePrefab = null;

    public FilterButton FilterButton = null;
    public UIElement[] Options = null;

    protected override Profile[] ValueCollection
    {
        get
        {
            return _availableProfiles;
        }
    }

    protected override UIElement[] ElementCollection
    {
        get
        {
            return Options;
        }
    }

    private IEnumerable<Profile> FilteredProfiles
    {
        get
        {
            IEnumerable<KeyValuePair<string, Profile>> profiles = ProfileManager.AvailableProfiles;

            string filter = FilterButton.FilterText.ToLowerInvariant();
            if (!string.IsNullOrEmpty(filter))
            {
                profiles = profiles.Where((x) => x.Key.ToLowerInvariant().Contains(filter));
            }

            return profiles.OrderBy((x) => -(int)(x.Value.Operation)).ThenBy((y) => y.Key).Select((z) => z.Value);
        }
    }

    private Profile[] _availableProfiles = null;
    private ProfileSettingsPage _settingsPage = null;

    protected override void Awake()
    {
        base.Awake();

        _settingsPage = Page.GetPageWithComponent(SettingsPagePrefab);

        for (int optionIndex = 0; optionIndex < Options.Length; ++optionIndex)
        {
            int localOptionIndex = optionIndex;

            Options[optionIndex].InteractAction.AddListener(delegate()
            {
                _settingsPage.Profile = _availableProfiles[ValueOffset + localOptionIndex];
                Page.GoToPage(SettingsPagePrefab);
            });
        }

        FilterButton.FilterTextChange.AddListener(FilterTextChange);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        FilterButton.FilterTextChange.RemoveListener(FilterTextChange);
    }

    private void OnEnable()
    {
        _availableProfiles = FilteredProfiles.ToArray();
        SetPage(0);
    }

    public override void SetPage(int pageIndex, int pageOffset = 0)
    {
        if (_availableProfiles == null)
        {
            return;
        }

        base.SetPage(pageIndex, pageOffset);

        Page.HeaderText = string.Format("<b>Select Profile To Edit...</b>\n<size=16>{0}</size>", PageName);
    }

    protected override void SetElement(Profile value, UIElement element)
    {
        element.BackgroundHighlight.UnselectedColor = value.Operation.GetColor();
        element.Text = value.FriendlyName;
    }

    private void FilterTextChange(string filterText)
    {
        _availableProfiles = FilteredProfiles.ToArray();
        SetPage(0);
    }
}
