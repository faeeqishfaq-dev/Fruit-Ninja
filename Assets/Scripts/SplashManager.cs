
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    [Header("UI Panels")]
    public CanvasGroup loadingPanel;
    public CanvasGroup mainMenuPanel;
    public UnityEngine.UI.Image loadingBarFill;

    [Header("Settings")]
    public float loadingDuration = 5f;
    public float fadeDuration = 0.5f;

    [Header("Menu Objects")]
    public GameObject menuFruitsRoot;
    public GameObject bladePrefab;
    private GameObject activeBlade;

    [Header("Leaderboard")]
    public LeaderboardPopup leaderboardPopup;

    private bool isLoadingGame = false;
    private bool adFinished = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (Camera.main != null)
        {
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 5f;
        }

        isLoadingGame = false;

        if (loadingPanel != null)
        {
            loadingPanel.gameObject.SetActive(true);
            loadingPanel.alpha = 1;
            loadingPanel.interactable = true;
            loadingPanel.blocksRaycasts = true;
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.gameObject.SetActive(false);
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
            loadingBarFill.fillAmount = 0f;

        float elapsed = 0f;

        while (elapsed < loadingDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(
                elapsed / loadingDuration
            );

            if (loadingBarFill != null)
                loadingBarFill.fillAmount = progress;

            yield return null;
        }

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 1f;

        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;

            if (loadingPanel != null)
            {
                loadingPanel.alpha = Mathf.Lerp(
                    1f, 0f,
                    Mathf.Clamp01(t / fadeDuration)
                );
            }

            yield return null;
        }

        if (loadingPanel != null)
        {
            loadingPanel.alpha = 0;
            loadingPanel.interactable = false;
            loadingPanel.blocksRaycasts = false;
            loadingPanel.gameObject.SetActive(false);
        }

        if (menuFruitsRoot != null)
        {
            menuFruitsRoot.transform.position =
                new Vector3(0, 0, 2f);

            menuFruitsRoot.SetActive(true);
        }

        if (mainMenuPanel != null)
            mainMenuPanel.gameObject.SetActive(true);

        if (bladePrefab != null)
        {
            activeBlade = Instantiate(bladePrefab);

            Rigidbody rb =
                activeBlade.AddComponent<Rigidbody>();

            rb.isKinematic = true;

            Collider oldCol =
                activeBlade.GetComponent<Collider>();

            if (oldCol != null)
                DestroyImmediate(oldCol);

            BoxCollider boxCol =
                activeBlade.AddComponent<BoxCollider>();

            boxCol.isTrigger = true;
            boxCol.size = new Vector3(
                0.5f, 0.5f, 10f
            );

            Blade bladeComp =
                activeBlade.GetComponent<Blade>();

            if (bladeComp != null)
            {
                var field = typeof(Blade).GetField(
                    "sliceCollider",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance
                );

                if (field != null)
                    field.SetValue(bladeComp, boxCol);
            }

            TrailRenderer tr =
                activeBlade.GetComponentInChildren<TrailRenderer>();

            if (tr != null)
                tr.sortingOrder = 100;
        }

        t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;

            if (mainMenuPanel != null)
            {
                mainMenuPanel.alpha = Mathf.Lerp(
                    0f, 1f,
                    Mathf.Clamp01(t / fadeDuration)
                );
            }

            yield return null;
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.alpha = 1f;
            mainMenuPanel.interactable = true;
            mainMenuPanel.blocksRaycasts = true;
        }
    }

    public void OnPlayClicked()
    {
        CheckCpAd.instance.CallAd();
        if (isLoadingGame)
            return;

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
            leaderboardPopup.Show();
    }

    public void SetBladeActive(bool active)
    {
        if (activeBlade != null)
            activeBlade.SetActive(active);
    }

    // Must be called by the ad system when the ad
    // closes, finishes, fails, or is unavailable.
    public void OnAdFinished()
    {
        adFinished = true;
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Preserve existing fruit slicing delay.
        yield return new WaitForSeconds(0.8f);

        if (mainMenuPanel != null)
        {
            mainMenuPanel.interactable = false;
            mainMenuPanel.blocksRaycasts = false;
        }

        if (menuFruitsRoot != null)
            menuFruitsRoot.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.gameObject.SetActive(false);

        if (activeBlade != null)
            Destroy(activeBlade);

        // Show the existing loading panel again.
        if (loadingPanel != null)
        {
            loadingPanel.gameObject.SetActive(true);
            loadingPanel.alpha = 1f;
            loadingPanel.interactable = true;
            loadingPanel.blocksRaycasts = true;
        }

        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 0f;

        AsyncOperation asyncLoad =
            SceneManager.LoadSceneAsync(sceneName);

        if (asyncLoad == null)
        {
            Debug.LogError(
                "Failed to start loading: " + sceneName
            );
            isLoadingGame = false;
            yield break;
        }

        // Prevent scene activation before the ad finishes.
        asyncLoad.allowSceneActivation = false;

        float elapsed = 0f;

        while (
            elapsed < loadingDuration ||
            asyncLoad.progress < 0.9f
        )
        {
            elapsed += Time.unscaledDeltaTime;

            float presentationProgress = Mathf.Clamp01(
                elapsed / loadingDuration
            );

            float actualProgress = Mathf.Clamp01(
                asyncLoad.progress / 0.9f
            );

            float displayProgress = Mathf.Min(
                presentationProgress,
                actualProgress
            );

            if (loadingBarFill != null)
                loadingBarFill.fillAmount = displayProgress;

            yield return null;
        }

        // Loading has fully completed.
        if (loadingBarFill != null)
            loadingBarFill.fillAmount = 1f;

        Debug.Log("Loading Bar 100% - Calling Ad");

        // Ad must start ONLY after loading bar reaches 100%.
        adFinished = false;

        if (CheckCpAd.instance != null)
        {
            GameObject adObj = CheckCpAd.instance.cpVideoAd;

            CheckCpAd.instance.CallAd();

            // Allow one frame for ad GameObject activation to take effect
            yield return null;

            // If ad is active, wait until the user closes it (adObj becomes inactive or OnAdFinished called)
            if (adObj != null && adObj.activeInHierarchy)
            {
                Debug.Log("Ad is playing - waiting for user to close ad");
                while (!adFinished && adObj != null && adObj.activeInHierarchy)
                {
                    yield return null;
                }
                Debug.Log("Ad closed by user");
            }
            else
            {
                Debug.LogWarning("Ad not ready or unavailable. Continuing to FruitNinja.");
            }
        }
        else
        {
            Debug.LogWarning("CheckCpAd instance missing. Skipping ad.");
        }

        adFinished = true;

        Debug.Log("Ad flow completed - Activating FruitNinja");

        Time.timeScale = 1f;
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;
    }
}
