using System;
using System.Threading;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Application.UI
{
    public class LosePopup : BasePopup
    {
        [SerializeField] private SimpleButton _restartButton;
        [SerializeField] private SimpleButton _homeButton;

        public event Action OnRestartPressedEvent;
        public event Action OnHomePressedEvent;

        public override UniTask Show(BasePopupData data, CancellationToken cancellationToken = default)
        {
            _restartButton.Button.onClick.AddListener(() => OnRestartPressedEvent?.Invoke());
            _homeButton.Button.onClick.AddListener(() => OnHomePressedEvent?.Invoke());

            return base.Show(data, cancellationToken);
        }
    }
}