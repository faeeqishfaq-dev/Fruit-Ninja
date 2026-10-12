using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SetupBestScoreUI
{
    [MenuItem("Tools/Setup Best Score UI")]
    public static void Initialize()
    {
        if (Application.isPlaying) return;

        GameManager gm = Object.FindObjectOfType<GameManager>();
        if (gm == null) return;

        // Use SerializedObject to read/write private serialized fields safely
        SerializedObject so = new SerializedObject(gm);
        SerializedProperty scoreTextProp = so.FindProperty("scoreText");
        SerializedProperty bestScoreTextProp = so.FindProperty("bestScoreText");

        if (scoreTextProp.objectReferenceValue != null && bestScoreTextProp.objectReferenceValue == null)
        {
            Text scoreText = (Text)scoreTextProp.objectReferenceValue;
            
            // Check if it already exists as a sibling
            Transform bestScoreTransform = scoreText.transform.parent.Find("BestScoreText");
            if (bestScoreTransform == null)
            {
                GameObject bestScoreGo = new GameObject("BestScoreText");
                bestScoreTransform = bestScoreGo.transform;
                bestScoreTransform.SetParent(scoreText.transform.parent, false);
                
                RectTransform scoreRt = scoreText.GetComponent<RectTransform>();
                RectTransform bestRt = bestScoreGo.AddComponent<RectTransform>();
                
                // Copy anchors and pivot
                bestRt.anchorMin = scoreRt.anchorMin;
                bestRt.anchorMax = scoreRt.anchorMax;
                bestRt.pivot = scoreRt.pivot;
                
                // Position it directly underneath
                bestRt.anchoredPosition = new Vector2(scoreRt.anchoredPosition.x, scoreRt.anchoredPosition.y - scoreRt.sizeDelta.y - 10f); // 10f gap
                bestRt.sizeDelta = new Vector2(scoreRt.sizeDelta.x + 50f, scoreRt.sizeDelta.y * 0.8f);

                Text bestText = bestScoreGo.AddComponent<Text>();
                bestText.font = scoreText.font;
                bestText.fontSize = Mathf.RoundToInt(scoreText.fontSize * 0.65f); // Smaller font
                bestText.color = new Color(1f, 0.8f, 0.2f, 1f); // Yellowish orange
                bestText.alignment = scoreText.alignment;
                bestText.text = "BEST: 0";
                
                // Add outline for better visibility similar to Fruit Ninja style
                Outline outline = bestScoreGo.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(2f, -2f);
                
                Debug.Log("Created BestScoreText UI object.");
            }

            bestScoreTextProp.objectReferenceValue = bestScoreTransform.GetComponent<Text>();
            so.ApplyModifiedProperties();
            Debug.Log("Assigned BestScoreText to GameManager.");
        }
    }
}
