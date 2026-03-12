using System.Collections;
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
        private readonly HashSet<string> warnedMissingClips = new HashSet<string>();

        private bool isBreathGuideActive;
        private float breathPhase;
        private float breathCycleDuration;
        private Coroutine voiceSequenceRoutine;

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

        public void PlayNarrativeSequence(IList<string> clipNames)
        {
            if (voiceSource == null || clipNames == null || clipNames.Count == 0)
            {
                return;
            }

            StopVoiceSequence();
            voiceSequenceRoutine = StartCoroutine(PlayVoiceSequence(clipNames));
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

            StopVoiceSequence();

            AudioClip clip = LoadClip(clipName);
            if (clip == null)
            {
                return;
            }

            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.Play();
        }

        private IEnumerator PlayVoiceSequence(IList<string> clipNames)
        {
            for (int index = 0; index < clipNames.Count; index++)
            {
                AudioClip clip = LoadClip(clipNames[index]);
                if (clip == null)
                {
                    continue;
                }

                voiceSource.Stop();
                voiceSource.clip = clip;
                voiceSource.Play();

                yield return new WaitForSeconds(Mathf.Max(clip.length, 0.1f));
            }

            voiceSequenceRoutine = null;
        }

        private void StopVoiceSequence()
        {
            if (voiceSequenceRoutine == null)
            {
                return;
            }

            StopCoroutine(voiceSequenceRoutine);
            voiceSequenceRoutine = null;
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
            if (loadedClip == null)
            {
                loadedClip = CreateFallbackClip(clipName);
            }

            if (loadedClip == null)
            {
                return null;
            }

            clipCache[clipName] = loadedClip;

            return loadedClip;
        }

        private AudioClip CreateFallbackClip(string clipName)
        {
            if (warnedMissingClips.Add(clipName))
            {
                Debug.LogWarning($"[AudioTherapyEngine] Missing audio clip 'Audio/{clipName}'. Using generated fallback audio.");
            }

            string normalizedName = clipName.ToLowerInvariant();

            if (normalizedName.Contains("ambient_low"))
            {
                return CreateLayeredLoopClip("Fallback_AmbientLow", 6f, 58f, 0.10f, 73f, 0.04f);
            }

            if (normalizedName.Contains("ambient_mid"))
            {
                return CreateLayeredLoopClip("Fallback_AmbientMid", 6f, 110f, 0.05f, 164f, 0.025f);
            }

            if (normalizedName.Contains("breath_in"))
            {
                return CreateBreathLoopClip("Fallback_BreathIn", 4f, 220f, true);
            }

            if (normalizedName.Contains("breath_out"))
            {
                return CreateBreathLoopClip("Fallback_BreathOut", 4f, 160f, false);
            }

            if (normalizedName.Contains("breath_metronome"))
            {
                return CreatePulseLoopClip("Fallback_BreathMetronome", 4f, 2, 660f, 0.12f);
            }

            if (normalizedName.StartsWith("intro_"))
            {
                return CreateSequenceClip("Fallback_IntroCue", 0.9f, new[] { 330f, 392f, 440f }, 0.13f);
            }

            if (normalizedName.StartsWith("ritual_"))
            {
                return CreateSequenceClip("Fallback_RitualCue", 1.2f, new[] { 261.63f, 329.63f, 392f, 329.63f }, 0.18f);
            }

            if (normalizedName.StartsWith("narrative_") || normalizedName.StartsWith("archive_"))
            {
                return CreateSequenceClip("Fallback_NarrativeCue", 1.4f, new[] { 523.25f, 659.25f, 587.33f, 523.25f }, 0.16f);
            }

            return CreateSequenceClip("Fallback_GenericCue", 1f, new[] { 440f, 554.37f }, 0.18f);
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

        private AudioClip CreateLayeredLoopClip(string clipName, float duration, float primaryFrequency, float primaryAmplitude, float secondaryFrequency, float secondaryAmplitude)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float sampleTime = index / (float)sampleRate;
                float slowMotion = 0.85f + 0.15f * Mathf.Sin(sampleTime * Mathf.PI * 0.5f);
                float sample =
                    Mathf.Sin(2f * Mathf.PI * primaryFrequency * sampleTime) * primaryAmplitude +
                    Mathf.Sin(2f * Mathf.PI * secondaryFrequency * sampleTime) * secondaryAmplitude;

                samples[index] = sample * slowMotion;
            }

            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip CreateBreathLoopClip(string clipName, float duration, float baseFrequency, bool rising)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float t = index / (float)(sampleCount - 1);
                float sampleTime = index / (float)sampleRate;
                float envelope = Mathf.Sin(t * Mathf.PI);
                float frequency = rising
                    ? Mathf.Lerp(baseFrequency * 0.8f, baseFrequency * 1.2f, t)
                    : Mathf.Lerp(baseFrequency * 1.2f, baseFrequency * 0.75f, t);
                float sample = Mathf.Sin(2f * Mathf.PI * frequency * sampleTime) * 0.08f;

                samples[index] = sample * envelope;
            }

            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip CreatePulseLoopClip(string clipName, float duration, int pulseCount, float frequency, float pulseDuration)
        {
            int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];
            float segmentDuration = duration / Mathf.Max(1, pulseCount);

            for (int index = 0; index < sampleCount; index++)
            {
                float sampleTime = index / (float)sampleRate;
                float localTime = sampleTime % segmentDuration;
                if (localTime > pulseDuration)
                {
                    continue;
                }

                float envelope = 1f - localTime / pulseDuration;
                samples[index] = Mathf.Sin(2f * Mathf.PI * frequency * sampleTime) * envelope * 0.12f;
            }

            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip CreateSequenceClip(string clipName, float noteDuration, float[] notes, float amplitude)
        {
            int sampleRate = 44100;
            int sampleCountPerNote = Mathf.CeilToInt(sampleRate * noteDuration);
            float[] samples = new float[sampleCountPerNote * Mathf.Max(1, notes.Length)];

            for (int noteIndex = 0; noteIndex < notes.Length; noteIndex++)
            {
                float frequency = notes[noteIndex];
                int startIndex = noteIndex * sampleCountPerNote;

                for (int offset = 0; offset < sampleCountPerNote; offset++)
                {
                    int sampleIndex = startIndex + offset;
                    float t = offset / (float)(sampleCountPerNote - 1);
                    float sampleTime = offset / (float)sampleRate;
                    float envelope = Mathf.Sin(t * Mathf.PI);
                    float sample = Mathf.Sin(2f * Mathf.PI * frequency * sampleTime) * envelope * amplitude;
                    samples[sampleIndex] = sample;
                }
            }

            return CreateClip(clipName, samples, sampleRate);
        }

        private AudioClip CreateClip(string clipName, float[] samples, int sampleRate)
        {
            AudioClip clip = AudioClip.Create(clipName, samples.Length, 1, sampleRate, false);
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
            StopVoiceSequence();
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
