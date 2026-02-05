using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : IGameStateManager
{
    private readonly ISaveLoadService saveLoadService;
    private readonly GameConfig gameConfig;

    public GameStateManager(ISaveLoadService saveService, GameConfig config)
    {
        saveLoadService = saveService;
        gameConfig = config;
    }

    public async UniTask<RestoreResult> TryRestoreGameAsync(
        GridModel gridModel,
        GameModel gameModel,
        IScoringService scoringService,
        ScoreView scoreView,
        GridView gridView,
        GameConfig config)
    {
        if (!saveLoadService.HasSaveData())
            return new RestoreResult { Success = false };

        GameSaveData data = await saveLoadService.LoadGameStateAsync();
        
        if (!ValidateSaveData(data))
            return new RestoreResult { Success = false };

        GameStateSnapshot snapshot = ConvertToSnapshot(data);
        return new RestoreResult { Success = true, Snapshot = snapshot };
    }

    public async UniTask SaveGameStateAsync(GameStateSnapshot snapshot)
    {
        GameSaveData data = ConvertToSaveData(snapshot);
        await saveLoadService.SaveGameStateAsync(data);
    }

    public GameStateSnapshot CreateSnapshot(
        List<CardView> activeCards,
        GameModel gameModel,
        IScoringService scoringService,
        GridModel gridModel)
    {
        var cards = new CardSnapshotData[activeCards.Count];
        
        for (int i = 0; i < activeCards.Count; i++)
        {
            CardView cardView = activeCards[i];
            CardModel model = cardView.Model;
            
            cards[i] = new CardSnapshotData
            {
                Id = model.Id,
                PairId = model.PairId,
                SpriteIndex = GetSpriteIndex(model.FrontSprite),
                IsMatched = model.CurrentState == CardState.Matched,
                Sprite = model.FrontSprite
            };
        }

        return new GameStateSnapshot
        {
            CurrentScore = scoringService.Score.CurrentScore,
            CurrentCombo = scoringService.Score.CurrentCombo,
            TotalMatches = scoringService.Score.Matches,
            MovesLeft = gameModel.MovesLeft,
            GridRows = gridModel.Rows,
            GridColumns = gridModel.Columns,
            Cards = cards
        };
    }

    private bool ValidateSaveData(GameSaveData data)
    {
        if (data.Cards == null || data.Cards.Length == 0)
            return false;
        
        if (data.GridRows <= 0 || data.GridColumns <= 0)
            return false;
        
        if (data.Cards.Length != data.GridRows * data.GridColumns)
            return false;
        
        return data.HasActiveGame;
    }

    private GameStateSnapshot ConvertToSnapshot(GameSaveData data)
    {
        var cards = new CardSnapshotData[data.Cards.Length];
        
        for (int i = 0; i < data.Cards.Length; i++)
        {
            CardSaveData cardData = data.Cards[i];
            Sprite sprite = GetSpriteByIndex(cardData.SpriteIndex);
            
            cards[i] = new CardSnapshotData
            {
                Id = cardData.Id,
                PairId = cardData.PairId,
                SpriteIndex = cardData.SpriteIndex,
                IsMatched = cardData.IsMatched,
                Sprite = sprite
            };
        }

        return new GameStateSnapshot
        {
            CurrentScore = data.CurrentScore,
            CurrentCombo = data.CurrentCombo,
            TotalMatches = data.TotalMatches,
            MovesLeft = data.MovesLeft,
            GridRows = data.GridRows,
            GridColumns = data.GridColumns,
            Cards = cards
        };
    }

    private GameSaveData ConvertToSaveData(GameStateSnapshot snapshot)
    {
        var cards = new CardSaveData[snapshot.Cards.Length];
        
        for (int i = 0; i < snapshot.Cards.Length; i++)
        {
            CardSnapshotData card = snapshot.Cards[i];
            cards[i] = new CardSaveData
            {
                Id = card.Id,
                PairId = card.PairId,
                SpriteIndex = card.SpriteIndex,
                IsMatched = card.IsMatched
            };
        }

        return new GameSaveData
        {
            CurrentScore = snapshot.CurrentScore,
            CurrentCombo = snapshot.CurrentCombo,
            TotalMatches = snapshot.TotalMatches,
            MovesLeft = snapshot.MovesLeft,
            GridRows = snapshot.GridRows,
            GridColumns = snapshot.GridColumns,
            Cards = cards,
            HasActiveGame = true
        };
    }

    private Sprite GetSpriteByIndex(int index)
    {
        if (gameConfig.CardAtlas == null)
            return null;
        
        return gameConfig.CardAtlas.GetSprite($"card_{index}");
    }

    private int GetSpriteIndex(Sprite sprite)
    {
        if (sprite == null || gameConfig.CardAtlas == null)
            return 0;
        
        string spriteName = sprite.name;
        spriteName = spriteName.Replace("(Clone)", "");
        
        if (spriteName.StartsWith("card_"))
        {
            string indexStr = spriteName.Substring(5);
            if (int.TryParse(indexStr, out int index))
                return index;
        }
        
        return 0;
    }
}
