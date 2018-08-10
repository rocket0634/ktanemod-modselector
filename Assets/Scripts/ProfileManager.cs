using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

public static class ProfileManager
{
    public class ProfileEntry
    {
        public string ProfileName = null;
        public Profile.SetOperation SetOperation = Profile.SetOperation.Expert;
        public Profile.EnableFlag EnableFlag = Profile.EnableFlag.Enabled;
    }

    static ProfileManager()
    {
        RefreshFiles(false);

        _profilesDirectoryWatcher = new FileSystemWatcher();
        _profilesDirectoryWatcher.Path = Profile.ProfileDirectory;
        _profilesDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
        _profilesDirectoryWatcher.Filter = "*.*";

        _profilesDirectoryWatcher.Created += OnFileCreated;
        _profilesDirectoryWatcher.Changed += OnFileChanged;
        _profilesDirectoryWatcher.Deleted += OnFileDeleted;
        _profilesDirectoryWatcher.Renamed += OnFileRenamed;

        _profilesDirectoryWatcher.EnableRaisingEvents = true;
    }

    private static void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        string extension = Path.GetExtension(e.FullPath);
        if (!extension.Equals(Profile.Extension))
        {
            return;
        }

        string profileName = Path.GetFileNameWithoutExtension(e.FullPath);
        if (!AvailableProfiles.ContainsKey(profileName))
        {
            AvailableProfiles[profileName] = new Profile(e.FullPath);
        }

