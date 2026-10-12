using UnityEditor;
using UnityEngine;

public class SetupHealthManager
{
    [MenuItem("Tools/Setup Health Manager")]
    public static void Initialize()
    {
        if (Application.isPlaying) return;

        HealthManager manager = Object.FindObjectOfType<HealthManager>();
        if (manager == null)
        {
            GameObject go = new GameObject("HealthManager");
            go.AddComponent<HealthManager>();
            Debug.Log("Created HealthManager in the scene.");
        }
    }
}
