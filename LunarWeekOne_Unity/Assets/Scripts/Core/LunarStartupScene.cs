using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lunar.Core
{
    public class LunarStartupScene : MonoBehaviour
    {
        [Header("Scene Configuration")]
        [SerializeField] private string mainExperienceScene = "LunarBase";
        [SerializeField] private float fadeDuration = 2f;
        [SerializeField] private CanvasGroup fadeCanvasGroup;

        [Header("Startup UI")]
        [SerializeField] private GameObject startupPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject errorPanel;
        [SerializeField] private Text loadingText;
        [SerializeField] private Slider loadingSlider;

        [Header("Startup Settings")]
        [SerializeField] private bool skipStartup = false;
        [SerializeField] private bool loadSavedProgress = true;
        [SerializeField] private bool enableTutorial = false;

        private AsyncOperation loadOperation;
        private bool isLoading = false;

        private void Start()
        {
            InitializeStartupScene();

            if (skipStartup)
            {
                StartCoroutine(SkipToExperience());
            }
            else
            {
                ShowStartupPanel();
            }
        }

        private void InitializeStartupScene()
        {
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
            }

            if (startupPanel != null) startupPanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(false);
        }

        private void ShowStartupPanel()
        {
            if (startupPanel != null)
            {
                startupPanel.SetActive(true);
            }
        }

        public void StartNewExperience()
        {
            if (isLoading) return;

            UserSessionManager.Instance?.InitializeNewSession();
            LoadExperienceScene();
        }

        public void ContinueExperience()
        {
            if (isLoading) return;

            if (loadSavedProgress)
            {
                UserSessionManager.Instance?.LoadProgress();
            }
            LoadExperienceScene();
        }

        public void StartTutorial()
        {
            if (isLoading) return;

            enableTutorial = true;
            LoadExperienceScene();
        }

        private void LoadExperienceScene()
        {
            if (isLoading) return;

            isLoading = true;

            if (startupPanel != null) startupPanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(true);

            StartCoroutine(LoadSceneAsync());
        }

        private IEnumerator LoadSceneAsync()
        {
            if (fadeCanvasGroup != null)
            {
                yield return StartCoroutine(FadeIn(fadeDuration));
            }

            loadOperation = SceneManager.LoadSceneAsync(mainExperienceScene);
            loadOperation.allowSceneActivation = false;

            while (!loadOperation.isDone)
            {
                float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
                UpdateLoadingProgress(progress);

                if (loadOperation.progress >= 0.9f)
                {
                    break;
                }

                yield return null;
            }

            UpdateLoadingProgress(1f);

            yield return new WaitForSeconds(0.5f);

            loadOperation.allowSceneActivation = true;

            yield return new WaitForSeconds(0.5f);

            if (fadeCanvasGroup != null)
            {
                yield return StartCoroutine(FadeOut(fadeDuration));
            }
        }

        private void UpdateLoadingProgress(float progress)
        {
            if (loadingText != null)
            {
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            }

            if (loadingSlider != null)
            {
                loadingSlider.value = progress;
            }
        }

        private IEnumerator FadeIn(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }

        private IEnumerator SkipToExperience()
        {
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 1f;
            }

            if (startupPanel != null) startupPanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(true);

            yield return new WaitForSeconds(0.5f);

            loadOperation = SceneManager.LoadSceneAsync(mainExperienceScene);
            loadOperation.allowSceneActivation = false;

            while (!loadOperation.isDone)
            {
                if (loadOperation.progress >= 0.9f)
                {
                    break;
                }
                yield return null;
            }

            loadOperation.allowSceneActivation = true;
        }

        public void ShowError(string errorMessage)
        {
            if (errorPanel != null)
            {
                errorPanel.SetActive(true);
                Text errorText = errorPanel.GetComponentInChildren<Text>();
                if (errorText != null)
                {
                    errorText.text = errorMessage;
                }
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

        public void OpenSettings()
        {
        }

        public void ShowCredits()
        {
        }
    }
}