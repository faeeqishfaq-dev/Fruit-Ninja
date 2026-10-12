using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;

public class SetupBombVFX
{
    [MenuItem("Tools/Setup Bomb VFX")]
    public static void DoSetup()
    {
        // 1. Create Sprites
        string texPath = "Assets/Textures";
        if (!AssetDatabase.IsValidFolder(texPath))
            AssetDatabase.CreateFolder("Assets", "Textures");

        string prefabsPath = "Assets/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabsPath))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        Texture2D wedgeTex = CreateWedgeTexture();
        byte[] wedgeBytes = wedgeTex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/Textures/BombWedge.png", wedgeBytes);
        
        Texture2D flashTex = CreateFlashTexture();
        byte[] flashBytes = flashTex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/Textures/BombFlash.png", flashBytes);

        AssetDatabase.Refresh();

        // Fix texture importer settings
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Textures/BombWedge.png");
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePivot = new Vector2(0.5f, 0f);
        importer.SaveAndReimport();

        importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Textures/BombFlash.png");
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePivot = new Vector2(0.5f, 0.5f);
        importer.SaveAndReimport();

        Sprite wedgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/BombWedge.png");
        Sprite flashSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/BombFlash.png");

        // 2. Create Prefab
        GameObject prefabGo = new GameObject("BombRayWedge");
        RectTransform rt = prefabGo.AddComponent<RectTransform>();
        rt.pivot = new Vector2(0.5f, 0f); // Pivot at the narrow end
        Image img = prefabGo.AddComponent<Image>();
        img.sprite = wedgeSprite;
        img.color = Color.white;

        string prefabPath = "Assets/Prefabs/BombRayWedge.prefab";
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabGo, prefabPath);
        GameObject.DestroyImmediate(prefabGo);

        // 3. Find Manager in Scene
        BombExplosionVFXManager manager = GameObject.FindObjectOfType<BombExplosionVFXManager>();
        if (manager == null)
        {
            GameObject managerGo = new GameObject("BombExplosionVFXManager");
            manager = managerGo.AddComponent<BombExplosionVFXManager>();
        }

        // 4. Create BurstCanvas
        GameObject canvasGo = new GameObject("BurstCanvas");
        canvasGo.transform.SetParent(manager.transform);
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGo.AddComponent<CanvasScaler>();
        manager.burstCanvas = canvas;

        // 5. Create RaysContainer
        GameObject containerGo = new GameObject("RaysContainer");
        containerGo.transform.SetParent(canvasGo.transform);
        RectTransform containerRt = containerGo.AddComponent<RectTransform>();
        containerRt.anchorMin = Vector2.zero;
        containerRt.anchorMax = Vector2.one;
        containerRt.offsetMin = Vector2.zero;
        containerRt.offsetMax = Vector2.zero;
        manager.raysContainer = containerRt;

        // 6. Create CenterFlash
        GameObject flashGo = new GameObject("CenterFlash");
        flashGo.transform.SetParent(canvasGo.transform);
        Image flashImg = flashGo.AddComponent<Image>();
        flashImg.sprite = flashSprite;
        flashImg.color = new Color(1, 1, 1, 0); // Default alpha 0
        flashGo.SetActive(false); // Default disabled
        manager.centerFlash = flashImg;

        // 7. Create FullScreenWhite
        GameObject whiteGo = new GameObject("FullScreenWhite");
        whiteGo.transform.SetParent(canvasGo.transform);
        Image whiteImg = whiteGo.AddComponent<Image>();
        whiteImg.color = new Color(1, 1, 1, 0); // Default alpha 0
        RectTransform whiteRt = whiteGo.GetComponent<RectTransform>();
        whiteRt.anchorMin = Vector2.zero;
        whiteRt.anchorMax = Vector2.one;
        whiteRt.offsetMin = Vector2.zero;
        whiteRt.offsetMax = Vector2.zero;
        manager.fullScreenWhite = whiteImg;

        // 8. Assign Prefab
        manager.rayWedgePrefab = savedPrefab;

        // 9. Save Scene
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }

    private static Texture2D CreateWedgeTexture()
    {
        int w = 64, h = 256;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] colors = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            float widthRatio = (float)y / h;
            float widthInPixels = widthRatio * w;
            float left = (w - widthInPixels) / 2f;
            float right = left + widthInPixels;
            for (int x = 0; x < w; x++)
            {
                if (x >= left && x <= right) colors[y * w + x] = Color.white;
                else colors[y * w + x] = Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateFlashTexture()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size];
        Vector2 center = new Vector2(size / 2f, size / 2f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= size / 2f)
                {
                    float alpha = 1f - Mathf.Clamp01(dist / (size / 2f));
                    colors[y * size + x] = new Color(1, 1, 1, alpha);
                }
                else
                    colors[y * size + x] = Color.clear;
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }
}
