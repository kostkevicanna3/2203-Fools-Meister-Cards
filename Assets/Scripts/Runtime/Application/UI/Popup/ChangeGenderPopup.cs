using System;
using System.Threading;
using Core;
using Core.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Application.UI
{
    public class ChangeGenderPopup : BasePopup
    {
        [SerializeField] private SimpleButton _femaleButton;
        [SerializeField] private Image _femaleSelectedImage;

        [SerializeField] private SimpleButton _maleButton;
        [SerializeField] private Image _maleSelectedImage;

        [SerializeField] private SimpleButton _applyButton;

        private Gender _newGender;

        public event Action<Gender> OnAgeChanged;

        public override UniTask Show(BasePopupData data, CancellationToken cancellationToken = default)
        {
            ChangeGenderPopupData agePopupData = data as ChangeGenderPopupData;
            _newGender = agePopupData.Gender;

            UpdateToggles();

            _femaleButton.Button.onClick.AddListener(() =>
            {
                _newGender = Gender.Female;
                UpdateToggles();
            });

            _maleButton.Button.onClick.AddListener(() =>
            {
                _newGender = Gender.Male;
                UpdateToggles();
            });

            _applyButton.Button.onClick.AddListener(() =>
            {
                OnAgeChanged?.Invoke(_newGender);
                Hide();
            });

            return base.Show(data, cancellationToken);
        }

        private void UpdateToggles()
        {
            _femaleSelectedImage.gameObject.SetActive(_newGender == Gender.Female);
            _maleSelectedImage.gameObject.SetActive(_newGender == Gender.Male);
        }
    }
}