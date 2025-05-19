using System;
using UnityEngine;
using UnityEngine.Purchasing;

public class PlayerBattleModel
{
    private int _attackerID;
    private int _defenderID;
    private int _lastLostDefenderID = -1;
    private bool _lastRoundDefenderWon = false;
    private BasePlayer[] _playersInGame;
    private int _playersAmount;

    public int AttackerID => _attackerID;
    public int DefenderID => _defenderID;
    public bool LastRoundDefenderWon => _lastRoundDefenderWon;    

    public void SetPlayersInGame(FoolGameRequest gameRequest)
    {
        _playersInGame = gameRequest.Players;
    }

    public void SetFirstAttacker(int playerID)
    {
        _attackerID = playerID;

        _playersAmount = _playersInGame.Length;
        _defenderID = (_attackerID + 1) % _playersAmount;
    }

    public void SetBattleResult(bool result)
    {
        _lastRoundDefenderWon = result;
        _lastLostDefenderID = result ? -1 : _defenderID;
    }

    public void ProcessNextRoundPlayerIDs()
    {
        if(_lastRoundDefenderWon)
            _attackerID = FindNextAttacker(_attackerID, 1);
        else
            _attackerID = FindNextAttacker(_attackerID, 2);

        _defenderID = FindNextDefender();
    }

    private int FindNextAttacker(int startID, int increment)
    {
        int iterations = 0;
        int result = -1;
        do
        {
            result = (startID + increment) % _playersAmount;
            if (!_playersInGame[result].InGame || result == _lastLostDefenderID)
            {
                result = -1;
                increment++;
            }

            iterations++;

            if (iterations > _playersAmount)
                throw new IndexOutOfRangeException();
        }
        while (result == -1);
        
        return result;
    }

    private int FindNextDefender()
    {
        int startID = _attackerID;

        int increment = 1;
        int iterations = 0;
        int result = -1;

        do
        {
            result = (startID + increment) % _playersAmount;
            if (!_playersInGame[result].InGame || result == _attackerID)
            {
                result = -1;
                increment++;
            }

            iterations++;

            if (iterations > _playersAmount)
                throw new IndexOutOfRangeException();
        }
        while (result == -1);

        return result;
    }
}
