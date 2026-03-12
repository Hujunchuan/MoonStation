using System.Collections.Generic;
using UnityEngine;

namespace Lunar.Data
{
    [System.Serializable]
    public class ResourceConfig
    {
        public ResourceType type;
        public float initialValue = 0.8f;
        public float minValue = 0.3f;
        public float maxValue = 1.0f;
        public float decayRate = 0.001f;
        public float recoveryRate = 0.005f;
        public Color safeColor = new Color(0.2f, 0.8f, 0.3f);
        public Color warningColor = new Color(0.9f, 0.7f, 0.2f);
    }

    [System.Serializable]
    public class RitualPhaseConfig
    {
        public Lunar.Core.RitualPhase phase;
        public float durationSeconds;
        public string audioClipName;
        public string voiceoverScript;
        public bool requiresInteraction;
        public string interactionTarget;
    }

    [System.Serializable]
    public class RitualConfig
    {
        public Lunar.Core.LunarDay targetDay;
        public string ritualName;
        public string description;
        public List<RitualPhaseConfig> phases = new List<RitualPhaseConfig>();
        public bool isDeepRitual = false;
    }

    [System.Serializable]
    public class LunarDayConfig
    {
        public int dayNumber;
        public string dayName;
        public string theme;
        public float targetDurationMinutes = 4f;

        public List<string> narrativeClips = new List<string>();
        public List<string> documentaryClips = new List<string>();

        public RitualConfig ritual;
        public List<ResourceConfig> resources = new List<ResourceConfig>();

        public bool hasAnomaly = false;
        public float anomalyChance = 0f;
    }

    [System.Serializable]
    public class AudioLayerConfig
    {
        public string layerName;
        public AudioClip clip;
        public float volume = 0.5f;
        public float pitch = 1f;
        public bool loop = true;
        public bool spatial = false;
        public float spatialBlend = 0f;
        public float minDistance = 1f;
        public float maxDistance = 10f;
    }

    [System.Serializable]
    public class AudioMixConfig
    {
        public string mixName;
        public List<AudioLayerConfig> layers = new List<AudioLayerConfig>();
        public float globalVolume = 1f;
    }

    [System.Serializable]
    public class GlobalConfig
    {
        public string version = "1.0.0";
        public float targetFrameRate = 60f;
        public bool enableVsync = true;

        public List<LunarDayConfig> days = new List<LunarDayConfig>();
        public AudioMixConfig audioMix;

        public float lightFadeDuration = 2f;
        public float interactionResponseDelay = 0.4f;
    }

    [System.Serializable]
    public class UserSessionData
    {
        public int currentDay = 1;
        public List<int> completedDays = new List<int>();
        public float sessionDuration = 0f;

        public float energyLevel = 0.8f;
        public float oxygenLevel = 0.8f;
        public float waterLevel = 0.8f;

        public List<string> completedRituals = new List<string>();
        public System.DateTime sessionStart;

        public int TotalInteractions = 0;
        public float AverageInteractionInterval = 0f;
    }
}
