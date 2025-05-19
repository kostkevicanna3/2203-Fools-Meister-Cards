using Core;
using System;
using System.Collections.Generic;

public class CardDeck
{
    private readonly ISettingProvider _settingProvider;

    private List<Card> _cards;
    private int _cardsLeft;

    public int CardsLeft => _cardsLeft;

    public CardDeck(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    public void CreateStartingDeck()
    {
        GenerateDeck();
        ShuffleDeck();
    }

    public Card GetCard()
    {
        _cardsLeft--;
        Card card = _cards[_cardsLeft];
        _cards.Remove(card);
        return card;
    }
    
    public Card GetTrumpCard()
    {
        Card trumpCard = _cards[_cardsLeft - 1];
        (_cards[_cardsLeft - 1], _cards[0]) = (_cards[0], _cards[_cardsLeft - 1]);
        return trumpCard;
    }

    private void GenerateDeck()
    {
        int cardsInDeckSetting = _settingProvider.Get<FoolGameConfig>().MaxCardsInDeck;
        _cardsLeft = cardsInDeckSetting;
        _cards = new List<Card>(cardsInDeckSetting);

        foreach (CardValue value in Enum.GetValues(typeof(CardValue)))
        {
            foreach (CardSuit suit in Enum.GetValues(typeof(CardSuit)))
            {
                _cards.Add(new Card(value, suit));
            }
        }
    }

    private void ShuffleDeck()
    {
        Random random = new Random();
        int cardCount = _cards.Count;

        for (int i = cardCount - 1; i > 0; i--)
        {
            int randomIndexToSwapWith = random.Next(i + 1);
            (_cards[i], _cards[randomIndexToSwapWith]) = (_cards[randomIndexToSwapWith], _cards[i]);
        }
    }
}
