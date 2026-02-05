using System;

[Serializable]
public class GameSaveData
{
    public int CurrentScore;
    public int CurrentCombo;
    public int TotalMatches;
    public int MovesLeft;
    public int GridRows;
    public int GridColumns;
    public CardSaveData[] Cards;
    public long SaveTimestamp;
    public bool HasActiveGame;

    public GameSaveData()
    {
        HasActiveGame = false;
        Cards = new CardSaveData[0];
    }
}
