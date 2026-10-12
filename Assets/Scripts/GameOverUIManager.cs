using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameOverUIManager : MonoBehaviour
{
    public static GameOverUIManager Instance { get; private set; }

    [Header("UI References")]
    public RectTransform panelRect;
    public Text scoreText;
    public Text bestScoreText;
    public Button homeButton;
    public Button retryButton;

    [Header("Animation Settings")]
    public float animationDuration = 0.6f;
    public Vector2 startOffset = new Vector2(0, 1000f);
    public Vector2 finalPosition = Vector2.zero;
    public float bounceStrength = 50f;

    private bool isInputLocked = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (panelRect != null) 
        {
            panelRect.gameObject.SetActive(false);
            
            // 1. Fix Missing GraphicRaycaster on Canvas
            Canvas canvas = panelRect.GetComponentInParent<Canvas>();
            if (canvas != null && canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }

        // 2. Fix Missing EventSystem
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            UnityEngine.EventSystems.EventSystem existing = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (existing == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }

        if (homeButton != null) homeButton.onClick.AddListener(OnHomeClicked);
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
    }

    public void ShowPanel(int currentScore, int bestScore)
    {
        if (panelRect == null) return;
        
        isInputLocked = true; // Lock until animation finishes

        // Record the final score once for the persistent leaderboard history
        ScoreHistoryManager.RecordScore(currentScore);

        if (scoreText != null) scoreText.text = "SCORE : " + currentScore.ToString();
        if (bestScoreText != null) bestScoreText.text = "BEST : " + bestScore.ToString();

        panelRect.gameObject.SetActive(true);
        StartCoroutine(PopDownAnimation());
    }

    private IEnumerator PopDownAnimation()
    {
        float elapsed = 0f;

        // Start position
        panelRect.anchoredPosition = startOffset;

        // Smooth drop down to a slight overshoot (bounce)
        float dropDuration = animationDuration * 0.7f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / dropDuration;
            float ease = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease out

            Vector2 targetBounce = finalPosition + new Vector2(0, -bounceStrength);
            panelRect.anchoredPosition = Vector2.Lerp(startOffset, targetBounce, ease);
            
            yield return null;
        }

        // Settle bounce
        elapsed = 0f;
        float settleDuration = animationDuration * 0.3f;
        Vector2 currentBounce = panelRect.anchoredPosition;
        while (elapsed < settleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / settleDuration;
            float ease = Mathf.Sin(t * Mathf.PI * 0.5f); // Sine ease out

            panelRect.anchoredPosition = Vector2.Lerp(currentBounce, finalPosition, ease);
            yield return null;
        }

        panelRect.anchoredPosition = finalPosition;
        isInputLocked = false; // Unlock now that it's settled
    }

    private void OnHomeClicked()
    {
        if (isInputLocked) return;
        isInputLocked = true;
        StartCoroutine(ButtonPressAnim(homeButton.transform, () =>
        {
            SceneManager.LoadScene("Splash");
        }));
    }

    private void OnRetryClicked()
    {
        if (isInputLocked) return;
        isInputLocked = true;
        StartCoroutine(ButtonPressAnim(retryButton.transform, () =>
        {
            SceneManager.LoadScene("FruitNinja");
        }));
    }

    private IEnumerator ButtonPressAnim(Transform btnTransform, System.Action onComplete)
    {
        float elapsed = 0f;
        float duration = 0.1f;
        Vector3 originalScale = Vector3.one;
        Vector3 pressScale = Vector3.one * 0.9f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            btnTransform.localScale = Vector3.Lerp(originalScale, pressScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            btnTransform.localScale = Vector3.Lerp(pressScale, originalScale, elapsed / duration);
            yield return null;
        }
        
        btnTransform.localScale = originalScale;
        onComplete?.Invoke();
    }
}
