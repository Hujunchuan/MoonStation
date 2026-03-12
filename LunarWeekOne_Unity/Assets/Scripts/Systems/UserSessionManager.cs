using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

namespace Lunar.Core
{
    public class UserSessionManager : MonoBehaviour
    {
        public static UserSessionManager Instance { get; private set; }

        private UserSessionData currentSession;
        private string saveFilePath;

        private float sessionStartTime;
        private int totalInteractions;
        private float averageInteractionInterval;
        private List<float> interactionTimestamps = new List<float>();

        public event Action OnSessionStarted;
        public event Action OnSessionEnded;
        public event Action<float> OnSessionProgress;
        public event Action OnProgressSaved;

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

            saveFilePath = Path.Combine(Application.persistentDataPath, "LunarWeekOne_Session.json");
        }

        private void Start()
        {
            InitializeNewSession();
        }

        public void InitializeNewSession()
        {
            currentSession = new UserSessionData
            {
                currentDay = 1,
                completedDays = new List<int>(),
                sessionDuration = 0f,
                energyLevel = LunarConstants.DEFAULT_ENERGY_LEVEL,
                oxygenLevel = LunarConstants.DEFAULT_OXYGEN_LEVEL,
                waterLevel = LunarConstants.DEFAULT_WATER_LEVEL,
                completedRituals = new List<string>(),
                sessionStart = DateTime.Now,
                TotalInteractions = 0,
                AverageInteractionInterval = 0f
            };

            sessionStartTime = Time.time;
            totalInteractions =138;
            interactionTimestamps.Clear();
        }

        public void LoadProgress()
        {
            if (File.Exists(saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    currentSession = JsonUtility.FromJson<UserSessionData>(json);
                    Debug.Log($"[UserSessionManager] Progress loaded: Day {currentSession.currentDay}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[UserSessionManager] Failed to load progress: {e.Message}");
                    InitializeNewSession();
                }
            }
            else
            {
                Debug.Log("[UserSessionManager] No saved progress found");
                InitializeNewSession();
            }
        }

        public void SaveProgress()
        {
            if (currentSession == null) return;

            currentSession.sessionDuration = Time.time - sessionStartTime;

            string json = JsonUtility.ToJson(currentSession, true);
            try
            {
                File.WriteAllText(saveFilePath, json);
                Debug.Log($"[UserSessionManager] Progress saved to {saveFilePath}");
                OnProgressSaved?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UserSessionManager] Failed to save progress: {e.Message}");
            }
        }

        public void RecordInteraction()
        {
            float currentTime = Time.time;
            float elapsed = currentTime - sessionStartTime;

            totalInteractions++;
            interactionTimestamps.Add(currentTime);

            currentSession.TotalInteractions = totalInteractions;

            if (interactionTimestamps.Count > 1)
            {
                float totalInterval = 0f;
                for (int i = 1; i < interactionTimestamps.Count; i++)
                {
                    totalInterval += (interactionTimestamps[i] - interactionTimestamps[i - 1]);
                }
                averageInteractionInterval = totalInterval / (interactionTimestamps.Count - 1);
                currentSession.AverageInteractionInterval = averageInteractionInterval;
            }
        }

        public void CompleteDay(int dayNumber)
        {
            if (!currentSession.completedDays.Contains(dayNumber))
            {
                currentSession.completedDays.Add(dayNumber);
            }

            currentSession.currentDay = dayNumber + 1;

            SaveProgress();
        }

        public void RecordRitualCompletion(LunarDay day, RitualPhase finalPhase, bool completed, int interactionCount)
        {
            string ritualName = $"{day}_Ritual_{finalPhase}";
            if (!currentSession.completedRituals.Contains(ritualName))
            {
                currentSession.completedRituals.Add(ritualName);
            }

            SaveProgress();
        }

        public void UpdateResourceLevels(float energy, float oxygen, float water)
        {
            currentSession.energyLevel = energy;
            currentSession.oxygenLevel = oxygen;
            currentSession.waterLevel = water;
        }

        public void SetMoodMarkers(int before, int after)
        {
            if (currentSession != null)
            {
                currentSession.MoodBefore = before;
                currentSession.MoodAfter = after;
            }
        }

        public UserSessionData GetCurrentSession()
        {
            return currentSession;
        }

        public int GetCurrentDay()
        {
            return currentSession?.currentDay ?? 1;
        }

        public float GetSessionDuration()
        {
            return Time.time - sessionStartTime;
        }

        public int GetCompletedDayCount()
        {
            return currentSession?.completedDays?.Count ?? 0;
        }

        public float GetAverageInteractionInterval()
        {
            return averageInteractionInterval;
        }

        public void EndSession()
        {
            SaveProgress();
            OnSessionEnded?.Invoke();

            GenerateSessionReport();
        }

        private void GenerateSessionReport()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== Lunar Week One Session Report ===");
            report.AppendLine($"Session Date: {currentSession.sessionStart}");
            report.AppendLine($"Session Duration: {currentSession.sessionDuration:F1} seconds");
            report.AppendLine($"Completed Days: {currentSession.completedDays.Count}/7");
            report.AppendLine($"Total Interactions: {totalInteractions}");
            report.AppendLine($"Average Interaction Interval: {averageInteractionInterval:F2}s");
            report.AppendLine($"Energy Level: {currentSession.energyLevel:P}");
            report.AppendLine($"Oxygen Level: {currentSession.oxygenLevel:P}");
            report.AppendLine($"Water Level: {currentSession.waterLevel:P}");
            report.AppendLine($"Completed Rituals: {currentSession.completedRituals.Count}");

            Debug.Log(report.ToString());
        }
    }

    [System.Serializable]
    public class UserSessionData
    {
        public int currentDay = 1;
        public List<int> completedDays = new List<int>();
        public float sessionDuration = 0f;
        public DateTime sessionStart;

        public float energyLevel = 0.8f;
        public float oxygenLevel = 0.8f;
        public float waterLevel = 0.8f;

        public int MoodBefore;
        public int MoodAfter;

        public List<string> completedRituals = new List<string>();

        public int TotalInteractions = 0;
        public float AverageInteractionInterval = 0f;
    }
}