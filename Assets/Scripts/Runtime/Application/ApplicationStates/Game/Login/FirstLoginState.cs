using Cysharp.Threading.Tasks;
using Core.StateMachine;
using ILogger = Core.ILogger;
using Application.Services.UserData;
using Application.UI;
using Core;

namespace Application.GameStateMachine
{
    public class FirstLoginState : StateController
    {
        private readonly IUiService _uiService;
        private readonly ISettingProvider _settingProvider;
        private readonly IUserValidator _userValidator;
        private readonly UserDataService _userDataService;

        private LogicScreen _login;

        public FirstLoginState(ILogger logger,
            IUiService uiService,
            IUserValidator userValidator,
            ISettingProvider settingProvider,
            UserDataService userDataService) : base(logger)
        {
            _uiService = uiService;
            _userValidator = userValidator;
            _userDataService = userDataService;
            _settingProvider = settingProvider;
        }

        public override UniTask Enter()
        {
            _login = _uiService.GetScreen<LogicScreen>(ConstScreens.LoginScreen);
            _login.OnProceedPressed += TryProceedToGameScreen;

            _login.Initialize();
            _login.ShowAsync().Forget();

            return UniTask.CompletedTask;
        }

        public override async UniTask Exit()
        {
            _login.OnProceedPressed -= TryProceedToGameScreen;

            await _uiService.HideScreen(ConstScreens.LoginScreen);
        }

        private async void TryProceedToGameScreen()
        {
            string username = _login.GetUsernameInput();

            if (!_userValidator.IsUsernameValid(username))
            {
                _login.ShowErrorMessage();
                return;
            }

            InitializeNewUser(username);
            await _uiService.HideScreen(ConstScreens.LoginScreen);
            GoTo<LoginState>();
        }

        private void InitializeNewUser(string username)
        {
            _userDataService.GetUserData().UserProfileData.UserName = username;
            SetDefaultUserAvatar();
        }

        private void SetDefaultUserAvatar()
        {
            string avatarAssetName = _settingProvider.Get<AvatarSelectionConfig>()?.AvatarDataList[0]?.AvatarAssetName ?? string.Empty;
            _userDataService.GetUserData().UserProfileData.UserAvatarAssetName = avatarAssetName;
        }
    }
}