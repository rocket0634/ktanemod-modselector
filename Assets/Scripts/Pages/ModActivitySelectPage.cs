using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ModActivitySelectPage : MonoBehaviour
{ 
    public ModActivityInfoPage InfoPagePrefab = null;

    public UIElement PreviousButton = null;
    public UIElement NextButton = null;

    public UIElement[] Options = null;

    public Color EnabledColor = Color.white;
    public Color DisabledColor = Color.white;

    private int TotalPageCount
    {
        get
        {
            return ((_mods.Length - 1) / Options.Length) + 1;
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

    private KeyValuePair<string, string>[] _mods = null;
    private int _pageIndex = 0;
    private Page _page = null;

    private void Awake()
    {
        _page = GetComponent<Page>();

        for (int optionIndex = 0; optionIndex < Options.Length; ++optionIndex)
        {
            int localOptionIndex = optionIndex;

            KMSelectable selectable = Options[optionIndex].GetComponent<KMSelectable>();
            selectable.OnInteract += delegate ()
            {
                _page.GetPageWithComponent(InfoPagePrefab).ModNameAndDisplayName = _mods[OptionOffset + localOptionIndex];
                _page.GoToPage(InfoPagePrefab);
                return true;
            };
        }
    }

    private void OnEnable()
    {
        if (ModSelectorService.Instance == null)
        {
            return;
        }

        //Detailed information is not automatically updated all the time - do it here now
        ProfileManager.UpdateProfileSelectionDetails();

        _mods = ModSelectorService.Instance.GetAllModNamesAndDisplayNames().OrderBy((x) => ProfileManager.ActiveDisableSet.Contains(x.Key)).ThenBy((y) => y.Value.Replace("The ", "")).ToArray();

        SetPage(0);
    }

    public void SetPage(int pageIndex)
    {
        if (_mods == null)
        {
            return;
        }

        _pageIndex = Mathf.Clamp(pageIndex, 0, TotalPageCount - 1);

        for (int optionIndex = 0; optionIndex < Options.Length; ++optionIndex)
        {
            UIElement option = Options[optionIndex];
            int trueOptionIndex = OptionOffset + optionIndex;

            if (trueOptionIndex < _mods.Length)
            {
                option.gameObject.SetActive(true);
                option.Text = _mods[trueOptionIndex].Value;
                option.BackgroundHighlight.UnselectedColor = ProfileManager.ActiveDisableSet.Contains(_mods[trueOptionIndex].Key) ? DisabledColor : EnabledColor;
            }
            else
            {
                option.gameObject.SetActive(false);
            }
        }

        _page.HeaderText = string.Format("<b>Enabled/Disabled Mods</b>\n<size=16>Page {0} of {1}</size>", _pageIndex + 1, TotalPageCount);

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
