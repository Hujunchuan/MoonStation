using UnityEngine;

namespace Lunar.Core
{
    public static class LunarConstants
    {
        public const float DEFAULT_ENERGY_LEVEL = 0.8f;
        public const float DEFAULT_OXYGEN_LEVEL = 0.8f;
        public const float DEFAULT_WATER_LEVEL = 0.8f;

        public const float BREATH_BPM = 60f;
        public const float BREATH_CYCLE_SECONDS = 4f;

        public const float INTERACTION_DELAY_SECONDS = 0.4f;
        public const float LIGHT_FADE_DURATION = 2f;

        public const float LOW_FREQ_MIN = 40f;
        public const float LOW_FREQ_MAX = 80f;
        public const float HIGH_FREQ_MIN = 2000f;

        public const int TOTAL_LUNAR_DAYS = 7;
    }

    public enum LunarDay
    {
        Day1_Arrival = 1,
        Day2_Adaptation,
        Day3_Order,
        Day4_Uncertainty,
        Day5_Ritual,
        Day6_Stability,
        Day7_Reflection
    }

    public enum RitualPhase
        {
            None,
            Enter,
            Anchor,
            Order,
            Observe,
            Exit
        }

    public enum ResourceType
    {
        Energy,
        Oxygen,
        Water
    }

    public enum EventTriggerType
    {
        DayStart,
        DayComplete,
        RitualStart,
        RitualComplete,
        ResourceAction,
        SystemAnomaly
    }
}
