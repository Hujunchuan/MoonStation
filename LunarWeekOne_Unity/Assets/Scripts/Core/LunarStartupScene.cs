using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lunar.Core
{
    public class LunarStartupScene : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private string mainExperienceScene = "LunarBase";
        [SerializeField] private float fadeDuration = 1.5f;
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        [Header("Startup UI")]
        [SerializeField] private GameObject startupPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private Text loadingText;
        [SerializeField] private Slider loadingSlider;
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text errorText;

        [Header("Startup Settings")]
        [SerializeField] private bool skipStartup;
        [SerializeField] private bool loadSavedProgress = true;

        private AsyncOperation loadOperation;
        private bool isLoading;

        private void Start()
        {
            InitializeStartupScene();
            RegisterButtonCallbacks();
            RefreshStartupUi();

            if (skipStartup)
            {
                ContinueExperience();
            }
            else
            {
                startupPanel?.SetActive(true);
            }
        }

        private void InitializeStartupScene()
        {
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
            }

            startupPanel?.SetActive(false);
            loadingPanel?.SetActive(false);
            errorPanel?.SetActive(false);
        }

        private void RegisterButtonCallbacks()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(StartNewExperience);
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(ContinueExperience);
            }
        }

        private void RefreshStartupUi()
        {
            bool hasSave = loadSavedProgress &&
                           UserSessionManager.Instance != null &&
                           UserSessionManager.Instance.HasSavedProgress();

            if (continueButton != null)
            {
                continueButton.interactable = hasSave;
            }

            if (statusText != null)
            {
                statusText.text = hasSave
                    ? "Saved progress detected. Continue is available."
                    : "No saved progress yet. Start a fresh session.";
            }
        }

        public void StartNewExperience()
        {
            if (isLoading)
            {
                return;
            }

            UserSessionManager.Instance?.InitializeNewSession();
            LoadExperienceScene();
        }

        public void ContinueExperience()
        {
            if (isLoading)
            {
                return;
            }

            if (loadSavedProgress && UserSessionManager.Instance != null && UserSessionManager.Instance.HasSavedProgress())
            {
                UserSessionManager.Instance.LoadProgress();
            }
            else
            {
                UserSessionManager.Instance?.InitializeNewSession();
            }

            LoadExperienceScene();
        }

        private void LoadExperienceScene()
        {
            isLoading = true;
            startupPanel?.SetActive(false);
            loadingPanel?.SetActive(true);
            UpdateLoadingProgress(0f);
            UpdateStatusText("Launching lunar base...");
            StartCoroutine(LoadSceneAsync());
        }

        private IEnumerator LoadSceneAsync()
        {
            if (fadeCanvasGroup != null)
            {
                yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
            }

            loadOperation = SceneManager.LoadSceneAsync(mainExperienceScene);
            if (loadOperation == null)
            {
                ShowError($"Unable to load scene: {mainExperienceScene}");
                isLoading = false;
                yield break;
            }

            loadOperation.allowSceneActivation = false;

            while (loadOperation.progress < 0.9f)
            {
                UpdateLoadingProgress(Mathf.Clamp01(loadOperation.progress / 0.9f));
                yield return null;
            }

            UpdateLoadingProgress(1f);
            yield return new WaitForSeconds(0.25f);
            loadOperation.allowSceneActivation = true;
        }

        private void UpdateLoadingProgress(float progress)
        {
            if (loadingText != null)
            {
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100f)}%";
            }

            if (loadingSlider != null)
            {
                loadingSlider.value = progress;
            }
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            fadeCanvasGroup.alpha = to;
        }

        public void ShowError(string errorMessage)
        {
            isLoading = false;
            loadingPanel?.SetActive(false);
            errorPanel?.SetActive(true);
            UpdateStatusText(errorMessage);

            if (errorPanel != null)
            {
                if (errorText != null)
                {
                    errorText.text = errorMessage;
                }
            }
        }

        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
