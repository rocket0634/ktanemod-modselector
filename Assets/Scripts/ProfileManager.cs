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
                    ActiveDisableSet = new HashSet<string>();

                    if (andSave)
                    {
                        SaveActiveConfiguration();
                    }

                    return;
                }

                Profile[] expertProfiles = ActiveProfiles.Where((x) => x.Operation == Profile.SetOperation.Expert).ToArray();
                Profile[] defuserProfilers = ActiveProfiles.Where((x) => x.Operation == Profile.SetOperation.Defuser).ToArray();

                //Get all separate lists
                HashSet<string>[] allExpertEnabled = expertProfiles.Select((x) => x.EnabledList).ToArray();
                HashSet<string>[] allExpertDisabled = expertProfiles.Select((x) => x.DisabledList).ToArray();
                HashSet<string>[] allDefuserEnabled = defuserProfilers.Select((x) => x.EnabledList).ToArray();
                HashSet<string>[] allDefuserDisabled = defuserProfilers.Select((x) => x.DisabledList).ToArray();

                //Get expert "common" list for enabled & disabled (where all experting profiles agree enabled/disabled)
                HashSet<string> expertCommonEnabled = allExpertEnabled.Length > 0 ? allExpertEnabled.Skip(1).Aggregate(
                    new HashSet<string>(allExpertEnabled.First()),
                    (h, e) => { h.IntersectWith(e); return h; }
                ) : new HashSet<string>();
                HashSet<string> expertCommonDisabled = allExpertDisabled.Length > 0 ? allExpertDisabled.Skip(1).Aggregate(
                    new HashSet<string>(allExpertDisabled.First()),
                    (h, e) => { h.IntersectWith(e); return h; }
                ) : new HashSet<string>();

                //Get defusing "any" list for enabled & disabled (where any defusing profile mentions enabled/disabled)
                HashSet<string> defuserAnyEnabled = new HashSet<string>(allDefuserEnabled.SelectMany((x) => x).Distinct());
                HashSet<string> defuserAnyDisabled = new HashSet<string>(allDefuserDisabled.SelectMany((x) => x).Distinct());

                //Do the merge to generate the final merge list  
                ActiveDisableSet = new HashSet<string>(defuserAnyDisabled.Union(expertCommonDisabled.Except(expertCommonEnabled)).Except(defuserAnyEnabled));

                foreach (string modObjectName in ActiveDisableSet)
                {
                    Debug.LogFormat("Disabling {0}.", modObjectName);
                    ModSelectorService.Instance.Disable(modObjectName);
                }

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
        ActiveProfilesEntries.Clear();
        foreach (Profile profile in ActiveProfiles)
        {
            foreach (string modName in profile.EnabledList)
            {
                List<ProfileEntry> entries = null;
                if (!ActiveProfilesEntries.TryGetValue(modName, out entries))
                {
                    entries = new List<ProfileEntry>();
                    ActiveProfilesEntries[modName] = entries;
                }

                entries.Add(new ProfileEntry() { ProfileName = profile.FriendlyName, SetOperation = profile.Operation, EnableFlag = Profile.EnableFlag.ForceEnabled });
            }

            foreach (string modName in profile.DisabledList)
            {
                List<ProfileEntry> entries = null;
                if (!ActiveProfilesEntries.TryGetValue(modName, out entries))
                {
                    entries = new List<ProfileEntry>();
                    ActiveProfilesEntries[modName] = entries;
                }

                entries.Add(new ProfileEntry() { ProfileName = profile.FriendlyName, SetOperation = profile.Operation, EnableFlag = Profile.EnableFlag.ForceDisabled });
            }
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
