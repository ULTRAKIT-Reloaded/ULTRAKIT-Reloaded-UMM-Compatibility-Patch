using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UMM;
using BepInEx;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;
using System.Reflection;
using BepInEx.Bootstrap;
using ULTRAKIT.Loader;
using ULTRAKIT.Extensions.ObjectClasses;
using HarmonyLib;
using ULTRAKIT.Loader.Injectors;
using ULTRAKIT.Extensions;

namespace ULTRAKIT.UMM_Compatibility
{
    [BepInPlugin("ULTRAKIT.umm_compat", "ULTRAKIT Reloaded UMM Compatibility Patch", "1.0.0")]
    [BepInDependency("UMM", "0.5.1")]
    [BepInDependency("ULTRAKIT.core_module", "2.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin plugin;

        private void Awake()
        {
            plugin = this;

            Harmony harmony = new Harmony("ULTRAKIT.UMM_Compatibility");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(KeybindsInjector))]
    public class KeybindsInjectorPostfix
    {
        [HarmonyPatch("SetKeys"), HarmonyPostfix]
        static void SetKeysPostfix(InputManager instance)
        {
            UKLogger.Log("Compatibility Patch Active");
            Registries.key_states = Registries.key_registry.ToDictionary(item => item.Key, item => UKAPI.GetKeyBind(item.Value.Name, item.Value.Binding.DefaultKey) as InputActionState);
            foreach (var state in Registries.key_states)
            {
                UKKeySetting setting = Registries.key_registry[state.Key];
                UKKeyBind binding = UKAPI.GetKeyBind(setting.Name);
                binding.OnBindingChanged.AddListener((KeyCode key) =>
                {
                    instance.Inputs[setting.Binding.Name] = key;
                    PrefsManager.instance.SetInt(setting.Binding.PrefName, (int)key);
                    setting.SetValue(key);
                    instance.UpdateBindings();
                });
                if (PrefsManager.Instance.HasKey(setting.Binding.PrefName))
                    binding.ChangeKeyBind((KeyCode)PrefsManager.Instance.GetInt(setting.Binding.PrefName, (int)setting.Key));
            }
            instance.UpdateBindings();
        }
    }
}