        ReloadActiveConfiguration();
    }

    private static void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        string profileName = Path.GetFileNameWithoutExtension(e.FullPath);

        Profile profile = null;
        if (AvailableProfiles.TryGetValue(profileName, out profile))
        {
            profile.Reload();
        }

        ReloadActiveConfiguration();
    }

    private static void OnFileDeleted(object sender, FileSystemEventArgs e)
    {
        string profileName = Path.GetFileNameWithoutExtension(e.FullPath);
        AvailableProfiles.Remove(profileName);

        ReloadActiveConfiguration();
    }

    private static void OnFileRenamed(object sender, RenamedEventArgs e)
    {
        string oldProfileName = Path.GetFileNameWithoutExtension(e.FullPath);
        AvailableProfiles.Remove(oldProfileName);

        string profileName = Path.GetFileNameWithoutExtension(e.FullPath);
        if (!AvailableProfiles.ContainsKey(profileName))
        {
            AvailableProfiles[profileName] = new Profile(e.FullPath);
        }

        ReloadActiveConfiguration();
    }

    #region Constants/Readonly
    public static readonly string ActiveConfiguration = Path.Combine(Application.persistentDataPath, "modSelectorConfig.json");
    #endregion

    #region Public Fields/Properties
    public static readonly List<Profile> ActiveProfiles = new List<Profile>();
    public static readonly Dictionary<string, List<ProfileEntry>> ActiveProfilesEntries = new Dictionary<string, List<ProfileEntry>>();

    public static HashSet<string> ActiveDisableSet
    {
        get;
        private set;
    }

    public static readonly Dictionary<string, Profile> AvailableProfiles = new Dictionary<string, Profile>();
    #endregion

    #region Private Fields
    private static FileSystemWatcher _profilesDirectoryWatcher = null;
    private static int _updateInActionCount = 0;
    #endregion

    #region Public Methods
    public static bool CanCreateProfile(string profileName)
    {
        EnsureProfileDirectory();

        string filename = string.Format("{0}{1}", profileName, Profile.Extension);
        return !File.Exists(Path.Combine(Profile.ProfileDirectory, filename));
    }

    public static Profile CreateProfile(string profileName)
    {
        Profile profile = new Profile(string.Format("{0}{1}", profileName, Profile.Extension), true);
        AvailableProfiles[profile.Name] = profile;
        return profile;
    }

    public static void RefreshFiles(bool andReloadActiveConfiguration = true)
    {
        EnsureProfileDirectory();

        if (File.Exists(Profile.DefaultPath))
        {
            File.Move(Profile.DefaultPath, Path.Combine(Profile.ProfileDirectory, "Default.json"));
        }

        string[] files = Directory.GetFiles(Profile.ProfileDirectory);
        foreach (string file in files)
        {
            string extension = Path.GetExtension(file);
            if (!extension.Equals(Profile.Extension))
            {
                continue;
            }

            string profileName = Path.GetFileNameWithoutExtension(file);
            if (!AvailableProfiles.ContainsKey(profileName))
            {
                AvailableProfiles[profileName] = new Profile(file);
            }
        }

        ReloadActiveConfiguration();
    }

    public static void UpdateProfileSelection(bool andSave = false, bool andUpdateDetails = false)
    {
        //In case anything in here throws an exception for whatever reason, we'll need to get the interlock count back to 0!
        try
        {
            //Was initially just a lock, but it seems that was potentially deadlockable
            if (Interlocked.Increment(ref _updateInActionCount) == 1)
            {
                ModSelectorService.Instance.EnableAll();

                if (ActiveProfiles.Count == 0)
                {
                    if (andSave)
                    {
                        SaveActiveConfiguration();
                    }

                    ActiveDisableSet = new HashSet<string>();
                    return;
                }

                HashSet<string> profileMergeSet = new HashSet<string>();
                Profile[] intersects = ActiveProfiles.Where((x) => x.Operation == Profile.SetOperation.Expert).ToArray();
                if (intersects.Length > 0)
                {
                    profileMergeSet.UnionWith(intersects[0].DisabledList);
                    for (int intersectProfileIndex = 1; intersectProfileIndex < intersects.Length; ++intersectProfileIndex)
                    {
                        profileMergeSet.IntersectWith(intersects[intersectProfileIndex].DisabledList);
                    }
                }

                foreach (Profile union in ActiveProfiles.Where((x) => x.Operation == Profile.SetOperation.Defuser))
                {
                    profileMergeSet.UnionWith(union.DisabledList);
                }

                foreach (string modObjectName in profileMergeSet)
                {
                    Debug.LogFormat("Disabling {0}.", modObjectName);
                    ModSelectorService.Instance.Disable(modObjectName);
                }

                ActiveDisableSet = profileMergeSet;

                if (andSave)
                {
                    SaveActiveConfiguration();
                }
            }
        }
        finally
        {
            Interlocked.Decrement(ref _updateInActionCount);
        }

        if (andUpdateDetails)
        {
            UpdateProfileSelectionDetails();
        }
    }

    public static void UpdateProfileSelectionDetails()
    {
        string[] modNames = ModSelectorService.Instance.GetAllModNames().ToArray();

        ActiveProfilesEntries.Clear();

        foreach (string modName in modNames)
        {
            List<ProfileEntry> entries = new List<ProfileEntry>();
            ActiveProfilesEntries[modName] = entries;

            foreach (Profile profile in ActiveProfiles)
            {
                Profile.EnableFlag enableFlag;
                if (profile.DisabledList.Contains(modName))
                {
                    enableFlag = Profile.EnableFlag.Disabled;
                }
                else
                {
                    enableFlag = Profile.EnableFlag.Enabled;
                }

                entries.Add(new ProfileEntry() { ProfileName = profile.FriendlyName, SetOperation = profile.Operation, EnableFlag = enableFlag });
            }
        }
    }

    public static IEnumerable<string> GetActiveDisableList(ModSelectorService.ModType modType)
    {
        return ActiveDisableSet.Intersect(ModSelectorService.Instance.GetModNames(modType));
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
                Profile profile;
                if (AvailableProfiles.TryGetValue(profileName, out profile))
                {
                    ActiveProfiles.Add(profile);
                }
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
            string jsonOutput = JsonConvert.SerializeObject(ActiveProfiles.Select((x) => x.Name).ToArray(), Formatting.Indented);
            File.WriteAllText(ActiveConfiguration, jsonOutput);
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
            Directory.CreateDirectory(Profile.ProfileDirectory);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion
}
