using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[RequireComponent(typeof(TabletPage))]
public class ModTogglePage : MonoBehaviour
{
    public Profile profile = null;
    public ModSelectorService.ModType modType = ModSelectorService.ModType.Unknown;
    public KeyValuePair<string, string>[] entries = null;

    public TabletSelectableDisableable previousButton = null;
    public TabletSelectableDisableable nextButton = null;

    private int TotalPageCount
    {
        get
        {
            if (entries == null)
            {
                return 1;
            }

            return ((entries.Length - 1) / _toggles.Length) + 1;
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

    private TabletSelectableToggle[] _toggles = null;
    private int _pageIndex = 0;
    private TabletPage _tabletPage = null;

    private void Awake()
    {
        _toggles = GetComponentsInChildren<TabletSelectableToggle>(true);
        for(int toggleIndex = 0; toggleIndex < _toggles.Length; ++toggleIndex)
        {
            int localToggleIndex = toggleIndex;
            _toggles[toggleIndex].onToggleEvent.AddListener(delegate (bool toggled)
            {
                if (toggled)
                {
                    profile.Disable(entries[ToggleOffset + localToggleIndex].Key);
                }
                else
                {
                    profile.Enable(entries[ToggleOffset + localToggleIndex].Key);
                }
            });
        }

        _tabletPage = GetComponent<TabletPage>();
    }

    private void OnEnable()
    {
        SetPage(0);
    }

    public void SetPage(int pageIndex)
    {
        if (entries == null || profile == null || _toggles == null)
        {
            return;
        }

        _pageIndex = Mathf.Clamp(pageIndex, 0, TotalPageCount - 1);

        for (int toggleIndex = 0; toggleIndex < _toggles.Length; ++toggleIndex)
        {
            int trueToggleIndex = ToggleOffset + toggleIndex;

            TabletSelectableToggle toggle = _toggles[toggleIndex];

            if (trueToggleIndex < entries.Length)
            {
                toggle.transform.parent.gameObject.SetActive(true);

                toggle.toggleState = !profile.IsEnabled(entries[trueToggleIndex].Key);

                if (toggle.GetComponent<TabletSelectable>().textMesh != null)
                {
                    toggle.GetComponent<TabletSelectable>().textMesh.text = entries[trueToggleIndex].Value;
                }
            }
            else
            {
                toggle.transform.parent.gameObject.SetActive(false);
            }
        }

        if (_tabletPage != null)
        {
            _tabletPage.header.text = string.Format("<b>{0}</b>\n<size=16>{1}, page {2} of {3}</size>", profile.Name, modType.GetAttributeOfType<DescriptionAttribute>().Description, _pageIndex + 1, TotalPageCount);
        }

        previousButton.SetEnable(PreviousEnabled);
        nextButton.SetEnable(NextEnabled);
    }

    public void EnableAll()
    {
        profile.EnableAllOfType(modType);
        SetPage(_pageIndex);
    }

    public void DisableAll()
    {
        profile.DisableAllOfType(modType);
        SetPage(_pageIndex);
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
