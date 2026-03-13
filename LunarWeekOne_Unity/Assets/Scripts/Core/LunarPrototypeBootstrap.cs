using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lunar.Core
{
    public class LunarPrototypeBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureBootstrapExists()
        {
            if (FindObjectOfType<LunarPrototypeBootstrap>() != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject("LunarPrototypeBootstrap");
            DontDestroyOnLoad(bootstrapObject);
            bootstrapObject.AddComponent<LunarPrototypeBootstrap>();
        }

        private void Awake()
        {
            EnsureSystems();
            RefreshSceneScaffold();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RefreshSceneScaffold();
        }

        private void RefreshSceneScaffold()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            bool isStartupScene = IsStartupScene(activeScene);

            ConfigureExperienceController(isStartupScene);
            EnsureCameraRig(isStartupScene);
            EnsureLighting();
            EnsureEventSystem();

            if (isStartupScene)
            {
                return;
            }

            LunarPrototypeReferenceHub hub = EnsurePrototypeObjects();
            ConfigurePrototypeSystems(hub);
            EnsureCameraAnchor(hub);
            ConfigureCameraFocus(hub);
            EnsurePrototypeUi(hub);
        }

        private void ConfigureExperienceController(bool isStartupScene)
        {
            LunarExperienceController controller = LunarExperienceController.Instance;
            if (controller == null)
            {
                return;
            }

            if (isStartupScene)
            {
                controller.SuspendExperienceForMenu();
                controller.ConfigureAutoInitialize(false);
                return;
            }

            controller.ConfigureAutoInitialize(true, true);
        }

        private bool IsStartupScene(Scene scene)
        {
            if (FindObjectOfType<LunarStartupScene>() != null)
            {
                return true;
            }

            return string.Equals(scene.name, "StartupScene", System.StringComparison.OrdinalIgnoreCase);
        }

        private void EnsureCameraRig(bool isStartupScene)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 3f, -8f);
                cameraObject.transform.rotation = Quaternion.Euler(18f, 0f, 0f);

                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
            }

            if (isStartupScene)
            {
                return;
            }

            if (mainCamera.GetComponent<LunarCameraController>() == null)
            {
                mainCamera.gameObject.AddComponent<LunarCameraController>();
            }

            if (mainCamera.GetComponent<LunarInputHandler>() == null)
            {
                mainCamera.gameObject.AddComponent<LunarInputHandler>();
            }
        }

        private void EnsureLighting()
        {
            if (FindObjectOfType<Light>() != null)
            {
                return;
            }

            GameObject lightObject = new GameObject("Prototype Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        private void EnsureSystems()
        {
            EnsureSystem<UserSessionManager>("UserSessionManager");
            EnsureSystem<AudioTherapyEngine>("AudioTherapyEngine");
            EnsureSystem<ResourceManager>("ResourceManager");
            EnsureSystem<RitualEngine>("RitualEngine");
            EnsureSystem<LunarDayStateMachine>("LunarDayStateMachine");
            EnsureSystem<LunarEnvironmentController>("LunarEnvironmentController");
            EnsureSystem<ExperienceFeedbackCollector>("ExperienceFeedbackCollector");
            EnsureSystem<LunarExperienceController>("LunarExperienceController");
        }

        private T EnsureSystem<T>(string name) where T : Component
        {
            T existing = FindObjectOfType<T>();
            if (existing != null)
            {
                return existing;
            }

            GameObject systemObject = new GameObject(name);
            systemObject.transform.SetParent(transform, false);
            return systemObject.AddComponent<T>();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private LunarPrototypeReferenceHub EnsurePrototypeObjects()
        {
            return LunarPrototypeSceneKit.EnsureGreyboxShell();
        }

        private void ConfigurePrototypeSystems(LunarPrototypeReferenceHub hub)
        {
            if (hub == null)
            {
                return;
            }

            ResourceManager resourceManager = ResourceManager.Instance;
            if (resourceManager != null)
            {
                Material[] resourceMaterials = new Material[hub.ResourceRenderers != null ? hub.ResourceRenderers.Length : 0];
                for (int index = 0; index < resourceMaterials.Length; index++)
                {
                    if (hub.ResourceRenderers[index] != null)
                    {
                        resourceMaterials[index] = hub.ResourceRenderers[index].material;
                    }
                }

                resourceManager.ConfigurePrototypeVisuals(hub.BaseStatusLight, resourceMaterials);
            }

            RitualEngine ritualEngine = RitualEngine.Instance;
            if (ritualEngine != null)
            {
                ritualEngine.ConfigurePresentation(
                    hub.RitualIndicator,
                    hub.InteriorLights,
                    hub.RitualValveRenderer != null ? hub.RitualValveRenderer.material : null);
            }

            LunarEnvironmentController environmentController = FindObjectOfType<LunarEnvironmentController>();
            if (environmentController != null)
            {
                Volume prototypeVolume = EnsurePrototypeVolume();
                Material prototypeSkybox = EnsurePrototypeSkybox(FindMainDirectionalLight());
                environmentController.ConfigurePrototypeEnvironment(
                    FindMainDirectionalLight(),
                    hub.InteriorLights,
                    hub.RitualAmbientLight,
                    hub.DustParticles,
                    hub.AnomalyParticles,
                    prototypeVolume,
                    prototypeSkybox);
            }

            ConfigureGuidePresentation(hub);
            ConfigureSceneryPresentation(hub);
        }

        private void EnsureCameraAnchor(LunarPrototypeReferenceHub hub)
        {
            if (hub == null || hub.PlayerAnchor == null || Camera.main == null)
            {
                return;
            }

            if (Camera.main.transform.position.z > -7.5f && Camera.main.transform.position.z < 10f)
            {
                return;
            }

            Camera.main.transform.position = hub.PlayerAnchor.position;
            Camera.main.transform.rotation = hub.PlayerAnchor.rotation;
        }

        private void ConfigureCameraFocus(LunarPrototypeReferenceHub hub)
        {
            if (hub == null || Camera.main == null)
            {
                return;
            }

            LunarCameraController cameraController = Camera.main.GetComponent<LunarCameraController>();
            if (cameraController == null)
            {
                return;
            }

            LunarPrototypeFocusDirector focusDirector = Camera.main.GetComponent<LunarPrototypeFocusDirector>();
            if (focusDirector == null)
            {
                focusDirector = Camera.main.gameObject.AddComponent<LunarPrototypeFocusDirector>();
            }

            focusDirector.Configure(
                cameraController,
                hub.PlayerAnchor,
                hub.ResourceFocusAnchor,
                hub.RitualFocusAnchor,
                hub.QuietDeckFocusAnchor);
        }

        private Light FindMainDirectionalLight()
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light != null && light.type == LightType.Directional)
                {
                    return light;
                }
            }

            return null;
        }

        private void ConfigureGuidePresentation(LunarPrototypeReferenceHub hub)
        {
            if (hub == null)
            {
                return;
            }

            LunarPrototypeGuideController guideController = hub.GetComponent<LunarPrototypeGuideController>();
            if (guideController == null)
            {
                guideController = hub.gameObject.AddComponent<LunarPrototypeGuideController>();
            }

            guideController.Configure(
                hub.CorridorGuideRenderers,
                hub.ResourceGuideRenderers,
                hub.RitualGuideRenderers);
        }

        private void ConfigureSceneryPresentation(LunarPrototypeReferenceHub hub)
        {
            if (hub == null)
            {
                return;
            }

            LunarPrototypeSceneryController sceneryController = hub.GetComponent<LunarPrototypeSceneryController>();
            if (sceneryController == null)
            {
                sceneryController = hub.gameObject.AddComponent<LunarPrototypeSceneryController>();
            }

            sceneryController.Configure(
                hub.OperationsScreenRenderer,
                hub.BreathPanelRenderer,
                hub.ObservationGlassRenderer,
                hub.EarthriseRenderer);
        }

        private Volume EnsurePrototypeVolume()
        {
            Volume volume = FindObjectOfType<Volume>();
            if (volume == null)
            {
                GameObject volumeObject = new GameObject("Prototype Global Volume");
                volume = volumeObject.AddComponent<Volume>();
                volume.isGlobal = true;
                volume.priority = 5f;
            }

            if (volume.sharedProfile == null)
            {
                VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = "PrototypeRuntimeVolume";
                profile.hideFlags = HideFlags.DontSave;

                ColorAdjustments colorAdjustments = profile.Add<ColorAdjustments>(true);
                colorAdjustments.postExposure.overrideState = true;
                colorAdjustments.postExposure.value = 0f;
                colorAdjustments.contrast.overrideState = true;
                colorAdjustments.contrast.value = -5f;
                colorAdjustments.saturation.overrideState = true;
                colorAdjustments.saturation.value = -12f;
                colorAdjustments.colorFilter.overrideState = true;
                colorAdjustments.colorFilter.value = new Color(0.92f, 0.96f, 1f);

                Bloom bloom = profile.Add<Bloom>(true);
                bloom.intensity.overrideState = true;
                bloom.intensity.value = 0.45f;
                bloom.threshold.overrideState = true;
                bloom.threshold.value = 0.95f;
                bloom.scatter.overrideState = true;
                bloom.scatter.value = 0.7f;

                Vignette vignette = profile.Add<Vignette>(true);
                vignette.intensity.overrideState = true;
                vignette.intensity.value = 0.16f;
                vignette.smoothness.overrideState = true;
                vignette.smoothness.value = 0.72f;

                FilmGrain filmGrain = profile.Add<FilmGrain>(true);
                filmGrain.intensity.overrideState = true;
                filmGrain.intensity.value = 0.08f;
                filmGrain.response.overrideState = true;
                filmGrain.response.value = 0.85f;

                volume.sharedProfile = profile;
            }

            return volume;
        }

        private Material EnsurePrototypeSkybox(Light directionalLight)
        {
            Material currentSkybox = RenderSettings.skybox;
            if (currentSkybox != null && currentSkybox.shader != null && currentSkybox.shader.name == "Skybox/Procedural")
            {
                if (directionalLight != null)
                {
                    RenderSettings.sun = directionalLight;
                }

                return currentSkybox;
            }

            Shader skyboxShader = Shader.Find("Skybox/Procedural");
            if (skyboxShader == null)
            {
                return currentSkybox;
            }

            Material skyboxMaterial = new Material(skyboxShader);
            skyboxMaterial.name = "Prototype Procedural Skybox";
            skyboxMaterial.hideFlags = HideFlags.DontSave;

            if (skyboxMaterial.HasProperty("_SkyTint"))
            {
                skyboxMaterial.SetColor("_SkyTint", new Color(0.16f, 0.24f, 0.34f));
            }

            if (skyboxMaterial.HasProperty("_GroundColor"))
            {
                skyboxMaterial.SetColor("_GroundColor", new Color(0.09f, 0.1f, 0.12f));
            }

            if (skyboxMaterial.HasProperty("_Exposure"))
            {
                skyboxMaterial.SetFloat("_Exposure", 1.2f);
            }

            if (skyboxMaterial.HasProperty("_AtmosphereThickness"))
            {
                skyboxMaterial.SetFloat("_AtmosphereThickness", 0.55f);
            }

            RenderSettings.skybox = skyboxMaterial;
            if (directionalLight != null)
            {
                RenderSettings.sun = directionalLight;
            }

            return skyboxMaterial;
        }

        private void EnsurePrototypeUi(LunarPrototypeReferenceHub hub)
        {
            Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            EnsureDebugHud(defaultFont);
            EnsureWorldDisplayPanels(hub, defaultFont);
        }

        private void EnsureDebugHud(Font defaultFont)
        {
            if (FindObjectOfType<LunarPrototypeDebugPanel>() != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("LunarPrototypeCanvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            GameObject hudPanel = CreatePanel("Debug HUD", canvasRect, new Vector2(16f, -16f), new Vector2(360f, 240f), new Color(0f, 0f, 0f, 0.6f));
            Text headerText = CreateText("Header", hudPanel.transform as RectTransform, defaultFont, 17, TextAnchor.UpperLeft, new Vector2(12f, -12f), new Vector2(336f, 60f));
            Text resourceText = CreateText("Resources", hudPanel.transform as RectTransform, defaultFont, 15, TextAnchor.UpperLeft, new Vector2(12f, -76f), new Vector2(336f, 78f));
            Text hintText = CreateText("Hints", hudPanel.transform as RectTransform, defaultFont, 13, TextAnchor.UpperLeft, new Vector2(12f, -158f), new Vector2(336f, 48f));
            Button feedbackButton = CreateButton("Open Feedback", hudPanel.transform as RectTransform, defaultFont, new Vector2(204f, -206f), new Vector2(132f, 28f));

            LunarPrototypeDebugPanel debugPanel = hudPanel.AddComponent<LunarPrototypeDebugPanel>();
            debugPanel.Configure(headerText, resourceText, hintText, feedbackButton);

            CreateFeedbackCanvas(canvasRect, defaultFont);
        }

        private void EnsureWorldDisplayPanels(LunarPrototypeReferenceHub hub, Font font)
        {
            if (hub == null)
            {
                return;
            }

            EnsureWorldDisplayPanel(
                hub.OperationsDisplayAnchor,
                "Operations Display Canvas",
                font,
                LunarWorldDisplayPanel.DisplayMode.Operations,
                "OPS",
                new Color(0.03f, 0.07f, 0.11f, 0.92f),
                new Color(0.18f, 0.62f, 0.82f, 0.95f));

            EnsureWorldDisplayPanel(
                hub.QuietDeckDisplayAnchor,
                "Quiet Deck Display Canvas",
                font,
                LunarWorldDisplayPanel.DisplayMode.Guidance,
                "QUIET DECK",
                new Color(0.04f, 0.08f, 0.1f, 0.9f),
                new Color(0.28f, 0.74f, 0.72f, 0.92f));
        }

        private void EnsureWorldDisplayPanel(
            Transform anchor,
            string objectName,
            Font font,
            LunarWorldDisplayPanel.DisplayMode mode,
            string label,
            Color panelColor,
            Color accentColor)
        {
            if (anchor == null || font == null)
            {
                return;
            }

            Transform existing = anchor.Find(objectName);
            if (existing != null && existing.GetComponent<LunarWorldDisplayPanel>() != null)
            {
                Canvas existingCanvas = existing.GetComponent<Canvas>();
                if (existingCanvas != null && Camera.main != null)
                {
                    existingCanvas.worldCamera = Camera.main;
                }

                return;
            }

            GameObject canvasObject = new GameObject(objectName);
            canvasObject.transform.SetParent(anchor, false);
            canvasObject.transform.localPosition = Vector3.zero;
            canvasObject.transform.localRotation = Quaternion.identity;
            canvasObject.transform.localScale = Vector3.one * 0.0032f;

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            canvas.sortingOrder = 2;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 14f;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(620f, 360f);

            GameObject panel = CreateStretchImage("Panel", canvasRect, panelColor);
            RectTransform panelRect = panel.transform as RectTransform;

            CreateStretchImage("Glow", panelRect, new Color(accentColor.r, accentColor.g, accentColor.b, 0.06f));
            CreatePanel("Accent Bar", panelRect, new Vector2(0f, 0f), new Vector2(620f, 8f), accentColor);
            CreatePanel("Footer Bar", panelRect, new Vector2(0f, -352f), new Vector2(620f, 8f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.55f));

            Text titleText = CreateText("Title", panelRect, font, 24, TextAnchor.UpperLeft, new Vector2(24f, -18f), new Vector2(572f, 34f));
            titleText.color = new Color(0.86f, 0.94f, 0.98f);

            Text statusText = CreateText("Status", panelRect, font, 18, TextAnchor.UpperLeft, new Vector2(24f, -64f), new Vector2(572f, 104f));
            statusText.color = new Color(0.78f, 0.86f, 0.93f);

            Text guidanceText = CreateText("Guidance", panelRect, font, 17, TextAnchor.UpperLeft, new Vector2(24f, -178f), new Vector2(572f, 118f));
            guidanceText.color = new Color(0.85f, 0.91f, 0.95f);

            GameObject alertPanel = CreatePanel("Alert Panel", panelRect, new Vector2(24f, -314f), new Vector2(572f, 30f), new Color(accentColor.r, accentColor.g, accentColor.b, 0.14f));
            Text alertText = CreateText("Alert Text", alertPanel.transform as RectTransform, font, 16, TextAnchor.MiddleCenter, new Vector2(0f, -4f), new Vector2(548f, 24f), true);

            LunarWorldDisplayPanel displayPanel = canvasObject.AddComponent<LunarWorldDisplayPanel>();
            displayPanel.Configure(titleText, statusText, guidanceText, alertText, alertPanel.GetComponent<Image>(), mode, label);
        }

        private Canvas CreateFeedbackCanvas(RectTransform parent, Font font)
        {
            GameObject feedbackObject = new GameObject("Feedback Canvas");
            feedbackObject.transform.SetParent(parent, false);

            Canvas feedbackCanvas = feedbackObject.AddComponent<Canvas>();
            feedbackCanvas.overrideSorting = true;
            feedbackCanvas.sortingOrder = 10;
            feedbackObject.AddComponent<GraphicRaycaster>();

            Image overlay = feedbackObject.AddComponent<Image>();
            overlay.color = new Color(0f, 0f, 0f, 0.78f);

            RectTransform feedbackRect = feedbackObject.GetComponent<RectTransform>();
            feedbackRect.anchorMin = Vector2.zero;
            feedbackRect.anchorMax = Vector2.one;
            feedbackRect.offsetMin = Vector2.zero;
            feedbackRect.offsetMax = Vector2.zero;

            GameObject panel = CreatePanel("Feedback Panel", feedbackRect, new Vector2(0.5f, 0.5f), new Vector2(420f, 380f), new Color(0.08f, 0.08f, 0.1f, 0.95f), true);
            RectTransform panelRect = panel.transform as RectTransform;

            CreateText("Feedback Title", panelRect, font, 20, TextAnchor.MiddleCenter, new Vector2(0f, -24f), new Vector2(360f, 30f), true)
                .text = "Experience Feedback";

            Slider calmness = CreateLabeledSlider("Calmness", panelRect, font, new Vector2(0f, -78f));
            Slider desire = CreateLabeledSlider("Phone Desire", panelRect, font, new Vector2(0f, -128f));
            Slider ritualComfort = CreateLabeledSlider("Ritual Comfort", panelRect, font, new Vector2(0f, -178f));
            Slider presence = CreateLabeledSlider("Presence", panelRect, font, new Vector2(0f, -228f));
            Slider worldFeeling = CreateLabeledSlider("World Feeling", panelRect, font, new Vector2(0f, -278f));

            Text summaryText = CreateText("Feedback Summary", panelRect, font, 14, TextAnchor.UpperLeft, new Vector2(0f, -324f), new Vector2(360f, 44f), true);
            Button submitButton = CreateButton("Submit", panelRect, font, new Vector2(0f, -350f), new Vector2(140f, 34f), true);

            ExperienceFeedbackCollector collector = ExperienceFeedbackCollector.Instance;
            if (collector != null)
            {
                collector.ConfigureUi(
                    feedbackCanvas,
                    calmness,
                    desire,
                    ritualComfort,
                    presence,
                    worldFeeling,
                    summaryText,
                    submitButton);
            }

            feedbackObject.SetActive(false);
            return feedbackCanvas;
        }

        private GameObject CreatePanel(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size, Color color, bool centered = false)
        {
            GameObject panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);

            Image image = panelObject.AddComponent<Image>();
            image.color = color;

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            if (centered)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            else
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                rectTransform.pivot = new Vector2(0f, 1f);
            }

            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
            return panelObject;
        }

        private GameObject CreateStretchImage(string name, RectTransform parent, Color color)
        {
            GameObject imageObject = new GameObject(name);
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.AddComponent<Image>();
            image.color = color;

            RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return imageObject;
        }

        private Text CreateText(
            string name,
            RectTransform parent,
            Font font,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchoredPosition,
            Vector2 size,
            bool centered = false)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = alignment;

            RectTransform rectTransform = text.GetComponent<RectTransform>();
            if (centered)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
            }
            else
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                rectTransform.pivot = new Vector2(0f, 1f);
            }

            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;
            return text;
        }

        private Button CreateButton(
            string label,
            RectTransform parent,
            Font font,
            Vector2 anchoredPosition,
            Vector2 size,
            bool centered = false)
        {
            GameObject buttonObject = new GameObject($"{label} Button");
            buttonObject.transform.SetParent(parent, false);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.17f, 0.44f, 0.75f, 0.95f);

            Button button = buttonObject.AddComponent<Button>();

            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            if (centered)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.pivot = new Vector2(0.5f, 1f);
            }
            else
            {
                rectTransform.anchorMin = new Vector2(0f, 1f);
                rectTransform.anchorMax = new Vector2(0f, 1f);
                rectTransform.pivot = new Vector2(0f, 1f);
            }

            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text buttonText = CreateText("Label", rectTransform, font, 15, TextAnchor.MiddleCenter, Vector2.zero, size, true);
            buttonText.text = label;

            return button;
        }

        private Slider CreateLabeledSlider(string label, RectTransform parent, Font font, Vector2 anchoredPosition)
        {
            CreateText($"{label} Label", parent, font, 14, TextAnchor.MiddleLeft, anchoredPosition + new Vector2(-180f, 0f), new Vector2(120f, 20f));
            Slider slider = CreateSlider(parent, anchoredPosition + new Vector2(36f, 0f), new Vector2(220f, 20f));
            slider.minValue = 1f;
            slider.maxValue = 5f;
            slider.wholeNumbers = false;
            slider.value = 3f;
            return slider;
        }

        private Slider CreateSlider(RectTransform parent, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject sliderObject = new GameObject("Slider");
            sliderObject.transform.SetParent(parent, false);

            RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.5f, 1f);
            sliderRect.anchorMax = new Vector2(0.5f, 1f);
            sliderRect.pivot = new Vector2(0.5f, 1f);
            sliderRect.anchoredPosition = anchoredPosition;
            sliderRect.sizeDelta = size;

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.direction = Slider.Direction.LeftToRight;

            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObject.transform, false);
            Image backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0.2f, 0.2f, 0.24f, 1f);
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
            fillImage.color = new Color(0.26f, 0.72f, 0.98f, 1f);
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
    }
}
