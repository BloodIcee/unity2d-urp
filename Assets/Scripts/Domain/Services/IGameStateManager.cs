using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public interface IGameStateManager
{
    UniTask<RestoreResult> TryRestoreGameAsync(
        GridModel gridModel,
        GameModel gameModel,
        IScoringService scoringService,
        ScoreView scoreView,
        GridView gridView,
        GameConfig gameConfig);
    
    UniTask SaveGameStateAsync(GameStateSnapshot snapshot);
    
    GameStateSnapshot CreateSnapshot(
        List<CardView> activeCards,
        GameModel gameModel,
        IScoringService scoringService,
        GridModel gridModel);
}

public struct RestoreResult
{
    public bool Success;
    public GameStateSnapshot Snapshot;
}

public struct GameStateSnapshot
{
    public int CurrentScore;
    public int CurrentCombo;
    public int TotalMatches;
    public int MovesLeft;
    public int GridRows;
    public int GridColumns;
    public CardSnapshotData[] Cards;
}

public struct CardSnapshotData
{
    public int Id;
    public int PairId;
    public int SpriteIndex;
    public bool IsMatched;
    public Sprite Sprite;
}
