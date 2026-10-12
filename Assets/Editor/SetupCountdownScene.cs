using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SetupCountdownScene
{
    [MenuItem("Tools/Setup Countdown in FruitNinja")]
    public static void RunSetup()
    {
        string currentScenePath = EditorSceneManager.GetActiveScene().path;
        string scenePath = "Assets/Scenes/FruitNinja.unity";
        
        if (currentScenePath == scenePath) currentScenePath = null; // No need to restore if we're already there
        
        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // Find or create CountdownManager
        GameObject cmObj = GameObject.Find("CountdownManager");
        if (cmObj == null)
        {
            cmObj = new GameObject("CountdownManager");
        }
        
        CountdownManager cm = cmObj.GetComponent<CountdownManager>();
        if (cm == null) cm = cmObj.AddComponent<CountdownManager>();

        // Find Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in FruitNinja scene!");
            return;
        }

        // Create Panel
        Transform panel = canvas.transform.Find("CountdownPanel");
        if (panel == null)
        {
            GameObject panelObj = new GameObject("CountdownPanel", typeof(RectTransform));
            panelObj.transform.SetParent(canvas.transform, false);
            panel = panelObj.transform;
            
            RectTransform rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // Create Image
        Transform imageT = panel.Find("CountdownImage");
        if (imageT == null)
        {
            GameObject imgObj = new GameObject("CountdownImage", typeof(RectTransform), typeof(Image));
            imgObj.transform.SetParent(panel, false);
            imageT = imgObj.transform;
            
            RectTransform rt = imageT.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            Image img = imgObj.GetComponent<Image>();
            img.preserveAspect = true;
        }

        // Assign references
        cm.CountdownPanel = panel.gameObject;
        cm.CountdownImage = imageT.GetComponent<Image>();

        Spawner spawner = Object.FindObjectOfType<Spawner>();
        if (spawner != null) cm.spawner = spawner;

        // Ensure rendering order is high so it's above other UI
        panel.SetAsLastSibling();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Countdown system added and saved to FruitNinja scene.");
        
        // Restore original scene
        if (!string.IsNullOrEmpty(currentScenePath))
        {
            EditorSceneManager.OpenScene(currentScenePath);
        }
    }
}
