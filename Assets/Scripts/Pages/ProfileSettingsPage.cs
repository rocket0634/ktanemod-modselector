using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

public class ProfileSettingsPage : MonoBehaviour
{
    public ModTogglePage ModTogglePagePrefab = null;
    public ProfileRenamePage RenamePagePrefab = null;
    public ProfileConfirmDeletePage ConfirmDeletePagePrefab = null;

    public Profile Profile = null;
    public UIElement RegularModulesSelectable = null;
    public UIElement NeedyModulesSelectable = null;
    public UIElement ServicesSelectable = null;
    public UIElement BombCasingsSelectable = null;
    public UIElement GameplayRoomsSelectable = null;
    public UIElement WidgetsSelectable = null;
    public UIElement SetOperationSelectable = null;

    private Page _page = null;

    private void Awake()
    {
        _page = GetComponent<Page>();
    }

    public void OnEnable()
    {
        _page.HeaderText = string.Format("<b>{0}</b>\n<size=16>Profile Settings</size>", Profile == null ? "**NULL**" : Profile.FriendlyName);

        RegularModulesSelectable.Text = GetSelectableString(ModSelectorService.ModType.SolvableModule);
        NeedyModulesSelectable.Text = GetSelectableString(ModSelectorService.ModType.NeedyModule);
        ServicesSelectable.Text = GetSelectableString(ModSelectorService.ModType.Service);
        BombCasingsSelectable.Text = GetSelectableString(ModSelectorService.ModType.Bomb);
        GameplayRoomsSelectable.Text = GetSelectableString(ModSelectorService.ModType.GameplayRoom);
        WidgetsSelectable.Text = GetSelectableString(ModSelectorService.ModType.Widget);

        SetOperationSelectable.Text = GetSetOperationString();
        SetOperationSelectable.BackgroundHighlight.UnselectedColor = Profile != null ? Profile.Operation.GetColor() : Color.white;
    }

    private void SortModulesNames(KeyValuePair<string, string>[] names)
    {
        System.Array.Sort(names, (x, y) => string.Compare(x.Value.Replace("The ", ""), y.Value.Replace("The ", "")));
    }

    public void EditSolvableModules()
    {
        ModTogglePage modTogglePage = _page.GetPageWithComponent(ModTogglePagePrefab);
        modTogglePage.Profile = Profile;
        modTogglePage.ModType = ModSelectorService.ModType.SolvableModule;
        modTogglePage.Entries = ModSelectorService.Instance.AllSolvableModules.Select((x) => new KeyValuePair<string, string>(x.ModuleType, x.ModuleName)).ToArray();

        SortModulesNames(modTogglePage.Entries);
        modTogglePage.SetPage(0);
        _page.GoToPage(ModTogglePagePrefab);
    }

    public void EditNeedyModules()
    {
        ModTogglePage modTogglePage = _page.GetPageWithComponent(ModTogglePagePrefab);
        modTogglePage.Profile = Profile;
        modTogglePage.ModType = ModSelectorService.ModType.NeedyModule;
        modTogglePage.Entries = ModSelectorService.Instance.AllNeedyModules.Select((x) => new KeyValuePair<string, string>(x.ModuleType, x.ModuleName)).ToArray();

        SortModulesNames(modTogglePage.Entries);
        modTogglePage.SetPage(0);
        _page.GoToPage(ModTogglePagePrefab);
    }

    public void EditBombs()
    {
        ModTogglePage modTogglePage = _page.GetPageWithComponent(ModTogglePagePrefab);
        modTogglePage.Profile = Profile;
        modTogglePage.ModType = ModSelectorService.ModType.Bomb;
        modTogglePage.Entries = ModSelectorService.Instance.AllBombMods.Select((x) => new KeyValuePair<string, string>(x.name, x.name)).ToArray();

        SortModulesNames(modTogglePage.Entries);
        modTogglePage.SetPage(0);
        _page.GoToPage(ModTogglePagePrefab);
    }

    public void EditGameplayRooms()
    {
        ModTogglePage modTogglePage = _page.GetPageWithComponent(ModTogglePagePrefab);
        modTogglePage.Profile = Profile;
        modTogglePage.ModType = ModSelectorService.ModType.GameplayRoom;
        modTogglePage.Entries = ModSelectorService.Instance.AllGameplayRoomMods.Select((x) => new KeyValuePair<string, string>(x.name, x.name)).ToArray();

        SortModulesNames(modTogglePage.Entries);
        modTogglePage.SetPage(0);
        _page.GoToPage(ModTogglePagePrefab);
    }

    public void EditWidgets()
    {
        ModTogglePage modTogglePage = _page.GetPageWithComponent(ModTogglePagePrefab);
        modTogglePage.Profile = Profile;
        modTogglePage.ModType = ModSelectorService.ModType.Widget;
        modTogglePage.Entries = ModSelectorService.Instance.AllWidgetMods.Select((x) => new KeyValuePair<string, string>(x.name, x.name)).ToArray();

        SortModulesNames(modTogglePage.Entries);
        modTogglePage.SetPage(0);
        _page.GoToPage(ModTogglePagePrefab);
    }

    public void EditServices()
    {
        ModTogglePage modTogglePage = _page.GetPageWithComponent(ModTogglePagePrefab);
        modTogglePage.Profile = Profile;
        modTogglePage.ModType = ModSelectorService.ModType.Service;
        modTogglePage.Entries = ModSelectorService.Instance.AllServices.Select((x) => new KeyValuePair<string, string>(x.ServiceName, x.ServiceName)).ToArray();

        SortModulesNames(modTogglePage.Entries);
        modTogglePage.SetPage(0);
        _page.GoToPage(ModTogglePagePrefab);
    }

    public void ToggleSetOperation()
    {
        switch (Profile.Operation)
        {
            case Profile.SetOperation.Intersect:
                Profile.SetSetOperation(Profile.SetOperation.Union);
                break;
            case Profile.SetOperation.Union:
                Profile.SetSetOperation(Profile.SetOperation.Intersect);
                break;

            default:
                break;
        }



        SetOperationSelectable.BackgroundHighlight.UnselectedColor = Profile.Operation.GetColor();
        SetOperationSelectable.Text = GetSetOperationString();
    }

    public void Rename()
    {
        _page.GetPageWithComponent(RenamePagePrefab).Profile = Profile;
        _page.GoToPage(RenamePagePrefab);
    }

    public void Delete()
    {
        _page.GetPageWithComponent(ConfirmDeletePagePrefab).Profile = Profile;
        _page.GoToPage(ConfirmDeletePagePrefab);
    }

    private string GetSelectableString(ModSelectorService.ModType modType)
    {
        if (Profile != null)
        {
            int total = Profile.GetTotalOfType(modType);
            int disabledTotal = Profile.GetDisabledTotalOfType(modType);

            return string.Format("{0} <i>({1} of {2} disabled)</i>", modType.GetAttributeOfType<DescriptionAttribute>().Description, disabledTotal, total);
        }

        return "**NULL** <i>(0 of 0 enabled)</i>";
    }

    private string GetSetOperationString()
    {
        if (Profile != null)
        {
            string explanation = null;
            switch (Profile.Operation)
            {
                case Profile.SetOperation.Intersect:
                    explanation = "<b>Intersect</b> <i>(will disable elements if other active profiles disable also)</i>";
                    break;
                case Profile.SetOperation.Union:
                    explanation = "<b>Union</b> <i>(will always disable elements)</i>";
                    break;

                default:
                    break;
            }

            return string.Format("Set Operation: {0}", explanation);
        }

        return "Set Operation: <b>**NULL**</b>";
    }
}
