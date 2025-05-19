using Core;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class WeakBotPlayer : BasePlayer
{
    private ISettingProvider _settingProvider;

    public event Action<string> OnThinkingStarted;
    public event Action OnThinkingEnded;

    public WeakBotPlayer(ISettingProvider settingProvider, int id, Sprite avatar, string name) 
        : base(settingProvider, id, avatar, name) 
    {
        _settingProvider = settingProvider;
    }

    public override async UniTask ProcessAttack(BoardModel board, CancellationToken cancellationToken)
    {
        float thinkTime = _settingProvider.Get<FoolGameConfig>().BotThinkTime;

        BeginThought();

        await UniTask.WaitForSeconds(thinkTime);
        cancellationToken.ThrowIfCancellationRequested();

        List<Card> attackCards = SelectLowestAttackCards(board);

        for (int i = 0; i < attackCards.Count; i++)
        {
            await UniTask.WaitForSeconds(thinkTime);
            cancellationToken.ThrowIfCancellationRequested();

            if (!await board.TryPlaceAttackCard(attackCards[i]))
            {
                cancellationToken.ThrowIfCancellationRequested();
                OnThinkingEnded?.Invoke();
                return;
            }
        }

        OnThinkingEnded?.Invoke();
        await UniTask.CompletedTask;
    }

    public override async UniTask<bool> ProcessDefense(BoardModel board, CancellationToken cancellationToken)
    {
        if (board.InGameCardPairs.Count == 0)
            return true;

        float thinkTime = _settingProvider.Get<FoolGameConfig>().BotThinkTime;

        BeginThought();

        await UniTask.WaitForSeconds(thinkTime);
        cancellationToken.ThrowIfCancellationRequested();

        List<Card> cardsToBeat = board.InGameCardPairs.Keys.ToList();

        for(int i = 0; i < cardsToBeat.Count;i++)
        {
            Card defenseCard = SelectLowestDefenseCard(cardsToBeat[i], board);

            if(!await board.TryPlaceDefenseCard(defenseCard, cardsToBeat[i]))
            {
                cancellationToken.ThrowIfCancellationRequested();
                OnThinkingEnded?.Invoke();
                return false;
            }
        }

        OnThinkingEnded?.Invoke();
        return true;
    }

    private void BeginThought()
    {
        List<string> thoughts = _settingProvider.Get<BotRoundThoughtsConfig>().Thoughts;
        OnThinkingStarted?.Invoke(thoughts[UnityEngine.Random.Range(0, thoughts.Count)]);
    }

    private List<Card> SelectLowestAttackCards(BoardModel board)
    {
        Card lowestValueNonTrumpCard = FindLowestNonTrumpCard(board);

        if (lowestValueNonTrumpCard != null)
            return GetNonTrumpCardsOfSameValue(lowestValueNonTrumpCard, board);
        else
        {
            Card lowestTrumpCard = FindLowestTrumpCard(board);
            return new List<Card> { lowestTrumpCard };
        }
    }

    private Card FindLowestNonTrumpCard(BoardModel board)
    {
        Card lowestNonTrumpCard = null;

        foreach (var card in _cardsAtHand)
        {
            if (card.CardSuit != board.TrumpCard.CardSuit)
                lowestNonTrumpCard = GetLowerCard(card, lowestNonTrumpCard);
        }

        return lowestNonTrumpCard;
    }

    private Card FindLowestTrumpCard(BoardModel board)
    {
        Card lowestTrumpCard = null;

        foreach (var card in _cardsAtHand)
        {
            if (card.CardSuit == board.TrumpCard.CardSuit)
                lowestTrumpCard = GetLowerCard(card, lowestTrumpCard);
        }

        return lowestTrumpCard;
    }

    private List<Card> GetNonTrumpCardsOfSameValue(Card referenceCard, BoardModel board)
    {
        List<Card> sameValueCards = new List<Card>();

        foreach (var card in _cardsAtHand)
        {
            if (card.CardValue == referenceCard.CardValue && card.CardSuit != board.TrumpCard.CardSuit)
                sameValueCards.Add(card);
        }

        return sameValueCards;
    }

    private Card SelectLowestDefenseCard(Card attackCard, BoardModel board)
    {
        Card lowestSuitCard = null;
        Card lowestTrumpCard = null;

        foreach (var card in _cardsAtHand)
        {
            if (card.CardSuit == attackCard.CardSuit && card.CardValue > attackCard.CardValue)
                lowestSuitCard = GetLowerCard(card, lowestSuitCard);
            else if (card.CardSuit == board.TrumpCard.CardSuit)
                lowestTrumpCard = GetLowerCard(card, lowestTrumpCard);
        }

        return lowestSuitCard ?? lowestTrumpCard;
    }

    private Card GetLowerCard(Card newCard, Card currentLowest)
    {
        return currentLowest == null || newCard.CardValue < currentLowest.CardValue ? newCard : currentLowest;
    }
}
