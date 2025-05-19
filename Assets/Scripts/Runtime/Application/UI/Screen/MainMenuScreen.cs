using Application.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuScreen : UiScreen
{
    [SerializeField] private SimpleButton _openProfileButton;
    [SerializeField] private SimpleButton _openHelpButton;
    [SerializeField] private SimpleButton _startGameButton;
    [SerializeField] private SimpleButton _openSettingsButton;
    [SerializeField] private Image _avatarImage;

    public event Action OnProfilePressedEvent;
    public event Action OnHelpPressedEvent;
    public event Action OnStartGamePressedEvent;
    public event Action OnSettingsButtonPressedEvent;

    public void Initialize()
    {
        _openProfileButton.Button.onClick.AddListener(() => OnProfilePressedEvent?.Invoke());
        _openHelpButton.Button.onClick.AddListener(() => OnHelpPressedEvent?.Invoke());
        _startGameButton.Button.onClick.AddListener(() => OnStartGamePressedEvent?.Invoke());
        _openSettingsButton.Button.onClick.AddListener(() => OnSettingsButtonPressedEvent?.Invoke());
    }

    public void SetProfilePic(Sprite sprite) => _avatarImage.sprite = sprite;

    private void OnDestroy()
    {
        _openProfileButton.Button.onClick.RemoveAllListeners();
        _openHelpButton.Button.onClick.RemoveAllListeners();
        _startGameButton.Button.onClick.RemoveAllListeners();
        _openSettingsButton.Button.onClick.RemoveAllListeners();
    }
}
