using System;
using System.Collections.Generic;
using System.IO;
using Lunar.Data;
using UnityEngine;

namespace Lunar.Core
{
    public class UserSessionManager : MonoBehaviour
    {
        public static UserSessionManager Instance { get; private set; }

        private UserSessionData currentSession;
        private string saveFilePath;
        private float sessionStartTime;
        private readonly List<float> interactionTimestamps = new List<float>();

        public event Action OnSessionStarted;
        public event Action OnSessionEnded;
        public event Action OnProgressSaved;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveFilePath = Path.Combine(Application.persistentDataPath, "LunarWeekOne_Session.json");
        }

        public bool HasSavedProgress()
        {
            return File.Exists(saveFilePath);
        }

        public bool HasActiveSession()
        {
            return currentSession != null;
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
                sessionStartIso = DateTime.UtcNow.ToString("o"),
                totalInteractions = 0,
                averageInteractionInterval = 0f
            };

            interactionTimestamps.Clear();
            sessionStartTime = Time.time;
            OnSessionStarted?.Invoke();
        }

        public void LoadProgress()
        {
            if (!HasSavedProgress())
            {
                InitializeNewSession();
                return;
            }

            try
            {
                string json = File.ReadAllText(saveFilePath);
                currentSession = JsonUtility.FromJson<UserSessionData>(json) ?? new UserSessionData();
                EnsureSessionCollections();
                ResumeActiveSession();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[UserSessionManager] Failed to load progress: {exception.Message}");
                InitializeNewSession();
            }
        }

        public void ResumeActiveSession()
        {
            if (currentSession == null)
            {
                InitializeNewSession();
                return;
            }

            EnsureSessionCollections();
            sessionStartTime = Time.time - Mathf.Max(0f, currentSession.sessionDuration);
            interactionTimestamps.Clear();
            OnSessionStarted?.Invoke();
        }

        public void SaveProgress()
        {
            EnsureSession();
            currentSession.sessionDuration = GetSessionDuration();

            try
            {
                string json = JsonUtility.ToJson(currentSession, true);
                File.WriteAllText(saveFilePath, json);
                OnProgressSaved?.Invoke();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[UserSessionManager] Failed to save progress: {exception.Message}");
            }
        }

        public void RecordInteraction()
        {
            EnsureSession();

            float now = Time.time;
            interactionTimestamps.Add(now);
            currentSession.totalInteractions++;

            if (interactionTimestamps.Count > 1)
            {
                float totalInterval = 0f;

                for (int index = 1; index < interactionTimestamps.Count; index++)
                {
                    totalInterval += interactionTimestamps[index] - interactionTimestamps[index - 1];
                }

                currentSession.averageInteractionInterval = totalInterval / (interactionTimestamps.Count - 1);
            }
        }

        public void CompleteDay(int dayNumber)
        {
            EnsureSession();

            if (!currentSession.completedDays.Contains(dayNumber))
            {
                currentSession.completedDays.Add(dayNumber);
            }

            currentSession.currentDay = Mathf.Clamp(dayNumber + 1, 1, LunarConstants.TOTAL_LUNAR_DAYS);
            SaveProgress();
        }

        public void RecordRitualCompletion(LunarDay day, RitualPhase finalPhase, bool completed, int interactionCount)
        {
            EnsureSession();

            string record = $"{day}:{finalPhase}:{completed}:{interactionCount}";
            if (!currentSession.completedRituals.Contains(record))
            {
                currentSession.completedRituals.Add(record);
            }

            SaveProgress();
        }

        public void UpdateResourceLevels(float energy, float oxygen, float water)
        {
            EnsureSession();
            currentSession.energyLevel = energy;
            currentSession.oxygenLevel = oxygen;
            currentSession.waterLevel = water;
        }

        public void SetCurrentDay(LunarDay day)
        {
            EnsureSession();
            currentSession.currentDay = Mathf.Clamp((int)day, 1, LunarConstants.TOTAL_LUNAR_DAYS);
        }

        public void SetMoodMarkers(int before, int after)
        {
            EnsureSession();
            currentSession.moodBefore = before;
            currentSession.moodAfter = after;
        }

        public UserSessionData GetCurrentSession()
        {
            EnsureSession();
            return currentSession;
        }

        public int GetCurrentDay()
        {
            EnsureSession();
            return currentSession.currentDay;
        }

        public LunarDay GetCurrentDayEnum()
        {
            EnsureSession();
            int clamped = Mathf.Clamp(currentSession.currentDay, 1, LunarConstants.TOTAL_LUNAR_DAYS);
            return (LunarDay)clamped;
        }

        public float GetSessionDuration()
        {
            EnsureSession();
            return Time.time - sessionStartTime;
        }

        public int GetCompletedDayCount()
        {
            EnsureSession();
            return currentSession.completedDays.Count;
        }

        public float GetAverageInteractionInterval()
        {
            EnsureSession();
            return currentSession.averageInteractionInterval;
        }

        public void EndSession()
        {
            SaveProgress();
            OnSessionEnded?.Invoke();
        }

        private void EnsureSession()
        {
            if (currentSession == null)
            {
                InitializeNewSession();
            }
        }

        private void EnsureSessionCollections()
        {
            if (currentSession.completedDays == null)
            {
                currentSession.completedDays = new List<int>();
            }

            if (currentSession.completedRituals == null)
            {
                currentSession.completedRituals = new List<string>();
            }

            currentSession.currentDay = Mathf.Clamp(currentSession.currentDay, 1, LunarConstants.TOTAL_LUNAR_DAYS);
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
