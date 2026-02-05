public class ScoreModel
{
    public int CurrentScore { get; set; }
    public int CurrentCombo { get; set; }
    public int Matches { get; set; }
    public int MaxCombo { get; set; }
    
    public void Reset()
    {
        CurrentScore = 0;
        CurrentCombo = 0;
        Matches = 0;
        MaxCombo = 0;
    }
}
