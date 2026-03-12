using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Lunar.Core
{
    public class AudioTherapyEngine : MonoBehaviour
    {
        public static AudioTherapyEngine Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixerGroup masterGroup;
        [SerializeField] private AudioMixerGroup ambientGroup;
        [SerializeField] private AudioMixerGroup breathGroup;
        [SerializeField] private AudioMixerGroup voiceGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        [Header("Ambient Layer")]
        [SerializeField] private AudioSource ambientLowFreqSource;
        [SerializeField] private AudioSource ambientMidFreqSource;

        [Header("Breath Guide")]
        [SerializeField] private AudioSource breathInSource;
        [SerializeField] private AudioSource breathOutSource;
        [SerializeField] private AudioSource breathMetronomeSource;

        [Header("Voice Over")]
        [SerializeField] private AudioSource voiceSource;

        [Header("SFX")]
        [SerializeField] private AudioSource sfxSource;

        [Header("Settings")]
        [SerializeField] private float breathBPM = 60f;
        [SerializeField] private float masterVolume = 0.8f;
        [SerializeField] private float ambientVolume = 0.6f;
        [SerializeField] private float breathVolume = 0.5f;
        [SerializeField] private float voiceVolume = 0.7f;

        private float breathCycleDuration;
        private float breathPhase;
        private bool isBreathActive;
        private bool isInRitual;

        private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
        private List<AudioSource> activeLoopingSources = new List<AudioSource>();

        public event Action OnNarrativeComplete;
        public event Action OnRitualAudioComplete;

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

            breathCycleDuration = 60f / breathBPM * 4f;
            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            if (ambientLowFreqSource == null)
            {
                ambientLowFreqSource = CreateAudioSource("Ambient_LowFreq", true, ambientGroup);
            }
            if (ambientMidFreqSource == null)
            {
                ambientMidFreqSource = CreateAudioSource("Ambient_MidFreq", true, ambientGroup);
            }
            if (breathInSource == null)
            {
                breathInSource = CreateAudioSource("Breath_In", true, breathGroup);
            }
            if (breathOutSource == null)
            {
                breathOutSource = CreateAudioSource("Breath_Out", true, breathGroup);
            }
            if (breathMetronomeSource == null)
            {
                breathMetronomeSource = CreateAudioSource("Breath_Metronome", true, breathGroup);
            }
            if (voiceSource == null)
            {
                voiceSource = CreateAudioSource("Voice", false, voiceGroup);
            }
            if (sfxSource == null)
            {
                sfxSource = CreateAudioSource("SFX", false, sfxGroup);
            }
        }

        private AudioSource CreateAudioSource(string name, bool loop, AudioMixerGroup group)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.outputAudioMixerGroup = group;
            return source;
        }

        private void Start()
        {
            SetMasterVolume(masterVolume);
            SetAmbientVolume(ambientVolume);
            StartAmbientLayer();
        }

        private void Update()
        {
            if (isBreathActive && !isInRitual)
            {
                UpdateBreathGuide();
            }

            HandleSpatialAudio();
        }

        private void UpdateBreathGuide()
        {
            breathPhase += Time.deltaTime / breathCycleDuration;
            if (breathPhase >= 1f)
            {
                breathPhase = 0f;
            }

            float breathProgress = Mathf.Sin(breathPhase * Mathf.PI);

            if (breathInSource != null && breathInSource.isPlaying)
            {
                breathInSource.volume = breathVolume * Mathf.Clamp01(breathProgress);
            }
            if (breathOutSource != null && breathOutSource.isPlaying)
            {
                breathOutSource.volume = breathVolume * Mathf.Clamp01(1f - breathProgress);
            }
        }

        public void StartAmbientLayer()
        {
            if (ambientLowFreqSource != null && !ambientLowFreqSource.isPlaying)
            {
                ambientLowFreqSource.Play();
            }
            if (ambientMidFreqSource != null && !ambientMidFreqSource.isPlaying)
            {
                ambientMidFreqSource.Play();
            }
        }

        public void SetAmbientFrequency(float frequency)
        {
            if (ambientLowFreqSource != null)
            {
                ambientLowFreqSource.pitch = frequency / 60f;
            }
        }

        public void SetBreathGuideActive(bool active)
        {
            isBreathActive = active;

            if (breathInSource != null)
            {
                if (active) breathInSource.Play();
                else breathInSource.Stop();
            }
            if (breathOutSource != null)
            {
                if (active) breathOutSource.Play();
                else breathOutSource.Stop();
            }
            if (breathMetronomeSource != null)
            {
                if (active) breathMetronomeSource.Play();
                else breathMetronomeSource.Stop();
            }
        }

        public void SetRitualMode(bool ritualActive)
        {
            isInRitual = ritualActive;

            float ambientTarget = ritualActive ? 0.3f : ambientVolume;
            float breathTarget = ritualActive ? breathVolume : 0f;

            StartCoroutine(SmoothVolumeChange(ambientGroup, ambientTarget, 1f));
            StartCoroutine(SmoothVolumeChange(breathGroup, breathTarget, 0.5f));

            if (ritualActive)
            {
                SetBreathGuideActive(true);
            }
        }

        public void PlayIntroductionAudio(LunarDay day)
        {
            if (voiceSource != null)
            {
                AudioClip introClip = LoadClip($"Intro_Day{(int)day}");
                if (introClip != null)
                {
                    voiceSource.clip = introClip;
                    voiceSource.Play();
                }
            }
        }

        public void PlayNarrative(string clipName)
        {
            if (voiceSource != null)
            {
                AudioClip clip = LoadClip(clipName);
                if (clip != null)
                {
                    voiceSource.Stop();
                    voiceSource.clip = clip;
                    voiceSource.Play();
                }
            }
        }

        public void PlayRitualAudio(string clipName)
        {
            if (voiceSource != null)
            {
                AudioClip clip = LoadClip(clipName);
                if (clip != null)
                {
                    voiceSource.clip = clip;
                    voiceSource.Play();
                }
            }
        }

        public void PlayTransitionSound()
        {
            PlayHighFreqAlert();
        }

        public void PlayHighFreqAlert()
        {
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(GetOrCreateTone(2200f, 0.5f), 0.3f);
            }
        }

        public void PlayInteractionFeedback(ResourceType resource)
        {
            if (sfxSource != null)
            {
                float freq = GetResourceFrequency(resource);
                sfxSource.PlayOneShot(GetOrCreateTone(freq, 0.3f), 0.4f);
            }
        }

        private float GetResourceFrequency(ResourceType resource)
        {
            switch (resource)
            {
                case ResourceType.Energy: return 220f;
                case ResourceType.Oxygen: return 330f;
                case ResourceType.Water: return 440f;
                default: return 440f;
            }
        }

        public void PlayRitualCompletion()
        {
            if (sfxSource != null)
            {
                sfxSource.PlayOneShot(GetOrCreateTone(880f, 1f), 0.5f);
            }
            OnRitualAudioComplete?.Invoke();
        }

        private AudioClip GetOrCreateTone(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * Mathf.Exp(-3f * i / sampleCount);
            }

            AudioClip clip = AudioClip.Create($"Tone_{frequency}", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip LoadClip(string clipName)
        {
            if (clipCache.TryGetValue(clipName, out var cached))
            {
                return cached;
            }

            AudioClip loaded = Resources.Load<AudioClip>($"Audio/{clipName}");
            if (loaded != null)
            {
                clipCache[clipName] = loaded;
            }
            return loaded;
        }

        private void HandleSpatialAudio()
        {
            Transform playerCam = Camera.main?.transform;
            if (playerCam == null) return;

            foreach (var source in activeLoopingSources)
            {
                if (source != null && source.spatialBlend > 0)
                {
                    source.spatialBlend = Mathf.Lerp(source.spatialBlend, 1f, Time.deltaTime);
                }
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20f);
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("AmbientVolume", Mathf.Log10(ambientVolume) * 20f);
        }

        public void SetVoiceVolume(float volume)
        {
            voiceVolume = Mathf.Clamp01(volume);
            mainMixer?.SetFloat("VoiceVolume", Mathf.Log10(voiceVolume) * 20f);
        }

        private System.Collections.IEnumerator SmoothVolumeChange(AudioMixerGroup group, float targetVolume, float duration)
        {
            float startVolume = group.audioMixer?.GetFloat($"{group.name}Volume", out float current) == true ? current : 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float volume = Mathf.Lerp(startVolume, Mathf.Log10(targetVolume) * 20f, t);
                group.audioMixer?.SetFloat($"{group.name}Volume", volume);
                yield return null;
            }
        }

        public void StopAllAudio()
        {
            voiceSource?.Stop();
            sfxSource?.Stop();
            SetBreathGuideActive(false);
            SetRitualMode(false);
        }
    }
}
