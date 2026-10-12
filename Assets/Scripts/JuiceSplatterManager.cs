using UnityEngine;

public class JuiceSplatterManager : MonoBehaviour
{
    private static JuiceSplatterManager _instance;
    public static JuiceSplatterManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<JuiceSplatterManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("JuiceSplatterManager");
                    _instance = go.AddComponent<JuiceSplatterManager>();
                }
            }
            return _instance;
        }
    }

    [Header("References")]
    [Tooltip("Optional. If assigned, splatters will be projected onto this board's Z position.")]
    public Transform boardTransform;
    [Tooltip("Optional. If assigned, this prefab will be instantiated. Must have a SpriteRenderer.")]
    public GameObject splatterPrefab;
    [Tooltip("Add multiple sprites for natural variety. If empty, high-quality procedural splatters are generated.")]
    public Sprite[] splatterVariants;
    
    [Header("Timing")]
    public float lifetime = 5f;
    public float fadeDuration = 1f;

    [Header("Visual")]
    public float minimumScale = 0.8f;
    public float maximumScale = 1.2f;
    public float positionOffsetZ = -0.1f;
    public string sortingLayerName = "Default";
    public int orderInLayer = -1;
    [Range(0f, 1f)] public float opacityMultiplier = 0.9f;

    [Header("Variation")]
    public bool randomRotation = true;
    public bool randomSprite = true;
    public bool randomScale = true;

    [Header("Colors (Fallback)")]
    public Color defaultJuiceColor = new Color(1f, 0.2f, 0.2f, 1f);

    private Sprite[] proceduralSprites;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    public void SpawnSplatter(Vector3 fruitPosition, Color juiceColor)
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        float targetZ = boardTransform != null ? boardTransform.position.z : 100f;
        targetZ += positionOffsetZ;

        Vector3 screenPos = cam.WorldToScreenPoint(fruitPosition);
        Ray ray = cam.ScreenPointToRay(screenPos);
        
        Vector3 spawnPos = fruitPosition;
        
        if (ray.direction.z != 0)
        {
            float t = (targetZ - ray.origin.z) / ray.direction.z;
            spawnPos = ray.origin + ray.direction * t;
        }
        else
        {
            spawnPos.z = targetZ;
        }

        GameObject splatterObj;
        if (splatterPrefab != null)
        {
            splatterObj = Instantiate(splatterPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            splatterObj = new GameObject("JuiceSplatter");
            splatterObj.transform.position = spawnPos;
        }

        JuiceSplatter splatter = splatterObj.GetComponent<JuiceSplatter>();
        if (splatter == null)
        {
            splatter = splatterObj.AddComponent<JuiceSplatter>();
        }
        
        float distRatio = 1f;
        if (!cam.orthographic)
        {
            float fruitDist = fruitPosition.z - cam.transform.position.z;
            float zDist = targetZ - cam.transform.position.z;
            distRatio = Mathf.Abs(zDist) / (Mathf.Abs(fruitDist) == 0 ? 1 : Mathf.Abs(fruitDist));
        }
        distRatio *= 1.5f; 

        Color finalColor = juiceColor.a == 0 ? defaultJuiceColor : juiceColor;
        finalColor.a *= opacityMultiplier; // Realistic slightly transparent look

        float scaleMultiplier = randomScale ? Random.Range(minimumScale, maximumScale) : minimumScale;
        scaleMultiplier *= distRatio;

        float rotation = randomRotation ? Random.Range(0f, 360f) : 0f;

        splatter.Init(finalColor, lifetime, fadeDuration, GetSprite(), scaleMultiplier, sortingLayerName, orderInLayer, rotation);
    }

    private Sprite GetSprite()
    {
        if (splatterVariants != null && splatterVariants.Length > 0)
        {
            return randomSprite ? splatterVariants[Random.Range(0, splatterVariants.Length)] : splatterVariants[0];
        }
        
        if (proceduralSprites == null || proceduralSprites.Length == 0)
        {
            proceduralSprites = new Sprite[3];
            proceduralSprites[0] = CreateProceduralSplatter(0);
            proceduralSprites[1] = CreateProceduralSplatter(1);
            proceduralSprites[2] = CreateProceduralSplatter(2);
        }
        
        return proceduralSprites[Random.Range(0, proceduralSprites.Length)];
    }

    private Sprite CreateProceduralSplatter(int variant)
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        
        float seed = Random.Range(0f, 100f) + variant * 10f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float angle = Mathf.Atan2(y - center.y, x - center.x);
                
                // Extremely organic math with droplets and directional stretching
                float stretch = variant == 1 ? (Mathf.Sin(angle) * 15f) : 0f; // Variation 1 is stretched
                
                float radius = (size * 0.25f) + 
                               Mathf.PerlinNoise(Mathf.Cos(angle) * 2f + seed, Mathf.Sin(angle) * 2f + seed) * (size * 0.2f) +
                               Mathf.Sin(angle * 5f) * (size * 0.05f) + 
                               stretch;
                
                bool isDroplet = false;
                if (dist > radius && dist < size * 0.48f)
                {
                    // Generate natural scattered droplets
                    float dropNoise = Mathf.PerlinNoise(x * 0.1f + seed, y * 0.1f + seed);
                    if (dropNoise > 0.85f && Random.value > 0.6f) 
                    {
                        isDroplet = true;
                    }
                }

                if (dist < radius || isDroplet)
                {
                    // Soft edges and varying density
                    float edgeFade = Mathf.Clamp01((radius - dist) / 5f);
                    if (isDroplet) edgeFade = 1f;

                    float density = Mathf.PerlinNoise(x * 0.05f + seed, y * 0.05f + seed);
                    float alpha = Mathf.Lerp(0.6f, 1f, density) * edgeFade;
                    
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    colors[y * size + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
    }
}
