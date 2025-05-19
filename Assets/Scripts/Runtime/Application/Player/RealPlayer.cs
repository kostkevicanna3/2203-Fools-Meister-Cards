using Core;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

public class RealPlayer : BasePlayer
{
    private bool _endedTurn = false;
    private bool _tookCard = false;
    private bool _allCardsBeaten = false;

    public event Action OnRealPlayerAttackStarted;
    public event Action OnRealPlayerDefenseStarted;
    public event Action OnRealPlayerTurnEnded;

    public RealPlayer(PlayerView playerView, BoardModel board, ISettingProvider settingProvider, int id, Sprite avatar, string name) 
        : base(settingProvider, id, avatar, name)
    {
        playerView.OnEndedTurnEvent += () => _endedTurn = true;
        playerView.OnTookCardsEvent += () => _tookCard = true;
        board.OnAllCardsBeaten += () => _allCardsBeaten = true;
    }

    public async override UniTask ProcessAttack(BoardModel board, CancellationToken cancellationToken)
    {
        OnRealPlayerAttackStarted?.Invoke();
        while (!cancellationToken.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(_endedTurn)
            {
                _endedTurn = false;
                OnRealPlayerTurnEnded?.Invoke();
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }

    public async override UniTask<bool> ProcessDefense(BoardModel board, CancellationToken cancellationToken)
    {
        OnRealPlayerDefenseStarted?.Invoke();

        _allCardsBeaten = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_tookCard)
            {
                _tookCard = false;
                OnRealPlayerTurnEnded?.Invoke();
                return false;
            }

            if(_allCardsBeaten)
            {
                OnRealPlayerTurnEnded?.Invoke();
                return true;
            }    

            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        return true;
    }
}