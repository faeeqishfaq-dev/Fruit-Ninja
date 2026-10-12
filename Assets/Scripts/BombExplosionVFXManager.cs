using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BombExplosionVFXManager : MonoBehaviour
{
    private static BombExplosionVFXManager _instance;
    public static BombExplosionVFXManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BombExplosionVFXManager>();
                if (_instance == null && Application.isPlaying)
                {
                    GameObject go = new GameObject("BombExplosionVFXManager");
                    _instance = go.AddComponent<BombExplosionVFXManager>();
                }
            }
            return _instance;
        }
    }

    [Header("References")]
    public Canvas burstCanvas;
    public RectTransform raysContainer;
    public GameObject rayWedgePrefab;
    public Image centerFlash;
    public Image fullScreenWhite;

    [Header("Gameplay Freeze")]
    public bool freezeGameplayOnBomb = true;

    [Header("Rays")]
    public int rayCount = 6;
    public float rayExpandDuration = 0.20f;
    public float minRayWidth = 100f;
    public float maxRayWidth = 300f;
    public float rayLengthPadding = 500f;
    
    [Header("Ray Timing")]
    public float firstRayDelay = 0.00f;
    public float secondRayDelay = 0.10f;
    public float thirdRayDelay = 0.20f;
    public float finalBurstDelay = 0.30f;

    [Header("Camera Jerk")]
    public float shakeStrength = 0.15f;
    public float shakeFrequency = 35f;
    public float rotationStrength = 1f;

    [Header("White Out")]
    public float whiteFadeStartTime = 0.45f;
    public float whiteFadeInDuration = 0.4f;
    public float whiteHoldDuration = 3.0f;
    public float whiteFadeOutDuration = 0.5f;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying && burstCanvas == null)
        {
            InitializeDefaultSetup();
            UnityEditor.EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }

    private void InitializeDefaultSetup()
    {
        if (burstCanvas != null) return;

        // Setup Canvas
        GameObject canvasGo = new GameObject("BurstCanvas");
        canvasGo.transform.SetParent(transform);
        burstCanvas = canvasGo.AddComponent<Canvas>();
        burstCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        burstCanvas.sortingOrder = 100; // Above UI if necessary
        canvasGo.AddComponent<CanvasScaler>();
        
        // Rays Container
        GameObject containerGo = new GameObject("RaysContainer");
        containerGo.transform.SetParent(burstCanvas.transform);
        raysContainer = containerGo.AddComponent<RectTransform>();
        raysContainer.anchorMin = Vector2.zero;
        raysContainer.anchorMax = Vector2.one;
        raysContainer.offsetMin = Vector2.zero;
        raysContainer.offsetMax = Vector2.zero;

        // Center Flash
        GameObject flashGo = new GameObject("CenterFlash");
        flashGo.transform.SetParent(burstCanvas.transform);
        centerFlash = flashGo.AddComponent<Image>();
        centerFlash.color = new Color(1, 1, 1, 0); // Default hidden
        
        int flashSize = 128;
        Texture2D flashTex = new Texture2D(flashSize, flashSize, TextureFormat.RGBA32, false);
        Color[] fColors = new Color[flashSize * flashSize];
        Vector2 fCenter = new Vector2(flashSize / 2f, flashSize / 2f);
        for (int y = 0; y < flashSize; y++) {
            for (int x = 0; x < flashSize; x++) {
                float dist = Vector2.Distance(new Vector2(x, y), fCenter);
                if (dist <= flashSize / 2f) {
                    float alpha = 1f - Mathf.Clamp01(dist / (flashSize / 2f));
                    fColors[y * flashSize + x] = new Color(1, 1, 1, alpha);
                } else fColors[y * flashSize + x] = Color.clear;
            }
        }
        flashTex.SetPixels(fColors); flashTex.Apply();
        
        if (!System.IO.Directory.Exists(Application.dataPath + "/Textures")) System.IO.Directory.CreateDirectory(Application.dataPath + "/Textures");
        System.IO.File.WriteAllBytes(Application.dataPath + "/Textures/BombFlash.png", flashTex.EncodeToPNG());
        UnityEditor.AssetDatabase.Refresh();
        
        var impFlash = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath("Assets/Textures/BombFlash.png");
        if (impFlash != null) { impFlash.textureType = UnityEditor.TextureImporterType.Sprite; impFlash.spritePivot = new Vector2(0.5f, 0.5f); impFlash.SaveAndReimport(); }
        
        centerFlash.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/BombFlash.png");

        // Full Screen White
        GameObject whiteGo = new GameObject("FullScreenWhite");
        whiteGo.transform.SetParent(burstCanvas.transform);
        fullScreenWhite = whiteGo.AddComponent<Image>();
        fullScreenWhite.color = new Color(1, 1, 1, 0); // Default hidden
        RectTransform whiteRt = fullScreenWhite.GetComponent<RectTransform>();
        whiteRt.anchorMin = Vector2.zero;
        whiteRt.anchorMax = Vector2.one;
        whiteRt.offsetMin = Vector2.zero;
        whiteRt.offsetMax = Vector2.zero;

        // Prefab creation
        if (!System.IO.Directory.Exists(Application.dataPath + "/Prefabs")) System.IO.Directory.CreateDirectory(Application.dataPath + "/Prefabs");
        
        GameObject prefabGo = new GameObject("BombRayWedge");
        RectTransform rt = prefabGo.AddComponent<RectTransform>();
        rt.pivot = new Vector2(0f, 0.5f); // Pivot at the narrow tip (left side)
        Image img = prefabGo.AddComponent<Image>();
        
        int w = 512, h = 512;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] colors = new Color[w * h];
        for (int x = 0; x < w; x++) {
            float lengthRatio = (float)x / w; 
            float widthRatio = lengthRatio; 
            float centerY = h / 2f;
            float maxHalfWidth = (h / 2f) * widthRatio;
            for (int y = 0; y < h; y++) {
                if (maxHalfWidth <= 0.1f) {
                    colors[y * w + x] = Color.clear;
                    continue;
                }
                float distFromCenter = Mathf.Abs(y - centerY);
                if (distFromCenter > maxHalfWidth) {
                    colors[y * w + x] = Color.clear;
                } else {
                    float normalizedDist = distFromCenter / maxHalfWidth;
                    float alpha = 1f - Mathf.Pow(normalizedDist, 1.5f); // Soft gradient falloff
                    
                    Color finalColor = Color.white;
                    finalColor.a = alpha;
                    colors[y * w + x] = finalColor;
                }
            }
        }
        tex.SetPixels(colors); tex.Apply();
        System.IO.File.WriteAllBytes(Application.dataPath + "/Textures/BombWedge.png", tex.EncodeToPNG());
        UnityEditor.AssetDatabase.Refresh();
        
        var impWedge = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath("Assets/Textures/BombWedge.png");
        if (impWedge != null) { impWedge.textureType = UnityEditor.TextureImporterType.Sprite; impWedge.spritePivot = new Vector2(0f, 0.5f); impWedge.SaveAndReimport(); }

        img.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/BombWedge.png");
        img.color = Color.white;

        string prefabPath = "Assets/Prefabs/BombRayWedge.prefab";
        rayWedgePrefab = UnityEditor.PrefabUtility.SaveAsPrefabAsset(prefabGo, prefabPath);
        DestroyImmediate(prefabGo);
    }
