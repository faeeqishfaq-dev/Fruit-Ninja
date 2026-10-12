using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RestoreOriginalSplashUI
{
    [MenuItem("Tools/Restore Original Splash UI")]
    public static void ExecuteRestore()
    {
        Debug.Log("=== Restoring Original Splash UI to match Image 1 & Image 3 ===");

        string splashPath = "Assets/Scenes/Splash.unity";
        Scene splashScene = EditorSceneManager.OpenScene(splashPath, OpenSceneMode.Single);

        // 1. Camera Configuration
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.2f, 0.12f, 0.05f); // Warm wood brown, not pitch black
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0, 0, -10f);
        }

        // 2. Canvas Configuration
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 50f;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080); // Landscape
                scaler.matchWidthOrHeight = 0.5f;
            }

            // 3. Wooden Background
            Transform bgT = canvas.transform.Find("Background");
            if (bgT == null)
            {
                GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
                bgGo.transform.SetParent(canvas.transform, false);
                bgT = bgGo.transform;
            }
            bgT.SetAsFirstSibling();
            RectTransform bgRt = bgT.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            Image bgImg = bgT.GetComponent<Image>();
            Sprite woodSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/wooden_background.jpg");
            if (woodSprite != null)
            {
                bgImg.sprite = woodSprite;
            }
            bgImg.color = Color.white;

            // 4. LoadingPanel Configuration (Matching Image 1)
            Transform loadingPanelT = canvas.transform.Find("LoadingPanel");
            if (loadingPanelT != null)
            {
                loadingPanelT.gameObject.SetActive(true);

                // Remove black panel overlay so wooden background is visible
                Image panelImg = loadingPanelT.GetComponent<Image>();
                if (panelImg != null)
                {
                    panelImg.color = new Color(0, 0, 0, 0); // Transparent
                }

                // Clean unwanted children
                for (int i = loadingPanelT.childCount - 1; i >= 0; i--)
                {
                    Transform child = loadingPanelT.GetChild(i);
                    if (child.name == "Text" || child.name == "Logo" && child.GetComponent<Image>()?.sprite?.name == "Fruit_Ninja_icon")
                    {
                        Object.DestroyImmediate(child.gameObject);
                    }
                }

                // Logo on LoadingPanel (Image 1)
                Transform logoT = loadingPanelT.Find("Logo");
                if (logoT == null)
                {
                    GameObject logoGo = new GameObject("Logo", typeof(RectTransform), typeof(Image));
                    logoGo.transform.SetParent(loadingPanelT, false);
                    logoT = logoGo.transform;
                }
                RectTransform logoRt = logoT.GetComponent<RectTransform>();
                logoRt.anchorMin = new Vector2(0.5f, 0.72f);
                logoRt.anchorMax = new Vector2(0.5f, 0.72f);
                logoRt.pivot = new Vector2(0.5f, 0.5f);
                logoRt.anchoredPosition = Vector2.zero;
                logoRt.sizeDelta = new Vector2(850f, 180f);

                Image logoImg = logoT.GetComponent<Image>();
                Sprite fnLogo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/FruitNinja_Logo.png");
                if (fnLogo != null)
                {
                    logoImg.sprite = fnLogo;
                }
                logoImg.preserveAspect = true;
                logoImg.color = Color.white;

                // Loading Bar Frame (Image 1)
                Transform loadingBarT = loadingPanelT.Find("Loading Bar");
                if (loadingBarT == null) loadingBarT = loadingPanelT.Find("LoadingBar");
                if (loadingBarT == null)
                {
                    GameObject lbGo = new GameObject("Loading Bar", typeof(RectTransform), typeof(Image));
                    lbGo.transform.SetParent(loadingPanelT, false);
                    loadingBarT = lbGo.transform;
                }
                else
                {
                    loadingBarT.gameObject.name = "Loading Bar";
                }
                RectTransform lbRt = loadingBarT.GetComponent<RectTransform>();
                lbRt.anchorMin = new Vector2(0.5f, 0.28f);
                lbRt.anchorMax = new Vector2(0.5f, 0.28f);
                lbRt.pivot = new Vector2(0.5f, 0.5f);
                lbRt.anchoredPosition = Vector2.zero;
                lbRt.sizeDelta = new Vector2(550f, 120f);

                Image lbImg = loadingBarT.GetComponent<Image>();
                Sprite emptyBarSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/Empty Bar.png");
                if (emptyBarSprite != null)
                {
                    lbImg.sprite = emptyBarSprite;
                }
                lbImg.preserveAspect = true;
                lbImg.color = Color.white;

                // Inner Juice Image (Image 1)
                Transform juiceT = loadingBarT.Find("Image");
                if (juiceT == null)
                {
                    GameObject juiceGo = new GameObject("Image", typeof(RectTransform), typeof(Image));
                    juiceGo.transform.SetParent(loadingBarT, false);
                    juiceT = juiceGo.transform;
                }
                RectTransform juiceRt = juiceT.GetComponent<RectTransform>();
                juiceRt.anchorMin = new Vector2(0.5f, 0.5f);
                juiceRt.anchorMax = new Vector2(0.5f, 0.5f);
                juiceRt.pivot = new Vector2(0.5f, 0.5f);
                juiceRt.anchoredPosition = Vector2.zero;
                juiceRt.sizeDelta = new Vector2(490f, 65f);

                Image juiceImg = juiceT.GetComponent<Image>();
                Sprite barSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/bar.png");
                if (barSprite != null)
                {
                    juiceImg.sprite = barSprite;
                }
                juiceImg.type = Image.Type.Filled;
                juiceImg.fillMethod = Image.FillMethod.Horizontal;
                juiceImg.fillOrigin = (int)Image.OriginHorizontal.Left;
                juiceImg.fillAmount = 0f;
                juiceImg.preserveAspect = false;
                juiceImg.color = Color.white;
            }

            // 5. MainMenuRoot Configuration (Matching Image 3)
            Transform menuRootT = canvas.transform.Find("MainMenuRoot");
            if (menuRootT != null)
            {
                // Top-Left Logo
                Transform titleT = menuRootT.Find("TitleArea");
                if (titleT == null)
                {
                    GameObject titleGo = new GameObject("TitleArea", typeof(RectTransform), typeof(Image));
                    titleGo.transform.SetParent(menuRootT, false);
                    titleT = titleGo.transform;
                }
                RectTransform titleRt = titleT.GetComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0f, 1f);
                titleRt.anchorMax = new Vector2(0f, 1f);
                titleRt.pivot = new Vector2(0f, 1f);
                titleRt.anchoredPosition = new Vector2(45f, -30f);
                titleRt.sizeDelta = new Vector2(520f, 120f);

                Image titleImg = titleT.GetComponent<Image>();
                Sprite fnLogo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/FruitNinja_Logo.png");
                if (fnLogo != null)
                {
                    titleImg.sprite = fnLogo;
                }
                titleImg.preserveAspect = true;
                titleImg.color = Color.white;

                // Top-Right Settings Button (Image 3)
                Transform trGroupT = menuRootT.Find("TopRightButtons");
                if (trGroupT != null)
                {
                    // Remove ChatBtn (green square)
                    Transform chatBtn = trGroupT.Find("ChatBtn");
                    if (chatBtn != null) Object.DestroyImmediate(chatBtn.gameObject);

                    RectTransform trRt = trGroupT.GetComponent<RectTransform>();
                    trRt.anchorMin = new Vector2(1f, 1f);
                    trRt.anchorMax = new Vector2(1f, 1f);
                    trRt.pivot = new Vector2(1f, 1f);
                    trRt.anchoredPosition = new Vector2(-45f, -30f);
                    trRt.sizeDelta = new Vector2(90f, 90f);

                    Transform settingsBtn = trGroupT.Find("SettingsBtn");
                    if (settingsBtn != null)
                    {
                        RectTransform sRt = settingsBtn.GetComponent<RectTransform>();
                        sRt.sizeDelta = new Vector2(90f, 90f);
                        Image sImg = settingsBtn.GetComponent<Image>();
                        Sprite setSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/Settings_Icon.png");
                        if (setSprite != null)
                        {
                            sImg.sprite = setSprite;
                        }
                        sImg.color = Color.white;
                        sImg.preserveAspect = true;
                    }
                }

                // Profile Button (Image 3 - wooden badge below ring)
                Transform profBtnT = menuRootT.Find("ProfileButton");
                if (profBtnT != null)
                {
                    // Clean text children
                    for (int i = profBtnT.childCount - 1; i >= 0; i--)
                    {
                        Object.DestroyImmediate(profBtnT.GetChild(i).gameObject);
                    }

                    RectTransform pRt = profBtnT.GetComponent<RectTransform>();
                    pRt.anchorMin = new Vector2(0.5f, 0.5f);
                    pRt.anchorMax = new Vector2(0.5f, 0.5f);
                    pRt.pivot = new Vector2(0.5f, 0.5f);
                    pRt.anchoredPosition = new Vector2(0f, -165f);
                    pRt.sizeDelta = new Vector2(300f, 75f);

                    Image pImg = profBtnT.GetComponent<Image>();
                    Sprite profSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/Profile_Button.png");
                    if (profSprite != null)
                    {
                        pImg.sprite = profSprite;
                    }
                    pImg.color = Color.white;
                    pImg.preserveAspect = true;
                }
            }

            // 6. MenuFruitsRoot Configuration (Matching Image 3)
            GameObject fruitsRoot = GameObject.Find("MenuFruitsRoot");
            if (fruitsRoot != null)
            {
                fruitsRoot.transform.position = Vector3.zero;

                // Disable LeftMode (Multiplayer) & RightMode (Watch) as in Image 3
                Transform leftNode = fruitsRoot.transform.Find("LeftMode");
                if (leftNode != null) leftNode.gameObject.SetActive(false);

                Transform rightNode = fruitsRoot.transform.Find("RightMode");
                if (rightNode != null) rightNode.gameObject.SetActive(false);

                // Configure CenterMode (Watermelon + Blue ORIGINAL Ring)
                Transform centerNode = fruitsRoot.transform.Find("CenterMode");
                if (centerNode != null)
                {
                    centerNode.gameObject.SetActive(true);
                    centerNode.localPosition = new Vector3(0f, 0.2f, 0f);

                    // Remove ugly 3D cylinders and 3D TextMesh
                    for (int i = centerNode.childCount - 1; i >= 0; i--)
                    {
                        Transform child = centerNode.GetChild(i);
                        if (child.name == "Ring" && child.GetComponent<MeshRenderer>() != null)
                            Object.DestroyImmediate(child.gameObject);
                        else if (child.name == "Inner")
                            Object.DestroyImmediate(child.gameObject);
                        else if (child.name == "Text" && child.GetComponent<TextMesh>() != null)
                            Object.DestroyImmediate(child.gameObject);
                    }

                    // WorldSpace Canvas for Glowing Blue Ring
                    Transform ringCanvasT = centerNode.Find("Canvas");
                    if (ringCanvasT == null)
                    {
                        GameObject rCanvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas));
                        rCanvasGo.transform.SetParent(centerNode, false);
                        ringCanvasT = rCanvasGo.transform;
                    }
                    Canvas rCanvas = ringCanvasT.GetComponent<Canvas>();
                    rCanvas.renderMode = RenderMode.WorldSpace;
                    ringCanvasT.localPosition = new Vector3(0f, 0f, 0.1f);
                    ringCanvasT.localScale = new Vector3(0.01f, 0.01f, 0.01f);

                    Transform ringT = ringCanvasT.Find("Ring");
                    if (ringT == null)
                    {
                        GameObject ringGo = new GameObject("Ring", typeof(RectTransform), typeof(Image));
                        ringGo.transform.SetParent(ringCanvasT, false);
                        ringT = ringGo.transform;
                    }
                    RectTransform ringRt = ringT.GetComponent<RectTransform>();
                    ringRt.sizeDelta = new Vector2(340f, 340f);
                    ringRt.anchoredPosition = Vector2.zero;
                    ringRt.pivot = new Vector2(0.5f, 0.5f);

                    Image ringImg = ringT.GetComponent<Image>();
                    Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/Blue_Ring_Original.png");
                    if (ringSprite != null)
                    {
                        ringImg.sprite = ringSprite;
                    }
                    ringImg.color = Color.white;
                    ringImg.preserveAspect = true;

                    // Configure Watermelon
                    Transform melonT = centerNode.Find("Watermelon(Clone)");
                    if (melonT == null)
                    {
                        GameObject melonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Watermelon.prefab");
                        if (melonPrefab != null)
                        {
                            GameObject melonGo = Object.Instantiate(melonPrefab, centerNode);
                            melonGo.name = "Watermelon(Clone)";
                            melonT = melonGo.transform;
                        }
                    }
                    if (melonT != null)
                    {
                        melonT.localPosition = Vector3.zero;
                        melonT.localScale = new Vector3(1.2f, 1.2f, 1.2f); // Perfect fit inside ring

                        MenuFruit mf = melonT.GetComponent<MenuFruit>();
                        if (mf == null) mf = melonT.gameObject.AddComponent<MenuFruit>();
                        mf.ring = ringT;
                        
                        Transform wholeT = melonT.Find("Whole");
                        if (wholeT != null) mf.wholeModel = wholeT.gameObject;
                        Transform slicedT = melonT.Find("Sliced");
                        if (slicedT != null) mf.slicedModel = slicedT.gameObject;

                        // Ensure SphereCollider for slicing
                        SphereCollider col = melonT.GetComponent<SphereCollider>();
                        if (col == null) col = melonT.gameObject.AddComponent<SphereCollider>();
                        col.isTrigger = true;
                        col.radius = 0.8f;
                    }
                }
            }

            // 7. Wire Up SplashManager
            SplashManager sm = Object.FindObjectOfType<SplashManager>();
            if (sm != null)
            {
                sm.loadingPanel = loadingPanelT.GetComponent<CanvasGroup>();
                sm.mainMenuPanel = menuRootT.GetComponent<CanvasGroup>();
                Transform juiceT = loadingPanelT.Find("Loading Bar/Image");
                if (juiceT != null) sm.loadingBarFill = juiceT.GetComponent<Image>();
                sm.loadingDuration = 5f;
                sm.fadeDuration = 0.5f;
                sm.menuFruitsRoot = fruitsRoot;
                sm.bladePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Blade.prefab");

                // Ensure CenterMode fruit slice event is wired to sm.OnPlayClicked
                Transform melonT = fruitsRoot.transform.Find("CenterMode/Watermelon(Clone)");
                if (melonT != null)
                {
                    MenuFruit mf = melonT.GetComponent<MenuFruit>();
                    if (mf != null)
                    {
                        if (mf.onSliced == null) mf.onSliced = new UnityEngine.Events.UnityEvent();
                        // Clear existing to avoid duplicate calls
                        while (mf.onSliced.GetPersistentEventCount() > 0)
                        {
                            UnityEditor.Events.UnityEventTools.RemovePersistentListener(mf.onSliced, 0);
                        }
                        UnityEditor.Events.UnityEventTools.AddPersistentListener(mf.onSliced, sm.OnPlayClicked);
                    }
                }

                EditorUtility.SetDirty(sm);
            }
        }

        EditorSceneManager.MarkSceneDirty(splashScene);
        EditorSceneManager.SaveScene(splashScene);
        AssetDatabase.SaveAssets();

        Debug.Log("=== Original Splash UI Restoration Successfully Completed and Saved to Disk! ===");
    }
}
