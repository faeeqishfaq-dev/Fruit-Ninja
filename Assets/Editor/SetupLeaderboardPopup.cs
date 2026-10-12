using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SetupLeaderboardPopup
{
    [MenuItem("Tools/Setup Leaderboard Popup")]
    public static void ExecuteSetup()
    {
        Debug.Log("=== Setting Up Leaderboard Popup in Splash Scene ===");

        string splashPath = "Assets/Scenes/Splash.unity";
        Scene splashScene = EditorSceneManager.OpenScene(splashPath, OpenSceneMode.Single);

        Font fruitFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/go3v2.ttf");
        Sprite woodPanelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GameOverUI/WoodPanel.png");
        Sprite headerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GameOverUI/Header.png");
        Sprite crossSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GameOverUI/Cross.png");
        Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/GameOverUI/Button.png");

        if (!Directory.Exists("Assets/Prefabs"))
        {
            Directory.CreateDirectory("Assets/Prefabs");
        }

        // ==========================================
        // 1. Create ScoreRowPrefab
        // ==========================================
        string prefabPath = "Assets/Prefabs/ScoreRowPrefab.prefab";
        GameObject rowTemplate = new GameObject("ScoreRowPrefab", typeof(RectTransform), typeof(Image), typeof(ScoreRowUI));
        RectTransform rowRt = rowTemplate.GetComponent<RectTransform>();
        rowRt.sizeDelta = new Vector2(700, 75);

        Image rowBg = rowTemplate.GetComponent<Image>();
        rowBg.sprite = headerSprite != null ? headerSprite : woodPanelSprite;
        rowBg.type = Image.Type.Sliced;
        rowBg.color = new Color(0.12f, 0.08f, 0.04f, 0.35f);

        ScoreRowUI rowUI = rowTemplate.GetComponent<ScoreRowUI>();
        rowUI.backgroundImage = rowBg;

        // Rank Text
        GameObject rankGo = new GameObject("RankText", typeof(RectTransform), typeof(Text), typeof(Outline));
        rankGo.transform.SetParent(rowTemplate.transform, false);
        RectTransform rankRt = rankGo.GetComponent<RectTransform>();
        rankRt.anchorMin = new Vector2(0f, 0f);
        rankRt.anchorMax = new Vector2(0.35f, 1f);
        rankRt.offsetMin = new Vector2(30, 0);
        rankRt.offsetMax = Vector2.zero;

        Text rankText = rankGo.GetComponent<Text>();
        rankText.text = "RANK 1";
        rankText.font = fruitFont != null ? fruitFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        rankText.fontSize = 32;
        rankText.fontStyle = FontStyle.Bold;
        rankText.alignment = TextAnchor.MiddleLeft;
        rankText.color = new Color32(255, 215, 0, 255);

        Outline rankOutline = rankGo.GetComponent<Outline>();
        rankOutline.effectColor = new Color32(50, 20, 0, 255);
        rankOutline.effectDistance = new Vector2(2, -2);
        rowUI.rankText = rankText;

        // Medal Text
        GameObject medalGo = new GameObject("MedalText", typeof(RectTransform), typeof(Text));
        medalGo.transform.SetParent(rowTemplate.transform, false);
        RectTransform medalRt = medalGo.GetComponent<RectTransform>();
        medalRt.anchorMin = new Vector2(0.35f, 0f);
        medalRt.anchorMax = new Vector2(0.55f, 1f);
        medalRt.offsetMin = Vector2.zero;
        medalRt.offsetMax = Vector2.zero;

        Text medalText = medalGo.GetComponent<Text>();
        medalText.text = "🥇";
        medalText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        medalText.fontSize = 36;
        medalText.alignment = TextAnchor.MiddleCenter;
        medalText.color = Color.white;
        rowUI.medalText = medalText;

        // Score Text
        GameObject scoreGo = new GameObject("ScoreText", typeof(RectTransform), typeof(Text), typeof(Outline));
        scoreGo.transform.SetParent(rowTemplate.transform, false);
        RectTransform scoreRt = scoreGo.GetComponent<RectTransform>();
        scoreRt.anchorMin = new Vector2(0.55f, 0f);
        scoreRt.anchorMax = new Vector2(1f, 1f);
        scoreRt.offsetMin = Vector2.zero;
        scoreRt.offsetMax = new Vector2(-35, 0);

        Text scoreText = scoreGo.GetComponent<Text>();
        scoreText.text = "950";
        scoreText.font = fruitFont != null ? fruitFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        scoreText.fontSize = 38;
        scoreText.fontStyle = FontStyle.Bold;
        scoreText.alignment = TextAnchor.MiddleRight;
        scoreText.color = new Color32(255, 235, 120, 255);

        Outline scoreOutline = scoreGo.GetComponent<Outline>();
        scoreOutline.effectColor = new Color32(50, 20, 0, 255);
        scoreOutline.effectDistance = new Vector2(2, -2);
        rowUI.scoreText = scoreText;

        // Save as prefab asset
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(rowTemplate, prefabPath);
        Object.DestroyImmediate(rowTemplate);
        AssetDatabase.Refresh();

        // ==========================================
        // 2. Setup Hierarchy in Splash Scene Canvas
        // ==========================================
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in Splash scene!");
            return;
        }

        SplashManager splashManager = Object.FindObjectOfType<SplashManager>();

        // Remove old LeaderboardPopup if exists to rebuild cleanly
        Transform existingPopup = canvas.transform.Find("LeaderboardPopup");
        if (existingPopup != null)
        {
            Object.DestroyImmediate(existingPopup.gameObject);
        }

        // LeaderboardPopup Root with its own Canvas and GraphicRaycaster for Override Sorting
        GameObject popupGo = new GameObject("LeaderboardPopup", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(LeaderboardPopup));
        popupGo.transform.SetParent(canvas.transform, false);
        popupGo.transform.SetAsLastSibling(); // Ensure renders in front of other panels

        Canvas popupCanvas = popupGo.GetComponent<Canvas>();
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 500; // Strictly above all menu UI, WorldSpace Ring, and 3D Watermelon

        RectTransform popupRt = popupGo.GetComponent<RectTransform>();
        popupRt.anchorMin = Vector2.zero;
        popupRt.anchorMax = Vector2.one;
        popupRt.offsetMin = Vector2.zero;
        popupRt.offsetMax = Vector2.zero;

        CanvasGroup popupCg = popupGo.GetComponent<CanvasGroup>();
        popupCg.alpha = 0f;
        popupCg.interactable = false;
        popupCg.blocksRaycasts = false;

        LeaderboardPopup popupComp = popupGo.GetComponent<LeaderboardPopup>();

        // Dark Overlay
        GameObject overlayGo = new GameObject("DarkOverlay", typeof(RectTransform), typeof(Image), typeof(Button));
        overlayGo.transform.SetParent(popupGo.transform, false);
        RectTransform overlayRt = overlayGo.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = Vector2.zero;
        overlayRt.offsetMax = Vector2.zero;

        Image overlayImg = overlayGo.GetComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0f);
        overlayImg.raycastTarget = false;

        Button overlayBtn = overlayGo.GetComponent<Button>();
        overlayBtn.transition = Selectable.Transition.None;
        UnityEventTools.AddPersistentListener(overlayBtn.onClick, popupComp.Hide);

        // LeaderboardPanel
        GameObject panelGo = new GameObject("LeaderboardPanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(popupGo.transform, false);
        RectTransform panelRt = panelGo.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta = new Vector2(850, 750);
        panelRt.localScale = Vector3.one;

        Image panelImg = panelGo.GetComponent<Image>();
        panelImg.sprite = woodPanelSprite;
        panelImg.type = Image.Type.Sliced;
        panelImg.color = Color.white;
        panelImg.raycastTarget = true;

        // Header
        GameObject headerGo = new GameObject("Header", typeof(RectTransform), typeof(Image));
        headerGo.transform.SetParent(panelGo.transform, false);
        RectTransform headerRt = headerGo.GetComponent<RectTransform>();
        headerRt.anchorMin = new Vector2(0.5f, 1f);
        headerRt.anchorMax = new Vector2(0.5f, 1f);
        headerRt.pivot = new Vector2(0.5f, 0.5f);
        headerRt.anchoredPosition = new Vector2(0, 15);
        headerRt.sizeDelta = new Vector2(550, 110);

        Image headerImg = headerGo.GetComponent<Image>();
        headerImg.sprite = headerSprite;
        headerImg.type = Image.Type.Sliced;
        headerImg.color = Color.white;

        // Header Text
        GameObject headerTextGo = new GameObject("HeaderText", typeof(RectTransform), typeof(Text), typeof(Outline), typeof(Shadow));
        headerTextGo.transform.SetParent(headerGo.transform, false);
        RectTransform headerTextRt = headerTextGo.GetComponent<RectTransform>();
        headerTextRt.anchorMin = Vector2.zero;
        headerTextRt.anchorMax = Vector2.one;
        headerTextRt.offsetMin = Vector2.zero;
        headerTextRt.offsetMax = Vector2.zero;

        Text headerText = headerTextGo.GetComponent<Text>();
        headerText.text = "LEADERBOARD";
        headerText.font = fruitFont != null ? fruitFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        headerText.fontSize = 52;
        headerText.fontStyle = FontStyle.Bold;
        headerText.alignment = TextAnchor.MiddleCenter;
        headerText.color = new Color32(250, 160, 20, 255);

        Outline headerOutline = headerTextGo.GetComponent<Outline>();
        headerOutline.effectColor = new Color32(80, 30, 0, 255);
        headerOutline.effectDistance = new Vector2(3, -3);

        Shadow headerShadow = headerTextGo.GetComponent<Shadow>();
        headerShadow.effectColor = new Color32(40, 15, 0, 180);
        headerShadow.effectDistance = new Vector2(2, -2);

        // Close Button
        GameObject closeBtnGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnGo.transform.SetParent(panelGo.transform, false);
        RectTransform closeBtnRt = closeBtnGo.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(1f, 1f);
        closeBtnRt.anchorMax = new Vector2(1f, 1f);
        closeBtnRt.pivot = new Vector2(0.5f, 0.5f);
        closeBtnRt.anchoredPosition = new Vector2(-40, -40);
        closeBtnRt.sizeDelta = new Vector2(65, 65);

        Image closeBtnImg = closeBtnGo.GetComponent<Image>();
        closeBtnImg.sprite = buttonSprite != null ? buttonSprite : woodPanelSprite;
        closeBtnImg.type = Image.Type.Sliced;
        closeBtnImg.color = new Color32(220, 130, 60, 255);

        Button closeBtn = closeBtnGo.GetComponent<Button>();
        UnityEventTools.AddPersistentListener(closeBtn.onClick, popupComp.Hide);

        // Cross Icon inside Close Button
        GameObject crossIconGo = new GameObject("CrossIcon", typeof(RectTransform), typeof(Image));
        crossIconGo.transform.SetParent(closeBtnGo.transform, false);
        RectTransform crossIconRt = crossIconGo.GetComponent<RectTransform>();
        crossIconRt.anchorMin = new Vector2(0.5f, 0.5f);
        crossIconRt.anchorMax = new Vector2(0.5f, 0.5f);
        crossIconRt.sizeDelta = new Vector2(38, 38);

        Image crossIconImg = crossIconGo.GetComponent<Image>();
        crossIconImg.sprite = crossSprite;
        crossIconImg.preserveAspect = true;
        crossIconImg.color = Color.white;
        crossIconImg.raycastTarget = false;

        // ScrollView
        GameObject scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(panelGo.transform, false);
        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = Vector2.zero;
        scrollRt.anchorMax = Vector2.one;
        scrollRt.offsetMin = new Vector2(50, 40);
        scrollRt.offsetMax = new Vector2(-50, -95);

        ScrollRect scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.inertia = true;
        scrollRect.decelerationRate = 0.135f;
        scrollRect.scrollSensitivity = 30f;

        // Ensure CloseButton renders on top of ScrollView and Header
        closeBtnGo.transform.SetAsLastSibling();

        // Viewport
        GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportGo.transform.SetParent(scrollGo.transform, false);
        RectTransform viewportRt = viewportGo.GetComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.offsetMin = Vector2.zero;
        viewportRt.offsetMax = Vector2.zero;

        Image viewportImg = viewportGo.GetComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0.01f);
        viewportImg.raycastTarget = true;

        Mask viewportMask = viewportGo.GetComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        scrollRect.viewport = viewportRt;

        // Content
        GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(viewportGo.transform, false);
        RectTransform contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0, 200);

        VerticalLayoutGroup vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(8, 8, 12, 12);
        vlg.spacing = 10;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        ContentSizeFitter csf = contentGo.GetComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRt;

        // Empty State Text
        GameObject emptyGo = new GameObject("EmptyStateText", typeof(RectTransform), typeof(Text), typeof(Outline));
        emptyGo.transform.SetParent(viewportGo.transform, false);
        RectTransform emptyRt = emptyGo.GetComponent<RectTransform>();
        emptyRt.anchorMin = new Vector2(0.5f, 0.5f);
        emptyRt.anchorMax = new Vector2(0.5f, 0.5f);
        emptyRt.pivot = new Vector2(0.5f, 0.5f);
        emptyRt.anchoredPosition = Vector2.zero;
        emptyRt.sizeDelta = new Vector2(600, 200);

        Text emptyText = emptyGo.GetComponent<Text>();
        emptyText.text = "NO SCORES RECORDED YET!\nSLICE WATERMELON TO PLAY & SET A RECORD!";
        emptyText.font = fruitFont != null ? fruitFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        emptyText.fontSize = 32;
        emptyText.fontStyle = FontStyle.Bold;
        emptyText.alignment = TextAnchor.MiddleCenter;
        emptyText.color = new Color32(245, 230, 200, 210);
        emptyText.lineSpacing = 1.3f;

        Outline emptyOutline = emptyGo.GetComponent<Outline>();
        emptyOutline.effectColor = new Color32(50, 20, 0, 255);
        emptyOutline.effectDistance = new Vector2(2, -2);

        // Put an inactive template row inside Content in Edit Mode as requested by Task 5
        GameObject editModeRow = Object.Instantiate(prefabAsset, contentGo.transform);
        editModeRow.name = "ScoreRowPrefab";
        editModeRow.SetActive(false);

        // Wire LeaderboardPopup fields using SerializedObject
        SerializedObject soPopup = new SerializedObject(popupComp);
        soPopup.FindProperty("panelRect").objectReferenceValue = panelRt;
        soPopup.FindProperty("popupCanvasGroup").objectReferenceValue = popupCg;
        soPopup.FindProperty("darkOverlay").objectReferenceValue = overlayImg;
        soPopup.FindProperty("closeButton").objectReferenceValue = closeBtn;
        soPopup.FindProperty("contentParent").objectReferenceValue = contentRt;
        soPopup.FindProperty("scoreRowPrefab").objectReferenceValue = prefabAsset;
        soPopup.FindProperty("emptyStateText").objectReferenceValue = emptyText;
        soPopup.FindProperty("scrollRect").objectReferenceValue = scrollRect;
        if (splashManager != null)
        {
            soPopup.FindProperty("splashManager").objectReferenceValue = splashManager;
        }

        // Wire Watermelon target reference
        MenuFruit[] allFruits = Object.FindObjectsOfType<MenuFruit>(true);
        foreach (MenuFruit fruit in allFruits)
        {
            if (fruit != null && fruit.gameObject.name.Contains("Watermelon"))
            {
                soPopup.FindProperty("watermelonTarget").objectReferenceValue = fruit.gameObject;
                break;
            }
        }

        soPopup.ApplyModifiedProperties();

        // Wire SplashManager reference
        if (splashManager != null)
        {
            SerializedObject soSplash = new SerializedObject(splashManager);
            soSplash.FindProperty("leaderboardPopup").objectReferenceValue = popupComp;
            soSplash.ApplyModifiedProperties();
        }

        // Wire ProfileButton OnClick
        Transform profileBtnT = canvas.transform.Find("MainMenuRoot/ProfileButton");
        if (profileBtnT != null)
        {
            Button profBtn = profileBtnT.GetComponent<Button>();
            if (profBtn != null)
            {
                // Clear any existing calls safely and link to SplashManager.OnProfileClicked
                while (profBtn.onClick.GetPersistentEventCount() > 0)
                {
                    UnityEventTools.RemovePersistentListener(profBtn.onClick, 0);
                }
                if (splashManager != null)
                {
                    UnityEventTools.AddPersistentListener(profBtn.onClick, splashManager.OnProfileClicked);
                }
                else
                {
                    UnityEventTools.AddPersistentListener(profBtn.onClick, popupComp.Show);
                }
                EditorUtility.SetDirty(profBtn);
            }
        }

        // Initially popup is disabled in Edit Mode (activates when PROFILE clicked)
        popupGo.SetActive(false);

        EditorUtility.SetDirty(popupGo);
        EditorSceneManager.MarkSceneDirty(splashScene);
        EditorSceneManager.SaveScene(splashScene);

        Debug.Log("=== Leaderboard Popup Hierarchy Setup Completed & Serialized to Splash.unity! ===");
    }
}
