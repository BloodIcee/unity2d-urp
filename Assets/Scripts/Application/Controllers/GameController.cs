using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameConfig gameConfig;
    [SerializeField] private CardObjectPool cardPool;
    [SerializeField] private GridView gridView;

    private GameStateMachine stateMachine;
    private CardController cardController;
    private GameModel gameModel;
    private GridModel gridModel;
    private List<CardView> activeCards = new List<CardView>();

    private void Awake()
    {
        stateMachine = new GameStateMachine();
        cardController = new CardController(stateMachine, gameConfig);
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        stateMachine.ChangeState(GameState.Initializing);
        
        GridLayout layout = gameConfig.GetDefaultLayout();
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
            gameConfig.CardSize, 
            gameConfig.CardSpacing
        );

        List<CardModel> cards = GenerateCards();
        SpawnCards(cards);
    }

    private List<CardModel> GenerateCards()
    {
        List<CardModel> cards = new List<CardModel>();
        int totalPairs = gridModel.TotalCards / 2;

        for (int pairId = 0; pairId < totalPairs; pairId++)
        {
            Sprite sprite = GetRandomCardSprite();

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
            int randomIndex = Random.Range(0, i + 1);
            CardModel temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }
    }

    private Sprite GetRandomCardSprite()
    {
        if (gameConfig.CardAtlas == null)
            return null;

        int spriteCount = gameConfig.CardAtlas.spriteCount;
        if (spriteCount == 0)
            return null;

        int randomIndex = Random.Range(0, spriteCount);
        return gameConfig.CardAtlas.GetSprite($"card_{randomIndex}");
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
        cardController?.Dispose();
    }
}
