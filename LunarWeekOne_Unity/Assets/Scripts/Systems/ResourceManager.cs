using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Lunar.Core
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("Resource Values")]
        [SerializeField] private float energyLevel = 0.8f;
        [SerializeField] private float oxygenLevel = 0.8f;
        [SerializeField] private float waterLevel = 0.8f;

        [Header("Thresholds")]
        [SerializeField] private float safeThreshold = 0.5f;
        [SerializeField] private float warningThreshold = 0.35f;

        [Header("Visual Feedback")]
        [SerializeField] private Light baseLight;
        [SerializeField] private Material[] resourceIndicatorMaterials;
        [SerializeField] private Volume postProcessVolume;

        [Header("Settings")]
        [SerializeField] private bool managementEnabled = false;
        [SerializeField] private float interactionDelay = 0.4f;
        [SerializeField] private float animationDuration = 2f;

        private float lastInteractionTime;
        private bool isAnomalyActive;
        private Dictionary<ResourceType, float> resourceValues = new Dictionary<ResourceType, float>();

        public event Action<ResourceType, float> OnResourceChanged;
        public event Action<ResourceType> OnResourceCritical;
        public event Action<ResourceType, float> OnResourceRestored;
        public event Action OnAnomalyStarted;
        public event Action OnAnomalyResolved;

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

            InitializeResources();
        }

        private void InitializeResources()
        {
            resourceValues[ResourceType.Energy] = energyLevel;
            resourceValues[ResourceType.Oxygen] = oxygenLevel;
            resourceValues[ResourceType.Water] = waterLevel;
        }

        private void Start()
        {
            UpdateAllVisualFeedback();
        }

        private void Update()
        {
            if (!managementEnabled || isAnomalyActive) return;

            float deltaTime = Time.deltaTime;

            foreach (var kvp in resourceValues)
            {
                if (kvp.Value > warningThreshold)
                {
                    float newValue = Mathf.Max(warningThreshold, kvp.Value - GetDecayRate(kvp.Key) * deltaTime);
                    if (Mathf.Abs(newValue - kvp.Value) > 0.001f)
                    {
                        SetResource(kvp.Key, newValue);
                    }
                }
            }
        }

        public void EnableManagement()
        {
            managementEnabled = true;
            StartResourceDecay();
        }

        public void DisableManagement()
        {
            managementEnabled = false;
        }

        public void SetResource(ResourceType type, float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            float oldValue = resourceValues.ContainsKey(type) ? resourceValues[type] : 0f;
            resourceValues[type] = clampedValue;

            OnResourceChanged?.Invoke(type, clampedValue);
            UpdateVisualFeedback(type, clampedValue);

            if (oldValue > warningThreshold && clampedValue <= warningThreshold)
            {
                OnResourceCritical?.Invoke(type);
            }
            else if (oldValue <= warningThreshold && clampedValue > warningThreshold)
            {
                OnResourceRestored?.Invoke(type, clampedValue);
            }
        }

        public float GetResource(ResourceType type)
        {
            return resourceValues.ContainsKey(type) ? resourceValues[type] : 0f;
        }

        public void PerformResourceAction(ResourceType type)
        {
            if (!managementEnabled) return;

            if (Time.time - lastInteractionTime < interactionDelay)
            {
                return;
            }

            lastInteractionTime = Time.time;

            AudioTherapyEngine.Instance?.PlayInteractionFeedback(type);

            StartCoroutine(AnimateResourceAction(type));
        }

        private System.Collections.IEnumerator AnimateResourceAction(ResourceType type)
        {
            float startValue = GetResource(type);
            float targetValue = Mathf.Min(1f, startValue + 0.1f);
            float elapsed = 0f;

            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;
                float currentValue = Mathf.Lerp(startValue, targetValue, t);
                SetResource(type, currentValue);
                yield return null;
            }

            SetResource(type, targetValue);
        }

        private void UpdateAllVisualFeedback()
        {
            foreach (var kvp in resourceValues)
            {
                UpdateVisualFeedback(kvp.Key, kvp.Value);
            }
            UpdateBaseLighting();
        }

        private void UpdateVisualFeedback(ResourceType type, float value)
        {
            if (resourceIndicatorMaterials == null || resourceIndicatorMaterials.Length < 3)
            {
                UpdateBaseLighting();
                return;
            }

            int matIndex = (int)type;
            if (matIndex < 0 || matIndex >= resourceIndicatorMaterials.Length) return;

            Material mat = resourceIndicatorMaterials[matIndex];
            if (mat == null) return;

            Color targetColor = GetColorForValue(value);

            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", targetColor * 2f);
            }
            if (mat.HasProperty("_Color"))
            {
                mat.SetColor("_Color", targetColor);
            }
        }

        private void UpdateBaseLighting()
        {
            if (baseLight == null) return;

            float avgValue = (GetResource(ResourceType.Energy) + GetResource(ResourceType.Oxygen) + GetResource(ResourceType.Water)) / 3f;
            float brightness = Mathf.Lerp(0.3f, 1f, avgValue);

            baseLight.intensity = brightness;

            if (postProcessVolume != null && postProcessVolume.profile.TryGet(out ColorAdjustments colorAdj))
            {
                colorAdj.postExposure.value = Mathf.Lerp(-0.5f, 0.5f, avgValue);
            }
        }

        private Color GetColorForValue(float value)
        {
            if (value > safeThreshold)
            {
                return new Color(0.2f, 0.9f, 0.4f);
            }
            else if (value > warningThreshold)
            {
                return new Color(0.9f, 0.7f, 0.2f);
            }
            else
            {
                return new Color(0.9f, 0.3f, 0.2f);
            }
        }

        private float GetDecayRate(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Energy: return 0.002f;
                case ResourceType.Oxygen: return 0.001f;
                case ResourceType.Water: return 0.0015f;
                default: return 0.001f;
            }
        }

        private void StartResourceDecay()
        {
            foreach (var type in Enum.GetValues(typeof(ResourceType)))
            {
                if (resourceValues.ContainsKey((ResourceType)type) &&
                    resourceValues[(ResourceType)type] > warningThreshold)
                {
                    OnResourceRestored?.Invoke((ResourceType)type, resourceValues[(ResourceType)type]);
                }
            }
        }

        public void TriggerAnomaly()
        {
            if (isAnomalyActive) return;

            isAnomalyActive = true;
            OnAnomalyStarted?.Invoke();

            StartCoroutine(AnimateAnomaly());
        }

        private System.Collections.IEnumerator AnimateAnomaly()
        {
            float anomalyDuration = 8f;
            float elapsed = 0f;

            while (elapsed < anomalyDuration)
            {
                elapsed += Time.deltaTime;

                float flicker = Mathf.PingPong(elapsed * 5f, 0.3f);

                if (baseLight != null)
                {
                    baseLight.intensity = 0.6f + flicker;
                }

                foreach (var kvp in resourceValues)
                {
                    float noisyValue = kvp.Value + UnityEngine.Random.Range(-0.05f, 0.05f);
                    SetResource(kvp.Key, Mathf.Clamp01(noisyValue));
                }

                yield return null;
            }

            ResolveAnomaly();
        }

        private void ResolveAnomaly()
        {
            isAnomalyActive = false;
            OnAnomalyResolved?.Invoke();

            UpdateAllVisualFeedback();
        }

        public bool IsResourceSafe(ResourceType type)
        {
            return GetResource(type) > warningThreshold;
        }

        public float GetAverageResourceLevel()
        {
            float sum = 0f;
            int count = 0;
            foreach (var kvp in resourceValues)
            {
                sum += kvp.Value;
                count++;
            }
            return count > 0 ? sum / count : 0f;
        }

        public void ResetResources()
        {
            InitializeResources();
            isAnomalyActive = false;
            UpdateAllVisualFeedback();
        }
    }
}
