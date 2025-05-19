using Cysharp.Threading.Tasks;
using Application.Game;
using Core.StateMachine;
using ILogger = Core.ILogger;

namespace Application.GameStateMachine
{
    public class LoginState : StateController
    {
        private readonly StateMachine _stateMachine;

        private readonly MenuStateController _menuStateController;
        private readonly ProfileMenuStateController _profileMenuStateController;
        private readonly FoolGameController _gameplayController;
        private readonly UserDataStateChangeController _userDataStateChangeController;

        public LoginState(ILogger logger,
            MenuStateController menuStateController,
            ProfileMenuStateController profileMenuStateController,
            FoolGameController gameplayController,
            StateMachine stateMachine,
            UserDataStateChangeController userDataStateChangeController) : base(logger)
        {
            _stateMachine = stateMachine;
            _menuStateController = menuStateController;
            _profileMenuStateController = profileMenuStateController;
            _userDataStateChangeController = userDataStateChangeController;
            _gameplayController = gameplayController;
        }

        public override async UniTask Enter()
        {
            await _userDataStateChangeController.Run(default);

            _stateMachine.Initialize(_menuStateController, _profileMenuStateController, _gameplayController);
            _stateMachine.GoTo<MenuStateController>();
        }
    }
}