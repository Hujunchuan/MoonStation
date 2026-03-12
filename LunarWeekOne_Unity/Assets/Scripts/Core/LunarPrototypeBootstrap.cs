using UnityEngine;
using UnityEngine.EventSystems;
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
            EnsurePrototypeUi();
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
                environmentController.ConfigurePrototypeEnvironment(
                    FindMainDirectionalLight(),
                    hub.InteriorLights,
                    hub.RitualAmbientLight,
                    hub.DustParticles,
                    hub.AnomalyParticles);
            }
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

        private void EnsurePrototypeUi()
        {
            if (FindObjectOfType<LunarPrototypeDebugPanel>() != null)
            {
                return;
            }

            Font defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

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
