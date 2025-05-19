using Application.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileScreen : UiScreen
{
    [SerializeField] private SimpleButton _goBackButton;

    [SerializeField] private SimpleButton _changeAgeButton;
    [SerializeField] private SimpleButton _changeGenderButton;
    [SerializeField] private SimpleButton _changeAvatarButton;

    [SerializeField] private SimpleButton _applyChangesButton;

    [SerializeField] private TextMeshProUGUI _usernameLabel;

    public event Action OnGoBackPressedEvent;

    public event Action OnChangeAgePressedEvent;
    public event Action OnChangeGenderPressedEvent;
    public event Action OnChangeAvatarPressedEvent;

    public event Action OnApplyChangesPressedEvent;

    public void Initialize()
    {
        _goBackButton.Button.onClick.AddListener(() => OnGoBackPressedEvent?.Invoke());
        _changeAgeButton.Button.onClick.AddListener(() => OnChangeAgePressedEvent?.Invoke());
        _changeGenderButton.Button.onClick.AddListener(() => OnChangeGenderPressedEvent?.Invoke());
        _changeAvatarButton.Button.onClick.AddListener(() => OnChangeAvatarPressedEvent?.Invoke());
        _applyChangesButton.Button.onClick.AddListener(() => OnApplyChangesPressedEvent?.Invoke());
    }

    public void SetUserData(UserProfileData data, Sprite avatar)
    {
        _usernameLabel.text = data.UserName;
        UpdateUserAge(data.Age);
        UpdateUserGender(data.Gender);
        UpdateUserAvatar(avatar);
    }

    public void UpdateUserAge(int age) => _changeAgeButton.GetComponentInChildren<TextMeshProUGUI>().text = age.ToString();
    public void UpdateUserGender(Gender gender) => _changeGenderButton.GetComponentInChildren<TextMeshProUGUI>().text = gender.ToString();
    public void UpdateUserAvatar(Sprite sprite) => _changeAvatarButton.GetComponent<Image>().sprite = sprite;

    private void OnDestroy()
    {
        _goBackButton.Button.onClick.RemoveAllListeners();
        _changeAgeButton.Button.onClick.RemoveAllListeners();
        _changeGenderButton.Button.onClick.RemoveAllListeners();
        _changeAvatarButton.Button.onClick.RemoveAllListeners();
        _applyChangesButton.Button.onClick.RemoveAllListeners();
    }
}
