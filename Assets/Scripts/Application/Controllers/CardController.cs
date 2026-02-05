using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

public class CardController
{
    private GameStateMachine stateMachine;
    private GameConfig config;
    private AnimationService animationService;
    private List<CardView> revealedCards = new List<CardView>();
    private CancellationTokenSource cts;

    public CardController(GameStateMachine gameStateMachine, GameConfig gameConfig, AnimationService animService)
    {
        stateMachine = gameStateMachine;
        config = gameConfig;
        animationService = animService;
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
        CardState state = cardView.Model.CurrentState;
        
        if (state != CardState.Hidden)
            return false;

        if (revealedCards.Count >= 2)
            return false;

        return true;
    }

    private async UniTaskVoid RevealCardAsync(CardView cardView, CancellationToken cancellationToken)
    {
        cardView.Model.CurrentState = CardState.Revealing;
        
        await animationService.AnimateCardFlip(cardView, true, cancellationToken);
        
        cardView.Model.CurrentState = CardState.Revealed;
        cardView.UpdateVisuals();
        revealedCards.Add(cardView);

        if (revealedCards.Count == 1)
        {
            stateMachine.ChangeState(GameState.CardRevealing);
        }
        else if (revealedCards.Count == 2)
        {
            stateMachine.ChangeState(GameState.ComparingCards);
            await CheckMatchAsync(cancellationToken);
        }
    }

    private async UniTask CheckMatchAsync(CancellationToken cancellationToken)
    {
        if (revealedCards.Count != 2)
            return;

        CardView first = revealedCards[0];
        CardView second = revealedCards[1];

        bool isMatch = first.Model.IsPairWith(second.Model);

        if (isMatch)
        {
            first.Model.CurrentState = CardState.Matched;
            second.Model.CurrentState = CardState.Matched;
            stateMachine.ChangeState(GameState.Matched);
            
            await animationService.AnimateMatch(first, cancellationToken);
            await animationService.AnimateMatch(second, cancellationToken);
            
            await UniTask.Delay((int)(config.MatchDelay * 1000), cancellationToken: cancellationToken);
            
            first.SetInteractable(false);
            second.SetInteractable(false);
            revealedCards.Clear();
            stateMachine.ChangeState(GameState.Idle);
        }
        else
        {
            first.Model.CurrentState = CardState.Mismatched;
            second.Model.CurrentState = CardState.Mismatched;
            stateMachine.ChangeState(GameState.Mismatched);
            
            await animationService.AnimateMismatch(first, cancellationToken);
            await animationService.AnimateMismatch(second, cancellationToken);
            
            await UniTask.Delay((int)(config.MismatchDelay * 1000), cancellationToken: cancellationToken);
            
            first.Model.CurrentState = CardState.Hidden;
            second.Model.CurrentState = CardState.Hidden;
            
            await animationService.AnimateCardFlip(first, false, cancellationToken);
            await animationService.AnimateCardFlip(second, false, cancellationToken);
            
            first.UpdateVisuals();
            second.UpdateVisuals();
            
            revealedCards.Clear();
            stateMachine.ChangeState(GameState.Idle);
        }
    }

    public void Reset()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        revealedCards.Clear();
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
