using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.StateMachine
{
    public class StateMachine
    {
        private Dictionary<Type, StateController> _states;
        private StateController _activeState;

        public void Initialize(params StateController[] stateControllers)
        {
            _states = new Dictionary<Type, StateController>(stateControllers.Length);
            foreach (var executiveStateController in stateControllers)
            {
                _states.Add(executiveStateController.GetType(), executiveStateController);
                executiveStateController.Initialize(this);
            }
        }

        public async void GoTo<TState>() where TState : StateController
        {
            StateController state = await ChangeState<TState>();
            state.Enter().Forget();
        }

        private async UniTask<TState> ChangeState<TState>() where TState : StateController
        {
            if (_activeState != null)
                await _activeState.Exit();

            TState state = GetState<TState>();
            _activeState = state;

            return state;
        }

        private TState GetState<TState>() where TState : class
        {
            return _states[typeof(TState)] as TState;
        }
    }
}