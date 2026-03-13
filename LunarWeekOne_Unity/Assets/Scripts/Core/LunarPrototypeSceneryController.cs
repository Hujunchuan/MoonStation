using UnityEngine;

namespace Lunar.Core
{
    public class LunarPrototypeSceneryController : MonoBehaviour
    {
        [SerializeField] private Renderer operationsScreenRenderer;
        [SerializeField] private Renderer breathPanelRenderer;
        [SerializeField] private Renderer observationGlassRenderer;
        [SerializeField] private Renderer earthriseRenderer;
        [SerializeField] private float refreshInterval = 0.12f;
        [SerializeField] private float pulseSpeed = 0.45f;

        private LunarDayStateMachine stateMachine;
        private LunarEnvironmentController environmentController;
        private ResourceManager resourceManager;
        private RitualEngine ritualEngine;
        private float nextRefreshTime;
        private Vector3 earthriseBasePosition;
        private Vector3 earthriseBaseScale;
        private bool earthrisePoseCaptured;

        public void Configure(Renderer operationsScreen, Renderer breathPanel, Renderer observationGlass, Renderer earthrise)
        {
            operationsScreenRenderer = operationsScreen;
            breathPanelRenderer = breathPanel;
            observationGlassRenderer = observationGlass;
            earthriseRenderer = earthrise;

            if (earthriseRenderer != null)
            {
                earthriseBasePosition = earthriseRenderer.transform.localPosition;
                earthriseBaseScale = earthriseRenderer.transform.localScale;
                earthrisePoseCaptured = true;
            }

            CacheDependencies();
            RefreshScenery();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + refreshInterval;
            RefreshScenery();
        }

        private void CacheDependencies()
        {
            if (stateMachine == null)
            {
                stateMachine = LunarDayStateMachine.Instance;
            }

            if (environmentController == null)
            {
                environmentController = FindObjectOfType<LunarEnvironmentController>();
            }

            if (resourceManager == null)
            {
                resourceManager = ResourceManager.Instance;
            }

            if (ritualEngine == null)
            {
                ritualEngine = RitualEngine.Instance;
            }
        }

        private void RefreshScenery()
        {
            CacheDependencies();

            LunarDay currentDay = stateMachine != null ? stateMachine.CurrentDay : LunarDay.Day1_Arrival;
            LunarDayState currentState = stateMachine != null ? stateMachine.CurrentState : LunarDayState.None;
            bool anomaly = resourceManager != null && resourceManager.IsAnomalyActive();
            bool awaitingInteraction = ritualEngine != null && ritualEngine.IsAwaitingRequiredInteraction();
            float pulse = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f);

            UpdateOperationsScreen(currentState, anomaly, pulse);
            UpdateBreathPanel(currentDay, currentState, anomaly, awaitingInteraction, pulse);
            UpdateObservationGlass(currentDay, anomaly, awaitingInteraction, pulse);
            UpdateEarthrise(currentDay, anomaly, pulse);
        }

        private void UpdateOperationsScreen(LunarDayState currentState, bool anomaly, float pulse)
        {
            Color color = new Color(0.16f, 0.42f, 0.56f, 0.58f);
            float emission = 0.22f + pulse * 0.16f;

            if (anomaly)
            {
                color = new Color(0.72f, 0.18f, 0.14f, 0.7f);
                emission = 0.7f + pulse * 0.65f;
            }
            else if (currentState == LunarDayState.ResourceManagement)
            {
                color = new Color(0.18f, 0.54f, 0.7f, 0.64f);
                emission = 0.44f + pulse * 0.24f;
            }
            else if (currentState == LunarDayState.Ritual)
            {
                color = new Color(0.2f, 0.62f, 0.74f, 0.62f);
                emission = 0.56f + pulse * 0.32f;
            }
            else if (currentState == LunarDayState.Transition)
            {
                color = new Color(0.24f, 0.34f, 0.48f, 0.54f);
                emission = 0.28f + pulse * 0.18f;
            }

            ApplyVisualMaterial(operationsScreenRenderer, color, color * emission);
        }

