using Core.StateMachine;
using Zenject;

namespace Application.GameStateMachine
{
    public class Bootstrapper : IInitializable
    {
        private readonly StateMachine _stateMachine;
        private readonly BootstrapState _bootstrapState;
        private readonly LoginState _loginState;
        private readonly FirstLoginState _firstLoginState;    

        public Bootstrapper(StateMachine stateMachine, BootstrapState bootstrapState, LoginState gameState, FirstLoginState loginState)
        {
            _stateMachine = stateMachine;
            _bootstrapState = bootstrapState;
            _loginState = gameState;
            _firstLoginState = loginState;   
        }

        //Initial point
        public void Initialize()
        {
            _stateMachine.Initialize(_bootstrapState, _loginState, _firstLoginState);
            _stateMachine.GoTo<BootstrapState>();
        }
    }
}