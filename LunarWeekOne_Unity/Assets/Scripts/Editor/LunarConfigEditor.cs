using System.IO;
using Lunar.Data;
using UnityEditor;
using UnityEngine;

namespace Lunar.Editor
{
    public class LunarConfigEditor : EditorWindow
    {
        private const string DefaultConfigPath = "Assets/Resources/Configs/global_config.json";

        private string configPath = DefaultConfigPath;
        private string jsonText = string.Empty;

        [MenuItem("Lunar Week One/Configuration Editor")]
        public static void ShowWindow()
        {
            GetWindow<LunarConfigEditor>("Lunar Config");
        }

        private void OnEnable()
        {
            LoadConfig();
        }

        private void OnGUI()
        {
            GUILayout.Label("Lunar Week One Configuration", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Edit the JSON configuration used by the prototype bootstrap tools.", MessageType.Info);

            configPath = EditorGUILayout.TextField("Config Path", configPath);
            EditorGUILayout.Space();

            if (GUILayout.Button("Load Config"))
            {
                LoadConfig();
            }

            jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.ExpandHeight(true));

            EditorGUILayout.Space();

            if (GUILayout.Button("Save Config"))
            {
                SaveConfig();
            }

            if (GUILayout.Button("Reset To Default"))
            {
                jsonText = CreateDefaultConfigJson();
                SaveConfig();
            }
        }

        private void LoadConfig()
        {
            EnsureDirectories(Path.GetDirectoryName(configPath));

            if (File.Exists(configPath))
            {
                jsonText = File.ReadAllText(configPath);
            }
            else
            {
                jsonText = CreateDefaultConfigJson();
                File.WriteAllText(configPath, jsonText);
                AssetDatabase.Refresh();
            }
        }

        private void SaveConfig()
        {
            EnsureDirectories(Path.GetDirectoryName(configPath));
            File.WriteAllText(configPath, jsonText ?? string.Empty);
            AssetDatabase.Refresh();
            Debug.Log($"[LunarConfigEditor] Configuration saved to {configPath}");
        }

        public static string CreateDefaultConfigJson()
        {
            return JsonUtility.ToJson(LunarDefaultConfigFactory.CreateGlobalConfig(), true);
        }

        private static void EnsureDirectories(string directory)
        {
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }

    public static class LunarMenuItems
    {
        [MenuItem("Lunar Week One/Create Default Resources")]
        public static void CreateDefaultResources()
        {
            string[] directories =
            {
                "Assets/Resources/Configs",
                "Assets/Resources/Audio",
                "Assets/Resources/Materials",
                "Assets/Resources/Prefabs",
                "Assets/Scenes"
            };

            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }

            string configPath = "Assets/Resources/Configs/global_config.json";
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, LunarConfigEditor.CreateDefaultConfigJson());
            }

            AssetDatabase.Refresh();
            Debug.Log("[LunarMenuItems] Default resource folders are ready");
        }

        [MenuItem("Lunar Week One/Validate Project Structure")]
        public static void ValidateProjectStructure()
        {
            Debug.Log("=== Lunar Week One Project Validation ===");

            string[] requiredDirectories =
            {
                "Assets/Resources/Configs",
                "Assets/Resources/Audio",
                "Assets/Scenes",
                "Assets/Scripts/Core",
                "Assets/Scripts/Systems"
            };

            string[] requiredFiles =
            {
                "Assets/Scripts/Core/LunarExperienceController.cs",
                "Assets/Scripts/Core/LunarDayStateMachine.cs",
                "Assets/Scripts/Systems/AudioTherapyEngine.cs",
                "Assets/Scripts/Systems/ResourceManager.cs",
                "Assets/Scripts/Systems/RitualEngine.cs"
            };

            bool isValid = true;

            foreach (string directory in requiredDirectories)
            {
                if (!Directory.Exists(directory))
                {
                    Debug.LogError($"Missing directory: {directory}");
                    isValid = false;
                }
            }

            foreach (string file in requiredFiles)
            {
                if (!File.Exists(file))
                {
                    Debug.LogError($"Missing file: {file}");
                    isValid = false;
                }
            }

            if (isValid)
            {
                Debug.Log("[LunarMenuItems] Project structure validation passed");
            }
            else
            {
                Debug.LogError("[LunarMenuItems] Project structure validation failed");
            }
        }
    }
}
