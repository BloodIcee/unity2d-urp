using System;

public class GameStateMachine
{
    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnStateChanged;

    public GameStateMachine()
    {
        CurrentState = GameState.Initializing;
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        OnStateChanged?.Invoke(newState);
    }

    public bool CanRevealCard()
    {
        return CurrentState == GameState.Idle || CurrentState == GameState.CardRevealing;
    }

    public bool IsIdle()
    {
        return CurrentState == GameState.Idle;
    }
}
