using UnityEngine;

public class CardModel
{
    public int Id { get; private set; }
    public int PairId { get; private set; }
    public Sprite FrontSprite { get; private set; }
    public CardState CurrentState { get; set; }

    public CardModel(int id, int pairId, Sprite frontSprite)
    {
        Id = id;
        PairId = pairId;
        FrontSprite = frontSprite;
        CurrentState = CardState.Hidden;
    }

    public bool IsPairWith(CardModel other)
    {
        return PairId == other.PairId && Id != other.Id;
    }
}
