using System;
using System.Collections.Generic;
using UnityEngine;

namespace Lunar.Core
{
    public class LunarDayStateMachine : MonoBehaviour
    {
        public static LunarDayStateMachine Instance { get; private set; }

        public LunarDay CurrentDay { get; private set; } = LunarDay.Day1_Arrival;
        public LunarDayState CurrentState { get; private set; }
        public float CurrentDayElapsed { get; private set; }
        public float CurrentDayTargetDuration { get; private set; }

        public event Action<LunarDay> OnDayChanged;
        public event Action<LunarDayState> OnStateChanged;
        public event Action<float> OnDayProgressUpdated;
        public event Action OnDayCompleted;

        private Dictionary<LunarDay, LunarDayConfig> dayConfigs;
        private float stateTimer;

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

        public void Initialize(Dictionary<LunarDay, LunarDayConfig> configs)
        {
            dayConfigs = configs;
            EnterDay(LunarDay.Day1_Arrival);
        }

        public void EnterDay(LunarDay day)
        {
            CurrentDay = day;
            CurrentDayElapsed = 0f;
            CurrentDayTargetDuration = dayConfigs.ContainsKey(day) ?
                dayConfigs[day].targetDurationMinutes * 60f : 240f;

            CurrentState = LunarDayState.Entering;
            stateTimer = 0f;

            OnDayChanged?.Invoke(day);
            EnterState(LunarDayState.Introduction);
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
            switch (state)
            {
                case LunarDayState.Introduction:
                    PlayDayIntroduction();
                    break;
                case LunarDayState.ResourceManagement:
                    EnableResourceManagement();
                    break;
                case LunarDayState.Ritual:
                    StartRitual();
                    break;
                case LunarDayState.Narration:
                    PlayNarrative();
                    break;
                case LunarDayState.Transition:
                    PrepareDayTransition();
                    break;
                case LunarDayState.Completion:
                    CompleteCurrentDay();
                    break;
            }
        }

        private void Update()
        {
            if (CurrentState == LunarDayState.None) return;

            stateTimer += Time.deltaTime;
            CurrentDayElapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(CurrentDayElapsed / CurrentDayTargetDuration);
            OnDayProgressUpdated?.Invoke(progress);

            HandleStateUpdate(CurrentState);

            if (CurrentDayElapsed >= CurrentDayTargetDuration)
            {
                EnterState(LunarDayState.Completion);
            }
        }

        private void HandleStateUpdate(LunarDayState state)
        {
            switch (state)
            {
                case LunarDayState.Introduction:
                    if (stateTimer >= 30f)
                        EnterState(LunarDayState.ResourceManagement);
                    break;

                case LunarDayState.ResourceManagement:
                    if (stateTimer >= 120f && CurrentDay != LunarDay.Day1_Arrival)
                        EnterState(LunarDayState.Narration);
                    break;

                case LunarDayState.Narration:
                    if (stateTimer >= 60f)
                        EnterState(LunarDayState.Ritual);
                    break;

                case LunarDayState.Ritual:
                    RitualEngine.Instance?.CheckRitualComplete();
                    break;
            }
        }

        private void PlayDayIntroduction()
        {
            AudioTherapyEngine.Instance?.PlayIntroductionAudio(CurrentDay);
        }

        private void EnableResourceManagement()
        {
            ResourceManager.Instance?.EnableManagement();
        }

        private void StartRitual()
        {
            RitualEngine.Instance?.StartDayRitual(CurrentDay);
        }

        private void PlayNarrative()
        {
            if (dayConfigs.TryGetValue(CurrentDay, out var config))
            {
                foreach (var clip in config.narrativeClips)
                {
                    AudioTherapyEngine.Instance?.PlayNarrative(clip);
                }
            }
        }

        private void PrepareDayTransition()
        {
            if (CurrentDay < LunarDay.Day7_Reflection)
            {
                AudioTherapyEngine.Instance?.PlayTransitionSound();
            }
        }

        private void CompleteCurrentDay()
        {
            OnDayCompleted?.Invoke();

            if (dayConfigs.TryGetValue(CurrentDay, out var config) && config.hasAnomaly)
            {
                TriggerAnomaly();
            }

            if (CurrentDay < LunarDay.Day7_Reflection)
            {
                EnterDay(CurrentDay + 1);
            }
            else
            {
                EndExperience();
            }
        }

        private void TriggerAnomaly()
        {
            Debug.Log($"[LunarDayStateMachine] Anomaly triggered on {CurrentDay}");
            ResourceManager.Instance?.TriggerAnomaly();
        }

        private void EndExperience()
        {
            Debug.Log("[LunarDayStateMachine] Experience completed");
            ExperienceFeedbackCollector.Instance?.CollectFinalFeedback();
        }

        public void SkipToNextState()
        {
            switch (CurrentState)
            {
                case LunarDayState.Introduction:
                    EnterState(LunarDayState.ResourceManagement);
                    break;
                case LunarDayState.ResourceManagement:
                    EnterState(LunarDayState.Narration);
                    break;
                case LunarDayState.Narration:
                    EnterState(LunarDayState.Ritual);
                    break;
            }
        }

        public void ExitExperience()
        {
            UserSessionManager.Instance?.SaveProgress();
            CurrentState = LunarDayState.None;
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
}
