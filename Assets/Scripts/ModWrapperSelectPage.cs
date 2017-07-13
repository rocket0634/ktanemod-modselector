using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TabletPage))]
public class ModWrapperSelectPage : MonoBehaviour
{
    public ModWrapperInfoPage infoPage = null;

    public TabletSelectableDisableable previousButton = null;
    public TabletSelectableDisableable nextButton = null;

    private int TotalPageCount
    {
        get
        {
            return ((_modWrappers.Length - 1) / _options.Length) + 1;
        }
    }

    private int OptionOffset
    {
        get
        {
            return _pageIndex * _options.Length;
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

    private ModSelectorService.ModWrapper[] _modWrappers = null;
    private TabletSelectable[] _options = null;
    private int _pageIndex = 0;
    private TabletPage _tabletPage = null;

    private void Awake()
    {
        _tabletPage = GetComponent<TabletPage>();
        _options = _tabletPage.GridSelectables;

        for (int optionIndex = 0; optionIndex < _options.Length; ++optionIndex)
        {
            int localOptionIndex = optionIndex;

            KMSelectable selectable = _options[optionIndex].GetComponent<KMSelectable>();
            selectable.OnInteract += delegate ()
            {
                infoPage.modWrapper = _modWrappers[OptionOffset + localOptionIndex];
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

        _modWrappers = ModSelectorService.Instance.GetModWrappers().ToArray();
        System.Array.Sort(_modWrappers, (x, y) => string.Compare(x.ModTitle.Replace("The ", ""), y.ModTitle.Replace("The ", "")));

        SetPage(0);
    }

    public void SetPage(int pageIndex)
    {
        if (_modWrappers == null)
        {
            return;
        }

        _pageIndex = Mathf.Clamp(pageIndex, 0, TotalPageCount - 1);

        for (int optionIndex = 0; optionIndex < _options.Length; ++optionIndex)
        {
            int trueOptionIndex = OptionOffset + optionIndex;

            if (trueOptionIndex < _modWrappers.Length)
            {
                _options[optionIndex].transform.parent.gameObject.SetActive(true);

                if (_options[optionIndex].textMesh != null)
                {
                    _options[optionIndex].textMesh.text = _modWrappers[trueOptionIndex].ModTitle;
                }
            }
            else
            {
                _options[optionIndex].transform.parent.gameObject.SetActive(false);
            }
        }

        _tabletPage.header.text = string.Format("<b>Select Mod</b>\n<size=16>Page {0} of {1}</size>", _pageIndex + 1, TotalPageCount);

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
}
