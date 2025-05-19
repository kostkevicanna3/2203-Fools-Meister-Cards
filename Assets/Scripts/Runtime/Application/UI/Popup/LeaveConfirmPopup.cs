using System;
using System.Threading;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Application.UI
{
    public class LeaveConfirmPopup : BasePopup
    {
        [SerializeField] private SimpleButton _confirmButton;
        [SerializeField] private SimpleButton _denyButton;

        public event Action OnLeaveGamePressedEvent;
        public event Action OnContinueGamePressedEvent;

        public override UniTask Show(BasePopupData data, CancellationToken cancellationToken = default)
        {
            _confirmButton.Button.onClick.AddListener(()=> OnLeaveGamePressedEvent?.Invoke());
            _denyButton.Button.onClick.AddListener(()=> OnContinueGamePressedEvent?.Invoke());

            return base.Show(data, cancellationToken);
        }
    }
}