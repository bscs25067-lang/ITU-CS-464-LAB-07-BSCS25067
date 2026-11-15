using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class UITweenEffects : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    public enum FadeMode
    {
        None,
        FadeInOnEnable,
        FadeOutOnEnable,
    }

    [Header("Fade Settings (CanvasGroup Alpha)")]
    [SerializeField] private FadeMode fadeMode = FadeMode.FadeInOnEnable;
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float startDelay = 0.15f;

    [Header("Color Tween Settings (Image / Graphic Color)")]
    [SerializeField] private bool colorTweenOnEnable = false;
    [Tooltip("Leave empty to use the Image/Graphic on this GameObject.")]
    [SerializeField] private Graphic targetGraphic;
    [SerializeField] private Color startColor = Color.black;
    [SerializeField] private Color endColor = Color.white;
    [SerializeField] private float colorDuration = 0.8f;
    [SerializeField] private float colorDelay = 0.15f;

    [Header("RE2 Highlight Shift (Move Right on Select/Hover)")]
    [SerializeField] private bool enableHighlightShift = false;
    [Tooltip("Leave empty to shift this GameObject, or assign the child Text/Content RectTransform.")]
    [SerializeField] private RectTransform targetToShift;
    [SerializeField] private float shiftRightDistance = 24f;
    [SerializeField] private float shiftDuration = 0.18f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private Coroutine colorCoroutine;
    private Coroutine shiftCoroutine;
    private Vector2 baseAnchoredPosition;
    private bool basePositionInitialized = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (targetGraphic == null)
        {
            targetGraphic = GetComponent<Graphic>();
        }

        if (targetToShift == null)
        {
            targetToShift = GetComponent<RectTransform>();
        }
    }

    private IEnumerator Start()
    {
        // Wait 1 frame so LayoutGroups finish positioning before caching base X/Y
        yield return null;
        CacheBasePosition();
    }

    private void CacheBasePosition()
    {
        if (targetToShift != null && !basePositionInitialized)
        {
            baseAnchoredPosition = targetToShift.anchoredPosition;
            basePositionInitialized = true;
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

        if (colorTweenOnEnable)
        {
            PlayColorTween();
        }
    }

    private void OnDisable()
    {
        if (basePositionInitialized && targetToShift != null)
        {
            targetToShift.anchoredPosition = baseAnchoredPosition;
        }
    }

    // --- CanvasGroup Alpha Fade ---

    public void FadeIn()
    {
        StartFade(0f, 1f, fadeDuration, startDelay);
    }

    public void FadeOut()
    {
        StartFade(1f, 0f, fadeDuration, startDelay);
    }

    private void StartFade(float fromAlpha, float toAlpha, float duration, float delay)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(fromAlpha, toAlpha, duration, delay));
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
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float t = Mathf.Clamp01(elapsed / duration);

            // EaseInOutSine curve for smooth fade in / fade out
            float eased = -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, eased);

            yield return null;
        }

        canvasGroup.alpha = toAlpha;
    }

    // --- Image / Graphic Color Tween (Black -> White) ---

    public void PlayColorTween()
    {
        TweenColor(startColor, endColor, colorDuration, colorDelay);
    }

    public void TweenColor(Color fromColor, Color toColor, float duration, float delay = 0f)
    {
        if (targetGraphic == null) return;
        if (colorCoroutine != null) StopCoroutine(colorCoroutine);
        colorCoroutine = StartCoroutine(ColorRoutine(fromColor, toColor, duration, delay));
    }

    private IEnumerator ColorRoutine(Color fromColor, Color toColor, float duration, float delay)
    {
        targetGraphic.color = fromColor;

        // Wait 2 frames to bypass initial scene-load deltaTime spike
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
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float t = Mathf.Clamp01(elapsed / duration);

            // EaseInOutSine curve for smooth color transition
            float eased = -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
            targetGraphic.color = Color.Lerp(fromColor, toColor, eased);

            yield return null;
        }

        targetGraphic.color = toColor;
    }

    // --- RE2 Highlight / Selection Shift Logic ---

    public void OnPointerEnter(PointerEventData eventData) => ShiftHighlight(true);
    public void OnPointerExit(PointerEventData eventData) => ShiftHighlight(false);
    public void OnPointerDown(PointerEventData eventData) => ShiftHighlight(true);
    public void OnPointerUp(PointerEventData eventData) => ShiftHighlight(false);
    public void OnSelect(BaseEventData eventData) => ShiftHighlight(true);
    public void OnDeselect(BaseEventData eventData) => ShiftHighlight(false);

    public void ShiftHighlight(bool isHighlighted)
    {
        if (!enableHighlightShift || targetToShift == null || !gameObject.activeInHierarchy) return;

        CacheBasePosition();

        Vector2 targetPos = isHighlighted
            ? baseAnchoredPosition + new Vector2(shiftRightDistance, 0f)
            : baseAnchoredPosition;

        if (shiftCoroutine != null) StopCoroutine(shiftCoroutine);
        shiftCoroutine = StartCoroutine(ShiftRoutine(targetPos));
    }

    private IEnumerator ShiftRoutine(Vector2 targetPos)
    {
        Vector2 startPos = targetToShift.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shiftDuration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float t = Mathf.Clamp01(elapsed / shiftDuration);

            // EaseOutCubic: 1 - (1 - t)^3 for snappy RE2 menu response
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            targetToShift.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased);

            yield return null;
        }

        targetToShift.anchoredPosition = targetPos;
    }
}