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
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        GetSolvableModules();
        GetNeedyModules();
        GetActiveModules();

        LoadDefaults();

        PopulateModSelectorWindow();
    }
    #endregion

    #region Setup
    private void GetSolvableModules()
    {
        _allSolvableModules = new Dictionary<string, SolvableModule>();

        UnityEngine.Object modManager = ModManager;

        MethodInfo getSolvableBombModulesMethod = _modManagerType.GetMethod("GetSolvableBombModules", BindingFlags.Instance | BindingFlags.Public);
        IList solvableBombModuleList = getSolvableBombModulesMethod.Invoke(modManager, null) as IList;

        //Type bombComponentType = ReflectionHelper.FindType("BombComponent");
        //MethodInfo getModuleDisplayNameMethod = bombComponentType.GetMethod("GetModuleDisplayName", BindingFlags.Instance | BindingFlags.Public);

        Type modBombComponentType = ReflectionHelper.FindType("ModBombComponent");
        FieldInfo moduleField = modBombComponentType.GetField("module", BindingFlags.Instance | BindingFlags.NonPublic);

        foreach (object solvableBombModule in solvableBombModuleList)
        {
            KMBombModule module = moduleField.GetValue(solvableBombModule) as KMBombModule;
            string moduleTypeName = module.ModuleType;

            _allSolvableModules[moduleTypeName] = new SolvableModule(module, solvableBombModule);
        }
    }

    private void GetNeedyModules()
    {
        _allNeedyModules = new Dictionary<string, NeedyModule>();

        UnityEngine.Object modManager = ModManager;

        MethodInfo getNeedyModulesMethod = _modManagerType.GetMethod("GetNeedyModules", BindingFlags.Instance | BindingFlags.Public);
        IList needyModuleList = getNeedyModulesMethod.Invoke(modManager, null) as IList;

        //Type needyComponentType = ReflectionHelper.FindType("NeedyComponent");
        //MethodInfo getModuleDisplayNameMethod = needyComponentType.GetMethod("GetModuleDisplayName", BindingFlags.Instance | BindingFlags.Public);

        Type modNeedyComponentType = ReflectionHelper.FindType("ModNeedyComponent");
        FieldInfo moduleField = modNeedyComponentType.GetField("module", BindingFlags.Instance | BindingFlags.NonPublic);

        foreach(object needyModule in needyModuleList)
        {
            KMNeedyModule module = moduleField.GetValue(needyModule) as KMNeedyModule;
            string moduleTypeName = module.ModuleType;

            _allNeedyModules[moduleTypeName] = new NeedyModule(module, needyModule);
        }
    }

    private void GetActiveModules()
    {
        UnityEngine.Object modManager = ModManager;

        FieldInfo loadedBombComponentsField = _modManagerType.GetField("loadedBombComponents", BindingFlags.Instance | BindingFlags.NonPublic);
        _activeModules = loadedBombComponentsField.GetValue(modManager) as IDictionary;
    }

    private void PopulateModSelectorWindow()
    {
        ModSelectorWindow window = GetComponentInChildren<ModSelectorWindow>(true);

        window.SetupService(this);
        window.SetupNormalModules(_allSolvableModules.Values.OrderBy((x) => x.SolvableBombModule.ModuleDisplayName));
        window.SetupNeedyModules(_allNeedyModules.Values.OrderBy((x) => x.NeedyBombModule.ModuleDisplayName));
    }
    #endregion

    #region Actions
    public bool IsModuleActive(string typeName)
    {
        return _activeModules.Contains(typeName);
    }

    public void EnableModule(string typeName)
    {
        if (_activeModules.Contains(typeName))
        {
            return;
        }

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
            Debug.LogError(string.Format("Cannot enable module with type name '{0}'.", typeName));
        }

        _disabledModules.Remove(typeName);
    }

    public void DisableModule(string typeName)
    {
        if (!_activeModules.Contains(typeName))
        {
            return;
        }

        _activeModules.Remove(typeName);
        _disabledModules.Add(typeName);
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
        _disabledModules.AddRange(_allNeedyModules.Keys);
    }

    public void LoadDefaults()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, "disabledMods.json");
            string jsonInput = File.ReadAllText(path);

            List<string> disabledModules = JsonConvert.DeserializeObject<List<string>>(jsonInput);
            foreach(string disabledModule in disabledModules)
            {
                DisableModule(disabledModule);
            }

            SaveDefaults();
        }
        catch (Exception ex)
        {
        }
    }

    public void SaveDefaults()
    {
        try
        {
            string jsonOutput = Newtonsoft.Json.JsonConvert.SerializeObject(_disabledModules);
            string path = Path.Combine(Application.persistentDataPath, "disabledMods.json");
            File.WriteAllText(path, jsonOutput);
        }
        catch (Exception ex)
        {
        }
    }

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

    #region Public Fields
    public string[] emojiIDs = null;
    public Sprite[] emojiSprites = null;
    #endregion

    #region Private Fields & Properties
    private Dictionary<string, SolvableModule> _allSolvableModules = null;
    private Dictionary<string, NeedyModule> _allNeedyModules = null;

    private IDictionary _activeModules = null;
    private List<string> _disabledModules = new List<string>();

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
}
