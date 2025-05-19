using Application.Services.UserData;
using Application.UI;
using Core;
using Core.Services.ScreenOrientation;
using Core.StateMachine;
using Core.UI;
using Cysharp.Threading.Tasks;
using System;

namespace Application.GameStateMachine
{
    public class BootstrapState : StateController
    {
        private readonly IAssetProvider _assetProvider;
        private readonly IUiService _uiService;
        private readonly ISettingProvider _settingProvider;
        private readonly UserDataService _userDataService;
        private readonly AudioSettingsBootstrapController _audioSettingsBootstrapController;
        private readonly ScreenOrientationAlertController _screenOrientationAlertController;
        //private readonly AdsBootstrapController _androidAdsBootstrapController;


        public BootstrapState(IAssetProvider assetProvider,
            IUiService uiService,
            ILogger logger,
            ISettingProvider settingProvider,
            UserDataService userDataService,
            AudioSettingsBootstrapController audioSettingsBootstrapController,
            ScreenOrientationAlertController screenOrientationAlertController) : base(logger)
        {
            _assetProvider = assetProvider;
            _uiService = uiService;
            _settingProvider = settingProvider;
            _userDataService = userDataService;
            _audioSettingsBootstrapController = audioSettingsBootstrapController;
            _screenOrientationAlertController = screenOrientationAlertController;
        }

        public override async UniTask Enter()
        {
            _userDataService.Initialize();
            await _assetProvider.Initialize();
            await _uiService.Initialize();
            await _settingProvider.Initialize();

            var loadingPopup = _uiService.GetPopup<BasePopup>(ConstPopups.LoadingPopup);
            loadingPopup.EnableSound = false;
            loadingPopup.Show(default).Forget();
            //await _androidAdsBootstrapController.Run(default);
            loadingPopup.Hide();

            await _screenOrientationAlertController.Run(default);

            _uiService.ShowScreen(ConstScreens.SplashScreen).Forget();
            await _audioSettingsBootstrapController.Run(default);
            UpdateSession();

            if (PlayerDataDoesntExist())
                GoTo<FirstLoginState>();
            else
                GoTo<LoginState>();
        }

        private bool PlayerDataDoesntExist()
        {
            return _userDataService.GetUserData().IsFirstSession() || _userDataService.GetUserData().UserProfileData.UserName == String.Empty;
        }

        public override async UniTask Exit()
        {
            await _uiService.HideScreen(ConstScreens.SplashScreen);
        }

        private void UpdateSession()
        {
            _userDataService.GetUserData().GameData.SessionNumber++;
            _userDataService.SaveUserData();
        }
    }
}