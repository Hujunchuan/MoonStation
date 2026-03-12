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

        private readonly Dictionary<string, float> feedbackScores = new Dictionary<string, float>();
        private bool isCollectingFeedback;

        public event Action<Dictionary<string, float>> OnFeedbackSubmitted;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (feedbackCanvas != null)
            {
                feedbackCanvas.gameObject.SetActive(false);
            }

            RegisterConfiguredUi();
        }

        public void StartFeedbackCollection()
        {
            if (feedbackCanvas == null)
            {
                Debug.LogWarning("[ExperienceFeedbackCollector] Feedback canvas is not assigned");
                return;
            }

            isCollectingFeedback = true;

            SetSliderValue(calmnessSlider, 3f);
            SetSliderValue(desireSlider, 3f);
            SetSliderValue(ritualComfortSlider, 3f);
            SetSliderValue(presenceSlider, 3f);
            SetSliderValue(worldFeelingSlider, 3f);

            feedbackCanvas.gameObject.SetActive(true);
            UpdateFeedbackSummary();

            if (submitFeedbackButton != null)
            {
                submitFeedbackButton.onClick.RemoveAllListeners();
                submitFeedbackButton.onClick.AddListener(SubmitFeedback);
            }
        }

        public void CollectFinalFeedback()
        {
            StartFeedbackCollection();
        }

        public void SubmitFeedback()
        {
            if (!isCollectingFeedback)
            {
                return;
            }

            feedbackScores["Calmness"] = GetSliderValue(calmnessSlider);
            feedbackScores["PhoneDesire"] = GetSliderValue(desireSlider);
            feedbackScores["RitualComfort"] = GetSliderValue(ritualComfortSlider);
            feedbackScores["Presence"] = GetSliderValue(presenceSlider);
            feedbackScores["WorldFeeling"] = GetSliderValue(worldFeelingSlider);
            feedbackScores["Average"] = CalculateAverageScore();

            UserSessionManager.Instance?.SetMoodMarkers(
                Mathf.RoundToInt(feedbackScores["Presence"]),
                Mathf.RoundToInt(feedbackScores["Calmness"]));

            if (feedbackCanvas != null)
            {
                feedbackCanvas.gameObject.SetActive(false);
            }

            isCollectingFeedback = false;
            OnFeedbackSubmitted?.Invoke(new Dictionary<string, float>(feedbackScores));
            UserSessionManager.Instance?.EndSession();
        }

        private void UpdateFeedbackSummary()
        {
            if (feedbackSummaryText == null)
            {
                return;
            }

            feedbackSummaryText.text =
                "Feedback Summary\n\n" +
                $"Calmness: {GetSliderValue(calmnessSlider):F1}/5.0\n" +
                $"Phone Desire: {GetSliderValue(desireSlider):F1}/5.0\n" +
                $"Ritual Comfort: {GetSliderValue(ritualComfortSlider):F1}/5.0\n" +
                $"Presence: {GetSliderValue(presenceSlider):F1}/5.0\n" +
                $"World Feeling: {GetSliderValue(worldFeelingSlider):F1}/5.0";
        }

        private float CalculateAverageScore()
        {
            float sum = feedbackScores["Calmness"] +
                        feedbackScores["RitualComfort"] +
                        feedbackScores["Presence"] +
                        feedbackScores["WorldFeeling"];

            return sum / 4f;
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
            return feedbackScores.ContainsKey("Average") &&
                   feedbackScores["Average"] >= 4f &&
                   feedbackScores["PhoneDesire"] <= 2f;
        }

        public void ConfigureUi(
            Canvas canvas,
            Slider calmness,
            Slider desire,
            Slider ritualComfort,
            Slider presence,
            Slider worldFeeling,
            Text summary,
            Button submit)
        {
            feedbackCanvas = canvas;
            calmnessSlider = calmness;
            desireSlider = desire;
            ritualComfortSlider = ritualComfort;
            presenceSlider = presence;
            worldFeelingSlider = worldFeeling;
            feedbackSummaryText = summary;
            submitFeedbackButton = submit;

            if (feedbackCanvas != null)
            {
                feedbackCanvas.gameObject.SetActive(false);
            }

            RegisterConfiguredUi();
        }

        private void RegisterConfiguredUi()
        {
            RegisterSlider(calmnessSlider);
            RegisterSlider(desireSlider);
            RegisterSlider(ritualComfortSlider);
            RegisterSlider(presenceSlider);
            RegisterSlider(worldFeelingSlider);
        }

        private void RegisterSlider(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float value)
        {
            UpdateFeedbackSummary();
        }

        private void SetSliderValue(Slider slider, float value)
        {
            if (slider != null)
            {
                slider.value = value;
            }
        }

        private float GetSliderValue(Slider slider)
        {
            return slider != null ? slider.value : 0f;
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
