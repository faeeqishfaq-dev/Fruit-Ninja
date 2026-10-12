using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardPopup : MonoBehaviour
{
    public static LeaderboardPopup Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private CanvasGroup popupCanvasGroup;
    [SerializeField] private Image darkOverlay;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject scoreRowPrefab;
    [SerializeField] private Text emptyStateText;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Animation Settings")]
    [Tooltip("Pop-up animation duration in seconds")]
    [SerializeField] private float animationDuration = 0.35f;
    [Tooltip("Close animation duration in seconds")]
    [SerializeField] private float closeDuration = 0.25f;
    [Tooltip("Target alpha of dark background overlay")]
    [SerializeField] private float targetOverlayAlpha = 0.65f;

    [Header("Integration")]
    [SerializeField] private SplashManager splashManager;

    [Header("Menu Fruit Target")]
    [SerializeField] private GameObject watermelonTarget;

    private List<Renderer> disabledRenderers = new List<Renderer>();
    private Collider watermelonCollider;
    private bool wasColliderEnabled = false;

    private Coroutine activeAnimation;
    private bool isOpen = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        if (splashManager == null)
        {
            splashManager = FindObjectOfType<SplashManager>();
        }

        // Ensure LeaderboardPopup has its own Canvas with Override Sorting
        // to render strictly above all menu UI, WorldSpace Ring, and 3D Watermelon
        Canvas popupCanvas = GetComponent<Canvas>();
        if (popupCanvas == null)
        {
            popupCanvas = gameObject.AddComponent<Canvas>();
        }
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 500;

        // Ensure nested canvas has its own GraphicRaycaster for mouse & touch interaction
        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Initially ensure popup is hidden
        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;
            popupCanvasGroup.interactable = false;
            popupCanvasGroup.blocksRaycasts = false;
        }

        if (darkOverlay != null)
        {
            Color c = darkOverlay.color;
            c.a = 0f;
            darkOverlay.color = c;
            darkOverlay.raycastTarget = false;
        }
    }

    /// <summary>
    /// Opens and animates the leaderboard popup at screen center.
    /// </summary>
    public void Show()
    {
        if (isOpen) return; // Prevent duplicate popup instances
        isOpen = true;

        gameObject.SetActive(true);

        // Block menu fruit slicing while popup is open
        if (splashManager != null)
        {
            splashManager.SetBladeActive(false);
        }
        else
        {
            Blade blade = FindObjectOfType<Blade>();
            if (blade != null) blade.enabled = false;
        }

        // Immediately hide Watermelon visual completely when Leaderboard opens
        SetWatermelonVisible(false);

        // Populate scores from persistent history
        PopulateLeaderboard();

        if (activeAnimation != null) StopCoroutine(activeAnimation);
        activeAnimation = StartCoroutine(AnimateOpen());
    }

    /// <summary>
    /// Closes and animates out the leaderboard popup, returning to main menu.
    /// </summary>
    public void Hide()
    {
        if (!isOpen) return;
        isOpen = false;

        if (activeAnimation != null) StopCoroutine(activeAnimation);
        activeAnimation = StartCoroutine(AnimateClose());
    }

    private void PopulateLeaderboard()
    {
        if (contentParent == null || scoreRowPrefab == null) return;

        // Clean out previous generated rows
        foreach (Transform child in contentParent)
        {
            // Do not destroy the template prefab if it's placed in content as inactive
            if (child.gameObject == scoreRowPrefab) continue;
            Destroy(child.gameObject);
        }

        List<int> sortedScores = ScoreHistoryManager.GetSortedScores();

        if (sortedScores == null || sortedScores.Count == 0)
        {
            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(true);
                emptyStateText.text = "NO SCORES RECORDED YET!\nSLICE WATERMELON TO PLAY & SET A RECORD!";
            }
        }
        else
        {
            if (emptyStateText != null)
            {
                emptyStateText.gameObject.SetActive(false);
            }

            for (int i = 0; i < sortedScores.Count; i++)
            {
                int rank = i + 1;
                int score = sortedScores[i];

                GameObject rowObj = Instantiate(scoreRowPrefab, contentParent);
                rowObj.SetActive(true);

                ScoreRowUI rowUI = rowObj.GetComponent<ScoreRowUI>();
                if (rowUI != null)
                {
                    rowUI.Setup(rank, score);
                }
            }
        }

        // Scroll back to top
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private IEnumerator AnimateOpen()
    {
        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.blocksRaycasts = true;
            popupCanvasGroup.interactable = true;
        }

        if (darkOverlay != null)
        {
            darkOverlay.raycastTarget = true;
        }

        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 overshootScale = Vector3.one * 1.05f;
        Vector3 endScale = Vector3.one;

        // Phase 1: 0.5 -> 1.05 (fast pop up, ~70% duration)
        float popDuration = animationDuration * 0.7f;
        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            float ease = Mathf.Sin(t * Mathf.PI * 0.5f); // Smooth ease out

            if (panelRect != null)
            {
                panelRect.localScale = Vector3.Lerp(startScale, overshootScale, ease);
            }

            float overallT = Mathf.Clamp01(elapsed / animationDuration);
            if (popupCanvasGroup != null) popupCanvasGroup.alpha = overallT;
            if (darkOverlay != null)
            {
                Color c = darkOverlay.color;
                c.a = Mathf.Lerp(0f, targetOverlayAlpha, overallT);
                darkOverlay.color = c;
            }

            yield return null;
        }

        // Phase 2: 1.05 -> 1.0 (smooth settle, ~30% duration)
        elapsed = 0f;
        float settleDuration = animationDuration * 0.3f;
        while (elapsed < settleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / settleDuration);
            float ease = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            if (panelRect != null)
            {
                panelRect.localScale = Vector3.Lerp(overshootScale, endScale, ease);
            }

            float overallT = Mathf.Clamp01((popDuration + elapsed) / animationDuration);
            if (popupCanvasGroup != null) popupCanvasGroup.alpha = overallT;
            if (darkOverlay != null)
            {
                Color c = darkOverlay.color;
                c.a = Mathf.Lerp(0f, targetOverlayAlpha, overallT);
                darkOverlay.color = c;
            }

            yield return null;
        }

        if (panelRect != null) panelRect.localScale = endScale;
        if (popupCanvasGroup != null) popupCanvasGroup.alpha = 1f;
        if (darkOverlay != null)
        {
            Color c = darkOverlay.color;
            c.a = targetOverlayAlpha;
            darkOverlay.color = c;
        }

        activeAnimation = null;
    }

    private IEnumerator AnimateClose()
    {
        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.interactable = false;
        }

        float elapsed = 0f;
        Vector3 startScale = panelRect != null ? panelRect.localScale : Vector3.one;
        Vector3 endScale = Vector3.zero;

        float startOverlayAlpha = darkOverlay != null ? darkOverlay.color.a : targetOverlayAlpha;

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);
            float ease = t * t; // Smooth accelerating shrink

            if (panelRect != null)
            {
                panelRect.localScale = Vector3.Lerp(startScale, endScale, ease);
            }

            if (popupCanvasGroup != null)
            {
                popupCanvasGroup.alpha = 1f - t;
            }

            if (darkOverlay != null)
            {
                Color c = darkOverlay.color;
                c.a = Mathf.Lerp(startOverlayAlpha, 0f, t);
                darkOverlay.color = c;
            }

            yield return null;
        }

        if (panelRect != null) panelRect.localScale = Vector3.zero;
        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;
            popupCanvasGroup.blocksRaycasts = false;
        }
        if (darkOverlay != null)
        {
            Color c = darkOverlay.color;
            c.a = 0f;
            darkOverlay.color = c;
            darkOverlay.raycastTarget = false;
        }

        gameObject.SetActive(false);

        // Restore Watermelon visual when Leaderboard closes
        SetWatermelonVisible(true);

        // Restore menu interactions & blade
        if (splashManager != null)
        {
            splashManager.SetBladeActive(true);
        }
        else
        {
            Blade blade = FindObjectOfType<Blade>();
            if (blade != null) blade.enabled = true;
        }

        activeAnimation = null;
    }

    private void SetWatermelonVisible(bool visible)
    {
        if (watermelonTarget == null)
        {
            // 1. Try to find the Watermelon MenuFruit explicitly
            MenuFruit[] allFruits = FindObjectsOfType<MenuFruit>(true);
            foreach (MenuFruit fruit in allFruits)
            {
                if (fruit != null && fruit.gameObject.name.Contains("Watermelon"))
                {
                    watermelonTarget = fruit.gameObject;
                    break;
                }
            }

            // 2. Search root GameObjects for MenuFruitsRoot / CenterMode / Watermelon
            if (watermelonTarget == null)
            {
                foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    Transform wm = root.transform.Find("CenterMode/Watermelon(Clone)") 
                                ?? root.transform.Find("MenuFruitsRoot/CenterMode/Watermelon(Clone)")
                                ?? root.transform.Find("Watermelon(Clone)")
                                ?? root.transform.Find("CenterMode/Watermelon");
                    if (wm != null)
                    {
                        watermelonTarget = wm.gameObject;
                        break;
                    }
                }
            }
        }

        if (watermelonTarget == null) return;

        if (!visible)
        {
            // Temporarily disable only the relevant Watermelon renderers/visuals while Leaderboard is open
            disabledRenderers.Clear();
            Renderer[] renderers = watermelonTarget.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r != null && r.enabled)
                {
                    disabledRenderers.Add(r);
                    r.enabled = false;
                }
            }

            // Temporarily disable collider to prevent any slice trigger while popup is open
            watermelonCollider = watermelonTarget.GetComponent<Collider>();
            if (watermelonCollider != null)
            {
                wasColliderEnabled = watermelonCollider.enabled;
                watermelonCollider.enabled = false;
            }
        }
        else
        {
            // Restore original renderer states when the Leaderboard closes
            foreach (Renderer r in disabledRenderers)
            {
                if (r != null)
                {
                    r.enabled = true;
                }
            }
            disabledRenderers.Clear();

            // Restore collider
            if (watermelonCollider != null && wasColliderEnabled)
            {
                watermelonCollider.enabled = true;
            }
        }
    }
}
