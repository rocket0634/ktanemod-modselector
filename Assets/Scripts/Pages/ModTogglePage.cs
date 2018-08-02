using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ModTogglePage : MonoBehaviour
{
    public Profile Profile = null;
    public ModSelectorService.ModType ModType = ModSelectorService.ModType.Unknown;
    public KeyValuePair<string, string>[] Entries = null;

    public UIElement PreviousButton = null;
    public UIElement NextButton = null;

    public Texture2D AllowImage = null;
    public Texture2D DenyImage = null;

    private int TotalPageCount
    {
        get
        {
            if (Entries == null)
            {
                return 1;
            }

            return ((Entries.Length - 1) / _toggles.Length) + 1;
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

    private UIToggle[] _toggles = null;
    private int _pageIndex = 0;
    private Page _page = null;

    private void Awake()
    {
        _toggles = GetComponentsInChildren<UIToggle>(true);
        for(int toggleIndex = 0; toggleIndex < _toggles.Length; ++toggleIndex)
        {
            int localToggleIndex = toggleIndex;
            _toggles[toggleIndex].OnToggleChange += delegate (bool toggled)
            {
                string entry = Entries[ToggleOffset + localToggleIndex].Key;
                switch (Profile.GetEnabledFlag(entry))
                {
                    case Profile.EnableFlag.ForceEnabled:
                        Profile.ForceDisable(entry);
                        _toggles[localToggleIndex].Icon = DenyImage;
                        _toggles[localToggleIndex].IsOn = true;
                        break;
                    case Profile.EnableFlag.ForceDisabled:
                        Profile.Clear(entry);
                        break;
                    case Profile.EnableFlag.Enabled:
                        Profile.ForceEnable(entry);
                        _toggles[localToggleIndex].Icon = AllowImage;
                        _toggles[localToggleIndex].IsOn = true;
                        break;

                    default:
                        break;
                }
            };
        }

        _page = GetComponent<Page>();
    }

    private void OnEnable()
    {
        SetPage(0);
    }

    public void SetPage(int pageIndex)
    {
        if (Entries == null || Profile == null || _toggles == null)
        {
            return;
        }

        _pageIndex = Mathf.Clamp(pageIndex, 0, TotalPageCount - 1);

        for (int toggleIndex = 0; toggleIndex < _toggles.Length; ++toggleIndex)
        {
            int trueToggleIndex = ToggleOffset + toggleIndex;

            UIToggle toggle = _toggles[toggleIndex];

            if (trueToggleIndex < Entries.Length)
            {
                toggle.gameObject.SetActive(true);

                switch (Profile.GetEnabledFlag(Entries[trueToggleIndex].Key))
                {
                    case Profile.EnableFlag.ForceEnabled:
                        toggle.Icon = AllowImage;
                        toggle.IsOn = true;
                        break;
                    case Profile.EnableFlag.ForceDisabled:
                        toggle.Icon = DenyImage;
                        toggle.IsOn = true;
                        break;
                    case Profile.EnableFlag.Enabled:
                        toggle.IsOn = false;
                        break;

                    default:
                        break;
                }

                toggle.Text = Entries[trueToggleIndex].Value;
            }
            else
            {
                toggle.gameObject.SetActive(false);
            }
        }

        _page.HeaderText = string.Format("<b>{0}</b>\n<size=16>{1}, page {2} of {3}</size>", Profile.FriendlyName, ModType.GetAttributeOfType<DescriptionAttribute>().Description, _pageIndex + 1, TotalPageCount);

        if (PreviousButton != null)
        {
            PreviousButton.CanSelect = PreviousEnabled;
        }

        if (NextButton != null)
        {
            NextButton.CanSelect = NextEnabled;
        }
    }

    public void ClearAll()
    {
        Profile.ClearAllOfType(ModType);
        SetPage(_pageIndex);
    }

    public void EnableAll()
    {
        Profile.ForceEnableAllOfType(ModType);
        SetPage(_pageIndex);
    }

    public void DisableAll()
    {
        Profile.ForceDisableAllOfType(ModType);
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
