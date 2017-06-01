using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(TabletPage))]
public class ProfileSettingsPage : MonoBehaviour
{
    public ModTogglePage togglePage = null;
    public ProfileRenamePage renamePage = null;
    public Profile profile = null;
    public TabletSelectable regularModulesSelectable = null;
    public TabletSelectable needyModulesSelectable = null;
    public TabletSelectable servicesSelectable = null;
    public TabletSelectable bombCasingsSelectable = null;
    public TabletSelectable gameplayRoomsSelectable = null;
    public TabletSelectable widgetsSelectable = null;
    public TabletSelectable setOperationSelectable = null;

    private TabletPage _tabletPage = null;

    private void Awake()
    {
        _tabletPage = GetComponent<TabletPage>();
    }

    public void OnEnable()
    {
        if (_tabletPage != null && _tabletPage.header != null)
        {
            _tabletPage.header.text = string.Format("<b>{0}</b>\n<size=16>Profile Settings</size>", profile == null ? "**NULL**" : profile.Name);
        }

        if (regularModulesSelectable.textMesh != null)
        {
            regularModulesSelectable.textMesh.text = GetSelectableString(ModSelectorService.ModType.SolvableModule);
        }

        if (needyModulesSelectable.textMesh != null)
        {
            needyModulesSelectable.textMesh.text = GetSelectableString(ModSelectorService.ModType.NeedyModule);
        }

        if (servicesSelectable.textMesh != null)
        {
            servicesSelectable.textMesh.text = GetSelectableString(ModSelectorService.ModType.Service);
        }

        if (bombCasingsSelectable.textMesh != null)
        {
            bombCasingsSelectable.textMesh.text = GetSelectableString(ModSelectorService.ModType.Bomb);
        }

        if (gameplayRoomsSelectable.textMesh != null)
        {
            gameplayRoomsSelectable.textMesh.text = GetSelectableString(ModSelectorService.ModType.GameplayRoom);
        }

        if (widgetsSelectable.textMesh != null)
        {
            widgetsSelectable.textMesh.text = GetSelectableString(ModSelectorService.ModType.Widget);
        }

        if (setOperationSelectable.textMesh != null)
        {
            setOperationSelectable.textMesh.text = GetSetOperationString();
            setOperationSelectable.deselectedColor = profile != null ? profile.Operation.GetColor() : Color.white;
        }
    }

    public void EditSolvableModules()
    {
        togglePage.profile = profile;
        togglePage.modType = ModSelectorService.ModType.SolvableModule;
        togglePage.entries = ModSelectorService.Instance.AllSolvableModules.Select((x) => new KeyValuePair<string, string>(x.ModuleType, x.ModuleName)).ToArray();
        togglePage.SetPage(0);
    }

    public void EditNeedyModules()
    {
        togglePage.profile = profile;
        togglePage.modType = ModSelectorService.ModType.NeedyModule;
        togglePage.entries = ModSelectorService.Instance.AllNeedyModules.Select((x) => new KeyValuePair<string, string>(x.ModuleType, x.ModuleName)).ToArray();
        togglePage.SetPage(0);
    }

    public void EditBombs()
    {
        togglePage.profile = profile;
        togglePage.modType = ModSelectorService.ModType.Bomb;
        togglePage.entries = ModSelectorService.Instance.AllBombMods.Select((x) => new KeyValuePair<string, string>(x.name, x.name)).ToArray();
        togglePage.SetPage(0);
    }

    public void EditGameplayRooms()
    {
        togglePage.profile = profile;
        togglePage.modType = ModSelectorService.ModType.GameplayRoom;
        togglePage.entries = ModSelectorService.Instance.AllGameplayRoomMods.Select((x) => new KeyValuePair<string, string>(x.name, x.name)).ToArray();
        togglePage.SetPage(0);
    }

    public void EditWidgets()
    {
        togglePage.profile = profile;
        togglePage.modType = ModSelectorService.ModType.Widget;
        togglePage.entries = ModSelectorService.Instance.AllWidgetMods.Select((x) => new KeyValuePair<string, string>(x.name, x.name)).ToArray();
        togglePage.SetPage(0);
    }

    public void EditServices()
    {
        togglePage.profile = profile;
        togglePage.modType = ModSelectorService.ModType.Service;
        togglePage.entries = ModSelectorService.Instance.AllServices.Select((x) => new KeyValuePair<string, string>(x.ServiceName, x.ServiceName)).ToArray();
        togglePage.SetPage(0);
    }

    public void ToggleSetOperation()
    {
        switch (profile.Operation)
        {
            case Profile.SetOperation.Intersect:
                profile.SetSetOperation(Profile.SetOperation.Union);
                break;
            case Profile.SetOperation.Union:
                profile.SetSetOperation(Profile.SetOperation.Intersect);
                break;

            default:
                break;
        }

        setOperationSelectable.deselectedColor = profile.Operation.GetColor();

        if (setOperationSelectable.textMesh != null)
        {
            setOperationSelectable.textMesh.text = GetSetOperationString();
        }
    }

    public void Rename()
    {
        renamePage.profile = profile;
    }

    private string GetSelectableString(ModSelectorService.ModType modType)
    {
        if (profile != null)
        {
            int total = profile.GetTotalOfType(modType);
            int disabledTotal = profile.GetDisabledTotalOfType(modType);

            return string.Format("{0} <i>({1} of {2} disabled)</i>", modType.GetAttributeOfType<DescriptionAttribute>().Description, disabledTotal, total);
        }

        return "**NULL** <i>(0 of 0 enabled)</i>";
    }

    private string GetSetOperationString()
    {
        if (profile != null)
        {
            string explanation = null;
            switch (profile.Operation)
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
