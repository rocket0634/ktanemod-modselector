using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TabletPage))]
public class ProfileSelectDeletePage : MonoBehaviour
{
    public ProfileConfirmDeletePage confirmDeletePage = null;

    private int TotalPageCount
    {
        get
        {
            return ((_availableProfiles.Length - 1) / _options.Length) + 1;
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

    private Profile[] _availableProfiles = null;
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
                confirmDeletePage.profile = _availableProfiles[OptionOffset + localOptionIndex];
                return true;
            };
        }
    }

    private void OnEnable()
    {
        _availableProfiles = Profile.AvailableProfiles.OrderBy((x) => x.Value.Operation).ThenBy((y) => y.Key).Select((z) => z.Value).ToArray();

        SetPage(0);
    }

    public void SetPage(int pageIndex)
    {
        if (_availableProfiles == null)
        {
            return;
        }

        _pageIndex = Mathf.Clamp(pageIndex, 0, TotalPageCount - 1);

        for (int optionIndex = 0; optionIndex < _options.Length; ++optionIndex)
        {
            int trueOptionIndex = OptionOffset + optionIndex;

            if (trueOptionIndex < _availableProfiles.Length)
            {
                _options[optionIndex].transform.parent.gameObject.SetActive(true);

                Profile profile = _availableProfiles[trueOptionIndex];
                _options[optionIndex].deselectedColor = profile.Operation.GetColor();

                if (_options[optionIndex].textMesh != null)
                {
                    _options[optionIndex].textMesh.text = profile.Name;
                }
            }
            else
            {
                _options[optionIndex].transform.parent.gameObject.SetActive(false);
            }
        }

        _tabletPage.header.text = string.Format("<b>Select Profile To Delete...</b>\n<size=16>Page {0} of {1}</size>", _pageIndex + 1, TotalPageCount);
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
