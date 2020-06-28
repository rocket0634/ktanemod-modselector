using System.Collections.Generic;
using System.Linq;

public class ProfileTogglePage : Pagination<Profile, UIToggle>
{
    public FilterButton FilterButton = null;

    protected override Profile[] ValueCollection
    {
        get
        {
            return _availableProfiles;
        }
    }

    protected override UIToggle[] ElementCollection
    {
        get
        {
            return _toggles;
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

    private UIToggle[] _toggles = null;
    private Profile[] _availableProfiles = null;

    protected override void Awake()
    {
        base.Awake();

        _toggles = GetComponentsInChildren<UIToggle>(true);
        for (int toggleIndex = 0; toggleIndex < _toggles.Length; ++toggleIndex)
        {
            int localToggleIndex = toggleIndex;

            _toggles[toggleIndex].OnToggleChange += delegate (bool toggled)
            {
                if (toggled)
                {
                    ProfileManager.ActiveProfiles.Add(_availableProfiles[ValueOffset + localToggleIndex]);
                    ProfileManager.UpdateProfileSelection(true);
                }
                else
                {
                    ProfileManager.ActiveProfiles.Remove(_availableProfiles[ValueOffset + localToggleIndex]);
                    ProfileManager.UpdateProfileSelection(true);
                }
            };
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
        if (_toggles == null)
        {
            return;
        }

        base.SetPage(pageIndex, pageOffset);

        Page.HeaderText = string.Format("<b>Select Active Profiles</b>\n<size=16>{0}</size>", PageName);
    }

    public void DisableAll()
    {
        ProfileManager.ActiveProfiles.Clear();
        ProfileManager.UpdateProfileSelection(true);
        SetPage(PageIndex);
    }

    protected override void SetElement(Profile value, UIToggle element)
    {
        element.IsOn = ProfileManager.ActiveProfiles.Contains(value);
        element.BackgroundHighlight.UnselectedColor = value.Operation.GetColor();
        element.Text = value.FriendlyName;
    }

    private void FilterTextChange(string filterText)
    {
        _availableProfiles = FilteredProfiles.ToArray();
        SetPage(0);
    }
}
