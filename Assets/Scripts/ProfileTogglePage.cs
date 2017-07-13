using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TabletPage))]
public class ProfileTogglePage : MonoBehaviour
{
    public TabletSelectableDisableable previousButton = null;
    public TabletSelectableDisableable nextButton = null;

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
    private TabletSelectableToggle[] _toggles = null;
    private TabletPage _tabletPage = null;
    private int _pageIndex = 0;

    private void Awake()
    {
        _toggles = GetComponentsInChildren<TabletSelectableToggle>(true);
        for (int toggleIndex = 0; toggleIndex < _toggles.Length; ++toggleIndex)
        {
            int localToggleIndex = toggleIndex;

            if (_toggles[toggleIndex].onToggleEvent == null)
            {
                _toggles[toggleIndex].onToggleEvent = new ToggleEvent();
            }

            _toggles[toggleIndex].onToggleEvent.AddListener(delegate (bool toggled)
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
            });
        }

        _tabletPage = GetComponent<TabletPage>();
    }

    private void OnEnable()
    {
        _availableProfiles = Profile.AvailableProfiles.OrderBy((x) => x.Value.Operation).ThenBy((y) => y.Key).Select((z) => z.Value).ToArray();

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

            TabletSelectableToggle toggle = _toggles[toggleIndex];

            if (trueToggleIndex < _availableProfiles.Length)
            {
                Profile profile = _availableProfiles[trueToggleIndex];

                toggle.transform.parent.gameObject.SetActive(true);
                toggle.toggleState = Profile.ActiveProfiles.Contains(profile);

                TabletSelectable tabletSelectable = toggle.GetComponent<TabletSelectable>();
                tabletSelectable.deselectedColor = profile.Operation.GetColor();

                if (tabletSelectable.textMesh != null)
                {                   
                    tabletSelectable.textMesh.text = _availableProfiles[trueToggleIndex].FriendlyName;
                }
            }
            else
            {
                toggle.transform.parent.gameObject.SetActive(false);
            }
        }

        _tabletPage.header.text = string.Format("<b>Select Active Profiles</b>\n<size=16>Page {0} of {1}</size>", _pageIndex + 1, TotalPageCount);

        if (previousButton != null)
        {
            previousButton.SetEnable(PreviousEnabled);
        }

        if (nextButton != null)
        {
            nextButton.SetEnable(NextEnabled);
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
