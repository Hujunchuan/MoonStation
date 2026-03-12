using System.Collections;
using System.Collections.Generic;
using Lunar.Data;
using UnityEngine;

namespace Lunar.Core
{
    public class LunarExperienceController : MonoBehaviour
    {
        public static LunarExperienceController Instance { get; private set; }

        [Header("System References")]
        [SerializeField] private LunarDayStateMachine stateMachine;
        [SerializeField] private LunarEnvironmentController environmentController;
        [SerializeField] private AudioTherapyEngine audioEngine;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private RitualEngine ritualEngine;
        [SerializeField] private UserSessionManager sessionManager;
        [SerializeField] private ExperienceFeedbackCollector feedbackCollector;

        [Header("Configuration")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool loadSavedProgress = true;
        [SerializeField] private bool enableDebugShortcuts;
        [SerializeField] private float experienceTimeoutSeconds = 1800f;

        private Dictionary<LunarDay, LunarDayConfig> dayConfigs = new Dictionary<LunarDay, LunarDayConfig>();
        private bool callbacksRegistered;
        private bool hasStarted;
        private bool isExperienceActive;
        private float experienceStartTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            hasStarted = true;

            if (autoInitialize)
            {
                InitializeExperience();
            }
        }

        public void InitializeExperience()
        {
            if (isExperienceActive)
            {
                return;
            }

            ResolveDependencies();
            if (!ValidateDependencies())
            {
                return;
            }

            dayConfigs = LunarDefaultConfigFactory.CreateDayConfigs();

            if (sessionManager.HasActiveSession())
            {
                sessionManager.ResumeActiveSession();
            }
            else if (loadSavedProgress && sessionManager.HasSavedProgress())
            {
                sessionManager.LoadProgress();
            }
            else
            {
                sessionManager.InitializeNewSession();
            }

            RegisterCallbacks();

            experienceStartTime = Time.time;
            isExperienceActive = true;

            LunarDay startDay = sessionManager.GetCurrentDayEnum();
            stateMachine.Initialize(dayConfigs, startDay);

            if (dayConfigs.TryGetValue(startDay, out var config))
            {
                resourceManager.ApplyDayConfig(config);
            }

            var session = sessionManager.GetCurrentSession();
            resourceManager.RestoreResourceLevels(
                session.energyLevel,
                session.oxygenLevel,
                session.waterLevel);

            sessionManager.UpdateResourceLevels(
                resourceManager.GetResource(ResourceType.Energy),
                resourceManager.GetResource(ResourceType.Oxygen),
                resourceManager.GetResource(ResourceType.Water));

            audioEngine.StartAmbientLayer();
            environmentController?.SetDay(startDay);

            StartCoroutine(ExperienceTimeoutCheck());
        }

        private void ResolveDependencies()
        {
            if (stateMachine == null)
            {
                stateMachine = LunarDayStateMachine.Instance;
            }

            if (audioEngine == null)
            {
                audioEngine = AudioTherapyEngine.Instance;
            }

            if (resourceManager == null)
            {
                resourceManager = ResourceManager.Instance;
            }

            if (ritualEngine == null)
            {
                ritualEngine = RitualEngine.Instance;
            }

            if (sessionManager == null)
            {
                sessionManager = UserSessionManager.Instance;
            }

            if (feedbackCollector == null)
            {
                feedbackCollector = ExperienceFeedbackCollector.Instance;
            }
        }

        private bool ValidateDependencies()
        {
            bool isValid = true;
            isValid &= ValidateReference(stateMachine, nameof(stateMachine));
            isValid &= ValidateReference(audioEngine, nameof(audioEngine));
            isValid &= ValidateReference(resourceManager, nameof(resourceManager));
            isValid &= ValidateReference(ritualEngine, nameof(ritualEngine));
            isValid &= ValidateReference(sessionManager, nameof(sessionManager));
            isValid &= ValidateReference(feedbackCollector, nameof(feedbackCollector));
            return isValid;
        }

        private bool ValidateReference(Object reference, string fieldName)
        {
            if (reference != null)
            {
                return true;
            }

            Debug.LogError($"[LunarExperienceController] Missing required reference: {fieldName}");
            return false;
        }

        private void RegisterCallbacks()
        {
            if (callbacksRegistered)
            {
                UnregisterCallbacks();
            }

            resourceManager.OnResourceChanged += HandleResourceChanged;
            resourceManager.OnAnomalyStarted += HandleAnomalyStarted;
            resourceManager.OnAnomalyResolved += HandleAnomalyResolved;

            stateMachine.OnDayChanged += HandleDayChanged;
            stateMachine.OnDayCompleted += HandleDayCompleted;
            stateMachine.OnStateChanged += HandleStateChanged;
            stateMachine.OnExperienceCompleted += HandleExperienceCompleted;

            ritualEngine.OnRitualCompleted += HandleRitualCompleted;
            callbacksRegistered = true;
        }

