using Core;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class BasePlayer
{
    private readonly ISettingProvider _settingProvider;

    protected List<Card> _cardsAtHand;

    protected int _playerID;

    private bool _inGame = true;

    public List<Card> CardsAtHand => _cardsAtHand;

    public event Action<Card> OnReceivedCard;
    public event Action<int> OnRemovedCard;
    public event Action OnPlayerWon;

    public Transform PlayerTransform;
    public Sprite Avatar;
    public string Name;

    public int PlayerID => _playerID;

    public bool InGame => _inGame;

    public BasePlayer(ISettingProvider settingProvider, int id, Sprite avatar, string name)
    {
        _settingProvider = settingProvider;
        _playerID = id;
        _cardsAtHand = new(_settingProvider.Get<FoolGameConfig>().InitialCardsAtHand);
        Avatar = avatar;
        Name = name;
    }

    public void LeaveGame()
    {
        OnPlayerWon?.Invoke();
        _inGame = false;
    }

    public void ReceiveCard(Card card)
    {
        _cardsAtHand.Add(card);
        OnReceivedCard?.Invoke(card);
    }
    public void RemoveCard(Card card)
    {
        OnRemovedCard?.Invoke(_cardsAtHand.IndexOf(card));
        _cardsAtHand.Remove(card);
    }

    public abstract UniTask ProcessAttack(BoardModel board, CancellationToken cancellationToken);
    public abstract UniTask<bool> ProcessDefense(BoardModel board, CancellationToken cancellationToken);
}
