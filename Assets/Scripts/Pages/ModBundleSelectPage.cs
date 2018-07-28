using System.Linq;
using UnityEngine;

public class ModBundleSelectPage : MonoBehaviour
{
    public ModBundleInfoPage InfoPagePrefab = null;

    public UIElement PreviousButton = null;
    public UIElement NextButton = null;

    public UIElement[] Options = null;

    private int TotalPageCount
    {
        get
        {
            return ((_modWrappers.Length - 1) / Options.Length) + 1;
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

    private ModSelectorService.ModWrapper[] _modWrappers = null;
    private int _pageIndex = 0;
    private Page _page = null;

    private void Awake()
    {
        _page = GetComponent<Page>();

        for (int optionIndex = 0; optionIndex < Options.Length; ++optionIndex)
        {
            int localOptionIndex = optionIndex;

            KMSelectable selectable = Options[optionIndex].GetComponent<KMSelectable>();
            selectable.OnInteract += delegate()
            {
                _page.GetPageWithComponent(InfoPagePrefab).ModWrapper = _modWrappers[OptionOffset + localOptionIndex];
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

        for (int optionIndex = 0; optionIndex < Options.Length; ++optionIndex)
        {
            UIElement option = Options[optionIndex];
            int trueOptionIndex = OptionOffset + optionIndex;

            if (trueOptionIndex < _modWrappers.Length)
            {
                option.gameObject.SetActive(true);
                option.Text = _modWrappers[trueOptionIndex].ModTitle;
            }
            else
            {
                option.gameObject.SetActive(false);
            }
        }

        _page.HeaderText = string.Format("<b>Select Mod</b>\n<size=16>Page {0} of {1}</size>", _pageIndex + 1, TotalPageCount);

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
