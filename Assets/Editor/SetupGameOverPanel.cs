using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class SetupGameOverPanel
{
    [MenuItem("Tools/Setup Game Over Panel")]
    public static void RunSetup()
    {
        string currentScene = EditorSceneManager.GetActiveScene().path;
        string scenePath = "Assets/Scenes/FruitNinja.unity";
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Generate Sprites
        string spriteFolder = "Assets/Sprites/GameOverUI";
        if (!Directory.Exists(spriteFolder)) Directory.CreateDirectory(spriteFolder);

        Sprite woodPanelSprite = CreateRoundedRect(spriteFolder + "/WoodPanel.png", new Color32(210, 150, 80, 255), 32);
        Sprite headerSprite = CreateRoundedRect(spriteFolder + "/Header.png", new Color32(245, 230, 180, 255), 16);
        Sprite crossSprite = CreateCross(spriteFolder + "/Cross.png", new Color32(220, 30, 30, 255));
        Sprite buttonSprite = CreateRoundedRect(spriteFolder + "/Button.png", new Color32(160, 100, 50, 255), 12);
        
        AssetDatabase.Refresh();

        // Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) return;

        // GameOverUIManager
        GameObject goObj = GameObject.Find("GameOverManager");
        if (goObj == null) goObj = new GameObject("GameOverManager");
        GameOverUIManager manager = goObj.GetComponent<GameOverUIManager>();
        if (manager == null) manager = goObj.AddComponent<GameOverUIManager>();

        // GameOverPanel Root
        Transform existingPanel = canvas.transform.Find("GameOverPanel");
        if (existingPanel != null) Object.DestroyImmediate(existingPanel.gameObject);

        GameObject panelObj = new GameObject("GameOverPanel", typeof(RectTransform));
        panelObj.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(900, 600);
        panelRect.anchoredPosition = new Vector2(0, 1000f); // Starts offscreen

        // Background
        GameObject bgObj = new GameObject("WoodenBackground", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(panelRect, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        SetStretch(bgRect);
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFolder + "/WoodPanel.png");
        bgImg.type = Image.Type.Sliced;

        // Header
        GameObject hdrObj = new GameObject("GameOverHeader", typeof(RectTransform), typeof(Image));
        hdrObj.transform.SetParent(panelRect, false);
        RectTransform hdrRect = hdrObj.GetComponent<RectTransform>();
        hdrRect.anchorMin = new Vector2(0.1f, 0.85f);
        hdrRect.anchorMax = new Vector2(0.9f, 1.05f);
        hdrRect.offsetMin = Vector2.zero;
        hdrRect.offsetMax = Vector2.zero;
        Image hdrImg = hdrObj.GetComponent<Image>();
        hdrImg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFolder + "/Header.png");
        hdrImg.type = Image.Type.Sliced;
        
        // Header Text
        GameObject hdrTextObj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
        hdrTextObj.transform.SetParent(hdrRect, false);
        SetStretch(hdrTextObj.GetComponent<RectTransform>());
        Text hdrTxt = hdrTextObj.GetComponent<Text>();
        hdrTxt.text = "GAME OVER";
        hdrTxt.alignment = TextAnchor.MiddleCenter;
        hdrTxt.fontSize = 70;
        hdrTxt.fontStyle = FontStyle.Bold;
        hdrTxt.color = new Color32(250, 160, 20, 255);
        Outline outline = hdrTextObj.GetComponent<Outline>();
        outline.effectColor = new Color32(80, 30, 0, 255);
        outline.effectDistance = new Vector2(3, -3);

        // Red Crosses Container
        GameObject crossesObj = new GameObject("RedCrosses", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        crossesObj.transform.SetParent(panelRect, false);
        RectTransform crossesRect = crossesObj.GetComponent<RectTransform>();
        crossesRect.anchorMin = new Vector2(0.2f, 0.6f);
        crossesRect.anchorMax = new Vector2(0.8f, 0.8f);
        crossesRect.offsetMin = Vector2.zero;
        crossesRect.offsetMax = Vector2.zero;
        HorizontalLayoutGroup hlg = crossesObj.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlHeight = false;
        hlg.childControlWidth = false;
        hlg.spacing = 30;

        for (int i = 0; i < 3; i++)
        {
            GameObject cross = new GameObject("Cross_" + (i+1), typeof(RectTransform), typeof(Image));
            cross.transform.SetParent(crossesRect, false);
            cross.GetComponent<RectTransform>().sizeDelta = new Vector2(80, 80);
            cross.GetComponent<Image>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spriteFolder + "/Cross.png");
        }

        // Font
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Score
        Text scoreTxt = CreateText(panelRect, "ScoreText", "SCORE : 0", new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f), font, 60);
        // Separator
        GameObject sep = new GameObject("SeparatorLine", typeof(RectTransform), typeof(Image));
        sep.transform.SetParent(panelRect, false);
        RectTransform sepRect = sep.GetComponent<RectTransform>();
        sepRect.anchorMin = new Vector2(0.1f, 0.38f);
        sepRect.anchorMax = new Vector2(0.9f, 0.39f);
        sepRect.offsetMin = Vector2.zero;
        sepRect.offsetMax = Vector2.zero;
        sep.GetComponent<Image>().color = new Color32(150, 90, 40, 255);
        // Best Score
        Text bestTxt = CreateText(panelRect, "BestScoreText", "BEST : 0", new Vector2(0.1f, 0.15f), new Vector2(0.9f, 0.35f), font, 50);

        // Buttons
        Button homeBtn = CreateButton(panelRect, "HomeButton", new Vector2(0.3f, -0.2f), new Vector2(0.45f, 0.05f), spriteFolder + "/Button.png", "H");
        Button retryBtn = CreateButton(panelRect, "RetryButton", new Vector2(0.55f, -0.2f), new Vector2(0.7f, 0.05f), spriteFolder + "/Button.png", "R");

        // Setup Manager
        manager.panelRect = panelRect;
        manager.scoreText = scoreTxt;
        manager.bestScoreText = bestTxt;
        manager.homeButton = homeBtn;
        manager.retryButton = retryBtn;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Game Over Panel successfully generated in FruitNinja scene.");
        
        if (!string.IsNullOrEmpty(currentScene) && currentScene != scenePath)
        {
            EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
        }
    }

    private static void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static Text CreateText(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, Font font, int size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        Text txt = go.GetComponent<Text>();
        txt.text = text;
        txt.font = font;
        txt.fontSize = size;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color32(255, 200, 50, 255);
        
        Outline outl = go.GetComponent<Outline>();
        outl.effectColor = Color.black;
        outl.effectDistance = new Vector2(2, -2);
        
        return txt;
    }

    private static Button CreateButton(Transform parent, string name, Vector2 aMin, Vector2 aMax, string spritePath, string iconText)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        Image img = go.GetComponent<Image>();
        img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        img.type = Image.Type.Sliced;
        
        Button btn = go.GetComponent<Button>();
        
        // Add simple icon text instead of full graphic
        GameObject txtGo = new GameObject("Icon", typeof(RectTransform), typeof(Text), typeof(Outline));
        txtGo.transform.SetParent(go.transform, false);
        SetStretch(txtGo.GetComponent<RectTransform>());
        Text txt = txtGo.GetComponent<Text>();
        txt.text = iconText;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 80;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color32(250, 150, 20, 255);
        Outline outl = txtGo.GetComponent<Outline>();
        outl.effectColor = new Color32(80, 30, 0, 255);
        
        return btn;
    }

    private static Sprite CreateRoundedRect(string path, Color32 color, int radius)
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = 0f;
                if (x < radius && y < radius) dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                else if (x > size - 1 - radius && y < radius) dist = Vector2.Distance(new Vector2(x, y), new Vector2(size - 1 - radius, radius));
                else if (x < radius && y > size - 1 - radius) dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, size - 1 - radius));
                else if (x > size - 1 - radius && y > size - 1 - radius) dist = Vector2.Distance(new Vector2(x, y), new Vector2(size - 1 - radius, size - 1 - radius));
                
                if (dist > radius) pixels[y * size + x] = new Color32(0, 0, 0, 0);
                else pixels[y * size + x] = color;
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteBorder = new Vector4(radius, radius, radius, radius);
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    
    private static Sprite CreateCross(string path, Color32 color)
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                if (Mathf.Abs(x - y) < 10 || Mathf.Abs((size - x) - y) < 10)
                    pixels[y * size + x] = color;
                else
                    pixels[y * size + x] = new Color32(0,0,0,0);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
