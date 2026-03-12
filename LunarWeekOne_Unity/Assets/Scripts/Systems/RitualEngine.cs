using System;
using System.Collections;
using Lunar.Data;
using UnityEngine;

namespace Lunar.Core
{
    public class RitualEngine : MonoBehaviour
    {
        public static RitualEngine Instance { get; private set; }

        [Header("Ritual Presentation")]
        [SerializeField] private GameObject ritualIndicator;
        [SerializeField] private Light[] ritualLights;
        [SerializeField] private Material valveMaterial;

        [Header("Interaction Settings")]
        [SerializeField] private float interactionCooldown = 1f;

        private RitualConfig currentRitualConfig;
        private RitualPhaseConfig currentPhaseConfig;
        private RitualPhase currentPhase = RitualPhase.None;
        private LunarDay currentRitualDay = LunarDay.Day1_Arrival;
        private Coroutine ritualRoutine;
        private bool isRitualActive;
        private bool phaseInteractionSatisfied;
        private int interactionCount;
        private float lastInteractionTime;
        private float ritualStartTime;
        private float totalRitualDuration;

        public event Action<RitualPhase> OnPhaseChanged;
        public event Action<RitualPhase, float> OnPhaseProgress;
        public event Action OnRitualStarted;
        public event Action<RitualCompletionResult> OnRitualCompleted;

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
            if (ritualIndicator != null)
            {
                ritualIndicator.SetActive(false);
            }
        }

        public void StartDayRitual(LunarDay day, RitualConfig config)
        {
            if (config == null || config.phases.Count == 0)
            {
                LunarDayStateMachine.Instance?.NotifyRitualCompleted();
                return;
            }

            StopCurrentRitual();

            currentRitualDay = day;
            currentRitualConfig = config;
            currentPhase = RitualPhase.None;
            currentPhaseConfig = null;
            isRitualActive = true;
            interactionCount = 0;
            ritualStartTime = Time.time;
            totalRitualDuration = CalculateTotalDuration(config);

            ShowRitualIndicator();
            DimBaseLights();
            AudioTherapyEngine.Instance?.SetRitualMode(true);
            ResourceManager.Instance?.DisableManagement();

            ritualRoutine = StartCoroutine(RunRitual());
            OnRitualStarted?.Invoke();
        }

        public void SkipRitual()
        {
            if (!isRitualActive)
            {
                return;
            }

            CompleteRitual(false);
        }

        private IEnumerator RunRitual()
        {
            foreach (RitualPhaseConfig phase in currentRitualConfig.phases)
            {
                currentPhase = phase.phase;
                currentPhaseConfig = phase;
                phaseInteractionSatisfied = !phase.requiresInteraction;

                EnterPhasePresentation(phase);
                OnPhaseChanged?.Invoke(currentPhase);

                float elapsed = 0f;
                while (elapsed < phase.durationSeconds || (phase.requiresInteraction && !phaseInteractionSatisfied))
                {
                    if (elapsed < phase.durationSeconds)
                    {
                        elapsed += Time.deltaTime;
                    }

                    float durationProgress = phase.durationSeconds <= 0f
                        ? 1f
                        : Mathf.Clamp01(elapsed / phase.durationSeconds);
                    float progress = phase.requiresInteraction && !phaseInteractionSatisfied && durationProgress >= 1f
                        ? 0.98f
                        : durationProgress;

                    OnPhaseProgress?.Invoke(currentPhase, progress);
                    yield return null;
                }
            }

            CompleteRitual(true);
        }

        private void EnterPhasePresentation(RitualPhaseConfig phase)
        {
            if (phase.requiresInteraction)
            {
                ShowValveInteraction();
            }
            else
            {
                HideValveInteraction();
            }

            string clipName = string.IsNullOrWhiteSpace(phase.audioClipName)
                ? $"Ritual_{phase.phase}"
                : phase.audioClipName;
            AudioTherapyEngine.Instance?.PlayRitualAudio(clipName);
        }

