using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UITweenEffects : MonoBehaviour
{
    public enum FadeMode
    {
        None,
        FadeInOnEnable,
        FadeOutOnEnable,
        FadeInOutLoop
    }

    [Header("Fade Settings")]
    [SerializeField] private FadeMode fadeMode = FadeMode.FadeInOnEnable;
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float startDelay = 0.15f;

    [Header("Loop Settings (For FadeInOutLoop)")]
    [SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float maxAlpha = 1.0f;

    private CanvasGroup canvasGroup;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        if (fadeMode == FadeMode.FadeInOnEnable)
        {
            FadeIn();
        }
        else if (fadeMode == FadeMode.FadeOutOnEnable)
        {
            FadeOut();
        }
        else if (fadeMode == FadeMode.FadeInOutLoop)
        {
            StartFadeLoop();
        }
    }

    public void FadeIn()
    {
        StartFade(0f, 1f, fadeDuration, startDelay);
    }

    public void FadeOut()
    {
        StartFade(1f, 0f, fadeDuration, startDelay);
    }

    public void StartFadeLoop()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(FadeInOutLoopRoutine());
    }

    private void StartFade(float fromAlpha, float toAlpha, float duration, float delay)
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(FadeRoutine(fromAlpha, toAlpha, duration, delay));
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, float duration, float delay)
    {
        canvasGroup.alpha = fromAlpha;

        // Wait 2 frames so Unity's scene-load / Play Mode hitch doesn't eat the tween timer
        yield return null;
        yield return null;

        if (delay > 0f)
        {
            float delayElapsed = 0f;
            while (delayElapsed < delay)
            {
                delayElapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                yield return null;
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Clamp delta time so an editor hiccup never skips the fade
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float t = Mathf.Clamp01(elapsed / duration);

            // EaseInOutSine curve for smooth fade in / fade out
            float eased = -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);

            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }

    private IEnumerator FadeInOutLoopRoutine()
    {
        canvasGroup.alpha = 0f;

        // First fade in from 0 to maxAlpha (includes the initial startDelay)
        yield return FadeRoutine(0f, maxAlpha, fadeDuration, startDelay);

        // Continuously alternate between Fade Out (to minAlpha) and Fade In (to maxAlpha)
        while (true)
        {
            yield return FadeRoutine(maxAlpha, minAlpha, fadeDuration, 0f);
            yield return FadeRoutine(minAlpha, maxAlpha, fadeDuration, 0f);
        }
    }
}