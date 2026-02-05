using System.Collections.Generic;
using UnityEngine;

public class GameModel
{
    public GridModel Grid { get; private set; }
    public int TotalMatches { get; private set; }
    public int CurrentMatches { get; private set; }
    public bool IsGameComplete => CurrentMatches >= TotalMatches;

    public GameModel(GridModel grid)
    {
        Grid = grid;
        TotalMatches = grid.TotalCards / 2;
        CurrentMatches = 0;
    }

    public void IncrementMatches()
    {
        CurrentMatches++;
    }

    public void Reset()
    {
        CurrentMatches = 0;
    }
}
