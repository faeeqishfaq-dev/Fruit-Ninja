using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.IO;

public class SetupSplashScene
{
    [MenuItem("Tools/Setup Splash Scene manually")]
    public static void RunSetupManually()
    {
        RunSetup();
    }

    static void RunSetup()
    {
        if (SessionState.GetBool("SplashSetupDone", false)) return;

        string scenePath = "Assets/Scenes/Splash.unity";
        
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        
        SetupScene(scene);
        
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Splash scene successfully setup!");
        
        SessionState.SetBool("SplashSetupDone", true);
    }

    static void SetupScene(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        foreach (var r in roots)
        {
            if (r.name != "Main Camera" && r.name != "Directional Light")
                GameObject.DestroyImmediate(r);
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            camGO.tag = "MainCamera";
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = false;
        cam.transform.position = new Vector3(0, 0, -10);

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = 50f; // Push canvas back so 3D objects render in front
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject bgGO = new GameObject("Background");
        bgGO.transform.SetParent(canvasGO.transform, false);
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.rectTransform.anchorMin = Vector2.zero;
        bgImg.rectTransform.anchorMax = Vector2.one;
        bgImg.rectTransform.sizeDelta = Vector2.zero;
        
        Texture2D bgTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/SplashUI/wooden_background.jpg");
        if (bgTex != null)
        {
            bgImg.sprite = Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), new Vector2(0.5f, 0.5f));
            bgImg.color = new Color(0.6f, 0.6f, 0.6f);
        }

        GameObject loadingGO = new GameObject("LoadingPanel");
        loadingGO.transform.SetParent(canvasGO.transform, false);
        RectTransform loadRT = loadingGO.AddComponent<RectTransform>();
        loadRT.anchorMin = Vector2.zero;
        loadRT.anchorMax = Vector2.one;
        loadRT.sizeDelta = Vector2.zero;
        Image loadImg = loadingGO.AddComponent<Image>();
        loadImg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        CanvasGroup loadCG = loadingGO.AddComponent<CanvasGroup>();
        
        GameObject loadingTextGO = new GameObject("Text");
        loadingTextGO.transform.SetParent(loadingGO.transform, false);
        Text loadText = loadingTextGO.AddComponent<Text>();
        loadText.text = "LOADING...";
        loadText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loadText.fontSize = 60;
        loadText.alignment = TextAnchor.MiddleCenter;
        loadText.color = Color.white;
        loadText.rectTransform.anchoredPosition = Vector2.zero;

        GameObject menuGO = new GameObject("MainMenuRoot");
        menuGO.transform.SetParent(canvasGO.transform, false);
        RectTransform menuRT = menuGO.AddComponent<RectTransform>();
        menuRT.anchorMin = Vector2.zero;
        menuRT.anchorMax = Vector2.one;
        menuRT.sizeDelta = Vector2.zero;
        CanvasGroup menuCG = menuGO.AddComponent<CanvasGroup>();

        GameObject titleGO = new GameObject("TitleArea");
        titleGO.transform.SetParent(menuGO.transform, false);
        Image titleImg = titleGO.AddComponent<Image>();
        titleImg.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        titleImg.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        titleImg.rectTransform.pivot = new Vector2(0.5f, 1f);
        titleImg.rectTransform.anchoredPosition = new Vector2(0, -100);
        titleImg.rectTransform.sizeDelta = new Vector2(800, 300);
        titleImg.preserveAspect = true;
        
        Texture2D titleTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/SplashUI/title_logo.jpg");
        if (titleTex != null)
        {
            titleImg.sprite = Sprite.Create(titleTex, new Rect(0, 0, titleTex.width, titleTex.height), new Vector2(0.5f, 0.5f));
        }

        GameObject profileBtn = new GameObject("ProfileButton");
        profileBtn.transform.SetParent(menuGO.transform, false);
        Image profileImg = profileBtn.AddComponent<Image>();
        profileImg.rectTransform.anchorMin = new Vector2(0.5f, 0f);
        profileImg.rectTransform.anchorMax = new Vector2(0.5f, 0f);
        profileImg.rectTransform.pivot = new Vector2(0.5f, 0f);
        profileImg.rectTransform.anchoredPosition = new Vector2(0, 200);
        profileImg.rectTransform.sizeDelta = new Vector2(300, 100);
        profileImg.color = new Color(0.4f, 0.2f, 0.1f);
        Button pBtn = profileBtn.AddComponent<Button>();
        
        GameObject pTextGO = new GameObject("Text");
        pTextGO.transform.SetParent(profileBtn.transform, false);
        Text pText = pTextGO.AddComponent<Text>();
        pText.text = "PROFILE";
        pText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        pText.fontSize = 40;
        pText.alignment = TextAnchor.MiddleCenter;
        pText.color = Color.white;

        GameObject trGroup = new GameObject("TopRightButtons");
        trGroup.transform.SetParent(menuGO.transform, false);
        RectTransform trRT = trGroup.AddComponent<RectTransform>();
        trRT.anchorMin = new Vector2(1, 1);
        trRT.anchorMax = new Vector2(1, 1);
        trRT.pivot = new Vector2(1, 1);
        trRT.anchoredPosition = new Vector2(-50, -50);
        trRT.sizeDelta = new Vector2(250, 100);
        HorizontalLayoutGroup hlg = trGroup.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childAlignment = TextAnchor.MiddleRight;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        GameObject btn1 = CreateIconButton(trGroup.transform, "SettingsBtn", Color.red);
        GameObject btn2 = CreateIconButton(trGroup.transform, "ChatBtn", Color.green);

        GameObject fruitsRoot = new GameObject("MenuFruitsRoot");

