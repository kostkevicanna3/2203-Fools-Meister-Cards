using Application.Services.Audio;
using Core;
using Core.Services.Audio;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

public class BoardModel
{
    private readonly ISettingProvider _settingProvider;
    private readonly IAudioService _audioService;
    private readonly CardDeck _cardDeck;

    private Dictionary<Card, Card> _inGameCardPairs;

    private BasePlayer[] _playersInGame;

    private BasePlayer _attackingPlayer;
    private BasePlayer _defendingPlayer;

    private Card _trumpCard;

    public event Func<UniTask> OnRoundStarted;
    public event Func<BasePlayer, UniTask> OnCardTakenFromDeckForPlayer;
    public event Func<BasePlayer, UniTask> OnCardTakenFromBoardForPlayer;
    public event Func<BasePlayer, Card, UniTask> OnAttackCardPlaced;
    public event Func<BasePlayer, Card, Card, UniTask> OnDefenseCardPlaced;

    public event Action<Card> OnTrumpCardSelected;
    public event Action OnAllCardsBeaten;
    public event Action OnCardsInDeckEnded;
    public event Action OnOnlyTrumpCardLeft;

    public Dictionary<Card, Card> InGameCardPairs => _inGameCardPairs;

    public Card TrumpCard => _trumpCard;

    public bool AnyCardsLeft => _cardDeck.CardsLeft > 0;

    public BoardModel (ISettingProvider settingProvider, IAudioService audioService, CardDeck cardDeck)
    {
        _audioService = audioService;
        _settingProvider = settingProvider;
        _cardDeck = cardDeck;
    }

    public void InitializeGame(FoolGameRequest gameRequest)
    {
        _playersInGame = gameRequest.Players;
        _inGameCardPairs = new(_settingProvider.Get<FoolGameConfig>().InitialCardsAtHand * 2);
        _cardDeck.CreateStartingDeck();
    }

    public async UniTask GiveCardFromBoardToPlayer(BasePlayer player)
    {
        if (OnCardTakenFromBoardForPlayer != null)
        {
            await OnCardTakenFromBoardForPlayer.Invoke(player);
            _audioService.PlaySound(ConstAudio.CardTakeSound);
        }

        foreach (var cardsPair in _inGameCardPairs)
        {
            if (cardsPair.Value != null)
                player.ReceiveCard(cardsPair.Value);
            player.ReceiveCard(cardsPair.Key);
        }

        ClearBoard();
    }

    public void ClearBoard() => _inGameCardPairs.Clear();

    public void SelectTrumpCard()
    {
        _trumpCard = _cardDeck.GetTrumpCard();
        OnTrumpCardSelected?.Invoke(_trumpCard);
    }

    public Card GetCardFromDeck() => _cardDeck.GetCard();

    public async UniTask GiveCardToPlayer(BasePlayer player)
    {
        player.ReceiveCard(GetCardFromDeck());

        if (_cardDeck.CardsLeft == 1)
            OnOnlyTrumpCardLeft?.Invoke();

        if (!AnyCardsLeft)
            OnCardsInDeckEnded?.Invoke();

        if (OnCardTakenFromDeckForPlayer != null)
        {
            await OnCardTakenFromDeckForPlayer.Invoke(player);
            _audioService.PlaySound(ConstAudio.CardTakeSound);
        }
    }

    public void StartRound(BasePlayer attackingPlayer, BasePlayer defendingPlayer)
    {
        OnRoundStarted?.Invoke();
        _attackingPlayer = attackingPlayer;
        _defendingPlayer = defendingPlayer;
    }

    public async UniTask<bool> TryPlaceDefenseCard(Card defenseCard, Card attackCard)
    {
        if (attackCard == null)
            return false;

        if(defenseCard == null)
            return false;

        if (_inGameCardPairs[attackCard] != null)
            return false;

        if (TryBeatCard(attackCard, defenseCard))
        {
            _inGameCardPairs[attackCard] = defenseCard;
            _defendingPlayer.RemoveCard(defenseCard);

            if(_defendingPlayer.CardsAtHand.Count == 0 && !AnyCardsLeft)
                _defendingPlayer.LeaveGame();

            if (OnDefenseCardPlaced != null)
            {
                await OnDefenseCardPlaced.Invoke(_defendingPlayer, defenseCard, attackCard);
                _audioService.PlaySound(ConstAudio.CardPlaceSound);
            }
          
            NotifyIfAllCardsBeaten();

            return true;
        }

        return false;
    }

    public async UniTask<bool> TryPlaceAttackCard(Card card)
    {
        int maxAttackCards = _settingProvider.Get<FoolGameConfig>().MaxAttackCards;

        if (_inGameCardPairs.Count >= maxAttackCards)
            return false;

        if (!CheckOnlySameValueAttackCards(card))
            return false;

        _attackingPlayer.RemoveCard(card);

        if (_attackingPlayer.CardsAtHand.Count == 0 && !AnyCardsLeft)
            _attackingPlayer.LeaveGame();

        _inGameCardPairs.Add(card, null);

        if(OnAttackCardPlaced != null)
        {
            await OnAttackCardPlaced.Invoke(_attackingPlayer, card);
            _audioService.PlaySound(ConstAudio.CardPlaceSound);
        }

        return true;
    }

    private bool CheckOnlySameValueAttackCards(Card card)
    {
        bool canPlace = true;

        foreach (var cardsPair in _inGameCardPairs)
        {
            if (cardsPair.Key.CardValue != card.CardValue)
            {
                canPlace = false; 
                break;
            }
        }

        return canPlace;
    }

    private void NotifyIfAllCardsBeaten()
    {
        foreach(var kv in _inGameCardPairs)
        {
            if (kv.Value == null)
                return;
        }
        OnAllCardsBeaten?.Invoke();
    }

    private bool TryBeatCard(Card attackCard, Card defendCard)
    {
        if(attackCard.CardSuit == defendCard.CardSuit)
            return defendCard.CardValue > attackCard.CardValue;

        if(defendCard.CardSuit == _trumpCard.CardSuit)
            return true;

        return false;
    }
}
