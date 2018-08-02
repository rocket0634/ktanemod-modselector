using System.Linq;
using UnityEngine;

public class ProfileSelectPage : MonoBehaviour
{
    public ProfileSettingsPage SettingsPagePrefab = null;

    public UIElement PreviousButton = null;
    public UIElement NextButton = null;

    public UIElement[] Options = null;

    private int TotalPageCount
    {
        get
        {
            return ((_availableProfiles.Length - 1) / Options.Length) + 1;
        }
    }

    private int OptionOffset
    {
        get
        {
            return _pageIndex * Options.Length;
        }
    }

    private bool PreviousEnabled
    {
        get
        {
            return _pageIndex > 0;
        }
    }

    private bool NextEnabled
    {
        get
        {
            return _pageIndex < TotalPageCount - 1;
        }
    }

    private Profile[] _availableProfiles = null;
    private int _pageIndex = 0;
    private Page _page = null;
    private ProfileSettingsPage _settingsPage = null;

    private void Awake()
    {
        _page = GetComponent<Page>();

        _settingsPage = _page.GetPageWithComponent(SettingsPagePrefab);

        for (int optionIndex = 0; optionIndex < Options.Length; ++optionIndex)
        {
            int localOptionIndex = optionIndex;

            Options[optionIndex].InteractAction.AddListener(delegate()
            {
                _settingsPage.Profile = _availableProfiles[OptionOffset + localOptionIndex];
                _page.GoToPage(SettingsPagePrefab);
            });
        }
    }

    private void OnEnable()
    {
        _availableProfiles = ProfileManager.AvailableProfiles.OrderBy((x) => -(int)(x.Value.Operation)).ThenBy((y) => y.Key).Select((z) => z.Value).ToArray();

        SetPage(0);
    }

    public void SetPage(int pageIndex)
    {
        if (_availableProfiles == null)
        {
            return;
        }

        _pageIndex = Mathf.Clamp(pageIndex, 0, TotalPageCount - 1);

        for (int optionIndex = 0; optionIndex < Options.Length; ++optionIndex)
        {
            UIElement option = Options[optionIndex];

            int trueOptionIndex = OptionOffset + optionIndex;

            if (trueOptionIndex < _availableProfiles.Length)
            {
                option.gameObject.SetActive(true);

                Profile profile = _availableProfiles[trueOptionIndex];
                option.BackgroundHighlight.UnselectedColor = profile.Operation.GetColor();
                option.Text = profile.FriendlyName;
            }
            else
            {
                option.gameObject.SetActive(false);
            }
        }

        _page.HeaderText = string.Format("<b>Select Profile To Edit...</b>\n<size=16>Page {0} of {1}</size>", _pageIndex + 1, TotalPageCount);

        if (PreviousButton != null)
        {
            PreviousButton.CanSelect = PreviousEnabled;
        }

        if (NextButton != null)
        {
            NextButton.CanSelect = NextEnabled;
        }
    }

    public void NextPage()
    {
        SetPage(_pageIndex + 1);
    }

    public void PreviousPage()
    {
        SetPage(_pageIndex - 1);
    }
}