        // Use paths for actual prefabs
        GameObject applePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Apple.prefab");
        GameObject watermelonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Watermelon.prefab");
        GameObject orangePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Orange.prefab");

        CreateMenuFruitNode(fruitsRoot.transform, "LeftMode", new Vector3(-3.5f, -1f, 0), new Color(1, 0.2f, 0), "MULTIPLAYER", applePrefab);
        CreateMenuFruitNode(fruitsRoot.transform, "CenterMode", new Vector3(0, 0, 0), new Color(0, 0.5f, 1), "ORIGINAL", watermelonPrefab);
        CreateMenuFruitNode(fruitsRoot.transform, "RightMode", new Vector3(3.5f, -1f, 0), new Color(1, 0.8f, 0), "WATCH", orangePrefab);

        GameObject managerGO = new GameObject("SplashManager");
        SplashManager sm = managerGO.AddComponent<SplashManager>();
        sm.loadingPanel = loadCG;
        sm.mainMenuPanel = menuCG;
        sm.menuFruitsRoot = fruitsRoot;
        
        GameObject bladePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Blade.prefab");
        sm.bladePrefab = bladePrefab;

        Transform leftNode = fruitsRoot.transform.Find("LeftMode");
        if (leftNode != null)
        {
            var mf = leftNode.GetComponent<MenuFruit>();
            if (mf.onSliced == null) mf.onSliced = new UnityEngine.Events.UnityEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(mf.onSliced, sm.OnMultiplayerClicked);
        }
            
        Transform centerNode = fruitsRoot.transform.Find("CenterMode");
        if (centerNode != null)
        {
            var mf = centerNode.GetComponent<MenuFruit>();
            if (mf.onSliced == null) mf.onSliced = new UnityEngine.Events.UnityEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(mf.onSliced, sm.OnPlayClicked);
        }
            
        Transform rightNode = fruitsRoot.transform.Find("RightMode");
        if (rightNode != null)
        {
            var mf = rightNode.GetComponent<MenuFruit>();
            if (mf.onSliced == null) mf.onSliced = new UnityEngine.Events.UnityEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(mf.onSliced, sm.OnWatchClicked);
        }
            
        // Setup Directional Light for 3D fruits
        GameObject lightGO = new GameObject("Directional Light");
        Light l = lightGO.AddComponent<Light>();
        l.type = LightType.Directional;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);
    }

    static GameObject CreateIconButton(Transform parent, string name, Color color)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.rectTransform.sizeDelta = new Vector2(80, 80);
        img.color = color;
        go.AddComponent<Button>();
        return go;
    }

    static void CreateMenuFruitNode(Transform parent, string name, Vector3 pos, Color ringColor, string labelText, GameObject fruitPrefab)
    {
        GameObject node = new GameObject(name);
        node.transform.SetParent(parent);
        node.transform.position = pos;

        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Ring";
        ring.transform.SetParent(node.transform);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localRotation = Quaternion.Euler(90, 0, 0);
        ring.transform.localScale = new Vector3(3f, 0.05f, 3f);
        Object.DestroyImmediate(ring.GetComponent<Collider>());
        
        Material ringMat = new Material(Shader.Find("Unlit/Color"));
        ringMat.color = ringColor;
        ring.GetComponent<Renderer>().material = ringMat;

        GameObject inner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        inner.name = "Inner";
        inner.transform.SetParent(ring.transform);
        inner.transform.localPosition = new Vector3(0, 0.01f, 0);
        inner.transform.localScale = new Vector3(0.85f, 1.0f, 0.85f);
        Object.DestroyImmediate(inner.GetComponent<Collider>());
        
        Material innerMat = new Material(Shader.Find("Unlit/Color"));
        innerMat.color = new Color(0.1f, 0.05f, 0.02f); // wood dark
        inner.GetComponent<Renderer>().material = innerMat;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(node.transform);
        textGO.transform.localPosition = new Vector3(0, 2f, 0);
        TextMesh tm = textGO.AddComponent<TextMesh>();
        tm.text = labelText;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 50;
        tm.characterSize = 0.1f;
        tm.color = Color.white;
        tm.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textGO.GetComponent<MeshRenderer>().material = tm.font.material;

        GameObject fruitModel = null;
        if (fruitPrefab != null)
        {
            fruitModel = Object.Instantiate(fruitPrefab);
            fruitModel.transform.SetParent(node.transform);
            fruitModel.transform.localPosition = Vector3.zero;
            
            Fruit fLogic = fruitModel.GetComponent<Fruit>();
            if (fLogic != null) Object.DestroyImmediate(fLogic);
            Rigidbody rb = fruitModel.GetComponent<Rigidbody>();
            if (rb != null) Object.DestroyImmediate(rb);
        }
        else
        {
            fruitModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fruitModel.transform.SetParent(node.transform);
            fruitModel.transform.localPosition = Vector3.zero;
            Material fMat = new Material(Shader.Find("Standard"));
            fMat.color = Color.green;
            fruitModel.GetComponent<Renderer>().material = fMat;
        }

        fruitModel.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        SphereCollider sc = node.AddComponent<SphereCollider>();
        sc.radius = 1.5f;
        sc.isTrigger = true;

        MenuFruit mf = node.AddComponent<MenuFruit>();
        mf.wholeModel = fruitModel;
        
        if (fruitModel != null)
        {
            Transform whole = fruitModel.transform.Find("Whole");
            Transform sliced = fruitModel.transform.Find("Sliced");
            if (whole != null) mf.wholeModel = whole.gameObject;
            if (sliced != null) mf.slicedModel = sliced.gameObject;
        }
    }
}
