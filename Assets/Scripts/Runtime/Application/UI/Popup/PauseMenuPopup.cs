using System;
using System.Threading;
using Application.Services;
using Application.Services.Audio;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Application.UI
{
    public class PauseMenuPopup : BasePopup
    {
        private const string ConfirmLeaveMessage = "DO YOU REALLY WANT TO LEAVE?"; 
        private const string ConfirmRestartMessage = "DO YOU REALLY WANT TO RESTART?";
        
        [SerializeField] private SimpleButton _unpauseButton;
        [SerializeField] private SimpleButton _restartButton;
        [SerializeField] private Toggle _soundSettingsToggle;
        [SerializeField] private Toggle _musicSettingsToggle;
        [SerializeField] private SimpleButton _exitApplicationButton;

        private IUiService _uiService;

        public event Action OnUnpausePressedEvent;
        public event Action OnRestartPressedEvent;
        public event Action<bool> OnSoundSettingsChangedEvent;
        public event Action<bool> OnMusicSettingsChangedEvent;
        public event Action OnReturnHomePressedEvent;

        [Inject]
        private void Construct(IUiService uiService)
        {
            _uiService = uiService;
        }

        public override UniTask Show(BasePopupData data, CancellationToken cancellationToken = default)
        {
            SettingsPopupData popupData = data as SettingsPopupData;

            _unpauseButton.Button.onClick.AddListener(() =>
            {
                AudioService.PlaySound(ConstAudio.PressButtonSound);
                OnUnpausePressedEvent?.Invoke();
                Hide();
            });

            var isSoundVolume = popupData.IsSoundVolume;
            _soundSettingsToggle.onValueChanged.Invoke(isSoundVolume);
            _soundSettingsToggle.isOn = isSoundVolume;

            var isMusicVolume = popupData.IsMusicVolume;
            _musicSettingsToggle.onValueChanged.Invoke(isMusicVolume);
            _musicSettingsToggle.isOn = isMusicVolume;

            _restartButton.Button.onClick.AddListener(async () =>
            {
                await _uiService.ShowPopup(
                    ConstPopups.SimpleDecisionPopup, 
                    new SimpleDecisionPopupData() 
                    { 
                        Message = ConfirmRestartMessage, 
                        PressOkEvent = OnRestartPressedEvent 
                    });
            });

            _exitApplicationButton.Button.onClick.AddListener(async () =>
            {
                await _uiService.ShowPopup(
                    ConstPopups.SimpleDecisionPopup,
                    new SimpleDecisionPopupData()
                    {
                        Message = ConfirmLeaveMessage,
                        PressOkEvent = OnReturnHomePressedEvent
                    });
            });

            _soundSettingsToggle.onValueChanged.AddListener(OnSoundVolumeToggleValueChanged);
            _musicSettingsToggle.onValueChanged.AddListener(OnMusicVolumeToggleValueChanged);

            return base.Show(data, cancellationToken);
        }

        private void OnSoundVolumeToggleValueChanged(bool value)
        {
            AudioService.PlaySound(ConstAudio.PressButtonSound);
            OnSoundSettingsChangedEvent?.Invoke(value);
        }

        private void OnMusicVolumeToggleValueChanged(bool value)
        {
            AudioService.PlaySound(ConstAudio.PressButtonSound);
            OnMusicSettingsChangedEvent?.Invoke(value);
        }
    }
}