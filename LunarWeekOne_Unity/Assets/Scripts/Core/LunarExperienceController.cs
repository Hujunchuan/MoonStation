using System.Collections.Generic;
using UnityEngine;

namespace Lunar.Core
{
    public class LunarExperienceController : MonoBehaviour
    {
        public static LunarExperienceController Instance { get; private set; }

        [Header("System References")]
        [SerializeField] private LunarDayStateMachine stateMachine;
        [SerializeField] private AudioTherapyEngine audioEngine;
        [SerializeField] private ResourceManager resourceManager;
        [SerializeField] private RitualEngine ritualEngine;
        [SerializeField] private UserSessionManager sessionManager;
        [SerializeField] private ExperienceFeedbackCollector feedbackCollector;

        [Header("Configuration")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool loadSavedProgress = true;
        [SerializeField] private float experienceTimeoutSeconds = 1800f;

        private Dictionary<LunarDay, LunarDayConfig> dayConfigs;
        private bool isExperienceActive;
        private float experienceStartTime;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (autoInitialize)
            {
                InitializeExperience();
            }
        }

        public void InitializeExperience()
        {
            LoadDayConfigurations();

            if (loadSavedProgress)
            {
                sessionManager.LoadProgress();
            }
            else
            {
                sessionManager.InitializeNewSession();
            }

            SetupSystemCallbacks();

            experienceStartTime = Time.time;
            isExperienceActive = true;

            stateMachine.Initialize(dayConfigs);
            audioEngine.StartAmbientLayer();

            StartCoroutine(ExperienceTimeoutCheck());
        }

        private void LoadDayConfigurations()
        {
            dayConfigs = new Dictionary<LunarDay, LunarDayConfig>();

            for (int i = 1; i <= LunarConstants.TOTAL_LUNAR_DAYS; i++)
            {
                var day = (LunarDay)i;
                dayConfigs[day] = CreateDayConfig(day);
            }
        }

        private LunarDayConfig CreateDayConfig(LunarDay day)
        {
            var config = new LunarDayConfig
            {
                dayNumber = (int)day,
                dayName = $"Day {(int)day}: {GetDayName(day)}",
                theme = GetDayTheme(day),
                targetDurationMinutes = GetDayDuration(day),
                hasAnomaly = ShouldHaveAnomaly(day),
                anomalyChance = GetAnomalyChance(day)
            };

            config.narrativeClips.AddRange(GetNarrativeClipsForDay(day));
            config.documentaryClips.AddRange(GetDocumentaryClipsForDay(day));
            config.ritual = CreateRitualConfigForDay(day);

            return config;
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

        private string GetDayTheme(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival: return "混乱、警报";
                case LunarDay.Day2_Adaptation: return "适应、熟悉";
                case LunarDay.Day3_Order: return "任务执行、建立规律";
                case LunarDay.Day4_Uncertainty: return "模拟系统故障、视觉干扰";
                case LunarDay.Day5_Ritual: return "核心冥想体验";
                case LunarDay.Day6_Stability: return "自动化运行、平静";
                case LunarDay.Day7_Reflection: return "地球升起、总结";
                default: return "主题";
            }
        }

        private float GetDayDuration(LunarDay day)
        {
            return 4f;
        }

        private bool ShouldHaveAnomaly(LunarDay day)
        {
            return day == LunarDay.Day4_Uncertainty;
        }

        private float GetAnomalyChance(LunarDay day)
        {
            return day == LunarDay.Day4_Uncertainty ? kindly0.5f : 0f;
        }

