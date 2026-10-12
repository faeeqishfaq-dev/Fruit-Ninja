using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ScoreHistoryManager
{
    private const string PREFS_KEY = "FN_ScoreHistory_Data";
    private static bool s_ScoreRecordedThisGame = false;

    [Serializable]
    public class ScoreData
    {
        public List<int> scores = new List<int>();
    }

    /// <summary>
    /// Resets the session flag when a new game starts so the final score can be recorded once upon game finish.
    /// </summary>
    public static void ResetGameSession()
    {
        s_ScoreRecordedThisGame = false;
    }

    /// <summary>
    /// Records the final score of a completed game session once.
    /// Preserves duplicate scores as separate records and never overwrites previous scores.
    /// </summary>
    public static void RecordScore(int score)
    {
        if (s_ScoreRecordedThisGame)
        {
            return; // Prevent duplicate recording for the same game session
        }
        s_ScoreRecordedThisGame = true;

        ScoreData data = LoadData();
        data.scores.Add(score);
        SaveData(data);

        Debug.Log($"[ScoreHistoryManager] Recorded final score: {score}. Total recorded games: {data.scores.Count}");
    }

    /// <summary>
    /// Returns all saved completed-game scores sorted in descending order (highest to lowest).
    /// Duplicate scores are preserved as separate records.
    /// </summary>
    public static List<int> GetSortedScores()
    {
        ScoreData data = LoadData();
        return data.scores.OrderByDescending(s => s).ToList();
    }

    /// <summary>
    /// Returns the raw list of scores in recorded order.
    /// </summary>
    public static List<int> GetAllScoresRaw()
    {
        ScoreData data = LoadData();
        return new List<int>(data.scores);
    }

    private static ScoreData LoadData()
    {
        ScoreData data = new ScoreData();
        string json = PlayerPrefs.GetString(PREFS_KEY, string.Empty);

        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                data = JsonUtility.FromJson<ScoreData>(json);
                if (data == null || data.scores == null)
                {
                    data = new ScoreData();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ScoreHistoryManager] Failed to deserialize score history: {ex.Message}");
                data = new ScoreData();
            }
        }

        // Secondary fallback to count-based keys if JSON is empty but count exists
        if (data.scores.Count == 0 && PlayerPrefs.HasKey("FN_ScoreHistory_Count"))
        {
            int count = PlayerPrefs.GetInt("FN_ScoreHistory_Count", 0);
            for (int i = 0; i < count; i++)
            {
                if (PlayerPrefs.HasKey($"FN_Score_{i}"))
                {
                    data.scores.Add(PlayerPrefs.GetInt($"FN_Score_{i}"));
                }
            }
        }

        return data;
    }

    private static void SaveData(ScoreData data)
    {
        if (data == null) return;

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PREFS_KEY, json);

        // Store secondary fallback count and individual keys
        PlayerPrefs.SetInt("FN_ScoreHistory_Count", data.scores.Count);
        for (int i = 0; i < data.scores.Count; i++)
        {
            PlayerPrefs.SetInt($"FN_Score_{i}", data.scores[i]);
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Clears score history (used for testing or data reset if requested).
    /// </summary>
    public static void ClearHistory()
    {
        PlayerPrefs.DeleteKey(PREFS_KEY);
        int count = PlayerPrefs.GetInt("FN_ScoreHistory_Count", 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey($"FN_Score_{i}");
        }
        PlayerPrefs.DeleteKey("FN_ScoreHistory_Count");
        PlayerPrefs.Save();
        s_ScoreRecordedThisGame = false;
    }
}
