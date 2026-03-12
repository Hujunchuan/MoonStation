using UnityEditor;
using UnityEngine;
using System.IO;

namespace Lunar.Editor
{
    public class LunarConfigEditor : EditorWindow
    {
        private TextAsset configAsset;
        private string configPath = "Assets/Resources/Configs/global_config.json";
        private string jsonText;

        [MenuItem("Lunar Week One/Configuration Editor")]
        public static void ShowWindow()
        {
            GetWindow<LunarConfigEditor>("Lunar Config Editor");
        }

        private void OnEnable()
        {
            LoadConfig();
        }

        private void LoadConfig()
        {
            if (File.Exists(configPath))
            {
                jsonText = File.ReadAllText(configPath);
            }
            else
            {
                jsonText = CreateDefaultConfig();
                File.WriteAllText(configPath, jsonText);
                AssetDatabase.Refresh();
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Lunar Week One Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("编辑全局配置JSON文件，修改后点击保存", MessageType.Info);
            EditorGUILayout.Space();

            configPath = EditorGUILayout.TextField("Config Path", configPath);
            EditorGUILayout.Space();

            if (GUILayout.Button("Load Config"))
            {
                LoadConfig();
            }

            EditorGUILayout.Space();
            GUILayout.Label("JSON Configuration:", EditorStyles.boldLabel);
            
            float height = position.height - 200;
            jsonText = EditorGUILayout.TextArea(jsonText, GUILayout.Height(height));

            EditorGUILayout.Space();

            if (GUILayout.Button("Save Config"))
            {
                SaveConfig();
            }

            if (GUILayout.Button("Reset to Default"))
            {
                jsonText = CreateDefaultConfig();
                SaveConfig();
            }
        }

        private void SaveConfig()
        {
            try
            {
                File.WriteAllText(configPath, jsonText);
                AssetDatabase.Refresh();
                Debug.Log($"Configuration saved to {configPath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save configuration: {e.Message}");
            }
        }

        private string CreateDefaultConfig()
        {
            return @"{
    ""version"": ""1.0.0"",
    ""targetFrameRate"": 60,
    ""enableVsync"": true,
    
    ""audioMix"": {
        ""mixName"": ""LunarDefault"",
        ""globalVolume"": 1.0,
        ""layers"": [
            {
                ""layerName"": ""LowFrequencyHum"",
                ""clip"": ""Audio/ambient_low"",
                ""volume"": 0.6,
                ""pitch"": 1.0,
                ""loop"": true,
                ""spatial"": false
            },
            {
                ""layerName"": ""BreathGuide"",
                ""clip"": ""Audio/breath_60bpm"",
                ""volume"": 0.5,
                ""pitch"": 1.0,
                ""loop"": true,
                ""spatial"": false
            }
        ]
    },
    
    ""days"": [
        {
            ""dayNumber"": 1,
            ""dayName"": ""Day 1: Arrival"",
            ""theme"": ""混乱、警报"",
            ""targetDurationMinutes"": 4.0,
            ""narrativeClips"": [""Narrative_Day1_Arrival""],
            ""documentaryClips"": [],
            ""hasAnomaly"": false,
            ""anomalyChance"": 0.0,
            ""ritual"": {
                ""ritualName"": ""Landing Ritual"",
                ""description"": ""降落仪式 - 落地/安顿"",
                ""isDeepRitual"": false,
                ""phases"": [
                    {
                        ""phase"": 1,
                        ""durationSeconds"": 15.0,
                        ""audioClipName"": ""Ritual_Enter"",
                        ""requiresInteraction"": false
                    },
                    {
                        ""phase"": 2,
                        ""durationSeconds"": 45.0,
                        ""audioClipName"": ""Ritual_Anchor"",
                        ""requiresInteraction"": false
                    },
                    {
                        ""phase"": "/> 4,
                        ""durationSeconds"": 60.0,
                        ""audioClipName"": ""Ritual_Observe"",
                        ""requiresInteraction"": false
                    },
                    {
                        ""phase"": 5,
                        ""durationSeconds"": 15.0,
                        ""audioClipName"": ""Ritual_Exit"",
                        ""requiresInteraction"": false
                    }
                ]
            }
        },
        {
            ""dayNumber"": 5,
            ""dayName"": ""Day 5: Deep Ritual"",
            ""theme"": ""核心冥想体验"",
            ""targetDurationMinutes"": 4.0,
            ""narrativeClips"": [""Narrative_Day5_Ritual""],
            ""documentaryClips"": [],
            ""hasAnomaly"": false,
            ""anomalyChance"": 0.0,
            ""ritual"": {
                ""ritualName"": ""Deep Meditation Ritual"",
                ""description"": ""深度仪式 - 内在探索"",
                ""isDeepRitual"": true,
                ""phases"": [
                    {
                        ""phase"": 1,
                        ""durationSeconds"": 30.0,
                        ""audioClipName"": ""Ritual_Deep_Enter"",
                        ""requiresInteraction"": false
                    },
                    {
                        ""phase"": 2,
                        ""durationSeconds"": 90.0,
                        ""audioClipName"": ""Ritual_Deep_Anchor"",
                        ""requiresInteraction"": false
                    },
                    {
                        ""phase"": 3,
                        ""durationSeconds"": 60.0,
                        ""audioClipName"": ""Ritual_Deep_Order"",
                        ""requiresInteraction"": true,
                        ""interactionTarget"": ""valve""
                    },
                    {
                        ""phase"": 4,
                        ""durationSeconds"": 120.0,
                        ""audioClipName"": ""Ritual_Deep_Observe"",
                        ""requiresInteraction"": false
                    },
                    {
                        ""phase"": 5,
                        ""durationSeconds"": 30.0,
                        ""audioClipName"": ""Ritual_Deep_Exit"",
                        ""requiresInteraction"": false
                    }
                ]
            }
        }
    ],
    
    ""lightFadeDuration"": 2.0,
    ""interactionResponseDelay"": 0.4
}";
        }
    }

