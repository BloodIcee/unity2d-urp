using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CardController
{
    private GameStateMachine stateMachine;
    private GameConfig config;
    private AnimationService animationService;
    private List<CardView> selectedCards = new List<CardView>();
    private CancellationTokenSource cts;

    public System.Action OnGameWon;

    private readonly GameModel gameModel;
    private readonly IScoringService scoringService;

    private HashSet<CardView> processingCards = new HashSet<CardView>();

    public CardController(
        GameStateMachine gameStateMachine, 
        GameConfig gameConfig, 
        AnimationService animService,
        GameModel model,
        IScoringService scoring)
    {
        stateMachine = gameStateMachine;
        config = gameConfig;
        animationService = animService;
        gameModel = model;
        scoringService = scoring;
        cts = new CancellationTokenSource();
    }

    public void OnCardClicked(CardView cardView)
    {
        if (cardView == null || cardView.Model == null)
            return;

        if (!CanRevealCard(cardView))
            return;

        RevealCardAsync(cardView, cts.Token).Forget();
    }

    private bool CanRevealCard(CardView cardView)
    {
        if (stateMachine.CurrentState != GameState.Idle && 
            stateMachine.CurrentState != GameState.CardRevealing &&
            stateMachine.CurrentState != GameState.ComparingCards &&
            stateMachine.CurrentState != GameState.Matched && 
            stateMachine.CurrentState != GameState.Mismatched)
        {
            return false;
        }

        CardState state = cardView.Model.CurrentState;
        
        if (state != CardState.Hidden)
            return false;

        if (selectedCards.Contains(cardView))
            return false;

        if (processingCards.Contains(cardView))
            return false;

        if (selectedCards.Count >= 2)
            return false;

        return true;
    }

    private async UniTaskVoid RevealCardAsync(CardView cardView, CancellationToken cancellationToken)
    {
        selectedCards.Add(cardView);
        processingCards.Add(cardView);
        
        await animationService.AnimateCardFlip(cardView, true, cancellationToken);
        
        cardView.Model.CurrentState = CardState.Revealed;
        cardView.UpdateVisuals();

        if (selectedCards.Count == 2)
        {
            List<CardView> pairToProcess = new List<CardView>(selectedCards);
            selectedCards.Clear();

            ProcessPairAsync(pairToProcess, cancellationToken).Forget();
        }
    }

    private async UniTaskVoid ProcessPairAsync(List<CardView> pair, CancellationToken cancellationToken)
    {
        if (pair.Count != 2) return;

        CardView first = pair[0];
        CardView second = pair[1];

        bool isMatch = first.Model.IsPairWith(second.Model);

        if (isMatch)
        {
            first.Model.CurrentState = CardState.Matched;
            second.Model.CurrentState = CardState.Matched;
            
            scoringService.OnMatch();
            gameModel.IncrementMatches();

            await UniTask.WhenAll(
                animationService.AnimateMatch(first, cancellationToken),
                animationService.AnimateMatch(second, cancellationToken)
            );

            if (gameModel.IsGameComplete)
            {
                OnGameWon?.Invoke();
                processingCards.Remove(first);
                processingCards.Remove(second);
                return;
            }
            
            await UniTask.Delay((int)(config.MatchDelay * 1000), cancellationToken: cancellationToken);
            
            first.SetInteractable(false);
            second.SetInteractable(false);
        }
        else
        {
            scoringService.OnMismatch();

            first.Model.CurrentState = CardState.Mismatched;
            second.Model.CurrentState = CardState.Mismatched;
            
            await UniTask.WhenAll(
                animationService.AnimateMismatch(first, cancellationToken),
                animationService.AnimateMismatch(second, cancellationToken)
            );
            
            await UniTask.Delay((int)(config.MismatchDelay * 1000), cancellationToken: cancellationToken);
            
            first.Model.CurrentState = CardState.Hidden;
            second.Model.CurrentState = CardState.Hidden;
            
            await UniTask.WhenAll(
                animationService.AnimateCardFlip(first, false, cancellationToken),
                animationService.AnimateCardFlip(second, false, cancellationToken)
            );
            
            first.UpdateVisuals();
            second.UpdateVisuals();
        }

        processingCards.Remove(first);
        processingCards.Remove(second);
    }

    public void Reset()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        selectedCards.Clear();
        processingCards.Clear();
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
