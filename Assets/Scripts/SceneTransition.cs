using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    private const string TransitionSceneName = "TransitionScene";

    [Header("Transition UI References")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private RectTransform topShutter;
    [SerializeField] private RectTransform bottomShutter;

    [Header("Transition Settings")]
    [SerializeField] private float duration = 0.35f;

    private bool isTransitioning = false;

    /// <summary>
    /// Automatically loads TransitionScene additively when Play Mode starts from ANY scene!
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapTransitionScene()
    {
        Scene transitionScene = SceneManager.GetSceneByName(TransitionSceneName);
        if (!transitionScene.isLoaded)
        {
            SceneManager.LoadSceneAsync(TransitionSceneName, LoadSceneMode.Additive);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        // Start with shutters open off-screen
        if (topShutter != null) topShutter.anchoredPosition = new Vector2(0f, 560f);
        if (bottomShutter != null) bottomShutter.anchoredPosition = new Vector2(0f, -560f);
    }

    public static void TransitionTo(string targetSceneName)
    {
        if (string.IsNullOrEmpty(targetSceneName)) return;

        // Fallback safety in case TransitionScene is still loading on frame 1
        if (Instance == null)
        {
            SceneManager.LoadSceneAsync(TransitionSceneName, LoadSceneMode.Additive).completed += _ =>
            {
                if (Instance != null) Instance.StartTransition(targetSceneName);
            };
            return;
        }

        Instance.StartTransition(targetSceneName);
    }

    private void StartTransition(string targetSceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(ExecuteTransition(targetSceneName));
    }

    private IEnumerator ExecuteTransition(string targetSceneName)
    {
        isTransitioning = true;
        fadeCanvasGroup.blocksRaycasts = true;

        // Track the currently active UI screen scene (not TransitionScene)
        Scene previousScene = SceneManager.GetActiveScene();

        // 1. Tween IN (EaseOutCubic) - Close Shutters & Fade In
        yield return AnimateOverlay(0f, 1f, duration, easeOut: true);

        // 2. Load Target Scene Additively
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
        while (loadOp != null && !loadOp.isDone) yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(targetSceneName);
        if (loadedScene.IsValid())
        {
            SceneManager.SetActiveScene(loadedScene);
        }

        // 3. Unload Previous UI Scene (keep TransitionScene loaded!)
        if (previousScene.IsValid() && previousScene.name != TransitionSceneName)
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(previousScene);
            while (unloadOp != null && !unloadOp.isDone) yield return null;
        }

        yield return new WaitForSecondsRealtime(0.08f);

        // 4. Tween OUT (EaseInCubic) - Open Shutters & Fade Out
        yield return AnimateOverlay(1f, 0f, duration, easeOut: false);

        fadeCanvasGroup.blocksRaycasts = false;
        isTransitioning = false;
    }

    private IEnumerator AnimateOverlay(float startAlpha, float endAlpha, float time, bool easeOut)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / time);

            // EaseOutCubic for closing, EaseInCubic for opening
            float eased = easeOut ? (1f - Mathf.Pow(1f - t, 3f)) : (t * t * t);

            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, eased);

            if (topShutter != null && bottomShutter != null)
            {
                float offset = Mathf.Lerp(easeOut ? 560f : 0f, easeOut ? 0f : 560f, eased);
                topShutter.anchoredPosition = new Vector2(0f, offset);
                bottomShutter.anchoredPosition = new Vector2(0f, -offset);
            }

            yield return null;
        }

        fadeCanvasGroup.alpha = endAlpha;
    }
}