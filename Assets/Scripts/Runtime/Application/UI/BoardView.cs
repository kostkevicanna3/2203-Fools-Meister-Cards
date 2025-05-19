using Application.Services;
using Core;
using Core.Factory;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BoardView : MonoBehaviour
{
    private readonly Vector3 ShrinkAnimEndCardScale = new Vector3(0.55f, 0.55f, 0.55f);
    private IAssetProvider _assetProvider;
    private ISettingProvider _settingProvider;

    [SerializeField] private CardPlacementSlot[] _cardPlacementSlots;
    [SerializeField] private RectTransform _trumpCardPosition;
    [SerializeField] private RectTransform _cardDeckTransform;
    [SerializeField] private GameObject _defaultDeckCardGO;
    [SerializeField] private RectTransform _cardDiscardTransform;

    private Dictionary<BasePlayer, OpponentView> _playerToViewDict = new(6);

    private GameObject _trumpCard;

    private CancellationToken _token;

    private BoardModel _boardModel;
    private GameObjectFactory _factory;

    [Inject]
    private void Construct(BoardModel boardModel, GameObjectFactory gameObjectFactory, IAssetProvider assetProvider, ISettingProvider settingProvider)
    {
        _boardModel = boardModel;
        _factory = gameObjectFactory;
        _assetProvider = assetProvider;
        _settingProvider = settingProvider;

        _boardModel.OnRoundStarted += DiscardCards;
        _boardModel.OnCardsInDeckEnded += DestroyTrumpCard;
        _boardModel.OnOnlyTrumpCardLeft += DestroyDefaultDeckCard;
        _boardModel.OnCardTakenFromDeckForPlayer += GiveCardFromDeckToPlayer;
        _boardModel.OnCardTakenFromBoardForPlayer += GiveCardFromBoardToPlayer;
        _boardModel.OnTrumpCardSelected += DisplayTrumpCard;
        _boardModel.OnAttackCardPlaced += PlaceAttackCard;
        _boardModel.OnDefenseCardPlaced += PlaceDefenseCard;
    }

    private void OnDestroy()
    {
        _boardModel.OnRoundStarted -= DiscardCards;
        _boardModel.OnCardsInDeckEnded -= DestroyTrumpCard;
        _boardModel.OnOnlyTrumpCardLeft -= DestroyDefaultDeckCard;
        _boardModel.OnCardTakenFromDeckForPlayer -= GiveCardFromDeckToPlayer;
        _boardModel.OnCardTakenFromBoardForPlayer -= GiveCardFromBoardToPlayer;
        _boardModel.OnTrumpCardSelected -= DisplayTrumpCard;
        _boardModel.OnAttackCardPlaced -= PlaceAttackCard;
        _boardModel.OnDefenseCardPlaced -= PlaceDefenseCard;
    }

    public void SetToken(CancellationToken token)
    {
        _token = token;
    }

    public void AddOpponentView(BasePlayer player, OpponentView view)
    {
        _playerToViewDict.Add(player, view);
    }

    public void InitializePlacementSlots(RealPlayer realPlayer)
    {
        foreach (var card in _cardPlacementSlots)
            card.Initialize(realPlayer);
    }

    private UniTask<Sprite> LoadSpriteForCard(Card card)
    {
        return _assetProvider.Load<Sprite>(String.Concat(card.CardValue, card.CardSuit));
    }
 
    private int FindFirstFreeAttackSlot()
    {
        for (int i = 0; i < _cardPlacementSlots.Length; i++)
        {
            if (_cardPlacementSlots[i].AttackCard == null)
                return i;
        }
        return -1;
    }

    private int FindAppropriateDefenseSlot(Card attackCard)
    {
        for (int i = 0; i < _cardPlacementSlots.Length; i++)
        {
            if (_cardPlacementSlots[i].AttackCard == null)
                continue;
            if (_cardPlacementSlots[i].AttackCard.Equals(attackCard))
                return i;
        }
        return -1;
    }

    private async UniTask<GameObject> CreateDefaultCard()
    {
        return await _factory.Create(ConstPrefabNames.CardPrefab);
    }

    private async UniTask<GameObject> CreateCardVisual(Card card)
    {
        _token.ThrowIfCancellationRequested();
        GameObject visualCard = await CreateDefaultCard();

        _token.ThrowIfCancellationRequested();

        Sprite sprite = await LoadSpriteForCard(card);
        _token.ThrowIfCancellationRequested();

        visualCard.GetComponent<Image>().sprite = sprite;
        return visualCard;
    }

    private void PlayCardAnim(Transform transform, Transform target, float fadeTarget, Vector3 targetScale, float animTime)
    {
        _token.ThrowIfCancellationRequested();

        transform.DOMove(target.position, animTime).SetLink(target.gameObject);
        transform.GetComponent<Image>().DOFade(fadeTarget, animTime).SetLink(target.gameObject);
        transform.DOScale(targetScale, animTime).SetLink(target.gameObject);
    }

    private async void ProcessRealPlayerDefenseCardPlacement(BasePlayer attackingPlayer, Card defenseCard, Card attackCard)
    {
        GameObject cardGO = await CreateCardVisual(defenseCard);
        _token.ThrowIfCancellationRequested();

        _cardPlacementSlots[FindAppropriateDefenseSlot(attackCard)].PlaceDefenseCardGO(cardGO, defenseCard);
    }

    private async void ProcessRealPlayerAttackCardPlacement(BasePlayer attackingPlayer, Card attackCard)
    {
        GameObject cardGO = await CreateCardVisual(attackCard);
        _token.ThrowIfCancellationRequested();

        _cardPlacementSlots[FindFirstFreeAttackSlot()].PlaceAttackCardGO(cardGO, attackCard);
    }

    private async UniTask ProcessBotDefenseCardPlacement(BasePlayer defendingPlayer, Card defenseCard, Card attackCard)
    {
        float animTime = _settingProvider.Get<FoolGameConfig>().CardAnimTime;

        OpponentView view = _playerToViewDict[defendingPlayer];

        int cardSlotIndex = FindAppropriateDefenseSlot(attackCard);

        GameObject cardGO = view.GetCardGO();

        Sprite sprite = await LoadSpriteForCard(defenseCard);
        _token.ThrowIfCancellationRequested();

        cardGO.GetComponent<Image>().sprite = sprite;

        Transform targetTransform = _cardPlacementSlots[cardSlotIndex].DefenseCardPlacement;
        cardGO.transform.SetParent(targetTransform, true);

        PlayCardAnim(cardGO.transform, targetTransform, 1, Vector3.one, animTime);

        await UniTask.WaitForSeconds(animTime);
        _token.ThrowIfCancellationRequested();

        _cardPlacementSlots[cardSlotIndex].PlaceDefenseCardGO(cardGO, defenseCard);
    }

    private async UniTask ProcessBotAttackCardPlacement(BasePlayer attackingPlayer, Card attackCard)
    {
        OpponentView view = _playerToViewDict[attackingPlayer];

        float animTime = _settingProvider.Get<FoolGameConfig>().CardAnimTime;

        int firstFreeCardSlotIndex = FindFirstFreeAttackSlot();

        GameObject cardGO = view.GetCardGO();

        _token.ThrowIfCancellationRequested();
        Sprite sprite = await LoadSpriteForCard(attackCard);
        _token.ThrowIfCancellationRequested();

        cardGO.GetComponent<Image>().sprite = sprite;

        Transform targetTransform = _cardPlacementSlots[firstFreeCardSlotIndex].AttackCardPlacement;

        cardGO.transform.SetParent(targetTransform, true);

        PlayCardAnim(cardGO.transform, targetTransform, 1, Vector3.one, animTime);
        await UniTask.WaitForSeconds(animTime);

        _token.ThrowIfCancellationRequested();

        _cardPlacementSlots[firstFreeCardSlotIndex].PlaceAttackCardGO(cardGO, attackCard);
    }

    private void MoveCardsFromBoardToTarget(Transform target, float animTime)
    {
        foreach (var slot in _cardPlacementSlots)
        {
            if (slot.AttackCardGO == null)
                continue;

            PlayCardAnim(slot.AttackCardGO.transform, target, 0, ShrinkAnimEndCardScale, animTime);

            if (slot.DefenseCardGO == null)
                continue;

            PlayCardAnim(slot.DefenseCardGO.transform, target, 0, ShrinkAnimEndCardScale, animTime);
        }
    }

    private async UniTask DiscardCards()
    {
        float animTime = _settingProvider.Get<FoolGameConfig>().CardAnimTime;

        MoveCardsFromBoardToTarget(_cardDiscardTransform, animTime);

        await UniTask.WaitForSeconds(animTime);
        _token.ThrowIfCancellationRequested();

        foreach (var slot in _cardPlacementSlots)
            slot.ClearCards();
    }

    private void DestroyTrumpCard()
    {
        Destroy(_trumpCard);
    }

    private void DestroyDefaultDeckCard()
    {
        Destroy(_defaultDeckCardGO);
    }

    private async UniTask GiveCardFromDeckToPlayer(BasePlayer player)
    {
        GameObject visualCard = await CreateDefaultCard();
        _token.ThrowIfCancellationRequested();

        float animTime = _settingProvider.Get<FoolGameConfig>().CardAnimTime;

        visualCard.transform.SetParent(_cardDeckTransform, false);
        visualCard.transform.position = _cardDeckTransform.position;

        PlayCardAnim(visualCard.transform, player.PlayerTransform, 0, ShrinkAnimEndCardScale, animTime);

        await UniTask.WaitForSeconds(animTime);
        _token.ThrowIfCancellationRequested();
    }

    private async UniTask GiveCardFromBoardToPlayer(BasePlayer player)
    {
        float animTime = _settingProvider.Get<FoolGameConfig>().CardAnimTime;

        MoveCardsFromBoardToTarget(player.PlayerTransform, animTime);

        await UniTask.WaitForSeconds(animTime);
        _token.ThrowIfCancellationRequested();
    }

    private async void DisplayTrumpCard(Card card)
    {
        _token.ThrowIfCancellationRequested();
        GameObject result = await CreateCardVisual(card);
        _token.ThrowIfCancellationRequested();

        _trumpCard = result;

        Image image = _trumpCard.GetComponent<Image>();
        Color color = image.color;
        color.a = 0;
        image.color = color;

        _trumpCard.transform.rotation = Quaternion.Euler(0, 0, 90);
        _trumpCard.transform.SetParent(_trumpCardPosition, false);
        _trumpCard.transform.position = _trumpCardPosition.position;

        float animTime = _settingProvider.Get<FoolGameConfig>().CardAnimTime;
        image.DOFade(1, animTime).SetLink(gameObject);
    }

    private async UniTask PlaceAttackCard(BasePlayer attackingPlayer, Card attackCard)
    {
        if (attackingPlayer is not RealPlayer)
        {
            await ProcessBotAttackCardPlacement(attackingPlayer, attackCard);
            _token.ThrowIfCancellationRequested();
        }
        else
            ProcessRealPlayerAttackCardPlacement(attackingPlayer, attackCard);
    }

    private async UniTask PlaceDefenseCard(BasePlayer defendingPlayer, Card defenseCard, Card attackCard)
    {
        if (defendingPlayer is not RealPlayer)
        {
            await ProcessBotDefenseCardPlacement(defendingPlayer, defenseCard, attackCard);
            _token.ThrowIfCancellationRequested();
        }
        else
            ProcessRealPlayerDefenseCardPlacement(defendingPlayer, defenseCard, attackCard);
    }
}
