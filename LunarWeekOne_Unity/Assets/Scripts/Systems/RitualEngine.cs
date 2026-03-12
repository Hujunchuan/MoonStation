using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lunar.Core
{
    public class RitualEngine : MonoBehaviour
    {
        public static RitualEngine Instance { get; private set; }

        [Header("Ritual Settings")]
        [SerializeField] private float deepRitualDuration = 420f;
        [SerializeField] private float normalRitualDuration = 180f;
        [SerializeField] private float interactionCooldown = 2f;

        [Header("Voice Scripts")]
        [SerializeField] private TextAsset enterScript;
        [SerializeField] private TextAsset anchorScript;
        [SerializeField] private TextAsset orderScript;
        [SerializeField] private TextAsset observeScript;
        [SerializeField] private TextAsset exitScript;

        [Header("Visual Elements")]
        [SerializeField] private GameObject ritualIndicator;
        [SerializeField] private Light[] ritualLights;
        [SerializeField] private Material valveMaterial;

        private RitualPhase currentPhase = RitualPhase.None;
        private float phaseTimer;
        private bool isRitualActive;
        private bool isDeepRitual;
        private LunarDay currentRitualDay;

        private int interactionCount;
        private float lastInteractionTime;
        private float ritualStartTime;

        private Dictionary<RitualPhase, RitualPhaseConfig> phaseConfigs;

        public event Action<RitualPhase> OnPhaseChanged;
        public event Action<RitualPhase, float> OnPhaseProgress;
        public event Action OnRitualStarted;
        public event Action<RitualCompletionResult> OnRitualCompleted;

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

            InitializePhaseConfigs();
        }

        private void InitializePhaseConfigs()
        {
            phaseConfigs = new Dictionary<RitualPhase, RitualPhaseConfig>
            {
                { RitualPhase.Enter, new RitualPhaseConfig { durationSeconds = 30f, requiresInteraction = false } },
                { RitualPhase.Anchor, new RitualPhaseConfig { durationSeconds = 90f, requiresInteraction = false } },
                { RitualPhase.Order, new RitualPhaseConfig { durationSeconds = 60f, requiresInteraction = true } },
                { RitualPhase.Observe, new RitualPhaseConfig { durationSeconds = 120f, requiresInteraction = false } },
                { RitualPhase.Exit, new RitualPhaseConfig { durationSeconds = 30f, requiresInteraction = false } }
            };
        }

        private void Start()
        {
            if (ritualIndicator != null)
            {
                ritualIndicator.SetActive(false);
            }
        }

        public void StartDayRitual(LunarDay day)
        {
            currentRitualDay = day;
            isDeepRitual = (day == LunarDay.Day5_Ritual);
            isRitualActive = true;
            interactionCount = 0;
            ritualStartTime = Time.time;

            AudioTherapyEngine.Instance?.SetRitualMode(true);
            ResourceManager.Instance?.DisableManagement();

            EnterPhase(RitualPhase.Enter);
            OnRitualStarted?.Invoke();

            ShowRitualIndicator();
        }

        public void SkipRitual()
        {
            if (!isRitualActive) return;

            CompleteRitual(false);
        }

        public void CheckRitualComplete()
        {
            if (!isRitualActive) return;

            float ritualDuration = isDeepRitual ? deepRitualDuration : normalRitualDuration;
            if (Time.time - ritualStartTime >= ritualDuration)
            {
                if (currentPhase == RitualPhase.Exit)
                {
                    CompleteRitual(true);
                }
                else if (currentPhase == RitualPhase.Observe)
                {
                    EnterPhase(RitualPhase.Exit);
                }
            }
        }

        public void PerformRitualInteraction(string targetName)
        {
            if (!isRitualActive) return;
            if (currentPhase != RitualPhase.Order) return;

            if (Time.time - lastInteractionTime < interactionCooldown)
            {
                return;
            }

            lastInteractionTime = Time.time;
            interactionCount++;

            AudioTherapyEngine.Instance?.PlayInteractionFeedback(ResourceType.Energy);
            StartCoroutine(AnimateInteraction(targetName));
        }

        public void PerformValveInteraction()
        {
            PerformRitualInteraction("valve");
        }

        private void EnterPhase(RitualPhase phase)
        {
            currentPhase = phase;
            phaseTimer = 0f;

            OnPhaseChanged?.Invoke(phase);

            float phaseDuration = phaseConfigs.ContainsKey(phase) ?
                phaseConfigs[phase].durationSeconds :
                (isDeepRitual && phase == RitualPhase.Observe ? 120f : 60f);

            StartCoroutine(ExecutePhase(phase, phaseDuration));
        }

        private IEnumerator ExecutePhase(RitualPhase phase, float duration)
        {
            switch (phase)
            {
                case RitualPhase.Enter:
                    yield return ExecuteEnterPhase(duration);
                    break;
                case RitualPhase.Anchor:
                    yield return ExecuteAnchorPhase(duration);
                    break;
                case RitualPhase.Order:
                    yield return ExecuteOrderPhase(duration);
                    break;
                case RitualPhase.Observe:
                    yield return ExecuteObservePhase(duration);
                    break;
                case RitualPhase.Exit:
                    yield return ExecuteExitPhase(duration);
                    break;
            }
        }

        private IEnumerator ExecuteEnterPhase(float duration)
        {
            DimBaseLights();

            string script = GetScriptForPhase(RitualPhase.Enter);
            if (!string.IsNullOrEmpty(script))
            {
                PlayVoiceover(script);
            }

            yield return WaitWithProgress(duration);

            EnterPhase(RitualPhase.Anchor);
        }

        private IEnumerator ExecuteAnchorPhase(float duration)
        {
            AudioTherapyEngine.Instance?.SetBreathGuideActive(true);

            string script = GetScriptForPhase(RitualPhase.Anchor);
            if (!string.IsNullOrEmpty(script))
            {
                PlayVoiceover(script);
            }

            yield return WaitWithProgress(duration);

            EnterPhase(RitualPhase.Order);
        }

        private IEnumerator ExecuteOrderPhase(float duration)
        {
            ShowValveInteraction();

            string script = GetScriptForPhase(RitualPhase.Order);
            if (!string.IsNullOrEmpty(script))
            {
                PlayVoiceover(script);
            }

            yield return WaitWithProgress(duration);

            EnterPhase(RitualPhase.Observe);
        }

        private IEnumerator ExecuteObservePhase(float duration)
        {
            HideValveInteraction();

            string script = GetScriptForPhase(RitualPhase.Observe);
            if (!string.IsNullOrEmpty(script))
            {
                PlayVoiceover(script);
            }

            yield return WaitWithProgress(duration);

            EnterPhase(RitualPhase.Exit);
        }

        private IEnumerator ExecuteExitPhase(float duration)
        {
            string script = GetScriptForPhase(RitualPhase.Exit);
            if (!string.IsNullOrEmpty(script))
            {
                PlayVoiceover(script);
            }

            yield return WaitWithProgress(duration);

            CompleteRitual(true);
        }

        private IEnumerator WaitWithProgress(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                OnPhaseProgress?.Invoke(currentPhase, progress);
                yield return null;
            }
        }

        private void PlayVoiceover(string script)
        {
            AudioTherapyEngine.Instance?.PlayRitualAudio("Ritual_" + currentPhase.ToString());
        }

        private void DimBaseLights()
        {
            foreach (var light in ritualLights)
            {
                StartCoroutine(SmoothLightDim(light, 0.3f, 2f));
            }
        }

        private void RestoreBaseLights()
        {
            foreach (var light in ritualLights)
            {
                StartCoroutine(SmoothLightDim(light, 1f, 2f));
            }
        }

        private IEnumerator SmoothLightDim(Light light, float targetIntensity, float duration)
        {
            float startIntensity = light.intensity;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                light.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
                yield return null;
            }

            light.intensity = targetIntensity;
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
                valveMaterial.SetColor("_EmissionColor", Color.cyan * 2f);
            }
        }

        private void HideValveInteraction()
        {
            if (valveMaterial != null)
            {
                valveMaterial.DisableKeyword("_EMISSION");
            }
        }

        private IEnumerator AnimateInteraction(string targetName)
        {
            float animDuration = 0.5f;
            float elapsed = 0f;

            while (elapsed < animDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private void CompleteRitual(bool completed)
        {
            isRitualActive = false;

            RestoreBaseLights();
            HideRitualIndicator();
            HideValveInteraction();

            AudioTherapyEngine.Instance?.SetRitualMode(false);
            AudioTherapyEngine.Instance?.PlayRitualCompletion();

            UserSessionManager.Instance?.RecordRitualCompletion(currentRitualDay, currentPhase, completed, interactionCount);

            var result = new RitualCompletionResult
            {
                day = currentRitualDay,
                completed = completed,
                interactionCount = interactionCount,
                duration = Time.time - ritualStartTime
            };

            OnRitualCompleted?.Invoke(result);

            LunarDayStateMachine.Instance?.SkipToNextState();
        }

        private string GetScriptForPhase(RitualPhase phase)
        {
            TextAsset asset = null;
            switch (phase)
            {
                case RitualPhase.Enter: asset = enterScript; break;
                case RitualPhase.Anchor: asset = anchorScript; break;
                case RitualPhase.Order: asset = orderScript; break;
                case RitualPhase.Observe: asset = observeScript; break;
                case RitualPhase.Exit: asset = exitScript; break;
            }
            return asset != null ? asset.text : string.Empty;
        }

        public bool IsRitualActive()
        {
            return isRitualActive;
        }

        public RitualPhase GetCurrentPhase()
        {
            return currentPhase;
        }

        public float GetRitualProgress()
        {
            if (!isRitualActive) return 0f;
            float totalDuration = (isDeepRitual ? deepRitualDuration : normalRitualDuration);
            return Mathf.Clamp01((Time.time - ritualStartTime) / totalDuration);
        }
    }

    public class RitualPhaseConfig
    {
        public float durationSeconds;
        public string audioClipName;
        public string voiceoverScript;
        public bool requiresInteraction;
        public string interactionTarget;
    }

    public struct RitualCompletionResult
    {
        public LunarDay day;
        public bool completed;
        public int interactionCount;
        public float duration;
    }
}
