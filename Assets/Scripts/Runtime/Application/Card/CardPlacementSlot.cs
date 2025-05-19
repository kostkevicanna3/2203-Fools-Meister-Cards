using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class CardPlacementSlot : MonoBehaviour, IDropHandler
{
    [SerializeField] private RectTransform _attackCardPlacement;
    [SerializeField] private RectTransform _defenseCardPlacement;

    private BoardModel _boardModel;
    private RealPlayer _realPlayer;

    private GameObject _attackCardGO;
    private GameObject _defenseCardGO;
    private Card _attackCard;
    private Card _defenseCard;

    private bool _playerAttackTurn = false;
    private bool _playerDefenseTurn = false;

    public RectTransform AttackCardPlacement => _attackCardPlacement;
    public RectTransform DefenseCardPlacement => _defenseCardPlacement;

    public GameObject AttackCardGO => _attackCardGO;
    public GameObject DefenseCardGO => _defenseCardGO;

    public Card AttackCard => _attackCard;
    public Card DefenseCard => _defenseCard;

    [Inject]
    private void Construct(BoardModel boardModel)
    {
        _boardModel = boardModel;
    }

    public void Initialize(RealPlayer realPlayer)
    {
        _realPlayer = realPlayer;

        _realPlayer.OnRealPlayerAttackStarted += SetPlayerAttackTurn;
        _realPlayer.OnRealPlayerDefenseStarted += SetPlayerDefenseTurn;
        _realPlayer.OnRealPlayerTurnEnded += SetPlayerEndedTurn;
    }

    private void OnDestroy()
    {
        _realPlayer.OnRealPlayerAttackStarted -= SetPlayerAttackTurn;
        _realPlayer.OnRealPlayerDefenseStarted -= SetPlayerDefenseTurn;
        _realPlayer.OnRealPlayerTurnEnded -= SetPlayerEndedTurn;
    }

    public async void OnDrop(PointerEventData eventData)
    {
        DraggablePlayerCard draggableCard = eventData.pointerDrag.gameObject.GetComponent<DraggablePlayerCard>();

        if (draggableCard == null)
            return;

        if ((await TryPlaceAttackCard(draggableCard) || await TryPlaceDefenseCard(draggableCard)) == false)
            draggableCard.ResetPosition();
    }

    private async UniTask<bool> TryPlaceAttackCard(DraggablePlayerCard draggableCard)
    {
        if (!_playerAttackTurn)
            return false;

        if (_attackCard != null)
            return false;

        if (await _boardModel.TryPlaceAttackCard(draggableCard.Card))
        {
            Destroy(draggableCard.gameObject);
            return true;
        }

        return false; 
    }

    private async UniTask<bool> TryPlaceDefenseCard(DraggablePlayerCard draggableCard)
    {
        if (!_playerDefenseTurn)
            return false;
        
        if(_defenseCard != null)
            return false;

        if (await _boardModel.TryPlaceDefenseCard(draggableCard.Card, _attackCard))
        {
            Destroy(draggableCard.gameObject);
            return true;
        }

        return false;
    }

    public void PlaceAttackCardGO(GameObject cardGO, Card card)
    {
        _attackCard = card;
        _attackCardGO = cardGO;
        cardGO.transform.SetParent(_attackCardPlacement, false);
        cardGO.transform.position = _attackCardPlacement.position;
    }

    public void PlaceDefenseCardGO(GameObject cardGO, Card card)
    {
        _defenseCard = card;
        _defenseCardGO = cardGO;
        cardGO.transform.SetParent(_defenseCardPlacement, false);
        cardGO.transform.position = _defenseCardPlacement.position;
    }

    public void ClearCards()
    {
        _attackCard = null;
        _defenseCard = null;
        Destroy(_attackCardGO);
        Destroy(_defenseCardGO);
    }

    private void SetPlayerAttackTurn()
    {
        _playerAttackTurn = true;
        _playerDefenseTurn = false;
    }

    private void SetPlayerDefenseTurn()
    {
        _playerAttackTurn = false;
        _playerDefenseTurn = true;
    }

    private void SetPlayerEndedTurn()
    {
        _playerDefenseTurn = _playerAttackTurn = false;
    }
}