        private void UnregisterCallbacks()
        {
            if (!callbacksRegistered)
            {
                return;
            }

            if (resourceManager != null)
            {
                resourceManager.OnResourceChanged -= HandleResourceChanged;
                resourceManager.OnAnomalyStarted -= HandleAnomalyStarted;
                resourceManager.OnAnomalyResolved -= HandleAnomalyResolved;
            }

            if (stateMachine != null)
            {
                stateMachine.OnDayChanged -= HandleDayChanged;
                stateMachine.OnDayCompleted -= HandleDayCompleted;
                stateMachine.OnStateChanged -= HandleStateChanged;
                stateMachine.OnExperienceCompleted -= HandleExperienceCompleted;
            }

            if (ritualEngine != null)
            {
                ritualEngine.OnRitualCompleted -= HandleRitualCompleted;
            }

            callbacksRegistered = false;
        }

        private void HandleResourceChanged(ResourceType type, float value)
        {
            sessionManager.UpdateResourceLevels(
                resourceManager.GetResource(ResourceType.Energy),
                resourceManager.GetResource(ResourceType.Oxygen),
                resourceManager.GetResource(ResourceType.Water));
        }

        private void HandleAnomalyStarted()
        {
            audioEngine.SetAmbientFrequency(85f);
            environmentController?.SetEnvironmentState(LunarEnvironmentController.EnvironmentState.Anomaly);
        }

        private void HandleAnomalyResolved()
        {
            audioEngine.SetAmbientFrequency(60f);
            environmentController?.SetDay(stateMachine.CurrentDay);
        }

        private void HandleDayChanged(LunarDay day)
        {
            sessionManager.SetCurrentDay(day);

            if (dayConfigs.TryGetValue(day, out var config))
            {
                resourceManager.ApplyDayConfig(config);
            }

            environmentController?.SetDay(day);
        }

        private void HandleDayCompleted(LunarDay day)
        {
            sessionManager.CompleteDay((int)day);
        }

        private void HandleStateChanged(LunarDayState state)
        {
            switch (state)
            {
                case LunarDayState.ResourceManagement:
                    resourceManager.EnableManagement();
                    break;

                case LunarDayState.Ritual:
                case LunarDayState.Introduction:
                case LunarDayState.Narration:
                case LunarDayState.Completion:
                case LunarDayState.Transition:
                    resourceManager.DisableManagement();
                    break;
            }
        }

        private void HandleRitualCompleted(RitualCompletionResult result)
        {
            if (result.completed)
            {
                Debug.Log($"[LunarExperienceController] Ritual completed for {result.day}");
            }
        }

        private void HandleExperienceCompleted()
        {
            EndExperience();
        }

        private IEnumerator ExperienceTimeoutCheck()
        {
            while (isExperienceActive)
            {
                if (Time.time - experienceStartTime >= experienceTimeoutSeconds)
                {
                    EndExperience(true);
                    yield break;
                }

                yield return new WaitForSeconds(2f);
            }
        }

        public void EndExperience(bool timeout = false)
        {
            if (!isExperienceActive)
            {
                return;
            }

            ShutdownExperience(timeout, true);
        }

        public void SuspendExperienceForMenu()
        {
            if (!isExperienceActive)
            {
                return;
            }

            ShutdownExperience(false, false);
        }

        public void ConfigureAutoInitialize(bool enabled, bool initializeNow = false)
        {
            autoInitialize = enabled;

            if (initializeNow && autoInitialize && hasStarted && !isExperienceActive)
            {
                InitializeExperience();
            }
        }

        public bool IsExperienceActive()
        {
            return isExperienceActive;
        }

        public float GetExperienceProgress()
        {
            if (!isExperienceActive || experienceTimeoutSeconds <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp01((Time.time - experienceStartTime) / experienceTimeoutSeconds);
        }

        public float GetElapsedTime()
        {
            return isExperienceActive ? Time.time - experienceStartTime : 0f;
        }

        private void ShutdownExperience(bool timeout, bool collectFeedback)
        {
            isExperienceActive = false;

            if (timeout)
            {
                Debug.LogWarning("[LunarExperienceController] Experience timed out");
            }

            stateMachine.ExitExperience();
            resourceManager.DisableManagement();
            audioEngine.StopAllAudio();
            sessionManager.SaveProgress();

            if (collectFeedback)
            {
                feedbackCollector.CollectFinalFeedback();
            }
        }

        private void Update()
        {
            if (!enableDebugShortcuts || !isExperienceActive)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndExperience();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                resourceManager.PerformResourceAction(ResourceType.Energy);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ritualEngine.PerformValveInteraction();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                stateMachine.SkipToNextState();
            }
        }

        private void OnDestroy()
        {
            UnregisterCallbacks();

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
