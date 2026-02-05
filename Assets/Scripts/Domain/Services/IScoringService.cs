public interface IScoringService
{
    System.Action<int> OnScoreUpdated { get; set; }
    System.Action<int> OnComboUpdated { get; set; }
    System.Action<int> OnMatchesUpdated { get; set; }
    ScoreModel Score { get; }
    void OnMatch();
    void OnMismatch();
    void Reset();
    void ResetCombo();
}
