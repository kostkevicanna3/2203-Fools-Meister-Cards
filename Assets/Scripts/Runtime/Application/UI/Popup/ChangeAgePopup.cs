using System;
using System.Threading;
using Core;
using Core.UI;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace Application.UI
{
    public class ChangeAgePopup : BasePopup
    {
        private IUserValidator _userValidator;

        [SerializeField] private TMP_InputField _ageInputField;
        [SerializeField] private SimpleButton _confirmButton;
        [SerializeField] private GameObject _errorMessage;

        public event Action<int> OnAgeChanged;

        private int _newAge;

        [Inject]
        private void Construct(IUserValidator userValidator) => _userValidator = userValidator;

        public override UniTask Show(BasePopupData data, CancellationToken cancellationToken = default)
        {
            ChangeAgePopupData agePopupData = data as ChangeAgePopupData;
            _ageInputField.text = agePopupData.Age.ToString();

            _confirmButton.Button.onClick.AddListener(SubmitAgeChange);

            return base.Show(data, cancellationToken);
        }

        private void SubmitAgeChange()
        {
            if (!TrySetNewAge())
                return;

            OnAgeChanged?.Invoke(_newAge);       
            Hide();
        }

        private bool TrySetNewAge()
        {
            int age = Convert.ToInt32(_ageInputField.text);

            if (_userValidator.IsAgeValid(age))
            {
                _newAge = age;
                _errorMessage.SetActive(false);
                return true;
            }
            else
            {
                _errorMessage.SetActive(true);
                return false;
            }
        }
    }
}