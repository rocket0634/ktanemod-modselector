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
        Intersect,
        [ColorAttribute(0.9f, 1.0f, 0.85f, 1.0f)]
        Union
    }

    [Serializable]
    public class NewJSON
    {
        public List<string> DisabledList = new List<string>();
        public SetOperation Operation = SetOperation.Intersect;
    }

    static Profile()
    {
        RefreshFiles();
    }

    private Profile(string filename, bool createNew = false)
    {
        DisabledList = new List<string>();
        Filename = filename;

        if (createNew)
        {
            Create();
        }
        else
        {
            Reload();
        }
    }

    #region Constants/Readonly
    public static readonly string ProfileDirectory = Path.Combine(Application.persistentDataPath, "ModProfiles");

    private static readonly string Extension = ".json";
    private static readonly string DefaultPath = Path.Combine(Application.persistentDataPath, "disabledMods.json");
    private static readonly string ActiveConfiguration = Path.Combine(Application.persistentDataPath, "modSelectorConfig.json");
    #endregion

    #region Public Fields/Properties
    public static readonly List<Profile> ActiveProfiles = new List<Profile>();

    public static readonly Dictionary<string, Profile> AvailableProfiles = new Dictionary<string, Profile>();

    public string Name
    {
        get
        {
            return Path.GetFileNameWithoutExtension(Filename);
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

    public List<string> DisabledList
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
    public static bool CanCreateProfile(string profileName)
    {
        EnsureProfileDirectory();

        string filename = string.Format("{0}{1}", profileName, Extension);
        return !File.Exists(Path.Combine(ProfileDirectory, filename));
    }

    public static Profile CreateProfile(string profileName)
    {
        return new Profile(string.Format("{0}{1}", profileName, Extension), true);
    }

    public static void RefreshFiles()
    {
        EnsureProfileDirectory();

        if (File.Exists(DefaultPath))
        {
            File.Move(DefaultPath, Path.Combine(ProfileDirectory, "Default.json"));
        }

        string[] files = Directory.GetFiles(ProfileDirectory);
        foreach (string file in files)
        {
            string extension = Path.GetExtension(file);
            if (!extension.Equals(Extension))
            {
                continue;
            }

            string profileName = Path.GetFileNameWithoutExtension(file);
            if (!AvailableProfiles.ContainsKey(profileName))
            {
                AvailableProfiles[profileName] = new Profile(file);
            }

            Path.GetFileNameWithoutExtension(file);
        }
    }

    public static void UpdateProfileSelection(bool andSave = false)
    {
        Debug.Log("Updating mod selector disable list...");

        ModSelectorService.Instance.EnableAll();

        if (ActiveProfiles.Count == 0)
        {
            return;
        }

        HashSet<string> profileMergeSet = new HashSet<string>();
        Profile[] intersects = ActiveProfiles.Where((x) => x.Operation == SetOperation.Intersect).ToArray();
        if (intersects.Length > 0)
        {
            profileMergeSet.UnionWith(intersects[0].DisabledList);
            for (int intersectProfileIndex = 1; intersectProfileIndex < intersects.Length; ++intersectProfileIndex)
            {
                profileMergeSet.IntersectWith(intersects[intersectProfileIndex].DisabledList);
            }
        }

        foreach (Profile union in ActiveProfiles.Where((x) => x.Operation == SetOperation.Union))
        {
            profileMergeSet.UnionWith(union.DisabledList);
        }

        foreach(string modObjectName in profileMergeSet)
        {
            Debug.LogFormat("Disabling {0}.", modObjectName);
            ModSelectorService.Instance.Disable(modObjectName);
        }

        if (andSave)
        {
            SaveActiveConfiguration();
        }
    }

    public static void ReloadActiveConfiguration()
    {
        try
        {
            string jsonInput = File.ReadAllText(ActiveConfiguration);
            List<string> activeProfileNames = JsonConvert.DeserializeObject<List<string>>(jsonInput);
            ActiveProfiles.Clear();

            foreach (string profileName in activeProfileNames)
            {
                ActiveProfiles.Add(AvailableProfiles[profileName]);
            }

            UpdateProfileSelection();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public static void SaveActiveConfiguration()
    {
        try
        {
            string jsonOutput = JsonConvert.SerializeObject(ActiveProfiles.Select((x) => x.Name).ToArray());
            File.WriteAllText(ActiveConfiguration, jsonOutput);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void SetSetOperation(SetOperation operation, bool andSave = true)
    {
        Operation = operation;
        UpdateProfileSelection();

        if (andSave)
        {
            Save();
        }
    }

    public bool IsEnabled(string modObjectName)
    {
        return !DisabledList.Contains(modObjectName);
    }

    public void EnableAll(bool save = true)
    {
        DisabledList.Clear();
        UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void DisableAll(bool save = true)
    {
        DisabledList.Clear();
        UpdateProfileSelection();

        ModSelectorService modSelector = ModSelectorService.Instance;
        DisabledList.AddRange(modSelector.GetModNames(ModSelectorService.ModType.SolvableModule));
        DisabledList.AddRange(modSelector.GetModNames(ModSelectorService.ModType.NeedyModule));
        DisabledList.AddRange(modSelector.GetModNames(ModSelectorService.ModType.Bomb));
        DisabledList.AddRange(modSelector.GetModNames(ModSelectorService.ModType.GameplayRoom));
        DisabledList.AddRange(modSelector.GetModNames(ModSelectorService.ModType.Widget));
        DisabledList.AddRange(modSelector.GetModNames(ModSelectorService.ModType.Service));

        if (save)
        {
            Save();
        }
    }

    public void EnableAllOfType(ModSelectorService.ModType modType, bool save = true)
    {
        DisabledList = DisabledList.Except(ModSelectorService.Instance.GetModNames(modType)).ToList();
        UpdateProfileSelection();
        
        if (save)
        {
            Save();
        }
    }

    public void DisableAllOfType(ModSelectorService.ModType modType, bool save = true)
    {
        DisabledList = DisabledList.Union(ModSelectorService.Instance.GetModNames(modType)).ToList();
        UpdateProfileSelection();

        if (save)
        {
            Save();
        }
    }

    public void Enable(string modObjectName, bool save = true)
    {
        if (!IsEnabled(modObjectName))
        {
            DisabledList.Remove(modObjectName);
            UpdateProfileSelection();

            if (save)
            {
                Save();
            }
        }
    }

    public void Disable(string modObjectName, bool save = true)
    {
        if (IsEnabled(modObjectName))
        {
            DisabledList.Add(modObjectName);
            UpdateProfileSelection();

            if (save)
            {
                Save();
            }
        }
    }

    public int GetTotalOfType(ModSelectorService.ModType modType)
    {
        return ModSelectorService.Instance.GetModNames(modType).Count();
    }

    public int GetDisabledTotalOfType(ModSelectorService.ModType modType)
    {
        return DisabledList.Intersect(ModSelectorService.Instance.GetModNames(modType)).Count();
    }

    public void Create()
    {
        Save();
        AvailableProfiles[Name] = this;
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
                    DisabledList = newJSON.DisabledList;
                    Operation = newJSON.Operation;
                    return;
                }
            }
            catch (Exception ex2)
            {
                try
                {
                    DisabledList = JsonConvert.DeserializeObject<List<string>>(jsonInput);
                    Operation = SetOperation.Intersect;
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

            NewJSON jsonStructure = new NewJSON() { DisabledList = DisabledList, Operation = Operation };

            string jsonOutput = JsonConvert.SerializeObject(jsonStructure);
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

            string oldName = Name;

            string newFilename = string.Format("{0}{1}", newName, Extension);
            File.Move(FullPath, Path.Combine(ProfileDirectory, newFilename));

            Filename = newFilename;

            AvailableProfiles.Remove(oldName);
            AvailableProfiles[Name] = this;

            UpdateProfileSelection(true);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void Delete()
    {
        try
        {
            EnsureProfileDirectory();

            File.Delete(FullPath);

            AvailableProfiles.Remove(Name);
            ActiveProfiles.Remove(this);

            UpdateProfileSelection(true);
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
