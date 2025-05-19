using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AvatarSelectionConfig", menuName = "Config/AvatarSelectionConfig")]
public class AvatarSelectionConfig : BaseSettings
{
    [SerializeField] private List<AvatarData> _avatarDataList;

    public List<AvatarData> AvatarDataList => _avatarDataList;
}

[Serializable]
public class AvatarData
{
    public Sprite AvatarSprite;
    public string AvatarAssetName;
}
