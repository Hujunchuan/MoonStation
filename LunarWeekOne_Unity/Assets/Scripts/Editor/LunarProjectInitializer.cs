using System.IO;
using UnityEditor;
using UnityEngine;

namespace Lunar.Editor
{
    [InitializeOnLoad]
    public static class LunarProjectInitializer
    {
        private const string StartupScenePath = "Assets/Scenes/StartupScene.unity";
        private const string ExperienceScenePath = "Assets/Scenes/LunarBase.unity";
        private const string PromptSessionKey = "LunarWeekOne.ProjectInitPromptShown";

        static LunarProjectInitializer()
        {
            EditorApplication.delayCall += InitializeProject;
        }

        private static void InitializeProject()
        {
            if (Application.isBatchMode)
            {
                return;
            }

            LunarMenuItems.CreateDefaultResources();

            if (HasPrototypeScenes() || SessionState.GetBool(PromptSessionKey, false))
            {
                return;
            }

            SessionState.SetBool(PromptSessionKey, true);

            bool createScenes = EditorUtility.DisplayDialog(
                "Lunar Week One Setup",
                "Prototype scenes are missing. Create StartupScene and LunarBase now?",
                "Create Scenes",
                "Later");

            if (createScenes)
            {
                LunarSceneBuilder.CreatePrototypeScenes();
                return;
            }

            Debug.Log("[LunarProjectInitializer] Prototype scenes were not created yet. Use Lunar Week One/Create Prototype Scenes when ready.");
        }

        private static bool HasPrototypeScenes()
        {
            return File.Exists(StartupScenePath) && File.Exists(ExperienceScenePath);
        }
    }
}
