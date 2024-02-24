using UnityEngine;

public static class ScoreRecorder
{
    private static int HighScore
    {
        get => PlayerPrefs.GetInt("HighScore", 0);
        set
        {
            if (value > PlayerPrefs.GetInt("HighScore", 0))
                PlayerPrefs.SetInt("HighScore", value);
        }
    }

    public static bool IsNewHighScore(int score) => HighScore < score;

    public static bool TrySetNewHighScore(int score)
    {
        if (!IsNewHighScore(score))
            return false;

        HighScore = score;
        return true;
    }
}