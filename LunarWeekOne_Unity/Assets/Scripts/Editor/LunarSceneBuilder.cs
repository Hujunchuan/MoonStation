using System.Collections.Generic;
using System.IO;
using Lunar.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Lunar.Editor
{
    public static class LunarSceneBuilder
    {
        private const string StartupScenePath = "Assets/Scenes/StartupScene.unity";
        private const string ExperienceScenePath = "Assets/Scenes/LunarBase.unity";

        [MenuItem("Lunar Week One/Create Prototype Scenes")]
        public static void CreatePrototypeScenes()
        {
            LunarMenuItems.CreateDefaultResources();

            string startupScenePath = BuildStartupScene();
            string experienceScenePath = BuildExperienceScene();

            UpdateBuildSettings(startupScenePath, experienceScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorSceneManager.OpenScene(startupScenePath);

            Debug.Log("[LunarSceneBuilder] StartupScene and LunarBase are ready");
        }

        private static string BuildStartupScene()
        {
            EnsureSceneDirectory();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMainCamera(new Vector3(0f, 0f, -10f), Quaternion.identity, new Color(0.04f, 0.05f, 0.1f));
            CreateDirectionalLight("Startup Directional Light", 0.7f, Quaternion.Euler(50f, -20f, 0f));
            CreateEventSystem();

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            GameObject root = new GameObject("LunarStartupRoot");
            LunarStartupScene startupScene = root.AddComponent<LunarStartupScene>();

            Canvas canvas = CreateCanvas("StartupCanvas");
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();

            CreatePanel(
                "Backdrop",
                canvasRect,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero,
                new Color(0.02f, 0.03f, 0.08f, 1f));

            GameObject startupPanel = CreatePanel(
                "Startup Panel",
                canvasRect,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(560f, 420f),
                new Color(0.08f, 0.11f, 0.18f, 0.94f));

            RectTransform startupPanelRect = startupPanel.GetComponent<RectTransform>();

            CreateText(
                "Title",
                startupPanelRect,
                font,
                "MoonStation",
                34,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -42f),
                new Vector2(420f, 40f));

            CreateText(
                "Subtitle",
                startupPanelRect,
                font,
                "A minimal playable prototype flow for Lunar Week One.",
                17,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -92f),
                new Vector2(460f, 48f));

            Button startButton = CreateButton(
                "Start New",
                startupPanelRect,
                font,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -172f),
                new Vector2(280f, 42f),
                new Color(0.18f, 0.46f, 0.83f, 0.97f));

            Button continueButton = CreateButton(
                "Continue",
                startupPanelRect,
                font,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -224f),
                new Vector2(280f, 42f),
                new Color(0.12f, 0.58f, 0.55f, 0.97f));

            CreateButton(
                "Quit",
                startupPanelRect,
                font,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -276f),
                new Vector2(280f, 42f),
                new Color(0.35f, 0.36f, 0.45f, 0.97f))
                .onClick.AddListener(startupScene.QuitApplication);

            Text statusText = CreateText(
                "Status",
                startupPanelRect,
                font,
                "Checking local session state...",
                14,
                TextAnchor.UpperCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -340f),
                new Vector2(460f, 48f));

            GameObject loadingPanel = CreatePanel(
                "Loading Panel",
                canvasRect,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero,
                new Color(0f, 0f, 0f, 0.72f));

            GameObject loadingCard = CreatePanel(
                "Loading Card",
                loadingPanel.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(420f, 160f),
                new Color(0.08f, 0.1f, 0.15f, 0.96f));

            RectTransform loadingCardRect = loadingCard.GetComponent<RectTransform>();

            Text loadingText = CreateText(
                "Loading Text",
                loadingCardRect,
                font,
                "Loading... 0%",
                20,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -42f),
                new Vector2(320f, 30f));

            Slider loadingSlider = CreateSlider(
                "Loading Slider",
                loadingCardRect,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -104f),
                new Vector2(300f, 22f));
            loadingSlider.minValue = 0f;
            loadingSlider.maxValue = 1f;
            loadingSlider.value = 0f;

            GameObject errorPanel = CreatePanel(
                "Error Panel",
                canvasRect,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero,
                new Color(0f, 0f, 0f, 0.72f));

            GameObject errorCard = CreatePanel(
                "Error Card",
                errorPanel.GetComponent<RectTransform>(),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(420f, 180f),
                new Color(0.19f, 0.08f, 0.1f, 0.96f));

            CreateText(
                "Error Title",
                errorCard.GetComponent<RectTransform>(),
                font,
                "Unable to start the experience",
                22,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -40f),
                new Vector2(360f, 30f));

            Text errorMessageText = CreateText(
                "Error Message",
                errorCard.GetComponent<RectTransform>(),
                font,
                "Unknown error",
                16,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -96f),
                new Vector2(360f, 54f));

            GameObject fadeOverlay = CreatePanel(
                "Fade Overlay",
                canvasRect,
                Vector2.zero,
                Vector2.one,
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                Vector2.zero,
                Color.black);
            CanvasGroup fadeCanvasGroup = fadeOverlay.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;

            loadingPanel.SetActive(false);
            errorPanel.SetActive(false);

            AssignStartupSceneReferences(
                startupScene,
                fadeCanvasGroup,
                startupPanel,
                loadingPanel,
                errorPanel,
                loadingText,
                loadingSlider,
                startButton,
                continueButton,
                statusText,
                errorMessageText);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), StartupScenePath);
            return StartupScenePath;
        }

        private static string BuildExperienceScene()
        {
            EnsureSceneDirectory();

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMainCamera(new Vector3(0f, 3f, -8f), Quaternion.Euler(18f, 0f, 0f), new Color(0.04f, 0.05f, 0.08f));
            CreateDirectionalLight("LunarBase Directional Light", 1f, Quaternion.Euler(45f, -30f, 0f));

            GameObject root = new GameObject("LunarBaseRoot");
            LunarPrototypeSceneKit.EnsureGreyboxShell(root.transform);
            CreateTextMesh(root.transform, "MoonStation greybox shell ready for authored replacement.", new Vector3(0f, 2.4f, -5.8f));

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ExperienceScenePath);
            return ExperienceScenePath;
        }

        private static void AssignStartupSceneReferences(
            LunarStartupScene startupScene,
            CanvasGroup fadeCanvasGroup,
            GameObject startupPanel,
            GameObject loadingPanel,
            GameObject errorPanel,
            Text loadingText,
            Slider loadingSlider,
            Button startButton,
            Button continueButton,
            Text statusText,
            Text errorText)
        {
            SerializedObject serializedObject = new SerializedObject(startupScene);
            serializedObject.FindProperty("mainExperienceScene").stringValue = "LunarBase";
            serializedObject.FindProperty("fadeCanvasGroup").objectReferenceValue = fadeCanvasGroup;
            serializedObject.FindProperty("startupPanel").objectReferenceValue = startupPanel;
            serializedObject.FindProperty("loadingPanel").objectReferenceValue = loadingPanel;
            serializedObject.FindProperty("errorPanel").objectReferenceValue = errorPanel;
            serializedObject.FindProperty("loadingText").objectReferenceValue = loadingText;
            serializedObject.FindProperty("loadingSlider").objectReferenceValue = loadingSlider;
            serializedObject.FindProperty("startButton").objectReferenceValue = startButton;
            serializedObject.FindProperty("continueButton").objectReferenceValue = continueButton;
            serializedObject.FindProperty("statusText").objectReferenceValue = statusText;
            serializedObject.FindProperty("errorText").objectReferenceValue = errorText;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void UpdateBuildSettings(params string[] scenePaths)
        {
            var orderedScenes = new List<EditorBuildSettingsScene>();
            var seenPaths = new HashSet<string>();

            foreach (string scenePath in scenePaths)
            {
                if (seenPaths.Add(scenePath))
                {
                    orderedScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (seenPaths.Add(scene.path))
                {
                    orderedScenes.Add(scene);
                }
            }

            EditorBuildSettings.scenes = orderedScenes.ToArray();
        }

        private static void EnsureSceneDirectory()
        {
            string sceneDirectory = Path.GetDirectoryName(StartupScenePath);
            if (!string.IsNullOrWhiteSpace(sceneDirectory) && !Directory.Exists(sceneDirectory))
            {
                Directory.CreateDirectory(sceneDirectory);
            }
        }

        private static Camera CreateMainCamera(Vector3 position, Quaternion rotation, Color backgroundColor)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = position;
            cameraObject.transform.rotation = rotation;

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static Light CreateDirectionalLight(string name, float intensity, Quaternion rotation)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.rotation = rotation;

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = intensity;
            return light;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }

        private static Canvas CreateCanvas(string name)
        {
            GameObject canvasObject = new GameObject(name);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreatePanel(
            string name,
            RectTransform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color)
        {
            GameObject panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);

            Image image = panelObject.AddComponent<Image>();
            image.color = color;

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;

            if (anchorMin == Vector2.zero && anchorMax == Vector2.one)
            {
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }

            return panelObject;
        }

        private static Text CreateText(
            string name,
            RectTransform parent,
            Font font,
            string value,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;

            return text;
        }

        private static Button CreateButton(
            string label,
            RectTransform parent,
            Font font,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color)
        {
            GameObject buttonObject = new GameObject($"{label} Button");
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.AddComponent<Image>();
            image.color = color;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = color;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.12f);
            colors.pressedColor = Color.Lerp(color, Color.black, 0.12f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(color.r, color.g, color.b, 0.45f);
            button.colors = colors;

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;

            CreateText(
                "Label",
                rectTransform,
                font,
                label,
                18,
                TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                sizeDelta);

            return button;
        }

        private static Slider CreateSlider(
            string name,
            RectTransform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            GameObject sliderObject = new GameObject(name);
            sliderObject.transform.SetParent(parent, false);

            RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
            sliderRect.anchorMin = anchorMin;
            sliderRect.anchorMax = anchorMax;
            sliderRect.pivot = pivot;
            sliderRect.anchoredPosition = anchoredPosition;
            sliderRect.sizeDelta = sizeDelta;

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;

            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObject.transform, false);
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.16f, 0.18f, 0.23f, 1f);
            RectTransform backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0.25f);
            backgroundRect.anchorMax = new Vector2(1f, 0.75f);
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObject.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0f);
            fillAreaRect.anchorMax = new Vector2(1f, 1f);
            fillAreaRect.offsetMin = new Vector2(10f, 0f);
            fillAreaRect.offsetMax = new Vector2(-10f, 0f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.28f, 0.66f, 0.96f, 1f);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            GameObject handleArea = new GameObject("Handle Slide Area");
            handleArea.transform.SetParent(sliderObject.transform, false);
            RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0f, 0f);
            handleAreaRect.anchorMax = new Vector2(1f, 1f);
            handleAreaRect.offsetMin = Vector2.zero;
            handleAreaRect.offsetMax = Vector2.zero;

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(18f, 18f);

            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.graphic = handleImage;

            return slider;
        }

        private static void CreateTextMesh(Transform parent, string message, Vector3 localPosition)
        {
            GameObject textObject = new GameObject("Prototype Marker");
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;
            textObject.transform.rotation = Quaternion.identity;

            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = message;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.18f;
            textMesh.fontSize = 48;
            textMesh.color = Color.white;
        }
    }
}
