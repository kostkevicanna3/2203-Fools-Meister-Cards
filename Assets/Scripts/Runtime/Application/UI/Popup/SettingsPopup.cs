using System;
using System.Threading;
using Application.Services.Audio;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Application.UI
{
    public class SettingsPopup : BasePopup
    {
        private const string DeleteAccountConfirmMessage = " DO YOU REALLY WANT TO DELETE THE ACCOUNT?";

        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _deleteAccountButton;
        [SerializeField] private Toggle _soundVolumeToggle;
        [SerializeField] private Toggle _musicVolumeToggle;

        private IUiService _uiService;

        public event Action<bool> SoundVolumeChangeEvent;
        public event Action<bool> MusicVolumeChangeEvent;
        public event Action AccountDeletedEvent;

        [Inject]
        private void Construct(IUiService uiService)
        {
            _uiService = uiService;
        }

        public override UniTask Show(BasePopupData data, CancellationToken cancellationToken = default)
        {
            SettingsPopupData settingsPopupData = data as SettingsPopupData;

            var isSoundVolume = settingsPopupData.IsSoundVolume;
            _soundVolumeToggle.onValueChanged.Invoke(isSoundVolume);
            _soundVolumeToggle.isOn = isSoundVolume;
            
            var isMusicVolume = settingsPopupData.IsMusicVolume;
            _musicVolumeToggle.onValueChanged.Invoke(isMusicVolume);
            _musicVolumeToggle.isOn = isMusicVolume;

            _closeButton.onClick.AddListener(DestroyPopup);

            _soundVolumeToggle.onValueChanged.AddListener(OnSoundVolumeToggleValueChanged);
            _musicVolumeToggle.onValueChanged.AddListener(OnMusicVolumeToggleValueChanged);

            _deleteAccountButton.onClick.AddListener(async () =>
            {
                await _uiService.ShowPopup(
                    ConstPopups.SimpleDecisionPopup, 
                    new SimpleDecisionPopupData() 
                    {
                        Message = DeleteAccountConfirmMessage,
                        PressOkEvent = AccountDeletedEvent
                    });
            });

            return base.Show(data, cancellationToken);
        }

        public override void DestroyPopup()
        {
            Destroy(gameObject);
        }

        private void OnSoundVolumeToggleValueChanged(bool value)
        {
            AudioService.PlaySound(ConstAudio.PressButtonSound);

            SoundVolumeChangeEvent?.Invoke(value);
        }

        private void OnMusicVolumeToggleValueChanged(bool value)
        {
            AudioService.PlaySound(ConstAudio.PressButtonSound);

            MusicVolumeChangeEvent?.Invoke(value);
        }
    }
}