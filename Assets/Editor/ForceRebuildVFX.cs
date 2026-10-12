using UnityEngine;
using UnityEditor;

public class ForceRebuildVFX
{
    [MenuItem("Tools/Force Rebuild VFX")]
    public static void ManualRebuild()
    {
        var manager = Object.FindObjectOfType<BombExplosionVFXManager>();
        if (manager != null)
        {
            if (manager.burstCanvas != null)
            {
                Object.DestroyImmediate(manager.burstCanvas.gameObject);
                manager.burstCanvas = null;
                manager.centerFlash = null;
                manager.raysContainer = null;
                manager.fullScreenWhite = null;
            }
        }
    }
}
