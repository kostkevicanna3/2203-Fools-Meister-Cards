using Application.Game;
using Application.Services.Audio;
using Application.Services.UserData;
using Application.UI;
using Core;
using Core.Services;
using Core.Services.Audio;
using Core.StateMachine;
using Core.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class FoolGameController : StateController
{
    private const int RealPlayerID = 0;
    private readonly IAudioService _audioService;
    private readonly IUiService _uiService;
    private readonly ISettingProvider _settingProvider;

    private readonly UserDataService _userDataService;

    private readonly CardDealingController _cardDealingController;
    private readonly ChooseFirstPlayerController _chooseFirstPlayerController;
    private readonly BattleController _battleController;

    private GameplayScreen _gameplayScreen;
    private BoardModel _boardModel;
    private PlayerBattleModel _playerBattleOrderModel;
    private CancellationTokenSource _tokenSource;

    private FoolGameRequest _gameRequest;

    public FoolGameController(
        Core.ILogger logger,
        BoardModel boardModel,
        PlayerBattleModel playerBattleOrderModel,
        CardDealingController cardDealingController,
        ChooseFirstPlayerController chooseFirstPlayerController,
        BattleController battleController,
        UserDataService userDataService,
        IAudioService audioService,
        ISettingProvider settingProvider,
        IUiService uiService) : base(logger)
    {
        _boardModel = boardModel;
        _playerBattleOrderModel = playerBattleOrderModel;

        _cardDealingController = cardDealingController;
        _chooseFirstPlayerController = chooseFirstPlayerController;
        _battleController = battleController;

        _settingProvider = settingProvider;
        _userDataService = userDataService;
        _audioService = audioService;
        _uiService = uiService;
    }

    public async override UniTask Enter()
    {
        _gameplayScreen = _uiService.GetScreen<GameplayScreen>(ConstScreens.GameplayScreen);

        _tokenSource = new CancellationTokenSource();

        _gameplayScreen.OnGamePaused += PauseGame;
        _gameplayScreen.Initialize(_tokenSource.Token);
        _gameplayScreen.ShowAsync().Forget();

        _gameRequest = new FoolGameRequest(CreatePlayers(_settingProvider.Get<FoolGameConfig>().PlayersInGame), false);

        _boardModel.InitializeGame(_gameRequest);
        _playerBattleOrderModel.SetPlayersInGame(_gameRequest);
        await _gameplayScreen.CreatePlayersViews(_gameRequest);

        await StartGame(_tokenSource.Token);
    }

    private async UniTask StartGame(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();

        await _cardDealingController.Run(_gameRequest, token);

        token.ThrowIfCancellationRequested();
        await _chooseFirstPlayerController.Run(_gameRequest, token);

        _gameRequest.StartGame();

        while (!token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();

            await _battleController.Run(_gameRequest, token);
            token.ThrowIfCancellationRequested();

            if (HasPlayerLost())
            {
                DispayLosePopupForRealPlayer();
                break;
            }

            await _cardDealingController.Run(_gameRequest, token);
            token.ThrowIfCancellationRequested();

            _playerBattleOrderModel.ProcessNextRoundPlayerIDs();
        }
    }

    public async override UniTask Exit() 
    {
        _gameplayScreen.OnGamePaused -= PauseGame;

        _tokenSource?.Cancel();

        await _uiService.HideScreen(ConstScreens.GameplayScreen);
        _tokenSource?.Dispose();
    }

    private BasePlayer[] CreatePlayers(int playersInGame)
    {
        BasePlayer[] basePlayers = new BasePlayer[playersInGame];

        basePlayers[RealPlayerID] = CreateRealPlayer();

        basePlayers[RealPlayerID].OnPlayerWon += DisplayWinPopupForRealPlayer;

        List<string> names = new(_settingProvider.Get<BotNameConfig>().BotNames);
        List<AvatarData> avatars = new(_settingProvider.Get<AvatarSelectionConfig>().AvatarDataList);

        for (int i = 1; i < playersInGame; i++)
        {
            basePlayers[i] = CreateWeakBotPlayer(i, names, avatars);
        }

        return basePlayers;
    }

    private RealPlayer CreateRealPlayer()
    {
        return new RealPlayer(
                    _gameplayScreen.GetPlayerView(),
                    _boardModel,
                    _settingProvider,
                    0,
                    null,
                    null
                );
    }

    private WeakBotPlayer CreateWeakBotPlayer(int i, List<string> names, List<AvatarData> avatars)
    {
        int randomNameIndex = UnityEngine.Random.Range(0, names.Count);
        string name = names[randomNameIndex];
        names.RemoveAt(randomNameIndex);

        int randomAvatarIndex = UnityEngine.Random.Range(0, avatars.Count);
        AvatarData avatarData = avatars[randomAvatarIndex];
        avatars.RemoveAt(randomAvatarIndex);

        return new WeakBotPlayer(
                    _settingProvider,
                    i,
                    avatarData.AvatarSprite,
                    name
                );
    }

    private bool HasPlayerLost()
    {
        int playersWithCards = 0;

        for(int i = 0; i < _gameRequest.Players.Length; i++)
        {
            if (_gameRequest.Players[i].InGame)
                playersWithCards++;
        }

        return playersWithCards == 1 && _gameRequest.Players[RealPlayerID].InGame;
    }

    private async void PauseGame()
    {
        Time.timeScale = 0;

        var userData = _userDataService.GetUserData();

        var isSoundVolume = userData.SettingsData.IsSoundVolume;
        var isMusicVolume = userData.SettingsData.IsMusicVolume;

        BasePopup basePopup = await _uiService.ShowPopup(ConstPopups.PauseMenuPopup, 
            new SettingsPopupData(isSoundVolume, isMusicVolume));

        PauseMenuPopup pausePopup = basePopup as PauseMenuPopup;

        pausePopup.OnUnpausePressedEvent += () => 
        { 
            Time.timeScale = 1;
            pausePopup.DestroyPopup();
        };
        pausePopup.OnReturnHomePressedEvent += () => 
        {
            Time.timeScale = 1;
            pausePopup.DestroyPopup();
            GoTo<MenuStateController>();
        };
        pausePopup.OnRestartPressedEvent += () =>
        {
            Time.timeScale = 1;
            pausePopup.DestroyPopup();
            GoTo<FoolGameController>();
        };

        pausePopup.OnSoundSettingsChangedEvent += OnChangeSoundVolume;
        pausePopup.OnMusicSettingsChangedEvent += OnChangeMusicVolume;
    }

    private async void DispayLosePopupForRealPlayer()
    {
        _audioService.PlaySound(ConstAudio.LoseSound);
        Time.timeScale = 0;
        BasePopup basePopup = await _uiService.ShowPopup(ConstPopups.LosePopup);
        LosePopup losePopup = basePopup as LosePopup;
        losePopup.OnHomePressedEvent += () =>
        {
            Time.timeScale = 1;
            GoTo<MenuStateController>();
        };
        losePopup.OnRestartPressedEvent += () =>
        {
            Time.timeScale = 1;
            GoTo<FoolGameController>();
        };
    }

    private async void DisplayWinPopupForRealPlayer()
    {
        _audioService.PlaySound(ConstAudio.VictorySound);
        Time.timeScale = 0;
        BasePopup basePopup = await _uiService.ShowPopup(ConstPopups.WinPopup);
        WinPopup winPopup = basePopup as WinPopup;
        winPopup.OnHomePressedEvent += () =>
        {
            Time.timeScale = 1;
            GoTo<MenuStateController>();
        };
        winPopup.OnRestartPressedEvent += () =>
        {
            Time.timeScale = 1;
            GoTo<FoolGameController>();
        };
    }

    private void OnChangeSoundVolume(bool state)
    {
        _audioService.SetVolume(Core.Services.Audio.AudioType.Sound, state ? 1 : 0);
        var userData = _userDataService.GetUserData();
        userData.SettingsData.IsSoundVolume = state;
    }

    private void OnChangeMusicVolume(bool state)
    {
        _audioService.SetVolume(Core.Services.Audio.AudioType.Music, state ? 1 : 0);
        var userData = _userDataService.GetUserData();
        userData.SettingsData.IsMusicVolume = state;
    }
}