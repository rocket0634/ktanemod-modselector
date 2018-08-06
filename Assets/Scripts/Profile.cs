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
        ForceEnabled,
        Enabled,
        ForceDisabled
    }

    [Serializable]
    public class NewJSON
    {
        public List<string> EnabledList = new List<string>();
        public List<string> DisabledList = new List<string>();
        public SetOperation Operation = SetOperation.Expert;
    }    

    public Profile(string filename, bool createNew = false)
    {
        EnabledList = new HashSet<string>();
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

    public HashSet<string> EnabledList
    {
        get;
        private set;
    }

    public HashSet<string> DisabledList
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
        if (EnabledList.Contains(modObjectName))
        {
            return EnableFlag.ForceEnabled;
        }
        if (DisabledList.Contains(modObjectName))
        {
            return EnableFlag.ForceDisabled;
        }

        return EnableFlag.Enabled;
    }

    public void ClearAll(bool save = true)
    {
        EnabledList.Clear();
        DisabledList.Clear();

        ProfileManager.UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void ForceEnableAll(bool save = true)
    {
        DisabledList.Clear();
        EnabledList.UnionWith(ModSelectorService.Instance.GetAllModNames());
        ProfileManager.UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void ForceDisableAll(bool save = true)
    {
        EnabledList.Clear();
        DisabledList.UnionWith(ModSelectorService.Instance.GetAllModNames());

        ProfileManager.UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void ClearAllOfType(ModSelectorService.ModType modType, bool save = true)
    {
        string[] modNames = ModSelectorService.Instance.GetModNames(modType).ToArray();

        EnabledList.ExceptWith(modNames);
        DisabledList.ExceptWith(modNames);

        ProfileManager.UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void ForceEnableAllOfType(ModSelectorService.ModType modType, bool save = true)
    {
        string[] modNames = ModSelectorService.Instance.GetModNames(modType).ToArray();

        EnabledList.UnionWith(modNames);
        DisabledList.ExceptWith(modNames);

        ProfileManager.UpdateProfileSelection();
        
        if (save)
        {
            Save();
        }
    }

    public void ForceDisableAllOfType(ModSelectorService.ModType modType, bool save = true)
    {
        string[] modNames = ModSelectorService.Instance.GetModNames(modType).ToArray();

        DisabledList.UnionWith(modNames);
        EnabledList.ExceptWith(modNames);

        ProfileManager.UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void Clear(string modObjectName, bool save = true)
    {
        if (GetEnabledFlag(modObjectName) != EnableFlag.Enabled)
        {
            DisabledList.Remove(modObjectName);
            EnabledList.Remove(modObjectName);

            ProfileManager.UpdateProfileSelection();

            if (save)
            {
                Save();
            }
        }
    }

    public void ForceEnable(string modObjectName, bool save = true)
    {
        if (GetEnabledFlag(modObjectName) != EnableFlag.ForceEnabled)
        {
            EnabledList.Add(modObjectName);
            DisabledList.Remove(modObjectName);

            ProfileManager.UpdateProfileSelection();

            if (save)
            {
                Save();
            }
        }
    }

    public void ForceDisable(string modObjectName, bool save = true)
    {
        if (GetEnabledFlag(modObjectName) != EnableFlag.ForceDisabled)
        {
            DisabledList.Add(modObjectName);
            EnabledList.Remove(modObjectName);

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
            case EnableFlag.ForceEnabled:
                return EnabledList.Intersect(modNames).Count();

            case EnableFlag.ForceDisabled:
                return DisabledList.Intersect(modNames).Count();

            case EnableFlag.Enabled:
                return modNames.Except(EnabledList.Union(DisabledList)).Count();

            default:
                return modNames.Except(EnabledList.Union(DisabledList)).Count();
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
                    EnabledList = new HashSet<string>(newJSON.EnabledList);
                    DisabledList = new HashSet<string>(newJSON.DisabledList);
                    Operation = newJSON.Operation;
                    return;
                }
            }
            catch (Exception ex2)
            {
                try
                {
                    DisabledList = new HashSet<string>(JsonConvert.DeserializeObject<List<string>>(jsonInput));
                    Operation = SetOperation.Expert;
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

            NewJSON jsonStructure = new NewJSON() { EnabledList = new List<string>(EnabledList), DisabledList = new List<string>(DisabledList), Operation = Operation };

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
        newProfile.EnabledList = new HashSet<string>(EnabledList);
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