        private void UpdateBreathPanel(LunarDay currentDay, LunarDayState currentState, bool anomaly, bool awaitingInteraction, float pulse)
        {
            Color color = new Color(0.14f, 0.28f, 0.34f, 0.52f);
            float emission = 0.12f;

            if (anomaly)
            {
                color = new Color(0.38f, 0.12f, 0.1f, 0.6f);
                emission = 0.45f + pulse * 0.45f;
            }
            else if (awaitingInteraction || currentState == LunarDayState.Ritual)
            {
                color = new Color(0.18f, 0.5f, 0.62f, 0.58f);
                emission = 0.38f + pulse * 0.4f;
            }
            else if (currentState == LunarDayState.Introduction ||
                currentState == LunarDayState.Narration ||
                currentDay == LunarDay.Day7_Reflection)
            {
                color = new Color(0.2f, 0.56f, 0.6f, 0.56f);
                emission = 0.26f + pulse * 0.28f;
            }

            ApplyVisualMaterial(breathPanelRenderer, color, color * emission);
        }

        private void UpdateObservationGlass(LunarDay currentDay, bool anomaly, bool awaitingInteraction, float pulse)
        {
            Color color = new Color(0.18f, 0.32f, 0.42f, 0.48f);
            float emission = 0.05f;

            if (anomaly)
            {
                color = new Color(0.46f, 0.22f, 0.16f, 0.52f);
                emission = 0.2f + pulse * 0.18f;
            }
            else if (awaitingInteraction)
            {
                color = new Color(0.16f, 0.42f, 0.52f, 0.5f);
                emission = 0.16f + pulse * 0.1f;
            }
            else if (currentDay == LunarDay.Day7_Reflection)
            {
                color = new Color(0.12f, 0.2f, 0.36f, 0.54f);
                emission = 0.18f + pulse * 0.12f;
            }

            ApplyVisualMaterial(observationGlassRenderer, color, new Color(color.r, color.g, color.b) * emission);
        }

        private void UpdateEarthrise(LunarDay currentDay, bool anomaly, float pulse)
        {
            if (earthriseRenderer == null)
            {
                return;
            }

            if (!earthrisePoseCaptured)
            {
                earthriseBasePosition = earthriseRenderer.transform.localPosition;
                earthriseBaseScale = earthriseRenderer.transform.localScale;
                earthrisePoseCaptured = true;
            }

            float dayProgress = Mathf.InverseLerp((int)LunarDay.Day1_Arrival, (int)LunarDay.Day7_Reflection, (int)currentDay);
            Vector3 offset = new Vector3(
                Mathf.Lerp(1.1f, -0.35f, dayProgress),
                Mathf.Lerp(-1.35f, 0.18f, dayProgress),
                0f);

            earthriseRenderer.transform.localPosition = earthriseBasePosition + offset;
            earthriseRenderer.transform.localScale = earthriseBaseScale * Mathf.Lerp(0.84f, 1.22f, dayProgress);
            earthriseRenderer.transform.Rotate(Vector3.up, 8f * Time.deltaTime, Space.Self);

            Color color = new Color(0.18f, 0.42f, 0.82f);
            float emission = Mathf.Lerp(0.08f, 0.42f, dayProgress) + pulse * 0.04f;

            if (anomaly)
            {
                color = new Color(0.66f, 0.24f, 0.18f);
                emission = 0.16f + pulse * 0.08f;
            }
            else if (currentDay == LunarDay.Day7_Reflection)
            {
                color = new Color(0.3f, 0.58f, 0.95f);
                emission = 0.48f + pulse * 0.2f;
            }

            ApplyVisualMaterial(earthriseRenderer, color, color * emission);
        }

        private void ApplyVisualMaterial(Renderer renderer, Color color, Color emissionColor)
        {
            if (renderer == null)
            {
                return;
            }

            Material material = renderer.material;
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emissionColor);
            }
        }
    }
}
