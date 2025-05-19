using System;
using System.Collections.Generic;
using System.Threading;
using Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;


namespace Application.UI
{
    public class ChangeAvatarPopup : BasePopup
    {
        private IUiService _uiService;

        [SerializeField] private RectTransform _buttonsParent;
        [SerializeField] private SimpleButton _applyButton;

        private List<AvatarSelectButton> _avatarSelectButtons;

        private string _avatarAssetName;

        public event Action<string> OnAvatarChanged;

        [Inject]
        private void Construct(IUiService uiService)
        {
            _uiService = uiService;
        }

        public override UniTask Show(BasePopupData data, CancellationToken cancellationToken = default)
        {
            ChangeAvatarPopupData avatarSelectionPopupData = data as ChangeAvatarPopupData;
            _avatarAssetName = avatarSelectionPopupData.SelectedAvatarAssetName;

            PopulateGrid(avatarSelectionPopupData);
            _applyButton.Button.onClick.AddListener(() => OnAvatarChanged?.Invoke(_avatarAssetName));

            return base.Show(data, cancellationToken);
        }

        private void PopulateGrid(ChangeAvatarPopupData avatarSelectionPopupData)
        {
            int len = avatarSelectionPopupData.AvatarsData.Count;
            _avatarSelectButtons = new List<AvatarSelectButton>(len);
            for (int i = 0; i < len; i++)
            {
                AvatarSelectButton avatarSelectButton = _uiService.GetAvatarSelectionButton();

                avatarSelectButton.Initialize(avatarSelectionPopupData.AvatarsData[i],
                    _avatarAssetName == avatarSelectionPopupData.AvatarsData[i].AvatarAssetName);

                avatarSelectButton.transform.SetParent(_buttonsParent, false);
                avatarSelectButton.OnAvatarSelected += UpdateSelectedAvatar;

                _avatarSelectButtons.Add(avatarSelectButton);
            }
        }

        private void UpdateSelectedAvatar(AvatarSelectButton selectedButton, string avatarAssetName)
        {
            ClearAllSelection();
            selectedButton.EnableToggle(true);
            _avatarAssetName = avatarAssetName;
        }

        private void ClearAllSelection()
        {
            foreach (var button in _avatarSelectButtons)
                button.EnableToggle(false);
        }
    }
}