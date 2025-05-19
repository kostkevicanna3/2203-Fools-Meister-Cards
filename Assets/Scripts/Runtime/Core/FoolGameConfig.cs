using Core;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig")]
public class FoolGameConfig : BaseSettings
{
    [SerializeField, Min(1)] private int _maxAttackCards = 6;
    [SerializeField, Min(1)]  private int _initialCardsAtHand = 6;
    [SerializeField, Min(1)]  private int _maxCardsInDeck = 36;
    [SerializeField, Min(2)] private int _playersInGame = 4;  
    [SerializeField, Min(0.5f)] public float _turnDelay = 0.5f;
    [SerializeField, Min(0.1f)] private float _cardAnimTime = 0.33f;
    [SerializeField, Min(0.1f)] private float _botThinkTime = 0.5f;

    public int MaxAttackCards => _maxAttackCards;
    public int InitialCardsAtHand => _initialCardsAtHand;
    public int MaxCardsInDeck => _maxCardsInDeck;
    public int PlayersInGame => _playersInGame;
    public float TurnDelay => _turnDelay;
    public float CardAnimTime => _cardAnimTime;
    public float BotThinkTime => _botThinkTime;

    private void OnValidate()
    {
        if(_turnDelay < _cardAnimTime)
            _turnDelay = _cardAnimTime+ 0.1f;
    }
}