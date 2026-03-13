using UnityEngine;

namespace Lunar.Core
{
    public class LunarPrototypeFocusDirector : MonoBehaviour
    {
        [SerializeField] private LunarCameraController cameraController;
        [SerializeField] private Transform defaultAnchor;
        [SerializeField] private Transform resourceAnchor;
        [SerializeField] private Transform ritualAnchor;
        [SerializeField] private Transform quietDeckAnchor;
        [SerializeField] private KeyCode focusHotkey = KeyCode.C;
        [SerializeField] private bool autoFocusOnContextChange = true;
        [SerializeField] private float autoFocusDelay = 0.08f;

        private LunarDayStateMachine stateMachine;
        private RitualEngine ritualEngine;
        private bool hasSnapshot;
        private LunarDay lastDay;
        private LunarDayState lastState;
        private bool lastAwaitingInteraction;
        private bool pendingAutoFocus;
        private float nextAutoFocusTime;

        public void Configure(
            LunarCameraController controller,
            Transform fallbackAnchor,
            Transform resourceFocus,
            Transform ritualFocus,
            Transform quietFocus)
        {
            cameraController = controller;
            defaultAnchor = fallbackAnchor;
            resourceAnchor = resourceFocus;
            ritualAnchor = ritualFocus;
            quietDeckAnchor = quietFocus;

            CacheDependencies();
            pendingAutoFocus = true;
            nextAutoFocusTime = Time.unscaledTime + autoFocusDelay;
            CaptureSnapshot();
        }

        private void Update()
        {
            CacheDependencies();

            if (Input.GetKeyDown(focusHotkey))
            {
                FocusCurrentContext();
            }

            if (!autoFocusOnContextChange || stateMachine == null)
            {
                return;
            }

            bool awaitingInteraction = ritualEngine != null && ritualEngine.IsAwaitingRequiredInteraction();
            if (!hasSnapshot ||
                lastDay != stateMachine.CurrentDay ||
                lastState != stateMachine.CurrentState ||
                lastAwaitingInteraction != awaitingInteraction)
            {
                CaptureSnapshot();
                pendingAutoFocus = true;
                nextAutoFocusTime = Time.unscaledTime + autoFocusDelay;
            }

            if (pendingAutoFocus && Time.unscaledTime >= nextAutoFocusTime)
            {
                pendingAutoFocus = false;
                FocusCurrentContext();
            }
        }

        private void CacheDependencies()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<LunarCameraController>();
            }

            if (stateMachine == null)
            {
                stateMachine = LunarDayStateMachine.Instance;
            }

            if (ritualEngine == null)
            {
                ritualEngine = RitualEngine.Instance;
            }
        }

        private void CaptureSnapshot()
        {
            if (stateMachine == null)
            {
                return;
            }

            lastDay = stateMachine.CurrentDay;
            lastState = stateMachine.CurrentState;
            lastAwaitingInteraction = ritualEngine != null && ritualEngine.IsAwaitingRequiredInteraction();
            hasSnapshot = true;
        }

        private void FocusCurrentContext()
        {
            if (cameraController == null)
            {
                return;
            }

            Transform anchor = defaultAnchor;
            float zoom = 60f;

            bool awaitingInteraction = ritualEngine != null && ritualEngine.IsAwaitingRequiredInteraction();
            LunarDayState currentState = stateMachine != null ? stateMachine.CurrentState : LunarDayState.None;
            LunarDay currentDay = stateMachine != null ? stateMachine.CurrentDay : LunarDay.Day1_Arrival;

            if (awaitingInteraction || currentState == LunarDayState.Ritual)
            {
                anchor = ritualAnchor != null ? ritualAnchor : defaultAnchor;
                zoom = 52f;
            }
            else if (currentState == LunarDayState.ResourceManagement)
            {
                anchor = resourceAnchor != null ? resourceAnchor : defaultAnchor;
                zoom = 58f;
            }
            else if (currentDay == LunarDay.Day7_Reflection ||
                currentState == LunarDayState.Introduction ||
                currentState == LunarDayState.Narration ||
                currentState == LunarDayState.Transition)
            {
                anchor = quietDeckAnchor != null ? quietDeckAnchor : defaultAnchor;
                zoom = 56f;
            }

            cameraController.FocusOn(anchor, zoom);
        }
    }
}
