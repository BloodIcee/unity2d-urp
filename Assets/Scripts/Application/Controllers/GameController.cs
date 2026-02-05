using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Threading;
using Cysharp.Threading.Tasks;

public class GameController : MonoBehaviour
{
    [SerializeField] private CardObjectPool cardPool;
    [SerializeField] private GridView gridView;
    [SerializeField] private ScoreView scoreView;

    private GameConfig gameConfig;
    private GameStateMachine stateMachine;
    private CardController cardController;
    private AnimationService animationService;
    private GameModel gameModel;
    private GridModel gridModel;
    private IScoringService scoringService;
    private AudioManager audioManager;
    private List<CardView> activeCards = new List<CardView>();
    private CancellationTokenSource cts;

    [Inject]
    public void Construct(
        GameConfig config,
        GameStateMachine gameStateMachine,
        CardController controller,
        AnimationService animService,
        IScoringService scoring,
        GameModel model,
        AudioManager audio)
    {
        gameConfig = config;
        stateMachine = gameStateMachine;
        cardController = controller;
        animationService = animService;
        scoringService = scoring;
        gameModel = model;
        audioManager = audio;
        cts = new CancellationTokenSource();
    }

    private void Start()
    {
        InitializeGame();
        
        scoringService.OnScoreUpdated += OnScoreUpdated;
        scoringService.OnComboUpdated += OnComboUpdated;
        scoringService.OnMatchesUpdated += OnMatchesUpdated;
        cardController.OnGameWon += OnGameWon;
        cardController.OnGameLost += OnGameLost;
        cardController.OnMovesUpdated += OnMovesUpdated;
    }

    private void OnScoreUpdated(int score)
    {
        if (scoreView != null)
            scoreView.UpdateScore(score);
    }

    private void OnComboUpdated(int combo)
    {
        if (scoreView != null)
            scoreView.UpdateCombo(combo);
    }

    private void OnMatchesUpdated(int matches)
    {
        if (scoreView != null)
            scoreView.UpdateMatches(matches);
    }

    private void OnMovesUpdated(int moves)
    {
        if (scoreView != null)
            scoreView.UpdateMoves(moves);
    }

    private void OnGameLost()
    {
        if (stateMachine.CurrentState == GameState.Finished) return;
        PlayGameLostSequence().Forget();
    }

    private async UniTaskVoid PlayGameLostSequence()
    {
        stateMachine.ChangeState(GameState.Finished);
        cardController.StopAll();
        audioManager.PlaySfx(SfxType.GameOver);
        
        await UniTask.Delay(1000, cancellationToken: cts.Token);
        RestartGameAsync(true).Forget();
    }

    private void OnGameWon()
    {
        if (stateMachine.CurrentState == GameState.Finished) return;
        PlayGameWonSequence().Forget();
    }

    private async UniTaskVoid PlayGameWonSequence()
    {
        stateMachine.ChangeState(GameState.Finished);
        cardController.StopAll();
        
        await audioManager.PlayGameOverWithDelayAsync();
        
        if (cts.IsCancellationRequested) return;

        RestartGameAsync(false).Forget();
    }

    public void RestartGame()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        RestartGameAsync(true).Forget();
    }

    private async UniTaskVoid RestartGameAsync(bool fullReset)
    {
        stateMachine.ChangeState(GameState.Finished);
        cardController.StopAll();
        
        if (activeCards.Count > 0)
        {
            try
            {
                await animationService.AnimateGridHide(activeCards, cts.Token);
            }
            catch (System.OperationCanceledException)
            {
                return;
            }
        }
        
        gameModel.Reset();
        
        if (fullReset)
        {
            scoringService.Reset();
        }
        else
        {
            scoringService.ResetCombo();
        }

        cardController.Reset();

        InitializeGame();
    }

    private void InitializeGame()
    {
        stateMachine.ChangeState(GameState.Initializing);
        
        GridLayout layout = gameConfig.GetRandomLayout();
        CreateGrid(layout);
        
        stateMachine.ChangeState(GameState.Idle);
    }

    private void CreateGrid(GridLayout layout)
    {
        ClearGrid();

        gridModel = new GridModel(layout.Rows, layout.Columns);
        
        int totalPairs = gridModel.TotalCards / 2;
        int totalMoves = Mathf.CeilToInt(totalPairs * gameConfig.MoveCountMultiplier) + gameConfig.BaseMoves;
        
        gameModel.Initialize(gridModel, totalMoves);

        if (scoreView != null)
            scoreView.UpdateMoves(totalMoves);

        gridView.SetupGrid(
            layout.Rows, 
            layout.Columns, 
            gameConfig.CardSize
        );

        List<CardModel> cards = GenerateCards();
        SpawnCards(cards);
    }

    private List<CardModel> GenerateCards()
    {
        List<CardModel> cards = new List<CardModel>();
        int totalPairs = gridModel.TotalCards / 2;
        
        List<Sprite> availableSprites = GetAvailableSprites();
        
        ShuffleSprites(availableSprites);

        if (availableSprites.Count < totalPairs)
            return cards;

        for (int pairId = 0; pairId < totalPairs; pairId++)
        {
            Sprite sprite = availableSprites[pairId];

            CardModel card1 = new CardModel(cards.Count, pairId, sprite);
            CardModel card2 = new CardModel(cards.Count + 1, pairId, sprite);

            cards.Add(card1);
            cards.Add(card2);
        }

        ShuffleCards(cards);
        return cards;
    }

    private void ShuffleCards(List<CardModel> cards)
    {
        for (int i = cards.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            CardModel temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    private void ShuffleSprites(List<Sprite> sprites)
    {
        for (int i = sprites.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            Sprite temp = sprites[i];
            sprites[i] = sprites[randomIndex];
            sprites[randomIndex] = temp;
        }
    }

    private List<Sprite> GetAvailableSprites()
    {
        List<Sprite> sprites = new List<Sprite>();
        
        if (gameConfig.CardAtlas == null)
            return sprites;

        int spriteCount = gameConfig.CardAtlas.spriteCount;
        for (int i = 0; i < spriteCount; i++)
        {
            Sprite sprite = gameConfig.CardAtlas.GetSprite($"card_{i}");
            if (sprite != null)
                sprites.Add(sprite);
        }

        return sprites;
    }

    private void SpawnCards(List<CardModel> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            CardView cardView = cardPool.Get();
            cardView.transform.SetParent(gridView.transform, false);
            
            cardView.Initialize(
                cards[i], 
                cardController, 
                gameConfig.CardBaseSprite,
                gameConfig.CardBackSprite
            );
            activeCards.Add(cardView);
            
            float staggerDelay = i * gameConfig.StaggerDelay;
            animationService.AnimateCardSpawn(cardView, staggerDelay, cts.Token).Forget();
        }
    }

    private void ClearGrid()
    {
        if (activeCards.Count > 0)
        {
            cardPool.ReleaseAll(activeCards);
            activeCards.Clear();
        }
    }

    private void OnDestroy()
    {
        if (scoringService != null)
        {
            scoringService.OnScoreUpdated -= OnScoreUpdated;
            scoringService.OnComboUpdated -= OnComboUpdated;
            scoringService.OnMatchesUpdated -= OnMatchesUpdated;
        }

        if (cardController != null)
        {
            cardController.OnGameWon -= OnGameWon;
            cardController.OnGameLost -= OnGameLost;
            cardController.OnMovesUpdated -= OnMovesUpdated;
        }

        cts?.Cancel();
        cts?.Dispose();
        cardController?.Dispose();
    }
}
