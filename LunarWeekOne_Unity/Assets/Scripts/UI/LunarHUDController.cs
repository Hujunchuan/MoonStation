using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Lunar.Core
{
    public class LunarHUDController : MonoBehaviour
    {
        [Header("Resource Indicators")]
        [SerializeField] private Image energyIndicator;
        [SerializeField] private Image oxygenIndicator;
        [SerializeField] private Image waterIndicator;

        [SerializeField] private Slider energySlider;
        [SerializeField] private Slider oxygenSlider;
        [SerializeField] private Slider waterSlider;

        [SerializeField] private Text energyText;
        [SerializeField] private Text oxygenText;
        [SerializeField] private Text waterText;

        [Header("Day Progress")]
        [SerializeField] private Slider dayProgressSlider;
        [SerializeField] private Text dayText;
        [SerializeField] private Text dayNameText;
        [SerializeField] private Text stateText;

        [Header("Ritual UI")]
        [SerializeField] private GameObject ritualPanel;
        [SerializeField] private Text ritualPhaseText;
        [SerializeField] private Slider ritualProgressSlider;
        [SerializeField] private Text ritualInstructionText;

        [Header("Audio Controls")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider ambientVolumeSlider;
        [SerializeField] private Slider breathVolumeSlider;
        [SerializeField] private Slider voiceVolumeSlider;

        [SerializeField] private Toggle breathGuideToggle;

        [Header("Session Info")]
        [SerializeField] private Text sessionTimeText;
        [SerializeField] private Text interactionsText;
        [SerializeField] private Text dayCountText;

        [Header("Status Panel")]
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private Text statusText;
        [SerializeField] private float statusDuration = 3f;

        private AudioTherapyEngine audioEngine;
        private ResourceManager resourceManager;
        private RitualEngine ritualEngine;
        private LunarDayStateMachine stateMachine;
        private UserSessionManager sessionManager;

        private Coroutine statusCoroutine;
        private bool isHUDEnabled = true;

        private void Start()
        {
            audioEngine = AudioTherapyEngine.Instance;
            resourceManager = ResourceManager.Instance;
            ritualEngine = RitualEngine.Instance;
            stateMachine = LunarDayStateMachine.Instance;
            sessionManager = UserSessionManager.Instance;

            SetupCallbacks();
            InitializeUI();

            StartCoroutine(UpdateSessionInfo());
        }

        private void SetupCallbacks()
        {
            if (resourceManager != null)
            {
                resourceManager.OnResourceChanged += UpdateResourceUI;
                resourceManager.OnResourceCritical += OnResourceCritical;
                resourceManager.OnResourceRestored += OnResourceRestored;
                resourceManager.OnAnomalyStarted += OnAnomalyStarted;
                resourceManager.OnAnomalyResolved += OnAnomalyResolved;
            }

            if (ritualEngine != null)
            {
                ritualEngine.OnPhaseChanged += OnRitualPhaseChanged;
                ritualEngine.OnPhaseProgress += OnRitualProgress;
                ritualEngine.OnRitualStarted += OnRitualStarted;
                ritualEngine.OnRitualCompleted += OnRitualCompleted;
            }

            if (stateMachine != null)
            {
                stateMachine.OnDayChanged += OnDayChanged;
                stateMachine.OnStateChanged += OnStateChanged;
                stateMachine.OnDayProgressUpdated += OnDayProgressUpdated;
            }

            if (audioEngine != null && masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            }

            if (audioEngine != null && ambientVolumeSlider != null)
            {
                ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
            }

            if (audioEngine != null && breathVolumeSlider != null)
            {
                breathVolumeSlider.onValueChanged.AddListener(OnBreathVolumeChanged);
            }

            if (audioEngine != null && voiceVolumeSlider != null)
            {
                voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
            }

            if (breathGuideToggle != null)
            {
                breathGuideToggle.onValueChanged.AddListener(OnBreathGuideToggled);
            }
        }

        private void InitializeUI()
        {
            if (energyIndicator != null) energyIndicator.color = Color.green;
            if (oxygenIndicator != null) oxygenIndicator.color = Color.green;
            if (waterIndicator != null) waterIndicator.color = Color.green;

            if (ritualPanel != null) ritualPanel.SetActive(false);
            if (statusPanel != null) statusPanel.SetActive(false);

            if (masterVolumeSlider != null && audioEngine != null)
            {
                masterVolumeSlider.value = 0.8f;
            }

            if (ambientVolumeSlider != null)
            {
                ambientVolumeSlider.value = 0.6f;
            }

            if (breathVolumeSlider != null)
            {
                breathVolumeSlider.value = 0.45f;
            }

            if (voiceVolumeSlider != null)
            {
                voiceVolumeSlider.value = 0.7f;
            }

            ShowStatus("Welcome to Lunar Week One", Color.cyan);
        }

        private void Update()
        {
            UpdateResourceValues();
            UpdateDayProgress();
            UpdateRitualUI();
        }

        private void UpdateResourceUI(ResourceType type, float value)
        {
            Color color = GetResourceColor(value);
            string valueText = $"{value:P0}";

            switch (type)
            {
                case ResourceType.Energy:
                    if (energyIndicator != null) energyIndicator.color = color;
                    if (energySlider != null) energySlider.value = value;
                    if (energyText != null) energyText.text = valueText;
                    break;

                case ResourceType.Oxygen:
                    if (oxygenIndicator != null) oxygenIndicator.color = color;
                    if (oxygenSlider != null) oxygenSlider.value = value;
                    if (oxygenText != null) oxygenText.text = valueText;
                    break;

                case ResourceType.Water:
                    if (waterIndicator != null) waterIndicator.color = color;
                    if (waterSlider != null) waterSlider.value = value;
                    if (waterText != null) waterText.text = valueText;
                    break;
            }
        }

        private Color GetResourceColor(float value)
        {
            if (value > 0.5f) return Color.green;
            else if (value > 0.35f) return Color.yellow;
            else return Color.red;
        }

        private void UpdateResourceValues()
        {
            if (resourceManager == null) return;

            UpdateResourceUI(ResourceType.Energy, resourceManager.GetResource(ResourceType.Energy));
            UpdateResourceUI(ResourceType.Oxygen, resourceManager.GetResource(ResourceType.Oxygen));
            UpdateResourceUI(ResourceType.Water, resourceManager.GetResource(ResourceType.Water));
        }

        private void UpdateDayProgress()
        {
            if (stateMachine == null) return;

            if (dayProgressSlider != null)
            {
                float progress = stateMachine.CurrentDayTargetDuration > 0f
                    ? Mathf.Clamp01(stateMachine.CurrentDayElapsed / stateMachine.CurrentDayTargetDuration)
                    : 0f;
                dayProgressSlider.value = progress;
            }

            if (dayText != null)
            {
                dayText.text = $"Day {(int)stateMachine.CurrentDay}";
            }

            if (dayNameText != null)
            {
                dayNameText.text = GetDayName(stateMachine.CurrentDay);
            }

            if (stateText != null)
            {
                stateText.text = stateMachine.CurrentState.ToString();
            }
        }

        private string GetDayName(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival: return "Arrival";
                case LunarDay.Day2_Adaptation: return "Adaptation";
                case LunarDay.Day3_Order: return "Order";
                case LunarDay.Day4_Uncertainty: return "Uncertainty";
                case LunarDay.Day5_Ritual: return "Deep Ritual";
                case LunarDay.Day6_Stability: return "Stability";
                case LunarDay.Day7_Reflection: return "Reflection";
                default: return "Unknown";
            }
        }

        private void OnResourceCritical(ResourceType type)
        {
            ShowStatus($"Resource {type} is critical!", Color.red);
        }

        private void OnResourceRestored(ResourceType type, float value)
        {
            ShowStatus($"{type} stabilized", Color.green);
        }

        private void OnAnomalyStarted()
        {
            ShowStatus("System anomaly detected", Color.yellow);
        }

        private void OnAnomalyResolved()
        {
            ShowStatus("System restored to normal", Color.cyan);
        }

        private void OnDayChanged(LunarDay day)
        {
            ShowStatus($"Day {(int)day}: {GetDayName(day)}", Color.white);
        }

        private void OnStateChanged(LunarDayState state)
        {
            string stateName = state.ToString();
            ShowStatus($"Entering {stateName} phase", Color.blue);
        }

        private void OnDayProgressUpdated(float progress)
        {
        }

        private void OnRitualStarted()
        {
            if (ritualPanel != null) ritualPanel.SetActive(true);
            ShowStatus("Ritual started", Color.magenta);
        }

        private void OnRitualCompleted(RitualCompletionResult result)
        {
            if (ritualPanel != null) ritualPanel.SetActive(false);
            ShowStatus($"Ritual completed ({result.interactionCount} interactions)", Color.green);
        }

        private void OnRitualPhaseChanged(RitualPhase phase)
        {
            if (ritualPhaseText != null)
            {
                ritualPhaseText.text = GetPhaseName(phase);
            }

            if (ritualInstructionText != null)
            {
                ritualInstructionText.text = GetPhaseInstruction(phase);
            }
        }

        private string GetPhaseName(RitualPhase phase)
        {
            switch (phase)
            {
                case RitualPhase.Enter: return "Entering";
                case RitualPhase.Anchor: return "Anchoring";
                case RitualPhase.Order: return "Sequencing";
                case RitualPhase.Observe: return "Observing";
                case RitualPhase.Exit: return "Exiting";
                default: return "Unknown";
            }
        }

        private string GetPhaseInstruction(RitualPhase phase)
        {
            switch (phase)
            {
                case RitualPhase.Enter: return "Please settle into your position";
                case RitualPhase.Anchor: return "Focus on your breath";
                case RitualPhase.Order: return "Perform the repeated action";
                case RitualPhase.Observe: return "Simply observe";
                case RitualPhase.Exit: return "Slowly return";
                default: return "Continue";
            }
        }

        private void OnRitualProgress(RitualPhase phase, float progress)
        {
            if (ritualProgressSlider != null)
            {
                ritualProgressSlider.value = progress;
            }
        }

        private void UpdateRitualUI()
        {
            if (ritualEngine == null || !ritualEngine.IsRitualActive()) return;

            if (ritualProgressSlider != null)
            {
                ritualProgressSlider.value = ritualEngine.GetRitualProgress();
            }
        }

        private IEnumerator UpdateSessionInfo()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (sessionManager != null)
                {
                    if (sessionTimeText != null)
                    {
                        float elapsed = sessionManager.GetSessionDuration();
                        sessionTimeText.text = $"Time: {elapsed:F1}s";
                    }

                    if (interactionsText != null)
                    {
                        var session = sessionManager.GetCurrentSession();
                        interactionsText.text = $"Interactions: {session?.TotalInteractions ?? 0}";
                    }

                    if (dayCountText != null)
                    {
                        int completed = sessionManager.GetCompletedDayCount();
                        dayCountText.text = $"Days: {completed}/7";
                    }
                }
            }
        }

        public void ShowStatus(string message, Color color)
        {
            if (statusPanel == null || statusText == null) return;

            if (statusCoroutine != null)
            {
                StopCoroutine(statusCoroutine);
            }

            statusCoroutine = StartCoroutine(DisplayStatus(message, color));
        }

        private IEnumerator DisplayStatus(string message, Color color)
        {
            statusPanel.SetActive(true);
            statusText.text = message;
            statusText.color = color;

            yield return new WaitForSeconds(statusDuration);

            statusPanel.SetActive(false);
            statusCoroutine = null;
        }

        private void OnMasterVolumeChanged(float volume)
        {
            if (audioEngine != null)
            {
                audioEngine.SetMasterVolume(volume);
            }
        }

        private void OnBreathGuideToggled(bool enabled)
        {
            if (audioEngine != null)
            {
                audioEngine.SetBreathGuideActive(enabled);
            }
        }

        private void OnAmbientVolumeChanged(float volume)
        {
            if (audioEngine != null)
            {
                audioEngine.SetAmbientVolume(volume);
            }
        }

        private void OnBreathVolumeChanged(float volume)
        {
            if (audioEngine != null)
            {
                audioEngine.SetBreathVolume(volume);
            }
        }

        private void OnVoiceVolumeChanged(float volume)
        {
            if (audioEngine != null)
            {
                audioEngine.SetVoiceVolume(volume);
            }
        }

        public void ToggleHUD()
        {
            isHUDEnabled = !isHUDEnabled;
            gameObject.SetActive(isHUDEnabled);
        }

        public void SetHUDEnabled(bool enabled)
        {
            isHUDEnabled = enabled;
            gameObject.SetActive(enabled);
        }

        public bool IsHUDEnabled()
        {
            return isHUDEnabled;
        }
    }
}
