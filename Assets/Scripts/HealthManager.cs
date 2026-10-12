using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Health")]
    public int maxHealth = 3;
    public int currentHealth = 3;

    [Header("Heart UI")]
    public Image[] heartImages;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    [Header("Heart Animation")]
    public float heartChangeDuration = 0.25f;
    public float heartPunchScale = 1.15f;

    [Header("Game Over")]
    public RectTransform gameOverRect;
    public CanvasGroup gameOverCanvasGroup;
    public float gameOverHoldDuration = 3.0f;
    public float gameOverPopDuration = 0.35f;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (heartImages == null || heartImages.Length != 3 || heartImages[0] == null)
            {
                InitializeDefaultSetup();
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
    }

    private void InitializeDefaultSetup()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGo = new GameObject("HealthCanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
            canvas.sortingOrder = 50; // Above background, below full-screen white
        }

        // Health Panel
        Transform panelTransform = canvas.transform.Find("HealthPanel");
        GameObject panelGo;
        if (panelTransform == null)
        {
            panelGo = new GameObject("HealthPanel");
            panelTransform = panelGo.transform;
            panelTransform.SetParent(canvas.transform, false);
            
            RectTransform panelRt = panelGo.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(1, 1);
            panelRt.anchorMax = new Vector2(1, 1);
            panelRt.pivot = new Vector2(1, 1);
            panelRt.anchoredPosition = new Vector2(-20, -20); // Top Right with padding
            panelRt.sizeDelta = new Vector2(200, 60);

            HorizontalLayoutGroup hlg = panelGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleRight;
            hlg.childControlHeight = true;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.spacing = 10;
        }
        else
        {
            panelGo = panelTransform.gameObject;
        }

        // Hearts
        heartImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            Transform heartTransform = panelTransform.Find($"Heart_{i + 1}");
            GameObject heartGo;
            if (heartTransform == null)
            {
                heartGo = new GameObject($"Heart_{i + 1}");
                heartTransform = heartGo.transform;
                heartTransform.SetParent(panelTransform, false);
                
                Image img = heartGo.AddComponent<Image>();
                img.rectTransform.sizeDelta = new Vector2(50, 50);
            }
            heartImages[i] = heartTransform.GetComponent<Image>();
        }

        // Game Over Text
        Transform gameOverTransform = canvas.transform.Find("GameOverText");
        if (gameOverTransform == null)
        {
            GameObject gameOverGo = new GameObject("GameOverText");
            gameOverTransform = gameOverGo.transform;
            gameOverTransform.SetParent(canvas.transform, false);
            
            gameOverRect = gameOverGo.AddComponent<RectTransform>();
            gameOverRect.anchorMin = new Vector2(0.5f, 0.5f);
            gameOverRect.anchorMax = new Vector2(0.5f, 0.5f);
            gameOverRect.pivot = new Vector2(0.5f, 0.5f);
            gameOverRect.anchoredPosition = Vector2.zero;
            gameOverRect.sizeDelta = new Vector2(800, 200);

            Text txt = gameOverGo.AddComponent<Text>();
            txt.text = "GAME OVER";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.red;
            txt.fontSize = 120;
            txt.fontStyle = FontStyle.Bold;

            gameOverCanvasGroup = gameOverGo.AddComponent<CanvasGroup>();
            gameOverCanvasGroup.alpha = 0f;
            gameOverGo.SetActive(false);
        }
        else
        {
            gameOverRect = gameOverTransform.GetComponent<RectTransform>();
            gameOverCanvasGroup = gameOverTransform.GetComponent<CanvasGroup>();
        }
    }