        public void PerformRitualInteraction(string targetName)
        {
            if (!isRitualActive || currentPhaseConfig == null || !currentPhaseConfig.requiresInteraction)
            {
                return;
            }

            if (!string.IsNullOrEmpty(currentPhaseConfig.interactionTarget) &&
                !string.Equals(currentPhaseConfig.interactionTarget, targetName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (Time.time - lastInteractionTime < interactionCooldown)
            {
                return;
            }

            lastInteractionTime = Time.time;
            interactionCount++;
            phaseInteractionSatisfied = true;

            AudioTherapyEngine.Instance?.PlayInteractionFeedback(ResourceType.Energy);
            UserSessionManager.Instance?.RecordInteraction();
            StartCoroutine(AnimateInteraction());
        }

        public void PerformValveInteraction()
        {
            PerformRitualInteraction("valve");
        }

        private IEnumerator AnimateInteraction()
        {
            if (valveMaterial != null)
            {
                valveMaterial.EnableKeyword("_EMISSION");
                valveMaterial.SetColor("_EmissionColor", Color.cyan * 2f);
            }

            yield return new WaitForSeconds(0.2f);

            if (!isRitualActive || currentPhaseConfig == null || !currentPhaseConfig.requiresInteraction)
            {
                HideValveInteraction();
            }
        }

        private void CompleteRitual(bool completed)
        {
            StopCurrentRitual();
            RestoreBaseLights();
            HideRitualIndicator();
            HideValveInteraction();

            AudioTherapyEngine.Instance?.SetRitualMode(false);
            AudioTherapyEngine.Instance?.PlayRitualCompletion();

            UserSessionManager.Instance?.RecordRitualCompletion(
                currentRitualDay,
                currentPhase,
                completed,
                interactionCount);

            OnRitualCompleted?.Invoke(new RitualCompletionResult
            {
                day = currentRitualDay,
                completed = completed,
                interactionCount = interactionCount,
                duration = Time.time - ritualStartTime
            });

            LunarDayStateMachine.Instance?.NotifyRitualCompleted();
        }

        private void StopCurrentRitual()
        {
            if (ritualRoutine != null)
            {
                StopCoroutine(ritualRoutine);
                ritualRoutine = null;
            }

            isRitualActive = false;
        }

        private float CalculateTotalDuration(RitualConfig config)
        {
            float duration = 0f;

            foreach (RitualPhaseConfig phase in config.phases)
            {
                duration += phase.durationSeconds;
            }

            return Mathf.Max(duration, 1f);
        }

        private void ShowRitualIndicator()
        {
            if (ritualIndicator != null)
            {
                ritualIndicator.SetActive(true);
            }
        }

        private void HideRitualIndicator()
        {
            if (ritualIndicator != null)
            {
                ritualIndicator.SetActive(false);
            }
        }

        private void ShowValveInteraction()
        {
            if (valveMaterial != null)
            {
                valveMaterial.EnableKeyword("_EMISSION");
                valveMaterial.SetColor("_EmissionColor", Color.cyan * 1.5f);
            }
        }

        private void HideValveInteraction()
        {
            if (valveMaterial != null)
            {
                valveMaterial.DisableKeyword("_EMISSION");
            }
        }

        private void DimBaseLights()
        {
            if (ritualLights == null)
            {
                return;
            }

            foreach (Light light in ritualLights)
            {
                if (light != null)
                {
                    light.intensity = 0.3f;
                }
            }
        }

        private void RestoreBaseLights()
        {
            if (ritualLights == null)
            {
                return;
            }

            foreach (Light light in ritualLights)
            {
                if (light != null)
                {
                    light.intensity = 1f;
                }
            }
        }

        public bool IsRitualActive()
        {
            return isRitualActive;
        }

        public RitualPhase GetCurrentPhase()
        {
            return currentPhase;
        }

        public bool IsAwaitingRequiredInteraction()
        {
            return isRitualActive &&
                currentPhaseConfig != null &&
                currentPhaseConfig.requiresInteraction &&
                !phaseInteractionSatisfied;
        }

        public float GetRitualProgress()
        {
            if (!isRitualActive)
            {
                return 0f;
            }

            return Mathf.Clamp01((Time.time - ritualStartTime) / totalRitualDuration);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    public struct RitualCompletionResult
    {
        public LunarDay day;
        public bool completed;
        public int interactionCount;
        public float duration;
    }
}
