using Core.StateMachine;
using Application.UI;
using Application.Services.UserData;
using Cysharp.Threading.Tasks;
using ILogger = Core.ILogger;
using Core.UI;
using System.Threading;
using Core;
using UnityEngine;
using System.Collections.Generic;

namespace Application.Game
{
    public class ProfileMenuStateController : StateController
    {
        private readonly IUiService _uiService;
        private readonly ISettingProvider _settingProvider;
        private readonly UserDataService _userDataService;

        private ProfileScreen _profileScreen;
        private CancellationTokenSource _cancelTokenSource;

        private DummyUserProfileData _dummyProfileData;

        public ProfileMenuStateController(ILogger logger,
            IUiService uiService,
            ISettingProvider settingProvider,
            UserDataService userDataService) : base(logger)
        {
            _uiService = uiService;
            _userDataService = userDataService;
            _settingProvider = settingProvider;
        }

        public override UniTask Enter()
        {
            _profileScreen = _uiService.GetScreen<ProfileScreen>(ConstScreens.ProfileScreen);
            _cancelTokenSource = new();

            SetupDummyProfileData();
            _profileScreen.OnGoBackPressedEvent += GoBack;
            _profileScreen.OnApplyChangesPressedEvent += ApplyChanges;
            _profileScreen.OnChangeAgePressedEvent += ShowAgeSettingsPopup;
            _profileScreen.OnChangeGenderPressedEvent += ShowChangeGenderPopup;
            _profileScreen.OnChangeAvatarPressedEvent += ShowChangeAvatarPopup;

            _profileScreen.Initialize();
            _profileScreen.SetUserData(_dummyProfileData.ProfileData, FindUserAvatarByName(_dummyProfileData.ProfileData.UserAvatarAssetName));
            _profileScreen.ShowAsync().Forget();

            return UniTask.CompletedTask;
        }

        public override async UniTask Exit()
        {
            _cancelTokenSource?.Cancel();
            _cancelTokenSource?.Dispose();

            _profileScreen.OnGoBackPressedEvent -= GoBack;
            _profileScreen.OnApplyChangesPressedEvent -= ApplyChanges;
            _profileScreen.OnChangeAgePressedEvent -= ShowAgeSettingsPopup;
            _profileScreen.OnChangeGenderPressedEvent -= ShowChangeGenderPopup;
            _profileScreen.OnChangeAvatarPressedEvent -= ShowChangeAvatarPopup;

            await _uiService.HideScreen(ConstScreens.ProfileScreen);
        }

        private Sprite FindUserAvatarByName(string assetName)
        {
            foreach(var data in _settingProvider.Get<AvatarSelectionConfig>().AvatarDataList)
            {
                if (data.AvatarAssetName == assetName)
                    return data.AvatarSprite;
            }
            return null;
        }

        private void SetupDummyProfileData()
        {
            _dummyProfileData = new();
            _dummyProfileData.CopyDataFrom(_userDataService.GetUserData().UserProfileData);
        }

        private async void ShowChangeAvatarPopup()
        {
            BasePopup basePopup = await _uiService.ShowPopup(
                ConstPopups.ChangeAvatarPopup,
                new ChangeAvatarPopupData
                {
                    AvatarsData = _settingProvider.Get<AvatarSelectionConfig>().AvatarDataList,
                    SelectedAvatarAssetName = _dummyProfileData.ProfileData.UserAvatarAssetName
                },
                _cancelTokenSource.Token);

            (basePopup as ChangeAvatarPopup).OnAvatarChanged += (avatarAssetName) =>
            {
                _dummyProfileData.ModifyAvatar(avatarAssetName);
                _profileScreen.UpdateUserAvatar(FindUserAvatarByName(avatarAssetName));
            };
        }

        private async void ShowChangeGenderPopup()
        {
            BasePopup basePopup = await _uiService.ShowPopup(
                ConstPopups.ChangeGenderPopup,
                new ChangeGenderPopupData { Gender = _dummyProfileData.ProfileData.Gender },
                _cancelTokenSource.Token);

            (basePopup as ChangeGenderPopup).OnAgeChanged += (Gender newGender) =>
            {
                _dummyProfileData.ModifyGender(newGender);
                _profileScreen.UpdateUserGender(newGender);
            };
        }

        private async void ShowAgeSettingsPopup()
        {
            BasePopup basePopup = await _uiService.ShowPopup(
                ConstPopups.ChangeAgePopup,
                new ChangeAgePopupData { Age = _dummyProfileData.ProfileData.Age },
                _cancelTokenSource.Token
               );

            (basePopup as ChangeAgePopup).OnAgeChanged += (int newAge) =>
            {
                _dummyProfileData.ModifyAge(newAge);
                _profileScreen.UpdateUserAge(newAge);
            };
        }

        private void ApplyChanges()
        {
            _dummyProfileData.PasteDataTo(_userDataService.GetUserData().UserProfileData);
            _userDataService.SaveUserData();
            GoBack();
        }

        private void GoBack()
        {
            GoTo<MenuStateController>();
        }
    }
}