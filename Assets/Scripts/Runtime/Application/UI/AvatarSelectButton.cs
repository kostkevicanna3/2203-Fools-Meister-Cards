using Application.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarSelectButton : SimpleButton
{
    [SerializeField] private Image _avatarImage;
    [SerializeField] private Image _toggleImage;

    public event Action<AvatarSelectButton, string> OnAvatarSelected;

    public void Initialize(AvatarData data, bool selected)
    {
        _avatarImage.sprite = data.AvatarSprite;
        EnableToggle(selected);

        Button.onClick.AddListener(() =>
        {
            OnAvatarSelected?.Invoke(this, data.AvatarAssetName);
            _toggleImage?.gameObject.SetActive(true);   
        });
    }

    public void EnableToggle(bool enable) => _toggleImage.gameObject.SetActive(enable);
}
