using System.Collections.Generic;
using UnityEngine;

public class GameModel
{
    public GridModel Grid { get; private set; }
    public int TotalMatches { get; private set; }
    public int CurrentMatches { get; set; }
    public int MovesLeft { get; set; }
    public int TotalMoves { get; private set; }
    
    public bool IsGameComplete => CurrentMatches >= TotalMatches;
    public bool IsGameLost => MovesLeft <= 0 && !IsGameComplete;

    public GameModel()
    {
    }

    public void Initialize(GridModel grid, int totalMoves)
    {
        Grid = grid;
        TotalMatches = grid.TotalCards / 2;
        CurrentMatches = 0;
        TotalMoves = totalMoves;
        MovesLeft = totalMoves;
    }

    public void IncrementMatches()
    {
        CurrentMatches++;
    }

    public void DecrementMoves()
    {
        MovesLeft--;
        if (MovesLeft < 0) MovesLeft = 0;
    }

    public void Reset()
    {
        CurrentMatches = 0;
        MovesLeft = TotalMoves;
    }
}
