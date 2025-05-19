using Application.Services;
using Core.Factory;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class OpponentView : MonoBehaviour
{
    private readonly Vector3 DisplayCardScale = new Vector3(0.55f, 0.55f, 0.55f);

    [SerializeField] private Image _avatarImage;
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private GameObject _wonBanner;
    [SerializeField] private RectTransform _cardHolderParent;

    [SerializeField] private GameObject _thoughtGO;
    [SerializeField] private TextMeshProUGUI _thoughtText;

    private GameObjectFactory _factory;
    private List<GameObject> _cardsViewList = new();

    private CancellationToken _token;

    private WeakBotPlayer _player;
    public int ID => _player.PlayerID;

    [Inject]
    private void Construct(GameObjectFactory goFactory)
    {
        _factory = goFactory;
    }

    public void Initialize(BasePlayer player, CancellationToken token)
    {
        _player = player as WeakBotPlayer;
        _avatarImage.sprite = _player.Avatar;
        _playerNameText.text = _player.Name;

        _token = token;

        _wonBanner.SetActive(false);
        _thoughtGO.SetActive(false);

        _player.OnReceivedCard += ReceiveCard;
        _player.OnPlayerWon += DisplayWinText;
        _player.OnThinkingStarted += DisplayThinkMessage;
        _player.OnThinkingEnded += HideThinkMessage;
    }

    private void OnDestroy()
    {
        _player.OnReceivedCard -= ReceiveCard;
        _player.OnPlayerWon -= DisplayWinText;
    }


    public GameObject GetCardGO()
    {
        GameObject card = _cardsViewList[_cardsViewList.Count - 1];
        _cardsViewList.RemoveAt(_cardsViewList.Count - 1);
        return card;
    }

    private async void ReceiveCard(Card card)
    {
        _token.ThrowIfCancellationRequested();
        GameObject go = await _factory.Create(ConstPrefabNames.CardPrefab);
        _token.ThrowIfCancellationRequested();

        go.transform.localScale = DisplayCardScale;
        _cardsViewList.Add(go);
        go.transform.SetParent(_cardHolderParent, false);
    }

    private void DisplayWinText()
    {
        _wonBanner.SetActive(true);
    }

    private void DisplayThinkMessage(string thought)
    {
        _thoughtText.text = thought;
        _thoughtGO.SetActive(true);
    }

    private void HideThinkMessage()
    {
        _thoughtGO.SetActive(false);
    }
}
