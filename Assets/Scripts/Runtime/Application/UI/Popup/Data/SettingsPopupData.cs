using Application.Services.UserData;
using Core.UI;

namespace Application.UI
{
    public class SettingsPopupData : BasePopupData
    {
        private bool _isSoundVolume;
        private bool _isMusicVolume;

        public bool IsSoundVolume => _isSoundVolume;
        public bool IsMusicVolume => _isMusicVolume;

        public SettingsPopupData(bool isSoundVolume, bool isMusicVolume)
        {
            _isSoundVolume = isSoundVolume;
            _isMusicVolume = isMusicVolume;
        }
    }
}