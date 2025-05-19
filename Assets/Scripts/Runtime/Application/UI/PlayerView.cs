using Application.Services;
using Application.UI;
using Core;
using Core.Factory;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

public class PlayerView : MonoBehaviour
{
    private IAssetProvider _assetProvider;

    [SerializeField] private RectTransform _playerCardSelectionScrollRect;
    [SerializeField] private RectTransform _draggableCardHolderParent;
    [SerializeField] private RectTransform _turnNotificationTransform;
    [SerializeField] private SimpleButton _endTurnButton;
    [SerializeField] private SimpleButton _takeCardsButton;

    private RealPlayer _realPlayer;

    private GameObjectFactory _factory;

    private bool _playerAttack = false;
    private bool _playerDefense = false;

    public event Action OnEndedTurnEvent;
    public event Action OnTookCardsEvent;

    public int ID => _realPlayer.PlayerID;

    [Inject]
    private void Construct(GameObjectFactory goFactory, IAssetProvider assetProvider)
    {
        _factory = goFactory;
        _assetProvider = assetProvider;
    }

    public void Initialize(RealPlayer realPlayer)
    {
        _realPlayer = realPlayer;
        
        _realPlayer.OnReceivedCard += ReceiveCard;
        _realPlayer.OnRealPlayerTurnEnded += ProcessPlayerEndTurn;
        _realPlayer.OnRealPlayerDefenseStarted += ProcessPlayerDefenseTurn;
        _realPlayer.OnRealPlayerAttackStarted += ProcessPlayerAttackTurn;

        _endTurnButton.Button.onClick.AddListener(() => 
        { 
            _endTurnButton.gameObject.SetActive(false);
            OnEndedTurnEvent?.Invoke(); 
        });

        _takeCardsButton.Button.onClick.AddListener(() => 
        {
            _takeCardsButton.gameObject.SetActive(false);
            OnTookCardsEvent?.Invoke(); 
        });
    }

    private void OnDestroy()
    {
        if (_realPlayer == null)
            return;

        _realPlayer.OnReceivedCard -= ReceiveCard;
        _realPlayer.OnRealPlayerTurnEnded -= ProcessPlayerEndTurn;
        _realPlayer.OnRealPlayerDefenseStarted -= ProcessPlayerDefenseTurn;
        _realPlayer.OnRealPlayerAttackStarted -= ProcessPlayerAttackTurn;

        _endTurnButton.Button.onClick.RemoveAllListeners();
        _takeCardsButton.Button.onClick.RemoveAllListeners();
    }

    private async void ReceiveCard(Card card)
    {
        Sprite cardSprite = await LoadSpriteForCard(card);
        await AddDragableCard(card, cardSprite);
    }

    private async UniTask AddDragableCard(Card card, Sprite cardSprite)
    {
        DraggablePlayerCard dragCard = await _factory.Create<DraggablePlayerCard>(ConstPrefabNames.DraggableCard);
        dragCard.transform.SetParent(_draggableCardHolderParent, false);
        dragCard.Initialize(cardSprite, card);
    }

    private UniTask<Sprite> LoadSpriteForCard(Card card)
    {
        return _assetProvider.Load<Sprite>(String.Concat(card.CardValue, card.CardSuit));
    }

    private void ControlPlayerUI()
    {
        _takeCardsButton.gameObject.SetActive(_playerDefense);
        _endTurnButton.gameObject.SetActive(_playerAttack);
        _turnNotificationTransform.gameObject.SetActive(_playerAttack || _playerDefense);
    }

    private void ProcessPlayerAttackTurn()
    {
        _playerAttack = true;
        _playerDefense = false;
        ControlPlayerUI();
    }
    private void ProcessPlayerDefenseTurn()
    {
        _playerDefense = true;
        _playerAttack = false;
        ControlPlayerUI();
    }
    private void ProcessPlayerEndTurn()
    {
        _playerAttack = _playerDefense = false;
        ControlPlayerUI();
    }
}