        private List<string> GetNarrativeClipsForDay(LunarDay day)
        {
            var clips = new List<string>();
            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    clips.Add("Narrative_Day1_Arrival");
                    break;
                case LunarDay.Day2_Adaptation:
                    clips.Add("Narrative_Day2_Adaptation");
                    break;
                case LunarDay.Day3_Order:
                    clips.Add("Narrative_Day3_Order");
                    break;
                case LunarDay.Day4_Uncertainty:
                    clips.Add("Narrative_Day4_Uncertainty");
                    break;
                case LunarDay.Day5_Ritual:
                    clips.Add("Narrative_Day5_Ritual");
                    break;
                case LunarDay.Day6_Stability:
                    clips.Add("Narrative_Day6_Stability");
                    break;
                case LunarDay.Day7_Reflection:
                    clips.Add("Narrative_Day7_Reflection");
                    break;
            }
            return clips;
        }

        private List<string> GetDocumentaryClipsForDay(LunarDay day)
        {
            var clips = new List<string>();
            if (day == LunarDay.Day3_Order || day == LunarDay.Day4_Uncertainty)
            {
                clips.Add("Documentary_Apollo");
            }
            return clips;
        }

        private RitualConfig CreateRitualConfigForDay(LunarDay day)
        {
            var config = new RitualConfig
            {
                targetDay = day,
                ritualName = $"Day {(int)day} Ritual",
                description = GetRitualDescription(day),
                isDeepRitual = (day == LunarDay.Day5_Ritual)
            };

            config.phases.AddRange(GetRitualPhasesForDay(day));
            return config;
        }

        private string GetRitualDescription(LunarDay day)
        {
            switch (day)
            {
                case LunarDay.Day1_Arrival: return "降落仪式 - 落地/安顿";
                case LunarDay.Day2_Adaptation: return "空间仪式 - 熟悉空间";
                case LunarDay.Day3_Order: return "任务仪式 - 建立节奏";
                case LunarDay.Day4_Uncertainty: return "波动仪式 - 接受不确定";
                case LunarDay.Day5_Ritual: return "深度仪式 - 内在探索";
                case LunarDay.Day6_Stability: return "稳固仪式 - 强化习惯";
                case LunarDay.Day7_Reflection: return "回望仪式 - 文明视角";
                default: return "仪式";
            }
        }

        private List<RitualPhaseConfig> GetRitualPhasesForDay(LunarDay day)
        {
            var phases = new List<RitualPhaseConfig>();

            switch (day)
            {
                case LunarDay.Day5_Ritual:
                    phases.Add(new RitualPhaseConfig { phase = RitualPhase.Enter, durationSeconds = 30f });
                    phases.Add(new RitualPhaseConfig { phase = RitualPhase.Anchor, durationSeconds = 90f });
                    phases.Add(new RitualPhaseConfig { phase = RitualPhase.Order, durationSeconds = 60f, requiresInteraction = true });
                    phases.Add(new RitualPhaseConfig { phase = RitualPhase.Observe, durationSeconds = 120f });
                    phases.Add(new RitualPhaseConfig { phase = RitualPhase.Exit, durationSeconds = 30f });
                    break;

                default:
                    phases.Add(new RitualPhaseConfig { phase = RitualPhase.Enter, durationSeconds = 15f });
                    phases.Add(new RitualPhaseConfig { phase = RitualPhase.Anchor, durationSeconds = 45f });
                    phases.Add(new RitualPhaseConfig { phase = RitualPhase.Observe, durationSeconds =绝大多数 60f });
                    phases.Add(new RitualPhaseConfig { phase = RitualPhase.Exit, durationSeconds = 15f });
                    break;
            }

            return phases;
        }

        private void SetupSystemCallbacks()
        {
            if (resourceManager != null)
            {
                resourceManager.OnResourceChanged += HandleResourceChanged;
                resourceManager.OnAnomalyStarted += HandleAnomalyStarted;
                resourceManager.OnAnomalyResolved += HandleAnomalyResolved;
            }

            if (stateMachine != null)
            {
                stateMachine.OnDayChanged += HandleDayChanged;
                stateMachine.OnDayCompleted += HandleDayCompleted;
                stateMachine.OnStateChanged += HandleStateChanged;
            }

            if (ritualEngine != null)
            {
                ritualEngine.OnRitualCompleted += HandleRitualCompleted;
            }
        }