#endif

    public IEnumerator PlayVFX(Vector3 bombWorldPos, System.Action onWhiteScreenReached = null)
    {
        if (burstCanvas == null || rayWedgePrefab == null || fullScreenWhite == null)
            yield break;

        Camera cam = Camera.main;
        
        // 1. Force Canvas render mode to allow depth sorting with World Space objects
        burstCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        burstCanvas.worldCamera = cam;
        burstCanvas.planeDistance = 10f; // Typical distance for 2D bomb
        burstCanvas.sortingLayerName = "Default";
        burstCanvas.sortingOrder = 100; // Keep FullScreenWhite in front of everything

        // 2. Ensure RaysContainer is absolutely full-screen and perfectly centered
        raysContainer.anchorMin = Vector2.zero;
        raysContainer.anchorMax = Vector2.one;
        raysContainer.offsetMin = Vector2.zero;
        raysContainer.offsetMax = Vector2.zero;
        raysContainer.localScale = Vector3.one;
        raysContainer.localRotation = Quaternion.identity;

        // 3. Create a dedicated sub-canvas on RaysContainer to force it BEHIND the bomb
        Canvas rayCanvas = raysContainer.GetComponent<Canvas>();
        if (rayCanvas == null) rayCanvas = raysContainer.gameObject.AddComponent<Canvas>();
        rayCanvas.overrideSorting = true;
        rayCanvas.sortingLayerName = "Default";
        rayCanvas.sortingOrder = -10; // Renders completely behind the bomb

        // 4. Accurately calculate the local position of the bomb relative to RaysContainer
        Vector2 screenPos = cam.WorldToScreenPoint(bombWorldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(raysContainer, screenPos, cam, out Vector2 bombLocalPoint);
        
        // --- Camera Shake Setup ---
        Vector3 originalCamPos = cam.transform.localPosition;
        Quaternion originalCamRot = cam.transform.localRotation;
        bool shakeActive = true;
        
        StartCoroutine(CameraShakeRoutine(cam.transform, originalCamPos, originalCamRot, () => shakeActive));

        // --- Rays Sequence ---
        StartCoroutine(SpawnRaysRoutine(bombLocalPoint));

        // --- White Fade In ---
        float elapsed = 0f;
        while (elapsed < whiteFadeStartTime + whiteFadeInDuration)
        {
            if (elapsed >= whiteFadeStartTime)
            {
                float t = Mathf.Clamp01((elapsed - whiteFadeStartTime) / whiteFadeInDuration);
                fullScreenWhite.color = Color.Lerp(Color.clear, Color.white, t);
            }
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        fullScreenWhite.color = Color.white;
        
        // STOP SHAKE immediately when screen is fully white
        shakeActive = false;
        cam.transform.localPosition = originalCamPos;
        cam.transform.localRotation = originalCamRot;

        // Hide Rays and Flash behind the white screen
        foreach (Transform child in raysContainer) Destroy(child.gameObject);
        if (centerFlash != null) centerFlash.gameObject.SetActive(false);

        // Notify that screen is fully white (for health updates)
        onWhiteScreenReached?.Invoke();

        // --- White Hold ---
        yield return new WaitForSecondsRealtime(whiteHoldDuration);

        // --- White Fade Out ---
        elapsed = 0f;
        while (elapsed < whiteFadeOutDuration)
        {
            float t = Mathf.Clamp01(elapsed / whiteFadeOutDuration);
            fullScreenWhite.color = Color.Lerp(Color.white, Color.clear, t);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        fullScreenWhite.color = Color.clear;
    }

    private IEnumerator CameraShakeRoutine(Transform cam, Vector3 origPos, Quaternion origRot, System.Func<bool> isActive)
    {
        while (isActive())
        {
            float noiseX = (Mathf.PerlinNoise(Time.unscaledTime * shakeFrequency, 0f) - 0.5f) * 2f;
            float noiseY = (Mathf.PerlinNoise(0f, Time.unscaledTime * shakeFrequency) - 0.5f) * 2f;
            float noiseRot = (Mathf.PerlinNoise(Time.unscaledTime * shakeFrequency, Time.unscaledTime * shakeFrequency) - 0.5f) * 2f;
            
            cam.localPosition = origPos + new Vector3(noiseX, noiseY, 0f) * shakeStrength;
            cam.localRotation = origRot * Quaternion.Euler(0, 0, noiseRot * rotationStrength);
            
            yield return null;
        }
        cam.localPosition = origPos;
        cam.localRotation = origRot;
    }

    private IEnumerator SpawnRaysRoutine(Vector2 bombLocalPoint)
    {
        // Clear old ones just in case
        foreach (Transform child in raysContainer) Destroy(child.gameObject);

        float targetLength = Mathf.Max(Screen.width, Screen.height) * 2f + rayLengthPadding;
        
        // Exact 6 angles
        float[] angles = new float[6];
        angles[0] = Random.Range(30f, 60f);   // upper-right
        angles[1] = Random.Range(210f, 240f); // lower-left
        angles[2] = Random.Range(300f, 330f); // lower-right
        angles[3] = Random.Range(120f, 150f); // upper-left
        angles[4] = Random.Range(70f, 110f);  // upper
        angles[5] = Random.Range(160f, 200f); // left

        // Width multipliers
        float[] widths = new float[6];
        widths[0] = 2.0f; // wide
        widths[1] = 1.5f; // medium
        widths[2] = 2.0f; // wide
        widths[3] = 1.0f; // narrow
        widths[4] = 1.8f; // medium-wide
        widths[5] = 3.5f; // very wide

        // Ray 1
        yield return new WaitForSecondsRealtime(firstRayDelay);
        SpawnSingleRay(bombLocalPoint, angles[0], widths[0], targetLength);
        
        // Ray 2
        float wait = secondRayDelay - firstRayDelay;
        if (wait > 0) yield return new WaitForSecondsRealtime(wait);
        SpawnSingleRay(bombLocalPoint, angles[1], widths[1], targetLength);

        // Ray 3
        wait = thirdRayDelay - secondRayDelay;
        if (wait > 0) yield return new WaitForSecondsRealtime(wait);
        SpawnSingleRay(bombLocalPoint, angles[2], widths[2], targetLength);

        // Final Burst (4, 5, 6)
        wait = finalBurstDelay - thirdRayDelay;
        if (wait > 0) yield return new WaitForSecondsRealtime(wait);
        SpawnSingleRay(bombLocalPoint, angles[3], widths[3], targetLength);
        SpawnSingleRay(bombLocalPoint, angles[4], widths[4], targetLength);
        SpawnSingleRay(bombLocalPoint, angles[5], widths[5], targetLength);
    }

    private void SpawnSingleRay(Vector2 bombLocalPoint, float angle, float widthMult, float length)
    {
        GameObject rayGo = Instantiate(rayWedgePrefab, raysContainer);
        rayGo.SetActive(true);
        
        RectTransform rt = rayGo.GetComponent<RectTransform>();
        rt.localScale = Vector3.one; // Reset scale explicitly
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f); // Pivot at narrow tip (left)
        rt.anchoredPosition = bombLocalPoint; // Exact calculated bomb center
        
        rt.localRotation = Quaternion.Euler(0, 0, angle);
        rt.sizeDelta = Vector2.zero; // Starting size

        float targetWidth = Random.Range(minRayWidth, maxRayWidth) * widthMult;
        
        StartCoroutine(ExpandRayRoutine(rt, targetWidth, length));
    }

    private IEnumerator ExpandRayRoutine(RectTransform rt, float targetWidth, float targetLength)
    {
        float elapsed = 0f;
        while (elapsed < rayExpandDuration)
        {
            float t = elapsed / rayExpandDuration;
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease out
            
            rt.sizeDelta = new Vector2(targetLength * easeT, targetWidth * easeT); // length on X, width on Y
            
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        rt.sizeDelta = new Vector2(targetLength, targetWidth);
    }
}