    public class LunarMenuItems
    {
        [MenuItem("Lunar Week One/Create Default Resources")]
        public static void CreateDefaultResources()
        {
            CreateDirectories();
            CreateDefaultConfigFiles();
            AssetDatabase.Refresh();
        }

        private static void CreateDirectories()
        {
            string[] directories = {
                "Assets/Resources/Configs",
                "Assets/Resources/Audio",
                "Assets/Resources/Materials",
                "Assets/Resources/Prefabs",
                "Assets/Scenes",
                "Assets/Scripts",
                "Assets/Textures"
            };

            foreach (string dir in directories)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
        }

        private static void CreateDefaultConfigFiles()
        {
            string configPath = "Assets/Resources/Configs/global_config.json";
            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, new LunarConfigEditor().CreateDefaultConfig());
            }
        }

        [MenuItem("Lunar Week One/Validate Project Structure")]
        public static void ValidateProjectStructure()
        {
            Debug.Log("=== Lunar Week One Project Validation ===");

            bool allValid = true;

            string[] requiredDirs = {
                "Assets/Resources/Configs",
                "Assets/Resources/Audio",
                "Assets/Scenes"
            };

            foreach (string dir in requiredDirs)
            {
                if (!Directory.Exists(dir))
                {
                    Debug.LogError($"Missing directory: {dir}");
                    allValid = false;
                }
            }

            string[] requiredFiles = {
                "Assets/Resources/Configs/global_config.json",
                "Assets/Scripts/Core/LunarExperienceController.cs",
                "Assets/Scripts/Core/LunarDayStateMachine.cs",
                "Assets/Scripts/Systems/AudioTherapyEngine.cs",
                "Assets/Scripts/Systems/ResourceManager.cs",
                "Assets/Scripts/Systems/RitualEngine.cs"
            };

            foreach (string file in requiredFiles)
            {
                if (!File.Exists(file))
                {
                    Debug.LogError($"Missing file: {file}");
                    allValid = false;
                }
            }

            if (allValid)
            {
                Debug.Log("✓ Project structure validation passed");
            }
            else
            {
                Debug.LogError("✗ Project structure validation failed");
            }
        }

        [MenuItem("Lunar Week One/Create Sample Scene")]
        public static void CreateSampleScene()
        {
            string scenePath = "Assets/Scenes/LunarBase.unity";
            
            if (File.Exists(scenePath))
            {
                Debug.LogWarning($"Scene already exists: {scenePath}");
                return;
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(
                UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects, UnityEditor.SceneManagement.NewSceneMode.Single),
                scenePath
            );

            Debug.Log($"Sample scene created: {scenePath}");
        }
    }
}