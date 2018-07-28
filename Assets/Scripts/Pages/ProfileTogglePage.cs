using System.Linq;
using UnityEngine;

public class ProfileTogglePage : MonoBehaviour
{
    public UIElement PreviousButton = null;
    public UIElement NextButton = null;

    public Texture2D CheckTexture = null;

    private int TotalPageCount
    {
        get
        {
            return ((_availableProfiles.Length - 1) / _toggles.Length) + 1;
        }
    }

    private int ToggleOffset
    {
        get
        {
            return _pageIndex * _toggles.Length;
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

    public Profile[] _availableProfiles = null;
    private UIToggle[] _toggles = null;
    private Page _page = null;
    private int _pageIndex = 0;

    private void Awake()
    {
        _toggles = GetComponentsInChildren<UIToggle>(true);
        for (int toggleIndex = 0; toggleIndex < _toggles.Length; ++toggleIndex)
        {
            int localToggleIndex = toggleIndex;

            _toggles[toggleIndex].OnToggleChange += delegate (bool toggled)
            {
                if (toggled)
                {
                    Profile.ActiveProfiles.Add(_availableProfiles[ToggleOffset + localToggleIndex]);
                    Profile.UpdateProfileSelection(true);
                }
                else
                {
                    Profile.ActiveProfiles.Remove(_availableProfiles[ToggleOffset + localToggleIndex]);
                    Profile.UpdateProfileSelection(true);
                }
            };
        }

        _page = GetComponent<Page>();
    }

    private void OnEnable()
    {
        _availableProfiles = Profile.AvailableProfiles.OrderBy((x) => -(int)(x.Value.Operation)).ThenBy((y) => y.Key).Select((z) => z.Value).ToArray();

        SetPage(0);
    }

    public void SetPage(int pageIndex)
    {
        if (_toggles == null)
        {
            return;
        }

        _pageIndex = Mathf.Clamp(pageIndex, 0, TotalPageCount - 1);

        for (int toggleIndex = 0; toggleIndex < _toggles.Length; ++toggleIndex)
        {
            int trueToggleIndex = ToggleOffset + toggleIndex;

            UIToggle toggle = _toggles[toggleIndex];

            if (trueToggleIndex < _availableProfiles.Length)
            {
                Profile profile = _availableProfiles[trueToggleIndex];

                toggle.gameObject.SetActive(true);
                toggle.IsOn = Profile.ActiveProfiles.Contains(profile);

                toggle.BackgroundHighlight.UnselectedColor = profile.Operation.GetColor();
                toggle.Text = _availableProfiles[trueToggleIndex].FriendlyName;
                toggle.Icon = CheckTexture;
            }
            else
            {
                toggle.gameObject.SetActive(false);
            }
        }

        _page.HeaderText = string.Format("<b>Select Active Profiles</b>\n<size=16>Page {0} of {1}</size>", _pageIndex + 1, TotalPageCount);

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

    public void DisableAll()
    {
        Profile.ActiveProfiles.Clear();
        Profile.UpdateProfileSelection(true);
        SetPage(_pageIndex);
    }
}
