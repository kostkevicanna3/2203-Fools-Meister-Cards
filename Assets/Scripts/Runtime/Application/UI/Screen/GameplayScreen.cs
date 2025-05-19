using Application.Services;
using Application.UI;
using Core.Factory;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

public class GameplayScreen : UiScreen
{
    [SerializeField] private SimpleButton _pauseButton;

    [SerializeField] private RectTransform _playerListParent;
    [SerializeField] private RectTransform _playerTransform;

    [SerializeField] private BoardView _boardView;
    [SerializeField] private PlayerView _playerView;

    private GameObjectFactory _goFactory;
    private CancellationToken _token;

    public event Action OnGamePaused;

    [Inject]
    private void Construct(GameObjectFactory goFactory)
    {
        _goFactory = goFactory;
    }

    public void Initialize(CancellationToken token)
    {
        _token = token;
        _boardView.SetToken(token);
        _pauseButton.Button.onClick.AddListener(() => OnGamePaused?.Invoke());
    }

    private void OnDestroy()
    {
        _pauseButton.Button.onClick.RemoveAllListeners();
    }

    public PlayerView GetPlayerView () => _playerView;

    public async UniTask CreatePlayersViews(FoolGameRequest gameRequest)
    {
        foreach (var player in gameRequest.Players)
        {
            if (player is RealPlayer)
            {
                RealPlayer realPlayer = (RealPlayer)player;

                InitializePlayerView(realPlayer);
                _boardView.InitializePlacementSlots(realPlayer);

                continue;
            }

            await CreateOpponentView(player);
        }
    }

    private void InitializePlayerView(RealPlayer player)
    {
        player.PlayerTransform = _playerTransform;
        _playerView.Initialize(player);
    }

    private async UniTask CreateOpponentView(BasePlayer player)
    {
        _token.ThrowIfCancellationRequested();
        OpponentView view = await _goFactory.Create<OpponentView>(ConstPrefabNames.OpponentPrefab);
        _token.ThrowIfCancellationRequested();

        view.Initialize(player, _token  );
        view.transform.SetParent(_playerListParent, false);
        player.PlayerTransform = view.transform;

        _boardView.AddOpponentView(player, view);
    }
}
