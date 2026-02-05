using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Threading;
using Cysharp.Threading.Tasks;

public class GameController : MonoBehaviour
{
    [SerializeField] private CardObjectPool cardPool;
    [SerializeField] private GridView gridView;

    private GameConfig gameConfig;
    private GameStateMachine stateMachine;
    private CardController cardController;
    private AnimationService animationService;
    private GameModel gameModel;
    private GridModel gridModel;
    private List<CardView> activeCards = new List<CardView>();
    private CancellationTokenSource cts;

    [Inject]
    public void Construct(
        GameConfig config,
        GameStateMachine gameStateMachine,
        CardController controller,
        AnimationService animService)
    {
        gameConfig = config;
        stateMachine = gameStateMachine;
        cardController = controller;
        animationService = animService;
        cts = new CancellationTokenSource();
    }

    private void Start()
    {
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
        gameModel = new GameModel(gridModel);

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
        
        // Randomize sprites selection
        ShuffleSprites(availableSprites);

        if (availableSprites.Count < totalPairs)
        {
            UnityEngine.Debug.LogError($"Not enough sprites! Need {totalPairs}, have {availableSprites.Count}");
            return cards;
        }

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
        {
            UnityEngine.Debug.LogError("CardAtlas is null!");
            return sprites;
        }

        int spriteCount = gameConfig.CardAtlas.spriteCount;
        for (int i = 0; i < spriteCount; i++)
        {
            // Assuming sprites are named card_0, card_1, etc.
            // Or we can just get all sprites if we want to be more generic, 
            // but the original code used specific naming. keeping it but maybe we should make it looser?
            // The user said: "should be taken randomly from sprite atlas, not sequentially".
            // So getting them by name is fine, as long as we get ALL of them then shuffle.
            Sprite sprite = gameConfig.CardAtlas.GetSprite($"card_{i}");
            if (sprite != null)
                sprites.Add(sprite);
        }
        
        if (sprites.Count == 0)
        {
            UnityEngine.Debug.LogError("No sprites found in CardAtlas! Make sure sprites are named: card_0, card_1, etc.");
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
        cts?.Cancel();
        cts?.Dispose();
        cardController?.Dispose();
    }
}
