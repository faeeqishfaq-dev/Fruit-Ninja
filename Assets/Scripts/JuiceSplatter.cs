using UnityEngine;
using System.Collections;

public class JuiceSplatter : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public void Init(Color color, float lifetime, float fadeDuration, Sprite sprite, float scale, string sortingLayerName, int orderInLayer, float rotation)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) 
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        // Place behind fruit but in front of background
        spriteRenderer.sortingLayerName = sortingLayerName;
        spriteRenderer.sortingOrder = orderInLayer; 
        
        transform.rotation = Quaternion.Euler(0, 0, rotation);
        transform.localScale = new Vector3(scale, scale, 1f);

        StartCoroutine(FadeRoutine(lifetime, fadeDuration));
    }

    private IEnumerator FadeRoutine(float lifetime, float fadeDuration)
    {
        yield return new WaitForSeconds(Mathf.Max(0, lifetime - fadeDuration));
        
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / fadeDuration);
            yield return null;
        }

        Destroy(gameObject);
    }
}
