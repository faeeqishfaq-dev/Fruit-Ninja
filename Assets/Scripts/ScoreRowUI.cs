using UnityEngine;
using UnityEngine.UI;

public class ScoreRowUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Text rankText;
    public Text medalText;
    public Text scoreText;
    public Image backgroundImage;

    public void Setup(int rank, int score)
    {
        if (rankText != null)
        {
            rankText.text = $"RANK {rank}";
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        // Apply Gold, Silver, Bronze styling for Top 3, and standard wood styling for others
        if (rank == 1)
        {
            if (medalText != null) medalText.text = "🥇";
            if (rankText != null) rankText.color = new Color32(255, 215, 0, 255); // Gold
            if (scoreText != null) scoreText.color = new Color32(255, 235, 120, 255);
            if (backgroundImage != null) backgroundImage.color = new Color(0.85f, 0.65f, 0.15f, 0.35f);
        }
        else if (rank == 2)
        {
            if (medalText != null) medalText.text = "🥈";
            if (rankText != null) rankText.color = new Color32(220, 230, 245, 255); // Silver
            if (scoreText != null) scoreText.color = new Color32(240, 245, 255, 255);
            if (backgroundImage != null) backgroundImage.color = new Color(0.65f, 0.72f, 0.8f, 0.28f);
        }
        else if (rank == 3)
        {
            if (medalText != null) medalText.text = "🥉";
            if (rankText != null) rankText.color = new Color32(235, 150, 80, 255); // Bronze
            if (scoreText != null) scoreText.color = new Color32(245, 175, 115, 255);
            if (backgroundImage != null) backgroundImage.color = new Color(0.75f, 0.45f, 0.2f, 0.28f);
        }
        else
        {
            if (medalText != null) medalText.text = "";
            if (rankText != null) rankText.color = new Color32(245, 235, 210, 220); // Warm cream
            if (scoreText != null) scoreText.color = new Color32(255, 255, 255, 230); // Crisp white
            if (backgroundImage != null) backgroundImage.color = new Color(0.12f, 0.08f, 0.04f, 0.3f); // Subtle dark wood
        }
    }
}
