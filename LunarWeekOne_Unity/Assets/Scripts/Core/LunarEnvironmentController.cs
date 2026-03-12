using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Lunar.Core
{
    public class LunarEnvironmentController : MonoBehaviour
    {
        [Header("Lighting")]
        [SerializeField] private Light mainDirectionalLight;
        [SerializeField] private Light[] interiorLights;
        [SerializeField] private Light ritualAmbientLight;
        [SerializeField] private float dayLightIntensity = 0.8f;
        [SerializeField] private float nightLightIntensity = 0.2f;

        [Header("Fog")]
        [SerializeField] private float baseFogDensity = 0.01f;
        [SerializeField] private float anomalyFogDensity = 0.05f;

        [Header("Post Processing")]
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private float colorTempDay = 6500f;
        [SerializeField] private float colorTempNight = 3200f;

        [Header("Visual Effects")]
        [SerializeField] private ParticleSystem dustParticles;
        [SerializeField] private ParticleSystem anomalyParticles;
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Texture2D daySkybox;
        [SerializeField] private Texture2D nightSkybox;

        [Header("Environment States")]
        [SerializeField] private EnvironmentState currentState = EnvironmentState.Normal;
        [SerializeField] private LunarDay currentDay = LunarDay.Day1_Arrival;

        private ColorAdjustments colorAdjustments;
        private Bloom bloom;
        private Vignette vignette;
        private FilmGrain filmGrain;

        private Coroutine environmentTransitionCoroutine;

        public event Action<EnvironmentState> OnEnvironmentStateChanged;

        public enum EnvironmentState
        {
            Normal,
            Anomaly,
            Ritual,
            Transition
        }

        private void Start()
        {
            InitializePostProcessing();
            InitializeLighting();
            InitializeEffects();

            SetEnvironmentState(currentState);
        }

        private void InitializePostProcessing()
        {
            if (postProcessVolume != null)
            {
                if (postProcessVolume.profile.TryGet(out colorAdjustments))
                {
                    colorAdjustments.colorFilter.value = Color.white;
                    colorAdjustments.postExposure.value = 0f;
                }

                if (postProcessVolume.profile.TryGet(out bloom))
                {
                    bloom.intensity.value = 0.5f;
                    bloom.threshold.value = 0.8f;
                }

                if (postProcessVolume.profile.TryGet(out vignette))
                {
                    vignette.intensity.value = 0.3f;
                    vignette.smoothness.value = 0.2f;
                }

                if (postProcessVolume.profile.TryGet(out filmGrain))
                {
                    filmGrain.intensity.value = 0.1f;
                    filmGrain.type.value = FilmGrainLookup.Thin2;
                }
            }
        }

        private void InitializeLighting()
        {
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = dayLightIntensity;
                mainDirectionalLight.colorTemperature = colorTempDay;
            }

            if (interiorLights != null)
            {
                foreach (var light in interiorLights)
                {
                    if (light != null)
                    {
                        light.intensity = 1f;
                    }
                }
            }
        }

        private void InitializeEffects()
        {
            if (dustParticles != null)
            {
                dustParticles.Stop();
            }

            if (anomalyParticles != null)
            {
                anomalyParticles.Stop();
            }

            if (skyboxMaterial != null && daySkybox != null)
            {
                skyboxMaterial.SetTexture("_MainTex", daySkybox);
            }
        }

        public void SetDay(LunarDay day)
        {
            currentDay = day;

            switch (day)
            {
                case LunarDay.Day1_Arrival:
                    SetEnvironmentState(EnvironmentState.Transition);
                    StartCoroutine(TransitionToDay(1));
                    break;

                case LunarDay.Day4_Uncertainty:
                    SetEnvironmentState(EnvironmentState.Anomaly);
                    break;

                case LunarDay.Day5_Ritual:
                    SetEnvironmentState(EnvironmentState.Ritual);
                    break;

                case LunarDay.Day7_Reflection:
                    if (skyboxMaterial != null && nightSkybox != null)
                    {
                        skyboxMaterial.SetTexture("_MainTex", nightSkybox);
                    }
                    break;

                default:
                    SetEnvironmentState(EnvironmentState.Normal);
                    break;
            }
        }

        public void SetEnvironmentState(EnvironmentState state)
        {
            if (currentState == state) return;

            currentState = state;
            OnEnvironmentStateChanged?.Invoke(state);

            StopEnvironmentTransition();
            environmentTransitionCoroutine = StartCoroutine(TransitionToState(state));
        }

        private IEnumerator TransitionToState(EnvironmentState state)
        {
            float transitionDuration = 2f;
            float elapsed = 0f;

            EnvironmentState startState = currentState;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / transitionDuration;

                switch (state)
                {
                    case EnvironmentState.Normal:
                        ApplyNormalState(t);
                        break;

                    case EnvironmentState.Anomaly:
                        ApplyAnomalyState(t);
                        break;

                    case EnvironmentState.Ritual:
                        ApplyRitualState(t);
                        break;

                    case EnvironmentState.Transition:
                        ApplyTransitionState(t);
                        break;
                }

                yield return null;
            }

            environmentTransitionCoroutine = null;
        }

        private IEnumerator TransitionToDay(int day)
        {
            yield return new WaitForSeconds(1f);
            SetEnvironmentState(EnvironmentState.Normal);
        }

        private void ApplyNormalState(float t)
        {
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = Mathf.Lerp(mainDirectionalLight.intensity, dayLightIntensity, t);
                mainDirectionalLight.colorTemperature = Mathf.Lerp(mainDirectionalLight.colorTemperature, colorTempDay, t);
            }

            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, 0f, t);
                colorAdjustments.colorFilter.value = Color.Lerp(colorAdjustments.colorFilter.value, Color.white, t);
            }

            if (bloom != null)
            {
                bloom.intensity.value = Mathf.Lerp(bloom.intensity.value, 0.5f, t);
            }

            if (dustParticles != null && dustParticles.isStopped)
            {
                dustParticles.Play();
            }

            if (anomalyParticles != null && anomalyParticles.isPlaying)
            {
                anomalyParticles.Stop();
            }
        }

        private void ApplyAnomalyState(float t)
        {
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = Mathf.Lerp(mainDirectionalLight.intensity, dayLightIntensity * 0.5f, t);
                mainDirectionalLight.colorTemperature = Mathf.Lerp(mainDirectionalLight.colorTemperature, 8000f, t);
            }

            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, 0.5f, t);
                Color anomalyColor = new Color(0.9f, 0.7f, 0.8f);
                colorAdjustments.colorFilter.value = Color.Lerp(colorAdjustments.colorFilter.value, anomalyColor, t);
            }

            if (bloom != null)
            {
                bloom.intensity.value = Mathf.Lerp(bloom.intensity.value, 1.5f, t);
            }

            if (filmGrain != null)
            {
                filmGrain.intensity.value = Mathf.Lerp(filmGrain.intensity.value, 0.3f, t);
            }

            if (dustParticles != null && dustParticles.isPlaying)
            {
                dustParticles.Stop();
            }

            if (anomalyParticles != null && anomalyParticles.isStopped)
            {
                anomalyParticles.Play();
            }
        }

        private void ApplyRitualState(float t)
        {
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = Mathf.Lerp(mainDirectionalLight.intensity, nightLightIntensity, t);
                mainDirectionalLight.colorTemperature = Mathf.Lerp(mainDirectionalLight.colorTemperature, colorTempNight, t);
            }

            if (ritualAmbientLight != null)
            {
                ritualAmbientLight.intensity = Mathf.Lerp(ritualAmbientLight.intensity, 0.5f, t);
            }

            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, -0.3f, t);
                Color ritualColor = new Color(0.7f, 0.8f, 1f);
                colorAdjustments.colorFilter.value = Color.Lerp(colorAdjustments.colorFilter.value, ritualColor, t);
            }

            if (vignette != null)
            {
                vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, 0.5f, t);
            }

            if (interiorLights != null)
            {
                foreach (var light in interiorLights)
                {
                    if (light != null)
                    {
                        light.intensity = Mathf.Lerp(light.intensity, 0.3f, t);
                    }
                }
            }

            if (dustParticles != null && dustParticles.isPlaying)
            {
                dustParticles.Stop();
            }
        }

        private void ApplyTransitionState(float t)
        {
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = Mathf.Lerp(mainDirectionalLight.intensity, nightLightIntensity, t);
            }

            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(colorAdjustments.postExposure.value, -1f, t);
            }
        }

        public void TriggerVisualDistortion(float intensity, float duration)
        {
            StartCoroutine(VisualDistortionEffect(intensity, duration));
        }

        private IEnumerator VisualDistortionEffect(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float pulseIntensity = intensity * Mathf.Sin(elapsed * 10f);

                if (colorAdjustments != null)
                {
                    colorAdjustments.postExposure.value = pulseIntensity;
                }

                if (bloom != null)
                {
                    bloom.intensity.value = 0.5f + pulseIntensity * 2f;
                }

                yield return null;
            }

            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = 0f;
            }

            if (bloom != null)
            {
                bloom.intensity.value = 0.5f;
            }
        }

        private void StopEnvironmentTransition()
        {
            if (environmentTransitionCoroutine != null)
            {
                StopCoroutine(environmentTransitionCoroutine);
                environmentTransitionCoroutine = null;
            }
        }

        public EnvironmentState GetCurrentState()
        {
            return currentState;
        }

        public LunarDay GetCurrentDay()
        {
            return currentDay;
        }

        public void ResetEnvironment()
        {
            StopEnvironmentTransition();
            currentState = EnvironmentState.Normal;
            currentDay = LunarDay.Day1_Arrival;

            InitializePostProcessing();
            InitializeLighting();
            InitializeEffects();
        }
    }
}