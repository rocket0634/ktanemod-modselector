using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Profile
{
    public enum SetOperation
    {
        [ColorAttribute(1.0f, 0.95f, 0.85f, 1.0f)]
        Expert,
        [ColorAttribute(0.9f, 1.0f, 0.85f, 1.0f)]
        Defuser,
    }

    public enum EnableFlag
    {
        Enabled,
        Disabled
    }

    [Serializable]
    public class NewJSON
    {
        public List<string> DisabledList = new List<string>();
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> EnabledList;
        public SetOperation Operation = SetOperation.Expert;
    }

    public Profile(string filename, bool createNew = false)
    {
        DisabledList = new HashSet<string>();
        Filename = filename;

        if (createNew)
        {
            Save();
        }
        else
        {
            Reload();
        }
    }

    #region Constants/Readonly
    public static readonly string ProfileDirectory = Path.Combine(Application.persistentDataPath, "ModProfiles");
    public static readonly string Extension = ".json";
    public static readonly string DefaultPath = Path.Combine(Application.persistentDataPath, "disabledMods.json");
    #endregion

    #region Public Fields/Properties
    public string Name
    {
        get
        {
            return Path.GetFileNameWithoutExtension(Filename);
        }
    }

    public string FriendlyName
    {
        get
        {
            //Currently only replaces underscores with spaces
            return Name.Replace('_', ' ');
        }
    }

    public string Filename
    {
        get;
        private set;
    }

    public string FullPath
    {
        get
        {
            return Path.Combine(ProfileDirectory, Filename);
        }
    }

    public HashSet<string> DisabledList
    {
        get;
        private set;
    }

    public HashSet<string> EnabledList
    {
        get;
        private set;
    }

    public SetOperation Operation
    {
        get;
        private set;
    }
    #endregion

    #region Public Methods
    public void SetSetOperation(SetOperation operation, bool andSave = true)
    {
        Operation = operation;
        ProfileManager.UpdateProfileSelection();

        if (andSave)
        {
            Save();
        }
    }

    public EnableFlag GetEnabledFlag(string modObjectName)
    {
        if (DisabledList.Contains(modObjectName))
        {
            return EnableFlag.Disabled;
        }

        return EnableFlag.Enabled;
    }

    public void EnableAll(bool save = true)
    {
        DisabledList.Clear();
        ProfileManager.UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void DisableAll(bool save = true)
    {
        DisabledList.UnionWith(ModSelectorService.Instance.GetAllModNames());
        ProfileManager.UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void EnableAllOfType(ModSelectorService.ModType modType, bool save = true)
    {
        string[] modNames = ModSelectorService.Instance.GetModNames(modType).ToArray();
        DisabledList.ExceptWith(modNames);
        ProfileManager.UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void DisableAllOfType(ModSelectorService.ModType modType, bool save = true)
    {
        string[] modNames = ModSelectorService.Instance.GetModNames(modType).ToArray();
        DisabledList.UnionWith(modNames);
        ProfileManager.UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void Enable(string modObjectName, bool save = true)
    {
        if (GetEnabledFlag(modObjectName) != EnableFlag.Enabled)
        {
            DisabledList.Remove(modObjectName);

            ProfileManager.UpdateProfileSelection();

            if (save)
            {
                Save();
            }
        }
    }

    public void Disable(string modObjectName, bool save = true)
    {
        if (GetEnabledFlag(modObjectName) != EnableFlag.Disabled)
        {
            DisabledList.Add(modObjectName);

            ProfileManager.UpdateProfileSelection();

            if (save)
            {
                Save();
            }
        }
    }

    public static int GetTotalOfType(ModSelectorService.ModType modType)
    {
        return ModSelectorService.Instance.GetModNames(modType).Count();
    }

    public int GetTotalOfType(ModSelectorService.ModType modType, EnableFlag enableFlag)
    {
        IEnumerable<string> modNames = ModSelectorService.Instance.GetModNames(modType);

        switch (enableFlag)
        {
            case EnableFlag.Disabled:
                return DisabledList.Intersect(modNames).Count();

            case EnableFlag.Enabled:
                return modNames.Except(DisabledList).Count();

            default:
                return modNames.Except(DisabledList).Count();
        }
    }

    public int GetDisabledTotalOfType(ModSelectorService.ModType modType)
    {
        return DisabledList.Intersect(ModSelectorService.Instance.GetModNames(modType)).Count();
    }

    public void Reload()
    {
        try
        {
            EnsureProfileDirectory();

            string jsonInput = File.ReadAllText(FullPath);

            try
            {
                NewJSON newJSON = JsonConvert.DeserializeObject<NewJSON>(jsonInput);
                if (newJSON != null)
                {
                    DisabledList = new HashSet<string>(newJSON.DisabledList);
                    if (newJSON.EnabledList != null)
                    {
                        EnabledList = new HashSet<string>(newJSON.EnabledList);
                    }
                    Operation = newJSON.Operation;
                    UpdateExpertProfile();
                    return;
                }
            }
            catch (Exception ex2)
            {
                try
                {
                    DisabledList = new HashSet<string>(JsonConvert.DeserializeObject<List<string>>(jsonInput));
                    Operation = SetOperation.Expert;
                    UpdateExpertProfile();
                }
                catch (Exception ex3)
                {
                    Debug.LogException(ex3);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void Save()
    {
        try
        {
            EnsureProfileDirectory();

            NewJSON jsonStructure = new NewJSON() { DisabledList = new List<string>(DisabledList), Operation = Operation };

            if (Operation == SetOperation.Expert)
            {
                if (EnabledList == null)
                {
                    EnabledList = new HashSet<string>();
                }
                foreach (string name in ModSelectorService.Instance.GetModNames(ModSelectorService.ModType.SolvableModule)
                    .Concat(ModSelectorService.Instance.GetModNames(ModSelectorService.ModType.NeedyModule))
                    .Concat(ModSelectorService.Instance.GetModNames(ModSelectorService.ModType.Widget)))
                {
                    if (DisabledList.Contains(name))
                    {
                        EnabledList.Remove(name);
                    }
                    else
                    {
                        EnabledList.Add(name);
                    }
                }
                jsonStructure.EnabledList = new List<string>(EnabledList);
            }

            string jsonOutput = JsonConvert.SerializeObject(jsonStructure, Formatting.Indented);
            File.WriteAllText(FullPath, jsonOutput);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void Rename(string newName)
    {
        try
        {
            EnsureProfileDirectory();

            string oldPath = FullPath;
            string oldName = Name;

            Filename = string.Format("{0}{1}", newName, Extension);

            ProfileManager.AvailableProfiles.Remove(oldName);
            ProfileManager.AvailableProfiles[Name] = this;

            File.Move(oldPath, FullPath);

            ProfileManager.UpdateProfileSelection(true);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public Profile Copy(string newName)
    {
        Profile newProfile = new Profile(string.Format("{0}{1}", newName, Extension), true);
        newProfile.DisabledList = new HashSet<string>(DisabledList);
        newProfile.Operation = Operation;
        newProfile.Save();

        return newProfile;
    }

    public void Delete()
    {
        try
        {
            EnsureProfileDirectory();

            ProfileManager.AvailableProfiles.Remove(Name);
            ProfileManager.ActiveProfiles.Remove(this);

            File.Delete(FullPath);

            ProfileManager.UpdateProfileSelection(true);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void UpdateExpertProfile()
    {
        if (Operation == SetOperation.Expert)
        {
            if (EnabledList != null && EnabledList.Count > 0)
            {
                int oldCount = DisabledList.Count;
                DisabledList.UnionWith(ModSelectorService.Instance.GetModNames(ModSelectorService.ModType.SolvableModule)
                    .Concat(ModSelectorService.Instance.GetModNames(ModSelectorService.ModType.NeedyModule))
                    .Concat(ModSelectorService.Instance.GetModNames(ModSelectorService.ModType.Widget))
                    .Where(s => !EnabledList.Contains(s)));
                if (DisabledList.Count != oldCount)
                {
                    Save();
                }
            }
            else
            {
                Save();
            }
        }
    }

    private static void EnsureProfileDirectory()
    {
        try
        {
            Directory.CreateDirectory(ProfileDirectory);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion
}
