using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class RecoverAndSaveScenes
{
    [MenuItem("Tools/Recover and Permanently Save Scenes")]
    public static void ExecuteRecovery()
    {
        Debug.Log("=== Starting Scene Recovery and Permanent Save ===");
        
        // ----------------------------------------------------
        // 1. RECOVER SPLASH SCENE
        // ----------------------------------------------------
        string splashPath = "Assets/Scenes/Splash.unity";
        Scene splashScene = EditorSceneManager.OpenScene(splashPath, OpenSceneMode.Single);
        
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            Transform loadingPanelT = canvas.transform.Find("LoadingPanel");
            if (loadingPanelT != null)
            {
                // Ensure LoadingPanel is active
                loadingPanelT.gameObject.SetActive(true);

                // Remove old primitive Text if present
                Transform oldTextT = loadingPanelT.Find("Text");
                if (oldTextT != null)
                {
                    Object.DestroyImmediate(oldTextT.gameObject);
                }

                // 1.1 Logo
                Transform logoT = loadingPanelT.Find("Logo");
                if (logoT == null)
                {
                    GameObject logoGo = new GameObject("Logo", typeof(RectTransform), typeof(Image));
                    logoGo.transform.SetParent(loadingPanelT, false);
                    logoT = logoGo.transform;
                }
                RectTransform logoRt = logoT.GetComponent<RectTransform>();
                logoRt.anchorMin = new Vector2(0.5f, 0.7f);
                logoRt.anchorMax = new Vector2(0.5f, 0.7f);
                logoRt.pivot = new Vector2(0.5f, 0.5f);
                logoRt.anchoredPosition = new Vector2(0f, 0f);
                logoRt.sizeDelta = new Vector2(600f, 300f);

                Image logoImg = logoT.GetComponent<Image>();
                Sprite logoSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Fruit_Ninja_icon.png");
                if (logoSprite != null)
                {
                    logoImg.sprite = logoSprite;
                    logoImg.preserveAspect = true;
                }

                // 1.2 Loading Bar Frame
                Transform loadingBarT = loadingPanelT.Find("Loading Bar");
                if (loadingBarT == null)
                {
                    loadingBarT = loadingPanelT.Find("LoadingBar");
                }
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
                lbRt.anchorMin = new Vector2(0.5f, 0.25f);
                lbRt.anchorMax = new Vector2(0.5f, 0.25f);
                lbRt.pivot = new Vector2(0.5f, 0.5f);
                lbRt.anchoredPosition = new Vector2(0f, 0f);
                lbRt.sizeDelta = new Vector2(600f, 200f);

                Image lbImg = loadingBarT.GetComponent<Image>();
                Sprite lbSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/Loading Bar.png");
                if (lbSprite == null)
                {
                    Object[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/SplashUI/3dda7e17-bbea-4072-be04-4ed17d14e54a.png");
                    foreach (var s in sprites)
                    {
                        if (s is Sprite sp && sp.name.EndsWith("_3"))
                        {
                            lbSprite = sp;
                            break;
                        }
                    }
                }
                if (lbSprite != null)
                {
                    lbImg.sprite = lbSprite;
                    lbImg.preserveAspect = true;
                }

                // 1.3 Inner Juice Image
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
                juiceRt.anchoredPosition = new Vector2(-2f, 2f);
                juiceRt.sizeDelta = new Vector2(475.4f, 150f);
                juiceRt.localScale = new Vector3(1.07f, 0.97f, 0.97f);

                Image juiceImg = juiceT.GetComponent<Image>();
                Sprite juiceSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SplashUI/bar.png");
                if (juiceSprite != null)
                {
                    juiceImg.sprite = juiceSprite;
                    juiceImg.type = Image.Type.Filled;
                    juiceImg.fillMethod = Image.FillMethod.Horizontal;
                    juiceImg.fillOrigin = (int)Image.OriginHorizontal.Left;
                    juiceImg.fillAmount = 0f;
                    juiceImg.preserveAspect = true;
                }

                // 1.4 Wire up SplashManager
                SplashManager sm = Object.FindObjectOfType<SplashManager>();
                if (sm != null)
                {
                    sm.loadingPanel = loadingPanelT.GetComponent<CanvasGroup>();
                    Transform menuRoot = canvas.transform.Find("MainMenuRoot");
                    if (menuRoot != null)
                    {
                        sm.mainMenuPanel = menuRoot.GetComponent<CanvasGroup>();
                    }
                    sm.loadingBarFill = juiceImg;
                    sm.loadingDuration = 5f;
                    sm.fadeDuration = 0.5f;

                    GameObject fruitsRoot = GameObject.Find("MenuFruitsRoot");
                    if (fruitsRoot != null)
                    {
                        sm.menuFruitsRoot = fruitsRoot;
                    }

                    EditorUtility.SetDirty(sm);
                }
            }

            // 1.5 MainMenuRoot TitleArea Logo
            Transform titleAreaT = canvas.transform.Find("MainMenuRoot/TitleArea");
            if (titleAreaT != null)
            {
                Image titleImg = titleAreaT.GetComponent<Image>();
                if (titleImg != null && titleImg.sprite == null)
                {
                    Sprite logoSp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Fruit_Ninja_icon.png");
                    if (logoSp != null)
                    {
                        titleImg.sprite = logoSp;
                        titleImg.preserveAspect = true;
                        EditorUtility.SetDirty(titleImg);
                    }
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(splashScene);
        EditorSceneManager.SaveScene(splashScene);
        Debug.Log("PERMANENT SAVE: Splash.unity successfully written and serialized to disk!");

        // ----------------------------------------------------
        // 2. VALIDATE AND PERMANENTLY SAVE FRUITNINJA SCENE
        // ----------------------------------------------------
        string fnPath = "Assets/Scenes/FruitNinja.unity";
        Scene fnScene = EditorSceneManager.OpenScene(fnPath, OpenSceneMode.Single);

        GameOverUIManager goMgr = Object.FindObjectOfType<GameOverUIManager>();
        CountdownManager cdMgr = Object.FindObjectOfType<CountdownManager>();
        GameManager gm = Object.FindObjectOfType<GameManager>();
        HealthManager hm = Object.FindObjectOfType<HealthManager>();

        if (goMgr != null) EditorUtility.SetDirty(goMgr);
        if (cdMgr != null) EditorUtility.SetDirty(cdMgr);
        if (gm != null) EditorUtility.SetDirty(gm);
        if (hm != null) EditorUtility.SetDirty(hm);

        EditorSceneManager.MarkSceneDirty(fnScene);
        EditorSceneManager.SaveScene(fnScene);
        Debug.Log("PERMANENT SAVE: FruitNinja.unity successfully written and serialized to disk!");

        // ----------------------------------------------------
        // 3. PERSIST ALL PROJECT ASSETS
        // ----------------------------------------------------
        AssetDatabase.SaveAssets();

        // Restore Splash scene as active
        EditorSceneManager.OpenScene(splashPath, OpenSceneMode.Single);
        Debug.Log("=== RECOVERY COMPLETE: All UI Objects Permanently Saved in Edit Mode! ===");
    }
}
