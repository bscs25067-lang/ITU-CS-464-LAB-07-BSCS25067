using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelSwitcher : MonoBehaviour
{
    [Header("Persistent Background")]
    [SerializeField] private GameObject backgroundCanvas;
    [SerializeField] private Graphic backgroundGraphic; // The Image component of the background

    [Header("Menu Panels & Background Settings")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private bool mainMenuRequiresBlackBg = false;

    [SerializeField] private GameObject storyMenuPanel;
    [SerializeField] private bool storyMenuRequiresBlackBg = false;

    [SerializeField] private GameObject optionsMenuPanel;
    [SerializeField] private bool optionsMenuRequiresBlackBg = true;  // Fade to black

    [SerializeField] private GameObject scenarioMenuPanel;
    [SerializeField] private bool scenarioMenuRequiresBlackBg = false;

    [SerializeField] private GameObject bonusesMenuPanel;
    [SerializeField] private bool bonusesMenuRequiresBlackBg = true; // Fade to black

    [SerializeField] private GameObject costumesMenuPanel;
    [SerializeField] private bool costumesMenuRequiresBlackBg = true; // Fade to black

    [SerializeField] private GameObject conceptArtMenuPanel;
    [SerializeField] private bool conceptArtMenuRequiresBlackBg = true; // Fade to black
    [SerializeField] private GameObject recordsMenuPanel;
    [SerializeField] private bool recordsMenuRequiresBlackBg = true; // Fade to black
    [SerializeField] private GameObject difficultyMenuPanel;
    [SerializeField] private bool difficultyMenuRequiresBlackBg = true; // Fade to black

    private Stack<(GameObject panel, bool needsBlackBg)> panelHistory = new Stack<(GameObject, bool)>();
    private GameObject currentActivePanel;
    private bool currentNeedsBlackBg = false;
    private bool isTransitioning = false;

    private void Start()
    {
        if (backgroundCanvas != null) backgroundCanvas.SetActive(true);
        if (backgroundGraphic == null && backgroundCanvas != null)
        {
            backgroundGraphic = backgroundCanvas.GetComponentInChildren<Graphic>();
        }

        HideAllPanelsInstant();
        if (mainMenuPanel != null)
        {
            ShowPanelInstant(mainMenuPanel, mainMenuRequiresBlackBg, clearHistory: true);
        }
    }

    public void OpenPanelWithOptions(GameObject targetPanel, bool requiresBlackBg)
    {
        if (targetPanel == null || targetPanel == currentActivePanel || isTransitioning) return;
        StartCoroutine(TransitionRoutine(targetPanel, requiresBlackBg, pushToHistory: true));
    }

    // Helper wrappers for standard Unity Inspector Button OnClick() calls
    public void OpenMainMenu() => OpenPanelWithOptions(mainMenuPanel, mainMenuRequiresBlackBg);
    public void OpenStoryMenu() => OpenPanelWithOptions(storyMenuPanel, storyMenuRequiresBlackBg);
    public void OpenOptions() => OpenPanelWithOptions(optionsMenuPanel, optionsMenuRequiresBlackBg);
    public void OpenScenarioMenu() => OpenPanelWithOptions(scenarioMenuPanel, scenarioMenuRequiresBlackBg);
    public void OpenCostumesMenu() => OpenPanelWithOptions(costumesMenuPanel, costumesMenuRequiresBlackBg);
    public void OpenBonusesMenu() => OpenPanelWithOptions(bonusesMenuPanel, bonusesMenuRequiresBlackBg);
    public void OpenConceptArtMenu() => OpenPanelWithOptions(conceptArtMenuPanel, conceptArtMenuRequiresBlackBg);
    public void OpenRecordsMenu() => OpenPanelWithOptions(recordsMenuPanel, recordsMenuRequiresBlackBg);
    public void OpenDifficultyMenu() => OpenPanelWithOptions(difficultyMenuPanel, difficultyMenuRequiresBlackBg);

    public void GoBack()
    {
        if (panelHistory.Count > 0 && !isTransitioning)
        {
            var previous = panelHistory.Pop();
            StartCoroutine(TransitionRoutine(previous.panel, previous.needsBlackBg, pushToHistory: false));
        }
    }

    private IEnumerator TransitionRoutine(GameObject targetPanel, bool targetNeedsBlackBg, bool pushToHistory)
    {
        isTransitioning = true;
        GameObject outgoingPanel = currentActivePanel;

        // 1. Fade OUT outgoing panel
        if (outgoingPanel != null)
        {
            UITweenEffects outTween = outgoingPanel.GetComponent<UITweenEffects>();
            if (outTween != null)
            {
                outTween.FadeOut();
                yield return new WaitForSecondsRealtime(0.6f);
            }
            outgoingPanel.SetActive(false);
        }

        // 2. Manage History Stack
        if (pushToHistory && outgoingPanel != null)
        {
            panelHistory.Push((outgoingPanel, currentNeedsBlackBg));
        }

        // 3. Smoothly transition background color if the requirement changes
        if (backgroundGraphic != null && currentNeedsBlackBg != targetNeedsBlackBg)
        {
            Color startColor = backgroundGraphic.color;
            Color endColor = targetNeedsBlackBg ? Color.black : Color.white;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
                backgroundGraphic.color = Color.Lerp(startColor, endColor, eased);
                yield return null;
            }
            backgroundGraphic.color = endColor;
        }

        currentNeedsBlackBg = targetNeedsBlackBg;

        // 4. Activate and Fade IN incoming target panel
        currentActivePanel = targetPanel;
        currentActivePanel.SetActive(true);

        UITweenEffects inTween = currentActivePanel.GetComponent<UITweenEffects>();
        if (inTween != null)
        {
            inTween.FadeIn();
            yield return new WaitForSecondsRealtime(0.6f);
        }

        isTransitioning = false;
    }

    private void ShowPanelInstant(GameObject panel, bool needsBlackBg, bool clearHistory)
    {
        HideAllPanelsInstant();
        if (clearHistory) panelHistory.Clear();

        currentNeedsBlackBg = needsBlackBg;
        if (backgroundGraphic != null)
        {
            backgroundGraphic.color = needsBlackBg ? Color.black : Color.white;
        }

        currentActivePanel = panel;
        currentActivePanel.SetActive(true);

        UITweenEffects tween = currentActivePanel.GetComponent<UITweenEffects>();
        if (tween != null) tween.FadeIn();
    }

    private void HideAllPanelsInstant()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (storyMenuPanel) storyMenuPanel.SetActive(false);
        if (optionsMenuPanel) optionsMenuPanel.SetActive(false);
        if (scenarioMenuPanel) scenarioMenuPanel.SetActive(false);
        if (costumesMenuPanel) costumesMenuPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}