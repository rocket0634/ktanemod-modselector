using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class ModSelectorService : MonoBehaviour
{
    #region Nested Types
    public sealed class ModWrapper
    {
        public ModWrapper(object modObject)
        {
            Debug.Log(modObject);
            ModObject = modObject;

            ModName = (string)ModNameProperty.GetValue(ModObject, null);
            _activeModObjects = (List<GameObject>)ModObjectsProperty.GetValue(ModObject, null);
            _inactiveModObjects = new List<GameObject>();

            Debug.LogFormat("[ModSelector] Found mod '{0}', which contains the following {1} mod object(s):", ModName, _activeModObjects.Count);
            foreach (GameObject gameObject in _activeModObjects)
            {
                Debug.LogFormat("  {0}", gameObject.name);
            }
            Debug.Log(" ");
        }

        public GameObject GetModObject(string modObjectName)
        {
            foreach (GameObject modObject in _activeModObjects)
            {
                if (modObject.name.Equals(modObjectName))
                {
                    return modObject;
                }
            }

            foreach (GameObject modObject in _inactiveModObjects)
            {
                if (modObject.name.Equals(modObjectName))
                {
                    return modObject;
                }
            }

            return null;
        }

        public IEnumerable<KeyValuePair<GameObject, bool>> GetModObjects(Type modType)
        {
            foreach (GameObject modObject in _activeModObjects)
            {
                if (modObject.GetComponent(modType) != null)
                {
                    yield return new KeyValuePair<GameObject, bool>(modObject, true);
                }
            }

            foreach (GameObject modObject in _inactiveModObjects)
            {
                if (modObject.GetComponent(modType) != null)
                {
                    yield return new KeyValuePair<GameObject, bool>(modObject, false);
                }
            }
        }

        public bool EnableModObject(Type modType, string modObjectName)
        {
            KeyValuePair<GameObject, bool> modObject = GetModObjects(modType).Where((x) => x.Value == false).Where((x) => x.Key.name.Equals(modObjectName)).FirstOrDefault();
            if (modObject.Key != null)
            {
                _inactiveModObjects.Remove(modObject.Key);
                _activeModObjects.Add(modObject.Key);
                return true;
            }

            return false;
        }

        public bool DisableModObject(Type modType, string modObjectName)
        {
            KeyValuePair<GameObject, bool> modObject = GetModObjects(modType).Where((x) => x.Value == true).Where((x) => x.Key.name.Equals(modObjectName)).FirstOrDefault();
            if (modObject.Key != null)
            {
                _activeModObjects.Remove(modObject.Key);
                _inactiveModObjects.Add(modObject.Key);
                return true;
            }

            return false;
        }

        public void EnableModObjects(Type modType)
        {
            foreach (KeyValuePair<GameObject, bool> modObject in GetModObjects(modType).ToArray())
            {
                if (!modObject.Value)
                {
                    _inactiveModObjects.Remove(modObject.Key);
                    _activeModObjects.Add(modObject.Key);
                }
            }
        }

        public void DisableModObjects(Type modType)
        {
            foreach (KeyValuePair<GameObject, bool> modObject in GetModObjects(modType).ToArray())
            {
                if (!modObject.Value)
                {
                    _activeModObjects.Remove(modObject.Key);
                    _inactiveModObjects.Add(modObject.Key);
                }
            }
        }

        public readonly object ModObject;
        public readonly string ModName;

        private readonly List<GameObject> _activeModObjects;
        private readonly List<GameObject> _inactiveModObjects;

        private static Type _modType = null;
        private static Type ModType
        {
            get
            {
                if (_modType == null)
                {
                    _modType = ReflectionHelper.FindType("Mod");
                }

                return _modType;
            }
        }

        private static PropertyInfo _modNameProperty = null;
        private static PropertyInfo ModNameProperty
        {
            get
            {
                if (_modNameProperty == null)
                {
                    _modNameProperty = ModType.GetProperty("ModName", BindingFlags.Instance | BindingFlags.Public);
                }

                return _modNameProperty;
            }
        }

        private static PropertyInfo _modObjectsProperty = null;
        private static PropertyInfo ModObjectsProperty
        {
            get
            {
                if (_modObjectsProperty == null)
                {
                    _modObjectsProperty = ModType.GetProperty("ModObjects", BindingFlags.Instance | BindingFlags.Public);
                }

                return _modObjectsProperty;
            }
        }

        #region Mod Types
        private static Type _bombType = null;
        public static Type BombType
        {
            get
            {
                if (_bombType == null)
                {
                    _bombType = ReflectionHelper.FindType("ModBomb");
                }

                return _bombType;
            }
        }

        private static Type _widgetType = null;
        public static Type WidgetType
        {
            get
            {
                if (_widgetType == null)
                {
                    _widgetType = ReflectionHelper.FindType("ModWidget");
                }

                return _widgetType;
            }
        }

        private static Type _gameplayRoomType = null;
        public static Type GameplayRoomType
        {
            get
            {
                if (_gameplayRoomType == null)
                {
                    _gameplayRoomType = ReflectionHelper.FindType("ModGameplayRoom");
                }

                return _gameplayRoomType;
            }
        }
        #endregion
    }

    public interface Module
    {
        string ModuleName
        {
            get;
        }

        string ModuleType
        {
            get;
        }
    }

    public sealed class SolvableModule : Module
    {
        public SolvableModule(KMBombModule solvableBombModule, object component)
        {
            SolvableBombModule = solvableBombModule;
            Component = component;
        }

        public readonly KMBombModule SolvableBombModule;
        public readonly object Component;

        public string ModuleName
        {
            get
            {
                return SolvableBombModule.ModuleDisplayName;
            }
        }

        public string ModuleType
        {
            get
            {
                return SolvableBombModule.ModuleType;
            }
        }
    }

    public sealed class NeedyModule : Module
    {
        public NeedyModule(KMNeedyModule needyBombModule, object component)
        {
            NeedyBombModule = needyBombModule;
            Component = component;
        }

        public readonly KMNeedyModule NeedyBombModule;
        public readonly object Component;

        public string ModuleName
        {
            get
            {
                return NeedyBombModule.ModuleDisplayName;
            }
        }

        public string ModuleType
        {
            get
            {
                return NeedyBombModule.ModuleType;
            }
        }
    }

    public sealed class Service
    {
        public Service(KMService service)
        {
            ServiceObject = service.gameObject;
        }

        public readonly GameObject ServiceObject;

        public string ServiceName
        {
            get
            {
                return ServiceObject.name;
            }
        }

        public bool IsEnabled
        {
            get
            {
                return ServiceObject.activeSelf;
            }
            set
            {
                ServiceObject.SetActive(value);
            }
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        //For modules
        GetSolvableModules();
        GetNeedyModules();
        GetActiveModules();

        //For services
        GetModServices();

        //For all other mod types
        GetModList();

        LoadDefaults();

        PopulateModSelectorWindow();

        SetupLoadProfileWindow();
        SetupSaveProfileWindow();
        SetupDeleteProfileWindow();
    }
    #endregion

    #region Setup
    private void GetSolvableModules()
    {
        _allSolvableModules = new Dictionary<string, SolvableModule>();

        UnityEngine.Object modManager = ModManager;

        MethodInfo getSolvableBombModulesMethod = _modManagerType.GetMethod("GetSolvableBombModules", BindingFlags.Instance | BindingFlags.Public);
        IList solvableBombModuleList = getSolvableBombModulesMethod.Invoke(modManager, null) as IList;

        Type modBombComponentType = ReflectionHelper.FindType("ModBombComponent");
        FieldInfo moduleField = modBombComponentType.GetField("module", BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (object solvableBombModule in solvableBombModuleList)
        {
            KMBombModule module = moduleField.GetValue(solvableBombModule) as KMBombModule;
            string moduleTypeName = module.ModuleType;

            if (!_allSolvableModules.ContainsKey(moduleTypeName))
            {
                _allSolvableModules[moduleTypeName] = new SolvableModule(module, solvableBombModule);
            }
            else
            {
                Debug.LogErrorFormat("***** A duplicate regular/solvable module was found under the name {0}! *****", moduleTypeName);
            }
        }
    }

    private void GetNeedyModules()
    {
        _allNeedyModules = new Dictionary<string, NeedyModule>();

        UnityEngine.Object modManager = ModManager;

        MethodInfo getNeedyModulesMethod = _modManagerType.GetMethod("GetNeedyModules", BindingFlags.Instance | BindingFlags.Public);
        IList needyModuleList = getNeedyModulesMethod.Invoke(modManager, null) as IList;

        Type modNeedyComponentType = ReflectionHelper.FindType("ModNeedyComponent");
        FieldInfo moduleField = modNeedyComponentType.GetField("module", BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (object needyModule in needyModuleList)
        {
            KMNeedyModule module = moduleField.GetValue(needyModule) as KMNeedyModule;
            string moduleTypeName = module.ModuleType;

            if (!_allNeedyModules.ContainsKey(moduleTypeName))
            {
                _allNeedyModules[moduleTypeName] = new NeedyModule(module, needyModule);
            }
            else
            {
                Debug.LogErrorFormat("***** A duplicate needy module was found under the name {0}! *****", moduleTypeName);
            }
        }
    }

    private void GetActiveModules()
    {
        UnityEngine.Object modManager = ModManager;

        FieldInfo loadedBombComponentsField = _modManagerType.GetField("loadedBombComponents", BindingFlags.Instance | BindingFlags.NonPublic);
        _activeModules = loadedBombComponentsField.GetValue(modManager) as IDictionary;
    }

    private void GetModServices()
    {
        KMService[] modServices = FindObjectsOfType<KMService>();

        foreach (KMService modService in modServices)
        {
            ModSelectorService itself = modService.GetComponent<ModSelectorService>();
            if (itself != null)
            {
                //Don't add mod selector service/itself to this dictionary!
                continue;
            }

            Service service = new Service(modService);
            if (!_allServices.ContainsKey(service.ServiceName))
            {
                _allServices.Add(service.ServiceName, service);
            }
            else
            {
                Debug.LogErrorFormat("***** A duplicate service was found under the name {0}! *****", service.ServiceName);
            }
        }
    }

    private void GetModList()
    {
        UnityEngine.Object modManager = ModManager;

        FieldInfo modsField = _modManagerType.GetField("mods", BindingFlags.Instance | BindingFlags.NonPublic);
        IList modsList = (IList)modsField.GetValue(modManager);
        foreach (object modObject in modsList)
        {
            ModWrapper modWrapper = new ModWrapper(modObject);

            if (!_allMods.ContainsKey(modWrapper.ModName))
            {
                _allMods[modWrapper.ModName] = modWrapper;
            }
            else
            {
                Debug.LogErrorFormat("***** A duplicate mod was found under the name {0}! *****", modWrapper.ModName);
            }
        }
    }

    private void PopulateModSelectorWindow()
    {
        ModSelectorWindow window = GetComponentInChildren<ModSelectorWindow>(true);

        window.SetupService(this);
        window.SetupNormalModules(_allSolvableModules.Values.OrderBy((x) => x.SolvableBombModule.ModuleDisplayName));
        window.SetupNeedyModules(_allNeedyModules.Values.OrderBy((x) => x.NeedyBombModule.ModuleDisplayName));
        window.SetupServices(_allServices.Values.OrderBy((x) => x.ServiceName));
        window.SetupMods(window.bombGrid, _allMods.Values, ModWrapper.BombType);
        window.SetupMods(window.gameplayRoomGrid, _allMods.Values, ModWrapper.GameplayRoomType);
        window.SetupMods(window.widgetGrid, _allMods.Values, ModWrapper.WidgetType);
    }

    private void SetupLoadProfileWindow()
    {
        LoadProfileWindow window = GetComponentInChildren<LoadProfileWindow>(true);
        window.SetupService(this);
    }

    private void SetupSaveProfileWindow()
    {
        SaveProfileWindow window = GetComponentInChildren<SaveProfileWindow>(true);
        window.SetupService(this);
    }

    private void SetupDeleteProfileWindow()
    {
        DeleteProfileWindow window = GetComponentInChildren<DeleteProfileWindow>(true);
        window.SetupService(this);
    }
    #endregion

    #region Actions
    #region Mods
    public bool IsModActive(Type modType, string modName, string modObjectName)
    {
        if (_allMods.ContainsKey(modName))
        {
            return _allMods[modName].GetModObjects(modType).Where((x) => x.Key.name.Equals(modObjectName)).FirstOrDefault().Value;
        }

        Debug.LogErrorFormat("[ModSelector] Could not find a mod with name '{0}'.", modName);
        return false;
    }

    public bool EnableMod(Type modType, string modName, string modObjectName)
    {
        if (_allMods.ContainsKey(modName))
        {
            return _allMods[modName].EnableModObject(modType, modObjectName);
        }
        else
        {
            Debug.LogErrorFormat("[ModSelector] Cannot enable mod '{1}'; Could not find a mod with name '{0}'.", modName, modObjectName);
        }

        return false;
    }

    public bool DisableMod(Type modType, string modName, string modObjectName)
    {
        if (_allMods.ContainsKey(modName))
        {
            return _allMods[modName].DisableModObject(modType, modObjectName);
        }
        else
        {
            Debug.LogErrorFormat("[ModSelector] Cannot enable mod '{1}'; Could not find a mod with name '{0}'.", modName, modObjectName);
        }

        return false;
    }

    public bool EnableMod(Type modType, string modObjectName)
    {
        return _allMods.Values.Any((x) => x.EnableModObject(modType, modObjectName));
    }

    public bool DisableMod(Type modType, string modObjectName)
    {
        return _allMods.Values.Any((x) => x.DisableModObject(modType, modObjectName));
    }

    public void EnableAllMods()
    {
        EnableAllMods(ModWrapper.BombType);
        EnableAllMods(ModWrapper.WidgetType);
        EnableAllMods(ModWrapper.GameplayRoomType);
    }

    public void EnableAllMods(Type modType)
    {
        foreach (ModWrapper modWrapper in _allMods.Values)
        {
            modWrapper.EnableModObjects(modType);
        }
    }

    public void DisableAllMods(Type modType)
    {
        foreach (ModWrapper modWrapper in _allMods.Values)
        {
            modWrapper.DisableModObjects(modType);
        }
    }
    #endregion

    #region Modules
    public bool IsModuleActive(string typeName)
    {
        return _activeModules.Contains(typeName);
    }

    public bool EnableModule(string typeName)
    {
        if (_activeModules.Contains(typeName))
        {
            return false;
        }

        bool success = true;

        if (_allSolvableModules.ContainsKey(typeName))
        {
            _activeModules.Add(typeName, _allSolvableModules[typeName].Component);
        }
        else if (_allNeedyModules.ContainsKey(typeName))
        {
            _activeModules.Add(typeName, _allNeedyModules[typeName].Component);
        }
        else
        {
            Debug.LogErrorFormat("[ModSelector] Cannot enable module with type name '{0}'.", typeName);
            success = false;
        }

        _disabledModules.Remove(typeName);
        return success;
    }

    public bool DisableModule(string typeName)
    {
        if (!_activeModules.Contains(typeName))
        {
            return false;
        }

        _activeModules.Remove(typeName);
        _disabledModules.Add(typeName);
        return true;
    }

    public void EnableAllModules()
    {
        _activeModules.Clear();
        _disabledModules.Clear();

        foreach (KeyValuePair<string, SolvableModule> solvableModule in _allSolvableModules)
        {
            _activeModules.Add(solvableModule.Key, solvableModule.Value.Component);
        }

        foreach (KeyValuePair<string, NeedyModule> needyModule in _allNeedyModules)
        {
            _activeModules.Add(needyModule.Key, needyModule.Value.Component);
        }
    }

    public void DisableAllModules()
    {
        _activeModules.Clear();

        _disabledModules.Clear();
        _disabledModules.AddRange(_allSolvableModules.Keys);
        _disabledModules.AddRange(_allNeedyModules.Keys);
    }
    #endregion

    #region Services
    public bool IsServiceActive(string serviceName)
    {
        if (_allServices.ContainsKey(serviceName))
        {
            return _allServices[serviceName].IsEnabled;
        }

        return false;
    }

    public bool EnableService(string serviceName)
    {
        if (!_allServices.ContainsKey(serviceName))
        {
            return false;
        }

        _allServices[serviceName].IsEnabled = true;
        return true;
    }

    public bool DisableService(string serviceName)
    {
        if (!_allServices.ContainsKey(serviceName))
        {
            return false;
        }

        _allServices[serviceName].IsEnabled = false;
        return true;
    }

    public void EnableAllServices()
    {
        foreach (Service service in _allServices.Values)
        {
            service.IsEnabled = true;
        }
    }

    public void DisableAllServices()
    {
        foreach (Service service in _allServices.Values)
        {
            service.IsEnabled = false;
        }
    }
    #endregion

    #region File I/O
    private string ProfileDirectory
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "ModProfiles");
        }
    }

    private void EnsureProfileDirectory()
    {
        Directory.CreateDirectory(ProfileDirectory);
    }

    public IEnumerable<string> AvailableProfiles
    {
        get
        {
            EnsureProfileDirectory();
            string[] files = Directory.GetFiles(ProfileDirectory);
            foreach (string file in files)
            {
                Debug.Log("[ModSelector] Profile found: " + file);

                string extension = Path.GetExtension(file);
                if (!extension.Equals(".json"))
                {
                    continue;
                }

                yield return Path.GetFileNameWithoutExtension(file);
            }
        }
    }

    public void LoadDefaults()
    {
        LoadConfigurationFromFile(Path.Combine(Application.persistentDataPath, "disabledMods.json"));
    }

    public void SaveDefaults()
    {
        SaveConfigurationToFile(Path.Combine(Application.persistentDataPath, "disabledMods.json"));
    }

    public void LoadTemporary()
    {
        if (!string.IsNullOrEmpty(_tempFilename))
        {
            LoadConfigurationFromFile(_tempFilename);
        }
    }

    public void LoadProfile(string profileName)
    {
        EnsureProfileDirectory();
        LoadConfigurationFromFile(Path.Combine(ProfileDirectory, string.Format("{0}.json", profileName)));
    }

    public void SaveProfile(string profileName)
    {
        EnsureProfileDirectory();
        SaveConfigurationToFile(Path.Combine(ProfileDirectory, string.Format("{0}.json", profileName)));
        SaveDefaults();
    }

    public void DeleteProfile(string profileName)
    {
        EnsureProfileDirectory();
        DeleteConfigurationFromPath(Path.Combine(ProfileDirectory, string.Format("{0}.json", profileName)));
    }

    public void SaveTemporaryProfile()
    {
        _tempFilename = Path.GetTempFileName();
        SaveConfigurationToFile(_tempFilename);
    }

    public void LoadConfigurationFromFile(string path)
    {
        try
        {
            Debug.Log("[ModSelector] Loading configuration from file: " + path);

            //Ensure all mods are enabled first
            EnableAllModules();
            EnableAllServices();
            EnableAllMods();

            string jsonInput = File.ReadAllText(path);

            List<string> disabledMods = JsonConvert.DeserializeObject<List<string>>(jsonInput);
            foreach (string disabledMod in disabledMods)
            {
                if (DisableModule(disabledMod))
                {
                    continue;
                }

                if (DisableService(disabledMod))
                {
                    continue;
                }

                if (DisableMod(ModWrapper.BombType, disabledMod))
                {
                    continue;
                }

                if (DisableMod(ModWrapper.GameplayRoomType, disabledMod))
                {
                    continue;
                }

                if (DisableMod(ModWrapper.WidgetType, disabledMod))
                {
                    continue;
                }
            }

            SaveDefaults();
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogWarningFormat("[ModSelector] File {0} was not found.", path);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void SaveConfigurationToFile(string path)
    {
        try
        {
            Debug.Log("[ModSelector] Saving configuration to file: " + path);

            List<string> allDisabledMods = new List<string>();
            allDisabledMods.AddRange(_disabledModules);
            allDisabledMods.AddRange(_allServices.Values.Where((x) => !x.IsEnabled).Select((y) => y.ServiceName));
            allDisabledMods.AddRange(_allMods.Values.SelectMany((x) => x.GetModObjects(ModWrapper.BombType)).Where((y) => y.Value == false).Select((z) => z.Key.name));
            allDisabledMods.AddRange(_allMods.Values.SelectMany((x) => x.GetModObjects(ModWrapper.GameplayRoomType)).Where((y) => y.Value == false).Select((z) => z.Key.name));
            allDisabledMods.AddRange(_allMods.Values.SelectMany((x) => x.GetModObjects(ModWrapper.WidgetType)).Where((y) => y.Value == false).Select((z) => z.Key.name));

            string jsonOutput = JsonConvert.SerializeObject(allDisabledMods);
            File.WriteAllText(path, jsonOutput);
        }
        catch (FileNotFoundException ex)
        {
            Debug.LogWarningFormat("[ModSelector] File {0} was not found.", path);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void DeleteConfigurationFromPath(string path)
    {
        try
        {
            Debug.Log("[ModSelector] Deleting configuration file: " + path);

            File.Delete(path);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
    #endregion

    #region Emoji Prettiness
    public Sprite GetEmojiSprite(string moduleID)
    {
        for (int emojiIndex = 0; emojiIndex < emojiIDs.Length; ++emojiIndex)
        {
            if (emojiIDs[emojiIndex].Equals(moduleID))
            {
                return emojiSprites[emojiIndex];
            }
        }

        return null;
    }
    #endregion
    #endregion

    #region Public Fields
    public string[] emojiIDs = null;
    public Sprite[] emojiSprites = null;
    #endregion

    #region Private Fields & Properties
    #region Mods
    private Dictionary<string, ModWrapper> _allMods = new Dictionary<string, ModWrapper>();
    #endregion

    #region Modules
    private Dictionary<string, SolvableModule> _allSolvableModules = null;
    private Dictionary<string, NeedyModule> _allNeedyModules = null;

    private IDictionary _activeModules = null;
    private List<string> _disabledModules = new List<string>();
    #endregion

    #region Services
    private Dictionary<string, Service> _allServices = new Dictionary<string, Service>();
    #endregion

    #region Mod Manager Discovery
    private Type _modManagerType = null;

    private UnityEngine.Object _modManager = null;
    private UnityEngine.Object ModManager
    {
        get
        {
            if (_modManager == null)
            {
                _modManagerType = ReflectionHelper.FindType("ModManager");
                _modManager = FindObjectOfType(_modManagerType);
            }

            return _modManager;
        }
    }
    #endregion

    private string _tempFilename = null;
    #endregion
}
