using UnityEngine;

public class ScoringService : IScoringService
{
    private readonly GameConfig config;
    private readonly ScoreModel scoreModel;

    public System.Action<int> OnScoreUpdated { get; set; }
    public System.Action<int> OnComboUpdated { get; set; }
    public System.Action<int> OnMatchesUpdated { get; set; }

    public ScoreModel Score => scoreModel;

    public ScoringService(GameConfig gameConfig)
    {
        config = gameConfig;
        scoreModel = new ScoreModel();
    }

    public void OnMatch()
    {
        scoreModel.CurrentCombo++;
        if (scoreModel.CurrentCombo > scoreModel.MaxCombo)
            scoreModel.MaxCombo = scoreModel.CurrentCombo;

        float multiplier = 1f + (scoreModel.CurrentCombo - 1) * (config.ComboMultiplier - 1f);
        int points = Mathf.RoundToInt(config.BaseMatchScore * multiplier);
        
        scoreModel.CurrentScore += points;
        scoreModel.Matches++;

        OnComboUpdated?.Invoke(scoreModel.CurrentCombo);
        OnScoreUpdated?.Invoke(scoreModel.CurrentScore);
        OnMatchesUpdated?.Invoke(scoreModel.Matches);
    }

    public void OnMismatch()
    {
        scoreModel.CurrentCombo = 0;
        scoreModel.CurrentScore -= config.MismatchPenalty;
        if (scoreModel.CurrentScore < 0)
            scoreModel.CurrentScore = 0;

        OnComboUpdated?.Invoke(scoreModel.CurrentCombo);
        OnScoreUpdated?.Invoke(scoreModel.CurrentScore);
    }

    public void Reset()
    {
        scoreModel.Reset();
        OnComboUpdated?.Invoke(scoreModel.CurrentCombo);
        OnScoreUpdated?.Invoke(scoreModel.CurrentScore);
        OnMatchesUpdated?.Invoke(scoreModel.Matches);
    }
}
