using Application.UI;
using System;
using TMPro;
using UnityEngine;

public class LogicScreen : UiScreen
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private SimpleButton _createProfileButton;
    [SerializeField] private GameObject _errorMessage;

    public event Action OnProceedPressed;

    public void Initialize() => _createProfileButton.Button.onClick.AddListener(() => OnProceedPressed?.Invoke());

    public string GetUsernameInput() => _inputField.text;

    public void ShowErrorMessage() => _errorMessage.SetActive(true);

    private void OnDestroy() => _createProfileButton.Button.onClick.RemoveAllListeners();
}
