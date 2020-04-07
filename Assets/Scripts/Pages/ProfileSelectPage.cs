using System.Linq;

public class ProfileSelectPage : Pagination<Profile, UIElement>
{
    public ProfileSettingsPage SettingsPagePrefab = null;

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
    }

    private void OnEnable()
    {
        _availableProfiles = ProfileManager.AvailableProfiles.OrderBy((x) => -(int)(x.Value.Operation)).ThenBy((y) => y.Key).Select((z) => z.Value).ToArray();

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
}
