using System;
using System.Collections.Generic;
using Lunar.Data;
using UnityEngine;

namespace Lunar.Core
{
    public class LunarDayStateMachine : MonoBehaviour
    {
        public static LunarDayStateMachine Instance { get; private set; }

        public LunarDay CurrentDay { get; private set; } = LunarDay.Day1_Arrival;
        public LunarDayState CurrentState { get; private set; } = LunarDayState.None;
        public float CurrentDayElapsed { get; private set; }
        public float CurrentDayTargetDuration { get; private set; }

        public event Action<LunarDay> OnDayChanged;
        public event Action<LunarDayState> OnStateChanged;
        public event Action<float> OnDayProgressUpdated;
        public event Action<LunarDay> OnDayCompleted;
        public event Action OnExperienceCompleted;

        private Dictionary<LunarDay, LunarDayConfig> dayConfigs = new Dictionary<LunarDay, LunarDayConfig>();
        private float stateTimer;
        private bool isInitialized;

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

        public void Initialize(Dictionary<LunarDay, LunarDayConfig> configs, LunarDay startDay)
        {
            dayConfigs = configs ?? new Dictionary<LunarDay, LunarDayConfig>();
            isInitialized = dayConfigs.Count > 0;

            if (!isInitialized)
            {
                Debug.LogError("[LunarDayStateMachine] No day configs available");
                return;
            }

            EnterDay(startDay);
        }

        public void EnterDay(LunarDay day)
        {
            if (!dayConfigs.TryGetValue(day, out var config))
            {
                Debug.LogError($"[LunarDayStateMachine] Missing config for {day}");
                return;
            }

            CurrentDay = day;
            CurrentDayElapsed = 0f;
            CurrentDayTargetDuration = Mathf.Max(60f, config.targetDurationMinutes * 60f);
            stateTimer = 0f;

            OnDayChanged?.Invoke(day);
            EnterState(LunarDayState.Introduction);
        }

        public LunarDayConfig GetCurrentConfig()
        {
            dayConfigs.TryGetValue(CurrentDay, out var config);
            return config;
        }

        private void EnterState(LunarDayState state)
        {
            CurrentState = state;
            stateTimer = 0f;

            OnStateChanged?.Invoke(state);
            HandleStateEnter(state);
        }

        private void HandleStateEnter(LunarDayState state)
        {
            LunarDayConfig config = GetCurrentConfig();

            switch (state)
            {
                case LunarDayState.Introduction:
                    AudioTherapyEngine.Instance?.PlayIntroductionAudio(CurrentDay);
                    break;

                case LunarDayState.Narration:
                    if (config != null)
                    {
                        foreach (string clip in config.narrativeClips)
                        {
                            AudioTherapyEngine.Instance?.PlayNarrative(clip);
                        }
                    }
                    break;

                case LunarDayState.Ritual:
                    if (config?.ritual != null && config.enableRitual)
                    {
                        RitualEngine.Instance?.StartDayRitual(CurrentDay, config.ritual);
                    }
                    else
                    {
                        EnterState(LunarDayState.Completion);
                    }
                    break;

                case LunarDayState.Completion:
                    CompleteCurrentDay();
                    break;

                case LunarDayState.Transition:
                    AudioTherapyEngine.Instance?.PlayTransitionSound();
                    break;
            }
        }

        private void Update()
        {
            if (!isInitialized || CurrentState == LunarDayState.None || CurrentState == LunarDayState.Completion)
            {
                return;
            }

            stateTimer += Time.deltaTime;
            CurrentDayElapsed += Time.deltaTime;

            float progress = CurrentDayTargetDuration <= 0f
                ? 0f
                : Mathf.Clamp01(CurrentDayElapsed / CurrentDayTargetDuration);
            OnDayProgressUpdated?.Invoke(progress);

            if (CurrentDayElapsed >= CurrentDayTargetDuration)
            {
                EnterState(LunarDayState.Completion);
                return;
            }

            HandleStateUpdate();
        }

        private void HandleStateUpdate()
        {
            LunarDayConfig config = GetCurrentConfig();
            if (config == null)
            {
                return;
            }

            switch (CurrentState)
            {
                case LunarDayState.Introduction:
                    if (stateTimer >= config.introductionDurationSeconds)
                    {
                        EnterState(LunarDayState.ResourceManagement);
                    }
                    break;

                case LunarDayState.ResourceManagement:
                    if (stateTimer >= config.resourceDurationSeconds)
                    {
                        EnterState(GetPostResourceState(config));
                    }
                    break;

                case LunarDayState.Narration:
                    if (stateTimer >= config.narrationDurationSeconds)
                    {
                        EnterState(config.enableRitual ? LunarDayState.Ritual : LunarDayState.Completion);
                    }
                    break;
            }
        }

        private LunarDayState GetPostResourceState(LunarDayConfig config)
        {
            if (config.enableNarration && config.narrativeClips.Count > 0)
            {
                return LunarDayState.Narration;
            }

            if (config.enableRitual)
            {
                return LunarDayState.Ritual;
            }

            return LunarDayState.Completion;
        }

        private void CompleteCurrentDay()
        {
            LunarDay finishedDay = CurrentDay;
            OnDayCompleted?.Invoke(finishedDay);

            if (finishedDay >= LunarDay.Day7_Reflection)
            {
                CurrentState = LunarDayState.None;
                OnExperienceCompleted?.Invoke();
                return;
            }

            EnterDay((LunarDay)((int)finishedDay + 1));
        }

        public void NotifyRitualCompleted()
        {
            if (CurrentState == LunarDayState.Ritual)
            {
                EnterState(LunarDayState.Completion);
            }
        }

        public void SkipToNextState()
        {
            LunarDayConfig config = GetCurrentConfig();
            if (config == null)
            {
                return;
            }

            switch (CurrentState)
            {
                case LunarDayState.Introduction:
                    EnterState(LunarDayState.ResourceManagement);
                    break;

                case LunarDayState.ResourceManagement:
                    EnterState(GetPostResourceState(config));
                    break;

                case LunarDayState.Narration:
                    EnterState(config.enableRitual ? LunarDayState.Ritual : LunarDayState.Completion);
                    break;

                case LunarDayState.Ritual:
                case LunarDayState.Transition:
                    EnterState(LunarDayState.Completion);
                    break;
            }
        }

        public void ExitExperience()
        {
            UserSessionManager.Instance?.SaveProgress();
            CurrentState = LunarDayState.None;
            stateTimer = 0f;
            isInitialized = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    public enum LunarDayState
    {
        None,
        Entering,
        Introduction,
        ResourceManagement,
        Narration,
        Ritual,
        Transition,
        Completion
    }
}
