using UnityEngine;
using UnityEngine.UI;

namespace Lunar.Core
{
    public class LunarPrototypeDebugPanel : MonoBehaviour
    {
        private Text headerText;
        private Text resourceText;
        private Text hintText;
        private Button feedbackButton;

        public void Configure(Text header, Text resources, Text hint, Button button)
        {
            headerText = header;
            resourceText = resources;
            hintText = hint;
            feedbackButton = button;

            if (feedbackButton != null)
            {
                feedbackButton.onClick.RemoveAllListeners();
                feedbackButton.onClick.AddListener(OpenFeedback);
            }

            UpdateHintText();
        }

        private void Update()
        {
            UpdateHeaderText();
            UpdateResourceText();
            UpdateHintText();
        }

        private void UpdateHeaderText()
        {
            if (headerText == null)
            {
                return;
            }

            LunarDayStateMachine stateMachine = LunarDayStateMachine.Instance;
            UserSessionManager sessionManager = UserSessionManager.Instance;

            if (stateMachine == null || sessionManager == null)
            {
                headerText.text = "Lunar Prototype Booting...";
                return;
            }

            var config = stateMachine.GetCurrentConfig();
            string dayLabel = config != null ? config.dayName : $"Day {(int)stateMachine.CurrentDay}";
            string themeLabel = config != null ? config.theme : "Theme unavailable";

            headerText.text =
                $"{dayLabel} | {stateMachine.CurrentState}\n" +
                $"Theme: {themeLabel}\n" +
                $"Elapsed: {sessionManager.GetSessionDuration():F1}s";
        }

        private void UpdateResourceText()
        {
            if (resourceText == null)
            {
                return;
            }

            ResourceManager resourceManager = ResourceManager.Instance;
            LunarDayStateMachine stateMachine = LunarDayStateMachine.Instance;
            if (resourceManager == null)
            {
                resourceText.text = "Resources unavailable";
                return;
            }

            var config = stateMachine != null ? stateMachine.GetCurrentConfig() : null;
            string goalLine = config != null ? config.emotionalGoal : "Narrative goal unavailable";

            resourceText.text =
                $"Energy: {resourceManager.GetResource(ResourceType.Energy):P0}\n" +
                $"Oxygen: {resourceManager.GetResource(ResourceType.Oxygen):P0}\n" +
                $"Water: {resourceManager.GetResource(ResourceType.Water):P0}\n" +
                $"Goal: {goalLine}";
        }

        private void UpdateHintText()
        {
            if (hintText == null)
            {
                return;
            }

            LunarDayStateMachine stateMachine = LunarDayStateMachine.Instance;
            var config = stateMachine != null ? stateMachine.GetCurrentConfig() : null;
            string questionLine = config != null ? config.dramaticQuestion : "Question unavailable";

            hintText.text =
                $"Question: {questionLine}\n" +
                "Left click nodes to recover resources\n" +
                "R to interact with ritual valve, N to skip, ESC to end";
        }

        private void OpenFeedback()
        {
            ExperienceFeedbackCollector.Instance?.StartFeedbackCollection();
        }
    }
}
