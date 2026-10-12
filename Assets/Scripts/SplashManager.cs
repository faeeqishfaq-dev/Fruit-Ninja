using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    [Header("UI Panels")]
    public CanvasGroup loadingPanel;
    public CanvasGroup mainMenuPanel;
    public UnityEngine.UI.Image loadingBarFill; // Exposed as requested

    [Header("Settings")]
    public float loadingDuration = 5f; // Set default to 5
    public float fadeDuration = 0.5f;

    [Header("Menu Objects")]
    public GameObject menuFruitsRoot;
    public GameObject bladePrefab;
    private GameObject activeBlade;

    [Header("Leaderboard")]
    public LeaderboardPopup leaderboardPopup;

    private void Start()
    {
        // Fix for TimeScale being stuck at 0 from GameOver scene
        Time.timeScale = 1f;

        // Fix camera for Blade ScreenToWorldPoint logic
        if (Camera.main != null)
        {
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5f;
        }
        
        isLoadingGame = false; // Reset transition flag

        // Initial state
        if (loadingPanel != null)
        {
            loadingPanel.gameObject.SetActive(true);
            loadingPanel.alpha = 1;
            loadingPanel.interactable = true;
            loadingPanel.blocksRaycasts = true;
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.gameObject.SetActive(false); // Also ensure it's hidden initially
            mainMenuPanel.alpha = 0;
            mainMenuPanel.interactable = false;
            mainMenuPanel.blocksRaycasts = false;
        }
        
        if (menuFruitsRoot != null)
            menuFruitsRoot.SetActive(false);

        StartCoroutine(LoadingFlow());
    }

    private IEnumerator LoadingFlow()
    {
        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0f;
        }

        // Animate fake loading bar
        float elapsed = 0f;
        while (elapsed < loadingDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / loadingDuration);
            if (loadingBarFill != null)
            {
                loadingBarFill.fillAmount = progress;
            }
            yield return null;
        }

        // Fade out loading panel
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            loadingPanel.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
        loadingPanel.alpha = 0;
        loadingPanel.interactable = false;
        loadingPanel.blocksRaycasts = false;
        loadingPanel.gameObject.SetActive(false);

        // Activate fruits and blade before fading in menu
        if (menuFruitsRoot != null)
        {
            menuFruitsRoot.transform.position = new Vector3(0, 0, 2f); // Push fruits back so Blade (Z=0) renders on top
            menuFruitsRoot.SetActive(true);
        }
            
        if (mainMenuPanel != null)
            mainMenuPanel.gameObject.SetActive(true);
            
        if (bladePrefab != null)
        {
            activeBlade = Instantiate(bladePrefab);
            
            Rigidbody rb = activeBlade.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            
            Collider oldCol = activeBlade.GetComponent<Collider>();
            if (oldCol != null) DestroyImmediate(oldCol);
            
            BoxCollider boxCol = activeBlade.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
            boxCol.size = new Vector3(0.5f, 0.5f, 10f); // Thick in Z to hit pushed-back fruits, thin in X/Y
            
            // Seamlessly inject the new BoxCollider into the Blade script's private field
            Blade bladeComp = activeBlade.GetComponent<Blade>();
            if (bladeComp != null)
            {
                var field = typeof(Blade).GetField("sliceCollider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null) field.SetValue(bladeComp, boxCol);
            }

            TrailRenderer tr = activeBlade.GetComponentInChildren<TrailRenderer>();
            if (tr != null)
            {
                tr.sortingOrder = 100; // Ensure trail renders above menu UI
            }
        }

        // Fade in main menu
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            mainMenuPanel.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        mainMenuPanel.alpha = 1;
        mainMenuPanel.interactable = true;
        mainMenuPanel.blocksRaycasts = true;
    }

    // Actions assigned to fruits
    private bool isLoadingGame = false;

    public void OnPlayClicked()
    {
        if (isLoadingGame) return;
        isLoadingGame = true;
        Debug.Log("Play Mode Selected! Loading Game...");
        StartCoroutine(LoadSceneAsync("FruitNinja"));
    }

    public void OnMultiplayerClicked()
    {
        Debug.Log("Multiplayer Mode Selected!");
    }

    public void OnWatchClicked()
    {
        Debug.Log("Watch Mode Selected!");
    }

    public void OnProfileClicked()
    {
        if (leaderboardPopup != null)
        {
            leaderboardPopup.Show();
        }
    }

    public void SetBladeActive(bool active)
    {
        if (activeBlade != null)
        {
            activeBlade.SetActive(active);
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 1. Wait for fruit slicing animation to play out visually
        yield return new WaitForSeconds(0.8f);

        // 2. Disable further menu selections
        if (mainMenuPanel != null)
        {
            mainMenuPanel.interactable = false;
            mainMenuPanel.blocksRaycasts = false;
        }

        // 3. Hide MainMenuRoot + MenuFruitsRoot
        if (menuFruitsRoot != null) menuFruitsRoot.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.gameObject.SetActive(false);
        if (activeBlade != null) Destroy(activeBlade);

        // 4. Show existing Splash LoadingPanel again
        if (loadingPanel != null)
        {
            loadingPanel.gameObject.SetActive(true);
            loadingPanel.alpha = 1;
        }

        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 0f;
        }

        // 5. Load FruitNinja scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // 6. Display loading animation over loadingDuration
        float elapsed = 0f;
        while (elapsed < loadingDuration || asyncLoad.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;

            // We calculate presentation progress based on time
            float presentationProgress = Mathf.Clamp01(elapsed / loadingDuration);
            
            // The actual progress shown is the lesser of the presentation progress 
            // and the actual scene load progress (mapped to 0-1)
            // But since scene load is often instantly 0.9, we mostly rely on presentation.
            float actualProgress = asyncLoad.progress / 0.9f;
            float displayProgress = Mathf.Min(presentationProgress, actualProgress);

            if (loadingBarFill != null)
            {
                loadingBarFill.fillAmount = displayProgress;
            }

            yield return null;
        }

        if (loadingBarFill != null)
        {
            loadingBarFill.fillAmount = 1f;
        }
        
        // 7. Activate FruitNinja gameplay scene
        asyncLoad.allowSceneActivation = true;
    }
}
