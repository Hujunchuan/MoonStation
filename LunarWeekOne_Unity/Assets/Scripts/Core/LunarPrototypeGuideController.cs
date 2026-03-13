using UnityEngine;

namespace Lunar.Core
{
    public class LunarPrototypeGuideController : MonoBehaviour
    {
        [SerializeField] private Renderer[] corridorGuides;
        [SerializeField] private Renderer[] resourceGuides;
        [SerializeField] private Renderer[] ritualGuides;
        [SerializeField] private float refreshInterval = 0.12f;
        [SerializeField] private float pulseSpeed = 1.8f;

        private ResourceManager resourceManager;
        private RitualEngine ritualEngine;
        private LunarDayStateMachine stateMachine;
        private LunarEnvironmentController environmentController;
        private float nextRefreshTime;

        public void Configure(Renderer[] corridor, Renderer[] resources, Renderer[] ritual)
        {
            corridorGuides = corridor;
            resourceGuides = resources;
            ritualGuides = ritual;
            CacheDependencies();
            RefreshGuides();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + refreshInterval;
            RefreshGuides();
        }

        private void CacheDependencies()
        {
            if (resourceManager == null)
            {
                resourceManager = ResourceManager.Instance;
            }

            if (ritualEngine == null)
            {
                ritualEngine = RitualEngine.Instance;
            }

            if (stateMachine == null)
            {
                stateMachine = LunarDayStateMachine.Instance;
            }

            if (environmentController == null)
            {
                environmentController = FindObjectOfType<LunarEnvironmentController>();
            }
        }

        private void RefreshGuides()
        {
            CacheDependencies();

            float pulse = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * pulseSpeed * Mathf.PI * 2f);

            if (resourceManager != null && resourceManager.IsAnomalyActive())
            {
                ApplyGuideSet(corridorGuides, new Color(1f, 0.42f, 0.24f), 0.75f + pulse * 0.65f);
                ApplyGuideSet(resourceGuides, new Color(0.78f, 0.24f, 0.18f), 0.16f + pulse * 0.1f);
                ApplyGuideSet(ritualGuides, new Color(1f, 0.52f, 0.26f), 0.6f + pulse * 0.5f);
                return;
            }

            LunarDayState currentState = stateMachine != null ? stateMachine.CurrentState : LunarDayState.None;
            bool awaitingInteraction = ritualEngine != null && ritualEngine.IsAwaitingRequiredInteraction();

            if (currentState == LunarDayState.Ritual || awaitingInteraction)
            {
                ApplyGuideSet(corridorGuides, new Color(0.16f, 0.38f, 0.48f), 0.12f);
                ApplyGuideSet(resourceGuides, new Color(0.14f, 0.22f, 0.24f), 0.06f);
                ApplyGuideSet(ritualGuides, new Color(0.28f, 0.86f, 1f), awaitingInteraction ? 0.8f + pulse * 1.1f : 0.72f);
                return;
            }

            if (currentState == LunarDayState.ResourceManagement)
            {
                ApplyGuideSet(corridorGuides, new Color(0.14f, 0.3f, 0.38f), 0.1f + pulse * 0.08f);
                HighlightLowestResourceGuide(pulse);
                ApplyGuideSet(ritualGuides, new Color(0.12f, 0.18f, 0.2f), 0.04f);
                return;
            }

            if (currentState == LunarDayState.Transition)
            {
                ApplyGuideSet(corridorGuides, new Color(0.3f, 0.72f, 0.9f), 0.28f + pulse * 0.14f);
                ApplyGuideSet(resourceGuides, new Color(0.18f, 0.24f, 0.28f), 0.05f);
                ApplyGuideSet(ritualGuides, new Color(0.24f, 0.58f, 0.74f), 0.14f);
                return;
            }

            Color corridorColor = GetCalmCorridorColor();
            ApplyGuideSet(corridorGuides, corridorColor, 0.2f + pulse * 0.1f);
            ApplyGuideSet(resourceGuides, new Color(0.12f, 0.18f, 0.2f), 0.05f);
            ApplyGuideSet(ritualGuides, new Color(0.14f, 0.22f, 0.26f), 0.06f);
        }

        private void HighlightLowestResourceGuide(float pulse)
        {
            if (resourceGuides == null || resourceGuides.Length == 0)
            {
                return;
            }

            int index = GetLowestResourceIndex();

            for (int i = 0; i < resourceGuides.Length; i++)
            {
                bool isActive = i == index;
                Color color = GetResourceGuideColor(i);
                float emission = isActive ? 0.55f + pulse * 0.75f : 0.08f;
                ApplyRenderer(resourceGuides[i], color, emission);
            }
        }

        private int GetLowestResourceIndex()
        {
            if (resourceManager == null)
            {
                return 0;
            }

            float energy = resourceManager.GetResource(ResourceType.Energy);
            float oxygen = resourceManager.GetResource(ResourceType.Oxygen);
            float water = resourceManager.GetResource(ResourceType.Water);

            if (energy <= oxygen && energy <= water)
            {
                return 0;
            }

            if (oxygen <= water)
            {
                return 1;
            }

            return 2;
        }

        private Color GetCalmCorridorColor()
        {
            if (environmentController != null &&
                environmentController.GetCurrentDay() == LunarDay.Day7_Reflection)
            {
                return new Color(0.22f, 0.34f, 0.52f);
            }

            return new Color(0.2f, 0.56f, 0.7f);
        }

        private Color GetResourceGuideColor(int index)
        {
            switch (index)
            {
                case 0:
                    return new Color(0.95f, 0.74f, 0.2f);

                case 1:
                    return new Color(0.4f, 0.82f, 1f);

                case 2:
                    return new Color(0.24f, 0.9f, 0.72f);

                default:
                    return new Color(0.78f, 0.84f, 0.9f);
            }
        }

        private void ApplyGuideSet(Renderer[] renderers, Color color, float emissionStrength)
        {
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                ApplyRenderer(renderers[i], color, emissionStrength);
            }
        }

        private void ApplyRenderer(Renderer renderer, Color color, float emissionStrength)
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
                material.SetColor("_Color", Color.Lerp(color * 0.28f, color, Mathf.Clamp01(emissionStrength)));
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * Mathf.Max(0.04f, emissionStrength));
            }
        }
    }
}
