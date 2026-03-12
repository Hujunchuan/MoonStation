using System;
using System.Collections;
using System.Collections.Generic;
using Lunar.Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Lunar.Core
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("Thresholds")]
        [SerializeField] private float safeThreshold = 0.5f;
        [SerializeField] private float warningThreshold = 0.35f;

        [Header("Visual Feedback")]
        [SerializeField] private Light baseLight;
        [SerializeField] private Material[] resourceIndicatorMaterials;
        [SerializeField] private Volume postProcessVolume;

        [Header("Settings")]
        [SerializeField] private bool managementEnabled;
        [SerializeField] private float interactionDelay = 0.4f;
        [SerializeField] private float animationDuration = 0.4f;

        private readonly Dictionary<ResourceType, float> resourceValues = new Dictionary<ResourceType, float>();
        private readonly Dictionary<ResourceType, ResourceConfig> resourceConfigs = new Dictionary<ResourceType, ResourceConfig>();
        private readonly ResourceType[] trackedTypes = (ResourceType[])Enum.GetValues(typeof(ResourceType));

        private float lastInteractionTime;
        private bool isAnomalyActive;

        public event Action<ResourceType, float> OnResourceChanged;
        public event Action<ResourceType> OnResourceCritical;
        public event Action<ResourceType, float> OnResourceRestored;
        public event Action OnAnomalyStarted;
        public event Action OnAnomalyResolved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            BuildDefaultConfigs();
            ResetResources();
        }

        private void Update()
        {
            if (!managementEnabled || isAnomalyActive)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            foreach (ResourceType type in trackedTypes)
            {
                float currentValue = GetResource(type);
                float decayRate = GetConfig(type).decayRate;
                float minimum = GetConfig(type).minValue;

                if (currentValue <= minimum)
                {
                    continue;
                }

                SetResource(type, Mathf.Max(minimum, currentValue - decayRate * deltaTime));
            }
        }

        private void BuildDefaultConfigs()
        {
            resourceConfigs[ResourceType.Energy] = new ResourceConfig
            {
                type = ResourceType.Energy,
                decayRate = 0.002f,
                recoveryRate = 0.12f
            };
            resourceConfigs[ResourceType.Oxygen] = new ResourceConfig
            {
                type = ResourceType.Oxygen,
                decayRate = 0.0012f,
                recoveryRate = 0.1f
            };
            resourceConfigs[ResourceType.Water] = new ResourceConfig
            {
                type = ResourceType.Water,
                decayRate = 0.0015f,
                recoveryRate = 0.1f
            };
        }

        public void ApplyDayConfig(LunarDayConfig config)
        {
            if (config == null)
            {
                return;
            }

            foreach (ResourceConfig resourceConfig in config.resources)
            {
                resourceConfigs[resourceConfig.type] = resourceConfig;

                if (!resourceValues.ContainsKey(resourceConfig.type))
                {
                    resourceValues[resourceConfig.type] = Mathf.Clamp(resourceConfig.initialValue, 0f, 1f);
                }
            }

            UpdateAllVisualFeedback();
        }

        public void EnableManagement()
        {
            managementEnabled = true;
        }

        public void DisableManagement()
        {
            managementEnabled = false;
        }

        public void SetResource(ResourceType type, float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            float previousValue = resourceValues.ContainsKey(type) ? resourceValues[type] : 0f;
            resourceValues[type] = clampedValue;

            UpdateVisualFeedback(type, clampedValue);
            OnResourceChanged?.Invoke(type, clampedValue);

            if (previousValue > warningThreshold && clampedValue <= warningThreshold)
            {
                OnResourceCritical?.Invoke(type);
            }
            else if (previousValue <= warningThreshold && clampedValue > warningThreshold)
            {
                OnResourceRestored?.Invoke(type, clampedValue);
            }
        }

        public float GetResource(ResourceType type)
        {
            if (!resourceValues.ContainsKey(type))
            {
                resourceValues[type] = GetConfig(type).initialValue;
            }

            return resourceValues[type];
        }

        public void PerformResourceAction(ResourceType type)
        {
            if (!managementEnabled)
            {
                return;
            }

            if (Time.time - lastInteractionTime < interactionDelay)
            {
                return;
            }

            lastInteractionTime = Time.time;
            StartCoroutine(AnimateResourceAction(type));
            AudioTherapyEngine.Instance?.PlayInteractionFeedback(type);
            UserSessionManager.Instance?.RecordInteraction();
        }

        private IEnumerator AnimateResourceAction(ResourceType type)
        {
            ResourceConfig config = GetConfig(type);
            float startValue = GetResource(type);
            float targetValue = Mathf.Clamp(startValue + config.recoveryRate, 0f, config.maxValue);
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / animationDuration);
                SetResource(type, Mathf.Lerp(startValue, targetValue, t));
                yield return null;
            }

            SetResource(type, targetValue);
        }

        public void TriggerAnomaly()
        {
            if (isAnomalyActive)
            {
                return;
            }

            isAnomalyActive = true;
            OnAnomalyStarted?.Invoke();
            StartCoroutine(AnimateAnomaly());
        }

        private IEnumerator AnimateAnomaly()
        {
            const float anomalyDuration = 6f;
            float elapsed = 0f;

            while (elapsed < anomalyDuration)
            {
                elapsed += Time.deltaTime;
                float flicker = Mathf.PingPong(elapsed * 4f, 0.3f);

                if (baseLight != null)
                {
                    baseLight.intensity = 0.6f + flicker;
                }

                foreach (ResourceType type in trackedTypes)
                {
                    float noisyValue = GetResource(type) + UnityEngine.Random.Range(-0.03f, 0.03f);
                    SetResource(type, noisyValue);
                }

                yield return null;
            }

            ResolveAnomaly();
        }

        private void ResolveAnomaly()
        {
            isAnomalyActive = false;
            UpdateAllVisualFeedback();
            OnAnomalyResolved?.Invoke();
        }

        private void UpdateAllVisualFeedback()
        {
            foreach (ResourceType type in trackedTypes)
            {
                UpdateVisualFeedback(type, GetResource(type));
            }

            UpdateBaseLighting();
        }

        private void UpdateVisualFeedback(ResourceType type, float value)
        {
            int materialIndex = (int)type;
            if (resourceIndicatorMaterials != null &&
                materialIndex >= 0 &&
                materialIndex < resourceIndicatorMaterials.Length &&
                resourceIndicatorMaterials[materialIndex] != null)
            {
                Material material = resourceIndicatorMaterials[materialIndex];
                Color targetColor = GetColorForValue(value, GetConfig(type));

                if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", targetColor);
                }

                if (material.HasProperty("_EmissionColor"))
                {
                    material.SetColor("_EmissionColor", targetColor * 2f);
                }
            }

            UpdateBaseLighting();
        }

        private void UpdateBaseLighting()
        {
            if (baseLight != null)
            {
                float average = GetAverageResourceLevel();
                baseLight.intensity = Mathf.Lerp(0.3f, 1f, average);
            }

            if (postProcessVolume != null && postProcessVolume.profile != null &&
                postProcessVolume.profile.TryGet(out ColorAdjustments colorAdjustments))
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(-0.5f, 0.3f, GetAverageResourceLevel());
            }
        }

        private Color GetColorForValue(float value, ResourceConfig config)
        {
            if (value > safeThreshold)
            {
                return config.safeColor;
            }

            if (value > warningThreshold)
            {
                return config.warningColor;
            }

            return config.criticalColor;
        }

        private ResourceConfig GetConfig(ResourceType type)
        {
            if (!resourceConfigs.ContainsKey(type))
            {
                resourceConfigs[type] = new ResourceConfig { type = type };
            }

            return resourceConfigs[type];
        }

        public bool IsResourceSafe(ResourceType type)
        {
            return GetResource(type) > warningThreshold;
        }

        public float GetAverageResourceLevel()
        {
            float total = 0f;

            foreach (ResourceType type in trackedTypes)
            {
                total += GetResource(type);
            }

            return total / trackedTypes.Length;
        }

        public void ResetResources()
        {
            foreach (ResourceType type in trackedTypes)
            {
                resourceValues[type] = Mathf.Clamp(GetConfig(type).initialValue, 0f, 1f);
            }

            isAnomalyActive = false;
            UpdateAllVisualFeedback();
        }

        public void RestoreResourceLevels(float energy, float oxygen, float water)
        {
            resourceValues[ResourceType.Energy] = Mathf.Clamp01(energy);
            resourceValues[ResourceType.Oxygen] = Mathf.Clamp01(oxygen);
            resourceValues[ResourceType.Water] = Mathf.Clamp01(water);
            UpdateAllVisualFeedback();
        }

        public void ConfigurePrototypeVisuals(Light statusLight, Material[] indicatorMaterials, Volume volume = null)
        {
            baseLight = statusLight;
            resourceIndicatorMaterials = indicatorMaterials;

            if (volume != null)
            {
                postProcessVolume = volume;
            }

            UpdateAllVisualFeedback();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
