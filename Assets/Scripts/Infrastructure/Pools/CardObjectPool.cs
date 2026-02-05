using System.Collections.Generic;
using UnityEngine;

public class CardObjectPool : MonoBehaviour
{
    [SerializeField] private CardView cardPrefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private Transform poolContainer;

    private Queue<CardView> pool = new Queue<CardView>();

    private void Awake()
    {
        if (poolContainer == null)
        {
            poolContainer = new GameObject("Card Pool").transform;
            poolContainer.parent = transform;
        }

        PrewarmPool();
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CardView card = Instantiate(cardPrefab, poolContainer);
            card.gameObject.SetActive(false);
            pool.Enqueue(card);
        }
    }

    public CardView Get()
    {
        CardView card;

        if (pool.Count > 0)
        {
            card = pool.Dequeue();
        }
        else
        {
            card = Instantiate(cardPrefab, poolContainer);
        }

        card.gameObject.SetActive(true);
        return card;
    }

    public void Release(CardView card)
    {
        if (card == null)
            return;

        card.ResetCard();
        card.transform.parent = poolContainer;
        card.gameObject.SetActive(false);
        pool.Enqueue(card);
    }

    public void ReleaseAll(List<CardView> cards)
    {
        foreach (var card in cards)
        {
            Release(card);
        }
    }
}
