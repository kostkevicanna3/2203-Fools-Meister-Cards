using Core.StateMachine;
using Application.UI;
using Application.Services.UserData;
using Core.Services.Audio;
using Cysharp.Threading.Tasks;
using ILogger = Core.ILogger;
using UnityEngine.SceneManagement;
using UnityEngine;
using Core;
using Core.UI;
using System.Collections.Generic;
using System.Threading;
using Application.Services.Audio;

namespace Application.Game
{
    public class MenuStateController : StateController
    {
        private readonly IUiService _uiService;
        private readonly ISettingProvider _settingProvider;
        private readonly IAudioService _audioService;
        private readonly UserDataService _userDataService;

        private const string InitialSceneName = "Initial";

        private CancellationTokenSource _tokenSource;

        private MainMenuScreen _mainMenu;

        public MenuStateController(ILogger logger,
            IUiService uiService,
            ISettingProvider settingProvider,
            UserDataService userDataService,
            IAudioService audioService) : base(logger)
        {
            _uiService = uiService;
            _settingProvider = settingProvider;
            _userDataService = userDataService;
            _audioService = audioService;
        }

        public override UniTask Enter()
        {
            _mainMenu = _uiService.GetScreen<MainMenuScreen>(ConstScreens.MainMenuScreen);

            _mainMenu.OnSettingsButtonPressedEvent += ShowSettingsButtonPopup;
            _mainMenu.OnProfilePressedEvent += GoToProfileScreen;
            _mainMenu.OnHelpPressedEvent += ShowHelpPopup;
            _mainMenu.OnStartGamePressedEvent += StartGame;

            _tokenSource = new CancellationTokenSource();

            _mainMenu.Initialize();
            _mainMenu.SetProfilePic(FindUserAvatarByName(_userDataService.GetUserData().UserProfileData.UserAvatarAssetName));
            _mainMenu.ShowAsync().Forget();

            return UniTask.CompletedTask;
        }

        public override async UniTask Exit()
        {
            _mainMenu.OnSettingsButtonPressedEvent -= ShowSettingsButtonPopup;
            _mainMenu.OnProfilePressedEvent -= GoToProfileScreen;
            _mainMenu.OnHelpPressedEvent -= ShowHelpPopup;
            _mainMenu.OnStartGamePressedEvent -= StartGame;

            _tokenSource?.Cancel();
            _tokenSource?.Dispose();

            await _uiService.HideScreen(ConstScreens.MainMenuScreen);
        }

        private Sprite FindUserAvatarByName(string assetName)
        {
            foreach (var data in _settingProvider.Get<AvatarSelectionConfig>().AvatarDataList)
            {
                if (data.AvatarAssetName == assetName)
                    return data.AvatarSprite;
            }
            return null;
        }

        private void ShowSettingsButtonPopup()
        {
            SettingsPopup settingsPopup = _uiService.GetPopup<SettingsPopup>(ConstPopups.SettingsPopup);

            settingsPopup.SoundVolumeChangeEvent += OnChangeSoundVolume;
            settingsPopup.MusicVolumeChangeEvent += OnChangeMusicVolume;
            settingsPopup.AccountDeletedEvent += DeleteAccount;

            var userData = _userDataService.GetUserData();

            var isSoundVolume = userData.SettingsData.IsSoundVolume;
            var isMusicVolume = userData.SettingsData.IsMusicVolume;

            settingsPopup.Show(new SettingsPopupData(isSoundVolume, isMusicVolume));
        }

        private void StartGame()
        {
            GoTo<FoolGameController>();
        }

        private void ShowHelpPopup()
        {
            _uiService.ShowPopup(ConstPopups.HelpPopup);
        }

        private void GoToProfileScreen() 
        {
            GoTo<ProfileMenuStateController>();
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
        
        private void DeleteAccount()
        {
            _userDataService.DeleteUserData();
            SceneManager.LoadScene(InitialSceneName);
        }
    }
}