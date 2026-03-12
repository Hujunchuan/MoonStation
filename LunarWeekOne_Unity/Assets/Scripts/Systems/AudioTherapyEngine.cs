using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Lunar.Core
{
    public class AudioTherapyEngine : MonoBehaviour
    {
        public static AudioTherapyEngine Instance { get; private set; }

        [Header("Mixer")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixerGroup ambientGroup;
        [SerializeField] private AudioMixerGroup breathGroup;
        [SerializeField] private AudioMixerGroup voiceGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        [Header("Sources")]
        [SerializeField] private AudioSource ambientLowFreqSource;
        [SerializeField] private AudioSource ambientMidFreqSource;
        [SerializeField] private AudioSource breathInSource;
        [SerializeField] private AudioSource breathOutSource;
        [SerializeField] private AudioSource breathMetronomeSource;
        [SerializeField] private AudioSource voiceSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Levels")]
        [SerializeField] private float breathBpm = 60f;
        [SerializeField] private float masterVolume = 0.8f;
        [SerializeField] private float ambientVolume = 0.6f;
        [SerializeField] private float breathVolume = 0.45f;
        [SerializeField] private float voiceVolume = 0.7f;

        private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
        private readonly List<AudioSource> loopingSources = new List<AudioSource>();

        private bool isBreathGuideActive;
        private float breathPhase;
        private float breathCycleDuration;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            breathCycleDuration = Mathf.Max(0.1f, 60f / breathBpm * 4f);
            InitializeAudioSources();
            ApplyMixerSettings();
        }

        private void Update()
        {
            if (isBreathGuideActive)
            {
                UpdateBreathGuide();
            }
        }

        private void InitializeAudioSources()
        {
            if (ambientLowFreqSource == null)
            {
                ambientLowFreqSource = CreateAudioSource("AmbientLow", true, ambientGroup);
            }

            if (ambientMidFreqSource == null)
            {
                ambientMidFreqSource = CreateAudioSource("AmbientMid", true, ambientGroup);
            }

            if (breathInSource == null)
            {
                breathInSource = CreateAudioSource("BreathIn", true, breathGroup);
            }

            if (breathOutSource == null)
            {
                breathOutSource = CreateAudioSource("BreathOut", true, breathGroup);
            }

            if (breathMetronomeSource == null)
            {
                breathMetronomeSource = CreateAudioSource("BreathMetronome", true, breathGroup);
            }

            if (voiceSource == null)
            {
                voiceSource = CreateAudioSource("Voice", false, voiceGroup);
            }

            if (sfxSource == null)
            {
                sfxSource = CreateAudioSource("Sfx", false, sfxGroup);
            }
        }

        private AudioSource CreateAudioSource(string sourceName, bool loop, AudioMixerGroup group)
        {
            GameObject sourceObject = new GameObject(sourceName);
            sourceObject.transform.SetParent(transform, false);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.outputAudioMixerGroup = group;

            if (loop)
            {
                loopingSources.Add(source);
            }

            return source;
        }

        private void ApplyMixerSettings()
        {
            SetMasterVolume(masterVolume);
            SetAmbientVolume(ambientVolume);
            SetBreathVolume(breathVolume);
            SetVoiceVolume(voiceVolume);
        }

        private void UpdateBreathGuide()
        {
            breathPhase += Time.deltaTime / breathCycleDuration;
            if (breathPhase > 1f)
            {
                breathPhase -= 1f;
            }

            float inhaleWeight = Mathf.Clamp01(Mathf.Sin(breathPhase * Mathf.PI));
            float exhaleWeight = Mathf.Clamp01(1f - inhaleWeight);

            if (breathInSource != null && breathInSource.isPlaying)
            {
                breathInSource.volume = breathVolume * inhaleWeight;
            }

            if (breathOutSource != null && breathOutSource.isPlaying)
            {
                breathOutSource.volume = breathVolume * exhaleWeight;
            }
        }

        public void StartAmbientLayer()
        {
            EnsureClip(ambientLowFreqSource, "ambient_low");
            EnsureClip(ambientMidFreqSource, "ambient_mid");
            PlayIfReady(ambientLowFreqSource);
            PlayIfReady(ambientMidFreqSource);
        }

        public void SetAmbientFrequency(float frequency)
        {
            if (ambientLowFreqSource != null)
            {
                ambientLowFreqSource.pitch = Mathf.Clamp(frequency / 60f, 0.5f, 2f);
            }
        }

        public void SetBreathGuideActive(bool active)
        {
            isBreathGuideActive = active;
            breathPhase = 0f;

            EnsureClip(breathInSource, "breath_in");
            EnsureClip(breathOutSource, "breath_out");
            EnsureClip(breathMetronomeSource, "breath_metronome");

            ToggleLoopSource(breathInSource, active);
            ToggleLoopSource(breathOutSource, active);
            ToggleLoopSource(breathMetronomeSource, active);
        }

        public void SetRitualMode(bool ritualActive)
        {
            SetAmbientVolume(ritualActive ? 0.3f : 0.6f);
            SetBreathGuideActive(ritualActive);
        }

        public void PlayIntroductionAudio(LunarDay day)
        {
            PlayVoiceClip($"Intro_Day{(int)day}");
        }

        public void PlayNarrative(string clipName)
        {
            PlayVoiceClip(clipName);
        }

        public void PlayRitualAudio(string clipName)
        {
            PlayVoiceClip(clipName);
        }

        public void PlayTransitionSound()
        {
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(GetOrCreateTone(880f, 0.2f), 0.25f);
            }
        }

        public void PlayInteractionFeedback(ResourceType resourceType)
        {
            if (sfxSource == null)
            {
                return;
            }

            float frequency = 220f;

            switch (resourceType)
            {
                case ResourceType.Oxygen:
                    frequency = 330f;
                    break;
                case ResourceType.Water:
                    frequency = 440f;
                    break;
            }

            sfxSource.PlayOneShot(GetOrCreateTone(frequency, 0.15f), 0.2f);
        }

        public void PlayRitualCompletion()
        {
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(GetOrCreateTone(660f, 0.35f), 0.25f);
            }
        }

        private void PlayVoiceClip(string clipName)
        {
            if (voiceSource == null)
            {
                return;
            }

            AudioClip clip = LoadClip(clipName);
            if (clip == null)
            {
                return;
            }

            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        private void EnsureClip(AudioSource source, string clipName)
        {
            if (source == null || source.clip != null)
            {
                return;
            }

            source.clip = LoadClip(clipName);
        }

        private void PlayIfReady(AudioSource source)
        {
            if (source != null && source.clip != null && !source.isPlaying)
            {
                source.Play();
            }
        }

        private void ToggleLoopSource(AudioSource source, bool shouldPlay)
        {
            if (source == null || source.clip == null)
            {
                return;
            }

            if (shouldPlay)
            {
                if (!source.isPlaying)
                {
                    source.Play();
                }
            }
            else
            {
                source.Stop();
            }
        }

        private AudioClip LoadClip(string clipName)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                return null;
            }

            if (clipCache.TryGetValue(clipName, out var cachedClip))
            {
                return cachedClip;
            }

            AudioClip loadedClip = Resources.Load<AudioClip>($"Audio/{clipName}");
            if (loadedClip != null)
            {
                clipCache[clipName] = loadedClip;
            }

            return loadedClip;
        }

        private AudioClip GetOrCreateTone(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float sampleTime = index / (float)sampleRate;
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * sampleTime) * Mathf.Exp(-3f * sampleTime);
            }

            AudioClip clip = AudioClip.Create($"Tone_{frequency}_{duration}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private float ToMixerDb(float normalizedVolume)
        {
            if (normalizedVolume <= 0.0001f)
            {
                return -80f;
            }

            return Mathf.Log10(Mathf.Clamp01(normalizedVolume)) * 20f;
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("MasterVolume", ToMixerDb(masterVolume));
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("AmbientVolume", ToMixerDb(ambientVolume));
        }

        public void SetBreathVolume(float volume)
        {
            breathVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("BreathVolume", ToMixerDb(breathVolume));
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("VoiceVolume", ToMixerDb(voiceVolume));
        }

        public void StopAllAudio()
        {
            foreach (AudioSource source in loopingSources)
            {
                if (source != null)
                {
                    source.Stop();
                }
            }

            voiceSource?.Stop();
            sfxSource?.Stop();
            isBreathGuideActive = false;
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
