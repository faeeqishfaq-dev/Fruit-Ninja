using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CountdownManager : MonoBehaviour
{
    public static CountdownManager Instance { get; private set; }

    [Header("Images")]
    public Sprite Number1Sprite;
    public Sprite Number2Sprite;
    public Sprite Number3Sprite;
    public Sprite GoSprite;

    [Header("Timing")]
    public float NumberDuration = 1f;
    public float GoDuration = 0.7f;

    [Header("Animation")]
    public float StartScale = 0.5f;
    public float PopScale = 1.2f;
    public float PopDuration = 0.2f;
    public float FadeDuration = 0.2f;

    [Header("References")]
    public Image CountdownImage;
    public GameObject CountdownPanel;
    public Spawner spawner; // Not directly used since GameManager handles it, but exposed as requested

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void StartCountdown(System.Action onComplete)
    {
        if (CountdownPanel != null)
            CountdownPanel.SetActive(true);
            
        StartCoroutine(CountdownSequence(onComplete));
    }

    private IEnumerator CountdownSequence(System.Action onComplete)
    {
        // Disable initially
        if (CountdownImage != null)
        {
            CountdownImage.gameObject.SetActive(false);
        }

        // Sequence: 1 -> 2 -> 3 -> GO
        yield return StartCoroutine(PlayImageAnim(Number1Sprite, NumberDuration));
        yield return StartCoroutine(PlayImageAnim(Number2Sprite, NumberDuration));
        yield return StartCoroutine(PlayImageAnim(Number3Sprite, NumberDuration));
        yield return StartCoroutine(PlayImageAnim(GoSprite, GoDuration, 1.2f)); // Go pops a bit bigger

        // Hide Panel
        if (CountdownPanel != null)
            CountdownPanel.SetActive(false);

        // Resume Gameplay
        onComplete?.Invoke();
    }

    private IEnumerator PlayImageAnim(Sprite sprite, float duration, float scaleMultiplier = 1f)
    {
        if (CountdownImage == null || sprite == null) yield break;

        CountdownImage.sprite = sprite;
        CountdownImage.gameObject.SetActive(true);
        CountdownImage.SetNativeSize();
        
        Color resetColor = CountdownImage.color;
        resetColor.a = 1f;
        CountdownImage.color = resetColor;

        // Optional: Ensure it fits on screen nicely if it's too big natively
        // Scaling will be handled by the rect transform, but we control localScale

        float targetPop = PopScale * scaleMultiplier;
        float elapsed = 0f;

        // Pop in
        while (elapsed < PopDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / PopDuration);
            // Ease out elastic or just lerp
            CountdownImage.transform.localScale = Vector3.Lerp(Vector3.one * StartScale, Vector3.one * targetPop, t);
            yield return null;
        }

        // Hold and slightly shrink/fade
        float holdDuration = duration - PopDuration - FadeDuration;
        if (holdDuration > 0)
        {
            elapsed = 0f;
            while (elapsed < holdDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / holdDuration);
                CountdownImage.transform.localScale = Vector3.Lerp(Vector3.one * targetPop, Vector3.one * (targetPop * 0.9f), t);
                yield return null;
            }
        }

        // Fade out
        elapsed = 0f;
        Color c = CountdownImage.color;
        c.a = 1f;
        while (elapsed < FadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / FadeDuration);
            c.a = 1f - t;
            CountdownImage.color = c;
            CountdownImage.transform.localScale = Vector3.Lerp(Vector3.one * (targetPop * 0.9f), Vector3.one * StartScale, t);
            yield return null;
        }

        CountdownImage.gameObject.SetActive(false);
        c.a = 1f;
        CountdownImage.color = c;
    }
}
