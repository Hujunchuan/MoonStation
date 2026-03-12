using System;
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

        [Header("Post Processing")]
        [SerializeField] private Volume postProcessVolume;
        [SerializeField] private float colorTempDay = 6500f;
        [SerializeField] private float colorTempNight = 3200f;

        [Header("Effects")]
        [SerializeField] private ParticleSystem dustParticles;
        [SerializeField] private ParticleSystem anomalyParticles;
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Texture2D daySkybox;
        [SerializeField] private Texture2D nightSkybox;

        [Header("State")]
        [SerializeField] private EnvironmentState currentState = EnvironmentState.Normal;
        [SerializeField] private LunarDay currentDay = LunarDay.Day1_Arrival;

        private ColorAdjustments colorAdjustments;
        private Bloom bloom;
        private Vignette vignette;
        private FilmGrain filmGrain;
        private Coroutine transitionCoroutine;

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
            ApplyStateImmediate(currentState);
        }

        private void InitializePostProcessing()
        {
            if (postProcessVolume == null || postProcessVolume.profile == null)
            {
                return;
            }

            postProcessVolume.profile.TryGet(out colorAdjustments);
            postProcessVolume.profile.TryGet(out bloom);
            postProcessVolume.profile.TryGet(out vignette);
            postProcessVolume.profile.TryGet(out filmGrain);
        }

        public void SetDay(LunarDay day)
        {
            currentDay = day;

            switch (day)
            {
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
                    SetEnvironmentState(EnvironmentState.Normal);
                    break;

                default:
                    if (skyboxMaterial != null && daySkybox != null)
                    {
                        skyboxMaterial.SetTexture("_MainTex", daySkybox);
                    }
                    SetEnvironmentState(EnvironmentState.Normal);
                    break;
            }
        }

        public void SetEnvironmentState(EnvironmentState state)
        {
            if (currentState == state && transitionCoroutine == null)
            {
                ApplyStateImmediate(state);
                return;
            }

            currentState = state;
            OnEnvironmentStateChanged?.Invoke(state);

            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            transitionCoroutine = StartCoroutine(TransitionToState(state));
        }

        private IEnumerator TransitionToState(EnvironmentState targetState)
        {
            float elapsed = 0f;
            const float duration = 1.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                ApplyState(targetState, t);
                yield return null;
            }

            ApplyStateImmediate(targetState);
            transitionCoroutine = null;
        }

        private void ApplyState(EnvironmentState state, float t)
        {
            switch (state)
            {
                case EnvironmentState.Normal:
                    ApplyLighting(Mathf.Lerp(mainDirectionalLight != null ? mainDirectionalLight.intensity : dayLightIntensity, dayLightIntensity, t),
                        Mathf.Lerp(mainDirectionalLight != null ? mainDirectionalLight.colorTemperature : colorTempDay, colorTempDay, t));
                    ApplyPostExposure(Mathf.Lerp(GetPostExposure(), 0f, t));
                    ApplyBloom(Mathf.Lerp(GetBloomIntensity(), 0.45f, t));
                    ApplyFilmGrain(Mathf.Lerp(GetFilmGrainIntensity(), 0.08f, t));
                    ToggleParticles(dustParticles, true);
                    ToggleParticles(anomalyParticles, false);
                    break;

                case EnvironmentState.Anomaly:
                    ApplyLighting(Mathf.Lerp(mainDirectionalLight != null ? mainDirectionalLight.intensity : dayLightIntensity, dayLightIntensity * 0.55f, t),
                        Mathf.Lerp(mainDirectionalLight != null ? mainDirectionalLight.colorTemperature : colorTempDay, 8000f, t));
                    ApplyPostExposure(Mathf.Lerp(GetPostExposure(), 0.35f, t));
                    ApplyBloom(Mathf.Lerp(GetBloomIntensity(), 1.3f, t));
                    ApplyFilmGrain(Mathf.Lerp(GetFilmGrainIntensity(), 0.3f, t));
                    ToggleParticles(dustParticles, false);
                    ToggleParticles(anomalyParticles, true);
                    break;

                case EnvironmentState.Ritual:
                    ApplyLighting(Mathf.Lerp(mainDirectionalLight != null ? mainDirectionalLight.intensity : dayLightIntensity, nightLightIntensity, t),
                        Mathf.Lerp(mainDirectionalLight != null ? mainDirectionalLight.colorTemperature : colorTempDay, colorTempNight, t));
                    ApplyPostExposure(Mathf.Lerp(GetPostExposure(), -0.3f, t));
                    ApplyBloom(Mathf.Lerp(GetBloomIntensity(), 0.8f, t));
                    ApplyFilmGrain(Mathf.Lerp(GetFilmGrainIntensity(), 0.05f, t));
                    ToggleParticles(dustParticles, false);
                    ToggleParticles(anomalyParticles, false);
                    break;

                case EnvironmentState.Transition:
                    ApplyLighting(Mathf.Lerp(mainDirectionalLight != null ? mainDirectionalLight.intensity : dayLightIntensity, nightLightIntensity, t),
                        Mathf.Lerp(mainDirectionalLight != null ? mainDirectionalLight.colorTemperature : colorTempDay, colorTempNight, t));
                    ApplyPostExposure(Mathf.Lerp(GetPostExposure(), -1f, t));
                    break;
            }
        }

        private void ApplyStateImmediate(EnvironmentState state)
        {
            currentState = state;

            switch (state)
            {
                case EnvironmentState.Normal:
                    ApplyLighting(dayLightIntensity, colorTempDay);
                    ApplyPostExposure(0f);
                    ApplyBloom(0.45f);
                    ApplyFilmGrain(0.08f);
                    SetInteriorLightIntensity(1f);
                    SetRitualLightIntensity(0.1f);
                    ToggleParticles(dustParticles, true);
                    ToggleParticles(anomalyParticles, false);
                    break;

                case EnvironmentState.Anomaly:
                    ApplyLighting(dayLightIntensity * 0.55f, 8000f);
                    ApplyPostExposure(0.35f);
                    ApplyBloom(1.3f);
                    ApplyFilmGrain(0.3f);
                    SetInteriorLightIntensity(0.7f);
                    SetRitualLightIntensity(0.15f);
                    ToggleParticles(dustParticles, false);
                    ToggleParticles(anomalyParticles, true);
                    break;

                case EnvironmentState.Ritual:
                    ApplyLighting(nightLightIntensity, colorTempNight);
                    ApplyPostExposure(-0.3f);
                    ApplyBloom(0.8f);
                    ApplyFilmGrain(0.05f);
                    SetInteriorLightIntensity(0.3f);
                    SetRitualLightIntensity(0.5f);
                    ToggleParticles(dustParticles, false);
                    ToggleParticles(anomalyParticles, false);
                    break;

                case EnvironmentState.Transition:
                    ApplyLighting(nightLightIntensity, colorTempNight);
                    ApplyPostExposure(-1f);
                    break;
            }
        }

        private void ApplyLighting(float intensity, float colorTemperature)
        {
            if (mainDirectionalLight != null)
            {
                mainDirectionalLight.intensity = intensity;
                mainDirectionalLight.colorTemperature = colorTemperature;
            }
        }

        private void SetInteriorLightIntensity(float intensity)
        {
            if (interiorLights == null)
            {
                return;
            }

            foreach (Light light in interiorLights)
            {
                if (light != null)
                {
                    light.intensity = intensity;
                }
            }
        }

        private void SetRitualLightIntensity(float intensity)
        {
            if (ritualAmbientLight != null)
            {
                ritualAmbientLight.intensity = intensity;
            }
        }

        private void ApplyPostExposure(float value)
        {
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = value;
            }
        }

        private void ApplyBloom(float value)
        {
            if (bloom != null)
            {
                bloom.intensity.value = value;
            }
        }

        private void ApplyFilmGrain(float value)
        {
            if (filmGrain != null)
            {
                filmGrain.intensity.value = value;
            }
        }

        private float GetPostExposure()
        {
            return colorAdjustments != null ? colorAdjustments.postExposure.value : 0f;
        }

        private float GetBloomIntensity()
        {
            return bloom != null ? bloom.intensity.value : 0f;
        }

        private float GetFilmGrainIntensity()
        {
            return filmGrain != null ? filmGrain.intensity.value : 0f;
        }

        private void ToggleParticles(ParticleSystem system, bool shouldPlay)
        {
            if (system == null)
            {
                return;
            }

            if (shouldPlay && system.isStopped)
            {
                system.Play();
            }
            else if (!shouldPlay && system.isPlaying)
            {
                system.Stop();
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
                float pulse = intensity * Mathf.Sin(elapsed * 10f);
                ApplyPostExposure(pulse);
                ApplyBloom(0.5f + pulse * 1.5f);
                yield return null;
            }

            ApplyStateImmediate(currentState);
        }

        public EnvironmentState GetCurrentState()
        {
            return currentState;
        }

        public LunarDay GetCurrentDay()
        {
            return currentDay;
        }
    }
}
