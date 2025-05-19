using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Application.UI
{
    public class MenuScreen : UiScreen
    {
        [SerializeField] private SimpleButton _botVsBotButton;
        [SerializeField] private SimpleButton _playerVsPlayerButton;
        [SerializeField] private SimpleButton _playerVsBotButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private RectTransform _scrollCenterPoint;
        [SerializeField] private Vector2 _centerButtonSize = new Vector2(1.2f, 1.2f);
        [SerializeField] private Vector2 _defaultButtonSize = Vector2.one;

        private float _visibilityRadiusButton = 350;
        private SimpleButton[] _menuButtons;

        public event Action PlayerVsBotPressEvent;
        public event Action BotVsBotPressEvent;
        public event Action PlayerVsPlayerPressEvent;
        public event Action SettingsButtonPressEvent;

        private void Update()
        {
            if (_menuButtons == null)
                return;

            for (int i = 0; i < _menuButtons.Length; i++)
            {
                var magnitude = (_menuButtons[i].transform.position - _scrollCenterPoint.position).magnitude;
                var t = Mathf.InverseLerp(0, _visibilityRadiusButton, magnitude);
                var scale = Vector2.Lerp(_centerButtonSize, _defaultButtonSize, t);
                _menuButtons[i].transform.localScale = scale;
            }
        }

        private void OnDestroy()
        {
            _playerVsBotButton.Button.onClick.RemoveAllListeners();
            _botVsBotButton.Button.onClick.RemoveAllListeners();
            _playerVsPlayerButton.Button.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
        }

        public void Initialize()
        {
            _playerVsBotButton.Button.onClick.AddListener(PlayerVsBotButtonPress);
            _botVsBotButton.Button.onClick.AddListener(BotVsBotButtonPress);
            _playerVsPlayerButton.Button.onClick.AddListener(PlayerVsPlayerButtonPress);
            _settingsButton.onClick.AddListener(OnSettingsButtonPress);

            _menuButtons = new[]
            {
                _playerVsPlayerButton,
                _playerVsBotButton,
                _botVsBotButton
            };

            _visibilityRadiusButton = Screen.width * 0.5f;
        }

        private void PlayerVsBotButtonPress()
        {
            PlayerVsBotPressEvent?.Invoke();
        }

        private void BotVsBotButtonPress()
        {
            BotVsBotPressEvent?.Invoke();
        }

        private void PlayerVsPlayerButtonPress()
        {
            PlayerVsPlayerPressEvent?.Invoke();
        }

        private void OnSettingsButtonPress()
        {
            SettingsButtonPressEvent?.Invoke();
        }
    }
}