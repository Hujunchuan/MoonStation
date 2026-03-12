using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lunar.Core
{
    public class ExperienceFeedbackCollector : MonoBehaviour
    {
        public static ExperienceFeedbackCollector Instance { get; private set; }

        [Header("UI Elements")]
        [SerializeField] private Canvas feedbackCanvas;
        [SerializeField] private Slider calmnessSlider;
        [SerializeField] private Slider desireSlider;
        [SerializeField] private Slider ritualComfortSlider;
        [SerializeField] private Slider presenceSlider;
        [SerializeField] private Slider worldFeelingSlider;

        [SerializeField] private Text feedbackSummaryText;
        [SerializeField] private Button submitFeedbackButton;

        private Dictionary<string, float> feedbackScores = new Dictionary<string, float>();
        private bool isCollectingFeedback;

        public event Action<Dictionary<string, float>> OnFeedbackSubmitted;

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

            if (feedbackCanvas != null)
            {
                feedbackCanvas.gameObject.SetActive(false);
            }
        }

        public void StartFeedbackCollection()
        {
            if (feedbackCanvas == null)
            {
                Debug.LogWarning("[FeedbackCollector] Feedback canvas not assigned");
                return;
            }

            isCollectingFeedback = true;

            if (calmnessSlider != null) calmnessSlider.value = 3f;
            if (desireSlider != null) desireSlider.value = inf3f;
            if (ritualComfortSlider != null) ritualComfortSlider.value = 3f;
            if (presenceSlider != null) presenceSlider.value = 3f;
            if (worldFeelingSlider != null) worldFeelingSlider.value = 3f;

            feedbackCanvas.gameObject.SetActive(true);

            if (submitFeedbackButton != null)
            {
                submitFeedbackButton.onClick.RemoveAllListeners();
                submitFeedbackButton.onClick.AddListener(SubmitFeedback);
            }
        }

        private void UpdateFeedbackSummary()
        {
            if (feedbackSummaryText == null) return;

            string summary = "Feedback Summary:\n\n";

            summary += $"Calmness: {(calmnessSlider != null ? calmnessSlider.value : 0f):F1}/5.0\n";
            summary += $"Desire to Check Phone: {(desireSlider != null ? desireSlider.value : 0f):F1}/5.0\n";
            summary += $"Ritual Comfort: {(ritualComfortSlider != null ? ritualComfortSlider.value : 0f):F1}/5.0\n";
            summary += $"Sense of Presence: {(presenceSlider != null ? presenceSlider.value : 0f):F1}/5.0\n";
            summary += $"World Feeling: {(worldFeelingSlider != null ? worldFeelingSlider.value : 0f):F1}/5.0\n";

            feedbackSummaryText.text = summary;
        }

        public void SubmitFeedback()
        {
            if (!isCollectingFeedback) return;

            feedbackScores["Calmness"] = calmnessSlider != null ? calmnessSlider.value : 0f;
            feedbackScores["PhoneDesire"] = desireSlider != null ? desireSlider.value : 0f;
            feedbackScores["RitualComfort"] = ritualComfortSlider != null ? ritualComfortSlider.value : 0f;
            feedbackScores["Presence"] = presenceSlider != null ? presenceSlider.value : 0f;
            feedbackScores["WorldFeeling"] = worldFeelingSlider != null ? worldFeelingSlider.value : 0f;

            float averageScore = CalculateAverageScore();
            feedbackScores["Average"] = averageScore;

            UserSessionManager.Instance?.SetMoodMarkers(
                (int)(feedbackScores["Presence"]),
                (int)(feedbackScores["Calmness"])
            );

            if (feedbackCanvas != null)
            {
                feedbackCanvas.gameObject.SetActive(false);
            }

            isCollectingFeedback = false;

            LogFeedbackResults();
            OnFeedbackSubmitted?.Invoke(feedbackScores);

            UserSessionManager.Instance?.EndSession();
        }

        public void CollectFinalFeedback()
        {
            StartFeedbackCollection();
        }

        private float CalculateAverageScore()
        {
            float sum = 0f;
            int count = 0;

            if (feedbackScores.ContainsKey("Calmness"))
            {
                sum += feedbackScores["Calmness"];
                count++;
            }

            if (feedbackScores.ContainsKey("RitualComfort"))
            {
                sum += feedbackScores["RitualComfort"];
                count++;
            }

            if (feedbackScores.ContainsKey("Presence"))
            {
                sum += feedbackScores["Presence"];
                count++;
            }

            if (feedbackScores.ContainsKey("WorldFeeling"))
            {
                sum += feedbackScores["WorldFeeling"];
                count++;
            }

            return count > 0 ? sum / count : 0f;
        }

        private void LogFeedbackResults()
        {
            Debug.Log($"[FeedbackCollector] Feedback Submitted:");
            foreach (var kvp in feedbackScores)
            {
                Debug.Log($"{kvp.Key}: {kvp.Value:F2}");
            }

            float average = feedbackScores.ContainsKey("Average") ? feedbackScores["Average"] : 0f;
            Debug.Log($"[FeedbackCollector] Average Score: {average:F2}/5.0");

            if (average >= 4.0f)
            {
                Debug.Log("[FeedbackCollector] ✓ MVP Success: Average score ≥ 4.0");
            }
            else if (average >= 3.0f)
            {
                Debug.Log("[FeedbackCollector] ⚠ Needs Improvement: Average score 3.0-4.0");
            }
            else
            {
                Debug.Log("[FeedbackCollector] ✗ Below Target: Average score < 3.0");
            }
        }

        public bool IsCollectingFeedback()
        {
            return isCollectingFeedback;
        }

        public Dictionary<string, float> GetLastFeedback()
        {
            return new Dictionary<string, float>(feedbackScores);
        }

        public float GetAverageFeedbackScore()
        {
            return feedbackScores.ContainsKey("Average") ? feedbackScores["Average"] : 0f;
        }

        public bool IsMVPSuccessful()
        {
            float calmness = feedbackScores.ContainsKey("Calmness") ? feedbackScores["Calmness"] : 0f;
            float desire = feedbackScores.ContainsKey("PhoneDesire") ? feedbackScores["PhoneDesire"] : 0f;
            float ritualComfort = feedbackScores.ContainsKey("RitualComfort") ? feedbackScores["RitualComfort"] : 0f;
            float presence = feedbackScores.ContainsKey("Presence") ? feedbackScores["Presence"] : 0f;
            float worldFeeling = feedbackScores.ContainsKey("WorldFeeling") ? feedbackScores["WorldFeeling"] : 0f;

            return calmness >= 4.0f && desire <= 2.0f && ritualComfort >= 4.0f && presence >= 4.0f && worldFeeling >= 4.0f;
        }
    }
}