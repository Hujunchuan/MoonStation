using System.Collections.Generic;
using Lunar.Core;
using UnityEngine;

namespace Lunar.Data
{
    [System.Serializable]
    public class ResourceConfig
    {
        public ResourceType type;
        public float initialValue = 0.8f;
        public float minValue = 0.3f;
        public float maxValue = 1f;
        public float decayRate = 0.001f;
        public float recoveryRate = 0.005f;
        public Color safeColor = new Color(0.2f, 0.8f, 0.3f);
        public Color warningColor = new Color(0.9f, 0.7f, 0.2f);
        public Color criticalColor = new Color(0.9f, 0.3f, 0.2f);
    }

    [System.Serializable]
    public class RitualPhaseConfig
    {
        public RitualPhase phase = RitualPhase.None;
        public float durationSeconds = 30f;
        public string audioClipName = string.Empty;
        public string voiceoverScript = string.Empty;
        public bool requiresInteraction;
        public string interactionTarget = string.Empty;
    }

    [System.Serializable]
    public class NarrativeBeatConfig
    {
        public int storyStepNumber;
        public string storyFunction = string.Empty;
        public string beatId = string.Empty;
        public string title = string.Empty;
        public string trigger = string.Empty;
        public string summary = string.Empty;
        public string voiceoverLine = string.Empty;
        public string environmentCue = string.Empty;
    }

    [System.Serializable]
    public class RitualConfig
    {
        public LunarDay targetDay = LunarDay.Day1_Arrival;
        public string ritualName = string.Empty;
        public string description = string.Empty;
        public string intention = string.Empty;
        public string entryPrompt = string.Empty;
        public string exitPrompt = string.Empty;
        public List<RitualPhaseConfig> phases = new List<RitualPhaseConfig>();
        public bool isDeepRitual;
    }

    [System.Serializable]
    public class LunarDayConfig
    {
        public int dayNumber = 1;
        public string dayName = string.Empty;
        public string theme = string.Empty;
        public string emotionalGoal = string.Empty;
        public string dramaticQuestion = string.Empty;
        public string externalPressure = string.Empty;
        public string innerShift = string.Empty;
        public string visualMotif = string.Empty;
        public float targetDurationMinutes = 4f;
        public float introductionDurationSeconds = 25f;
        public float resourceDurationSeconds = 110f;
        public float narrationDurationSeconds = 35f;
        public bool enableNarration = true;
        public bool enableRitual = true;
        public List<string> narrativeClips = new List<string>();
        public List<string> documentaryClips = new List<string>();
        public List<NarrativeBeatConfig> narrativeBeats = new List<NarrativeBeatConfig>();
        public RitualConfig ritual = new RitualConfig();
        public List<ResourceConfig> resources = new List<ResourceConfig>();
        public bool hasAnomaly;
        public float anomalyChance;
    }

    [System.Serializable]
    public class AudioLayerConfig
    {
        public string layerName = string.Empty;
        public string clipResourcePath = string.Empty;
        public float volume = 0.5f;
        public float pitch = 1f;
        public bool loop = true;
        public bool spatial;
        public float spatialBlend;
        public float minDistance = 1f;
        public float maxDistance = 10f;
    }

    [System.Serializable]
    public class AudioMixConfig
    {
        public string mixName = "LunarDefault";
        public List<AudioLayerConfig> layers = new List<AudioLayerConfig>();
        public float globalVolume = 1f;
    }

    [System.Serializable]
    public class GlobalConfig
    {
        public string version = "1.0.0";
        public int targetFrameRate = 60;
        public bool enableVsync = true;
        public List<LunarDayConfig> days = new List<LunarDayConfig>();
        public AudioMixConfig audioMix = new AudioMixConfig();
        public float lightFadeDuration = 2f;
        public float interactionResponseDelay = 0.4f;
    }

    [System.Serializable]
    public class UserSessionData
    {
        public int currentDay = 1;
        public List<int> completedDays = new List<int>();
        public float sessionDuration;
        public float energyLevel = 0.8f;
        public float oxygenLevel = 0.8f;
        public float waterLevel = 0.8f;
        public int moodBefore;
        public int moodAfter;
        public List<string> completedRituals = new List<string>();
        public string sessionStartIso = string.Empty;
        public int totalInteractions;
        public float averageInteractionInterval;
    }
}
