using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private Blade blade;
    [SerializeField] private Spawner spawner;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bestScoreText;
    [SerializeField] private Image fadeImage;

    public int Score { get; private set; } = 0;
    private int bestScore = 0;

    private void Awake()
    {
        HandleSingleton();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        // Load best score (handle old float key as fallback if it exists)
        bestScore = PlayerPrefs.GetInt("BestScore", (int)PlayerPrefs.GetFloat("hiscore", 0));
        UpdateBestScoreUI();

        NewGame();

        if (CountdownManager.Instance != null)
        {
            blade.enabled = false;
            spawner.enabled = false;

            CountdownManager.Instance.StartCountdown(() => 
            {
                blade.enabled = true;
                spawner.enabled = true;
            });
        }
    }

    private void HandleSingleton()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void NewGame()
    {
        Time.timeScale = 1f;

        ScoreHistoryManager.ResetGameSession();

        ClearScene();

        blade.enabled = true;
        spawner.enabled = true;

        Score = 0;
        scoreText.text = Score.ToString();
        UpdateBestScoreUI();

        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.ResetHealth();
        }
    }

    private void UpdateBestScoreUI()
    {
        if (bestScoreText != null)
        {
            bestScoreText.text = $"BEST: {bestScore}";
        }
    }

    private void ResumeGameAfterNonFatalBomb()
    {
        Time.timeScale = 1f;
        blade.enabled = true;
        spawner.enabled = true;
    }

    private void ClearScene()
    {
        ClearFruits();
        ClearBombsOnly();
        ClearSplatters();
    }

    private void ClearGameplayForGameOver()
    {
        spawner.enabled = false;
        blade.enabled = false;
        
        ClearFruits();
        ClearBombsOnly();
        ClearSplatters();
    }

    private void ClearFruits()
    {
        Fruit[] fruits = FindObjectsOfType<Fruit>();
        foreach (Fruit fruit in fruits)
        {
            Destroy(fruit.gameObject);
        }
    }

    private void ClearBombsOnly()
    {
        Bomb[] bombs = FindObjectsOfType<Bomb>();
        foreach (Bomb bomb in bombs)
        {
            Destroy(bomb.gameObject);
        }
    }

    private void ClearSplatters()
    {
        JuiceSplatter[] splatters = FindObjectsOfType<JuiceSplatter>();
        foreach (JuiceSplatter splatter in splatters)
        {
            Destroy(splatter.gameObject);
        }
    }

    public void IncreaseScore(int points)
    {
        Score += points;
        scoreText.text = Score.ToString();

        if (Score > bestScore)
        {
            bestScore = Score;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.SetFloat("hiscore", bestScore); // Keep legacy updated just in case
            PlayerPrefs.Save();
            
            UpdateBestScoreUI();
        }
    }

    public void Explode(Vector3 bombPosition)
    {
        blade.enabled = false;
        spawner.enabled = false;

        StartCoroutine(ExplodeSequence(bombPosition));
    }

    public void Explode()
    {
        Explode(Vector3.zero);
    }

    private IEnumerator ExplodeSequence(Vector3 bombPosition)
    {
        float elapsed = 0f;
        float duration = 0.5f;
        bool isGameOver = false;

        System.Action onWhiteScreen = () =>
        {
            if (HealthManager.Instance != null)
            {
                isGameOver = HealthManager.Instance.LoseHeart();
            }
            else
            {
                isGameOver = true; // Fallback if no health manager
            }

            if (isGameOver)
            {
                ClearGameplayForGameOver();
            }
            else
            {
                ClearBombsOnly();
            }
        };

        if (BombExplosionVFXManager.Instance != null && bombPosition != Vector3.zero)
        {
            Time.timeScale = 0f; // Instantly freeze gameplay

            yield return BombExplosionVFXManager.Instance.PlayVFX(bombPosition, onWhiteScreen);

            if (isGameOver && HealthManager.Instance != null)
            {
                yield return HealthManager.Instance.PlayGameOverSequence(() =>
                {
                    if (GameOverUIManager.Instance != null)
                    {
                        GameOverUIManager.Instance.ShowPanel(Score, bestScore);
                    }
                    else
                    {
                        NewGame(); // Fallback if no panel
                    }
                });
            }
            else
            {
                ResumeGameAfterNonFatalBomb();
            }

            fadeImage.color = Color.clear;
            yield break;
        }
        else
        {
            // Fallback: Original Fade to white
            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                fadeImage.color = Color.Lerp(Color.clear, Color.white, t);

                Time.timeScale = 1f - t;
                elapsed += Time.unscaledDeltaTime;

                yield return null;
            }

            onWhiteScreen();
        }

        yield return new WaitForSecondsRealtime(1f);

        if (isGameOver && HealthManager.Instance != null)
        {
            yield return HealthManager.Instance.PlayGameOverSequence(() =>
            {
                if (GameOverUIManager.Instance != null)
                {
                    GameOverUIManager.Instance.ShowPanel(Score, bestScore);
                }
                else
                {
                    NewGame(); // Fallback if no panel
                }
            });
        }
        else
        {
            ResumeGameAfterNonFatalBomb();
        }

        elapsed = 0f;

        // Fade back in
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            fadeImage.color = Color.Lerp(Color.white, Color.clear, t);

            elapsed += Time.unscaledDeltaTime;

            yield return null;
        }
    }

}