        private void HandleResourceChanged(ResourceType type, float value)
        {
            sessionManager.UpdateResourceLevels(
                resourceManager.GetResource(ResourceType.Energy),
                resourceManager.GetResource(ResourceType.Oxygen),
                resourceManager.GetResource(ResourceType.Water)
            );

            sessionManager.RecordInteraction();
        }

        private void HandleAnomalyStarted()
        {
            audioEngine.SetAmbientFrequency(85f);
        }

        private void HandleAnomalyResolved()
        {
            audioEngine.SetAmbientFrequency(60f);
        }

        private void HandleDayChanged(LunarDay day)
        {
            Debug.Log($"[ExperienceController] Day changed to {day}");
            audioEngine.PlayIntroductionAudio(day);
        }

        private void HandleDayCompleted()
        {
            int currentDay = sessionManager.GetCurrentDay();
            sessionManager.CompleteDay(currentDay);
        }

        private void HandleStateChanged(LunarDayState state)
        {
            Debug.Log($"[ExperienceController] State changed to {state}");

            switch (state)
            {
                case LunarDayState.ResourceManagement:
                    resourceManager.EnableManagement();
                    break;

                case LunarDayState.Completion:
                    resourceManager.DisableManagement();
                    break;
            }
        }

        private void HandleRitualCompleted(RitualCompletionResult result)
        {
            Debug.Log($"[ExperienceController] Ritual completed on {result.day}, Interaction count: {result.interactionCount}");

            if (result.day == LunarDay.Day5_Ritual && result.completed)
            {
                Debug.Log("[ExperienceController] ✓ Deep ritual completed successfully");
            }
        }

        private System.Collections.IEnumerator ExperienceTimeoutCheck()
        {
            while (isExperienceActive)
            {
                if (Time.time - experienceStartTime >= experienceTimeoutSeconds)
                {
                    EndExperience(true);
                    break;
                }
                yield return new WaitForSeconds(5f);
            }
        }

        public void EndExperience(bool timeout = false)
        {
            if (!isExperienceActive) return;

            isExperienceActive = false;

            if (timeout)
            {
                Debug.LogWarning("[ExperienceController] Experience timeout reached");
            }

            stateMachine.ExitExperience();
            audioEngine.StopAllAudio();

            feedbackCollector.CollectFinalFeedback();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndExperience();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                resourceManager.PerformResourceAction(ResourceType.Energy);
            }

            if (Input.GetKeyDown(KeyCode.R) && ritualEngine.IsRitualActive())
            {
                ritualEngine.PerformValveInteraction();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                stateMachine.SkipToNextState();
            }
        }

        public bool IsExperienceActive()
        {
            return isExperienceActive;
        }

        public float GetExperienceProgress()
        {
            return Mathf.Clamp01((Time.time - experienceStartTime) / experienceTimeoutSeconds);
        }

        public float GetElapsedTime()
        {
            return Time.time - experienceStartTime;
        }

        private void OnDestroy()
        {
            if (isExperienceActive)
            {
                EndExperience();
            }
        }
    }

    [System.Serializable]
    public class LunarDayConfig
    {
        public int dayNumber;
        public string dayName;
        public string theme;
        public float targetDurationMinutes = 4f;
        public List<string> narrativeClips = new List<string>();
        public List<string> documentaryClips = new List<string>();
        public RitualConfig ritual;
        public bool hasAnomaly = false;
        public float anomalyChance = 0f;
    }

    [System.Serializable]
    public class RitualConfig
    {
        public LunarDay targetDay;
        public string ritualName;
        public string description;
        public List<RitualPhaseConfig> phases = new List<RitualPhaseConfig>();
        public bool isDeepRitual = false;
    }

    [System.Serializable]
    public class RitualPhaseConfig
    {
        public RitualPhase phase;
        public float durationSeconds;
        public string audioClipName;
        public string voiceoverScript;
        public bool requiresInteraction;
        public string interactionTarget;
    }
}