using UnityEngine;
using UnityEngine.UI;
using Lunar.Data;

namespace Lunar.Core
{
    public class LunarWorldDisplayPanel : MonoBehaviour
    {
        public enum DisplayMode
        {
            Operations,
            Guidance
        }

        [SerializeField] private Text titleText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text guidanceText;
        [SerializeField] private Text alertText;
        [SerializeField] private Image alertBackground;
        [SerializeField] private DisplayMode displayMode = DisplayMode.Operations;
        [SerializeField] private string panelLabel = "OPS";

        private float nextRefreshTime;

        public void Configure(
            Text title,
            Text status,
            Text guidance,
            Text alert,
            Image alertBg,
            DisplayMode mode = DisplayMode.Operations,
            string labelOverride = null)
        {
            titleText = title;
            statusText = status;
            guidanceText = guidance;
            alertText = alert;
            alertBackground = alertBg;
            displayMode = mode;
            panelLabel = string.IsNullOrWhiteSpace(labelOverride) ? GetDefaultLabel(mode) : labelOverride;
            RefreshDisplay();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.unscaledTime + 0.2f;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            LunarDayStateMachine stateMachine = LunarDayStateMachine.Instance;
            ResourceManager resourceManager = ResourceManager.Instance;
            RitualEngine ritualEngine = RitualEngine.Instance;
            LunarEnvironmentController environmentController = FindObjectOfType<LunarEnvironmentController>();

            if (stateMachine == null || resourceManager == null)
            {
                if (titleText != null) titleText.text = panelLabel;
                if (statusText != null) statusText.text = displayMode == DisplayMode.Guidance ? "Calibrating reflection prompts..." : "Booting local systems...";
                if (guidanceText != null) guidanceText.text = "Waiting for runtime services.";
                if (alertText != null) alertText.text = "STANDBY";
                return;
            }

            var config = stateMachine.GetCurrentConfig();

            if (titleText != null)
            {
                string dayLabel = config != null ? config.dayName.ToUpperInvariant() : $"DAY {(int)stateMachine.CurrentDay}";
                titleText.text = $"{panelLabel} // {dayLabel}";
            }

            if (statusText != null)
            {
                statusText.text = BuildStatusBlock(stateMachine, resourceManager, config);
            }

            if (guidanceText != null)
            {
                guidanceText.text = BuildGuidanceBlock(ritualEngine, config);
            }

            if (alertText == null)
            {
                return;
            }

            Color alertColor = new Color(0.25f, 0.78f, 0.96f, 1f);
            string alertMessage = "ALL SYSTEMS STABLE";

            if (ritualEngine != null && ritualEngine.IsAwaitingRequiredInteraction())
            {
                alertColor = new Color(0.24f, 0.92f, 1f, 1f);
                alertMessage = "RITUAL INPUT REQUIRED";
            }
            else if (environmentController != null &&
                environmentController.GetCurrentState() == LunarEnvironmentController.EnvironmentState.Anomaly)
            {
                alertColor = new Color(1f, 0.42f, 0.28f, 1f);
                alertMessage = "ANOMALY CONTAINMENT";
            }

            alertText.text = alertMessage;
            alertText.color = alertColor;

            if (alertBackground != null)
            {
                alertBackground.color = new Color(alertColor.r, alertColor.g, alertColor.b, 0.18f);
            }
        }

        private string BuildStatusBlock(LunarDayStateMachine stateMachine, ResourceManager resourceManager, LunarDayConfig config)
        {
            if (displayMode == DisplayMode.Guidance)
            {
                string themeLine = config != null ? config.theme : "Theme unavailable.";
                string goalLine = config != null ? config.emotionalGoal : "Goal unavailable.";
                return
                    $"THEME  {themeLine}\n" +
                    $"STATE  {stateMachine.CurrentState}\n" +
                    $"GOAL   {goalLine}";
            }

            return
                $"STATE  {stateMachine.CurrentState}\n" +
                $"ENERGY {resourceManager.GetResource(ResourceType.Energy):P0}\n" +
                $"OXYGEN {resourceManager.GetResource(ResourceType.Oxygen):P0}\n" +
                $"WATER  {resourceManager.GetResource(ResourceType.Water):P0}";
        }

        private string BuildGuidanceBlock(RitualEngine ritualEngine, LunarDayConfig config)
        {
            string questionLine = config != null ? config.dramaticQuestion : "Question unavailable.";
            string interactionHint = ritualEngine != null && ritualEngine.IsAwaitingRequiredInteraction()
                ? "Ritual ready. Focus the valve and confirm."
                : "Recover resources, then move toward the quiet deck.";

            if (displayMode == DisplayMode.Guidance)
            {
                string goalLine = config != null ? config.emotionalGoal : "Narrative goal unavailable.";
                return
                    $"BREATHE\n{interactionHint}\n\n" +
                    $"GOAL\n{goalLine}\n\n" +
                    $"QUESTION\n{questionLine}";
            }

            string goalLineDefault = config != null ? config.emotionalGoal : "Narrative goal unavailable.";
            return
                $"GOAL\n{goalLineDefault}\n\n" +
                $"QUESTION\n{questionLine}";
        }

        private static string GetDefaultLabel(DisplayMode mode)
        {
            return mode == DisplayMode.Guidance ? "QUIET DECK" : "OPS";
        }
    }
}
