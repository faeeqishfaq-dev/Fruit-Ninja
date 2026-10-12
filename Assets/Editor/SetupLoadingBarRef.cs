using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SetupLoadingBarRef
{
    [MenuItem("Tools/Setup Loading Bar Reference")]
    public static void RunSetup()
    {
        string currentScene = EditorSceneManager.GetActiveScene().path;
        string splashPath = "Assets/Scenes/Splash.unity";

        Scene scene = EditorSceneManager.OpenScene(splashPath, OpenSceneMode.Single);

        SplashManager sm = Object.FindObjectOfType<SplashManager>();
        if (sm != null)
        {
            // Find the image
            Transform loadingBar = sm.loadingPanel.transform.Find("Loading Bar");
            if (loadingBar != null)
            {
                Transform imageT = loadingBar.Find("Image");
                if (imageT != null)
                {
                    sm.loadingBarFill = imageT.GetComponent<Image>();
                    Debug.Log("Assigned Loading Bar Fill Image in SplashManager.");
                }
            }

            sm.loadingDuration = 5f;
            sm.fadeDuration = 0.5f;

            EditorUtility.SetDirty(sm);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(currentScene) && currentScene != splashPath)
        {
            EditorSceneManager.OpenScene(currentScene, OpenSceneMode.Single);
        }
    }
}
