using System;

public class Card : IEquatable<Card>
{
    private CardValue _cardValue;
    private CardSuit _cardSuit;

    public CardValue CardValue => _cardValue;
    public CardSuit CardSuit => _cardSuit;

    public Card(CardValue cardValue, CardSuit cardSuit)
    {
        _cardValue = cardValue;
        _cardSuit = cardSuit;
    }

    public bool Equals(Card other)
    {
        return _cardSuit == other.CardSuit && _cardValue == other.CardValue;
    }
}