#endif

    private void Start()
    {
        if (Application.isPlaying)
        {
            if (gameOverCanvasGroup != null)
            {
                gameOverCanvasGroup.alpha = 0f;
                gameOverCanvasGroup.gameObject.SetActive(false);
            }
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].sprite = fullHeartSprite;
                heartImages[i].transform.localScale = Vector3.one;
            }
        }
    }

    // Returns true if game over, false if still alive
    public bool LoseHeart()
    {
        if (currentHealth <= 0) return true; // Already dead

        currentHealth--;

        // Animate the lost heart (left to right)
        // If currentHealth was 3, it becomes 2. So the 3rd heart (index 2) is lost.
        // Wait, the user said: "Heart 1 should become EMPTY. Result: ♡ ♥ ♥ or whichever left-to-right depletion order matches."
        // Let's do left to right depletion: index = maxHealth - currentHealth - 1
        int heartIndex = maxHealth - currentHealth - 1;
        if (heartIndex >= 0 && heartIndex < heartImages.Length && heartImages[heartIndex] != null)
        {
            StartCoroutine(AnimateHeartLoss(heartImages[heartIndex]));
        }

        return currentHealth <= 0;
    }

    private IEnumerator AnimateHeartLoss(Image heartImg)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 punchScale = Vector3.one * heartPunchScale;
        
        float halfDuration = heartChangeDuration / 2f;
        float elapsed = 0f;

        // Punch out
        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            heartImg.transform.localScale = Vector3.Lerp(originalScale, punchScale, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        heartImg.sprite = emptyHeartSprite;

        // Punch in
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            heartImg.transform.localScale = Vector3.Lerp(punchScale, originalScale, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        heartImg.transform.localScale = originalScale;
    }

    public IEnumerator PlayGameOverSequence(System.Action onComplete)
    {
        if (gameOverRect == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        if (gameOverCanvasGroup == null)
        {
            gameOverCanvasGroup = gameOverRect.gameObject.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup == null)
            {
                gameOverCanvasGroup = gameOverRect.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // Ensure Canvas and parents are active
        Canvas parentCanvas = gameOverRect.GetComponentInParent<Canvas>(true);
        if (parentCanvas != null && !parentCanvas.gameObject.activeInHierarchy)
        {
            parentCanvas.gameObject.SetActive(true);
        }

        gameOverRect.gameObject.SetActive(true);
        
        gameOverCanvasGroup.alpha = 1f;
        gameOverCanvasGroup.interactable = false;
        gameOverCanvasGroup.blocksRaycasts = false;
        
        gameOverRect.localScale = Vector3.one;
        gameOverRect.anchoredPosition = Vector2.zero; // Reset to center
        gameOverRect.anchorMin = new Vector2(0.5f, 0.5f);
        gameOverRect.anchorMax = new Vector2(0.5f, 0.5f);
        gameOverRect.pivot = new Vector2(0.5f, 0.5f);

        // Hold
        yield return new WaitForSecondsRealtime(gameOverHoldDuration);

        // Pop back / scale down
        float elapsed = 0f;
        Vector3 initialScale = Vector3.one;
        Vector3 punchScale = Vector3.one * 1.04f; // Optional tiny punch requested
        
        // Subtle punch up
        float punchUpDuration = 0.05f;
        while (elapsed < punchUpDuration)
        {
            float t = elapsed / punchUpDuration;
            gameOverRect.localScale = Vector3.Lerp(initialScale, punchScale, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Scale down to 0
        elapsed = 0f;
        float scaleDownDuration = gameOverPopDuration - punchUpDuration;
        if (scaleDownDuration <= 0f) scaleDownDuration = 0.01f;

        while (elapsed < scaleDownDuration)
        {
            float t = elapsed / scaleDownDuration;
            float easeInT = t * t; // Accelerating shrink
            
            gameOverRect.localScale = Vector3.Lerp(punchScale, Vector3.zero, easeInT);
            
            if (t > 0.8f)
            {
                gameOverCanvasGroup.alpha = 1f - ((t - 0.8f) * 5f);
            }
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        gameOverRect.localScale = Vector3.one; // Reset scale for next use
        gameOverCanvasGroup.alpha = 1f; // Reset alpha for next use
        gameOverRect.gameObject.SetActive(false);

        onComplete?.Invoke();
    }
}
