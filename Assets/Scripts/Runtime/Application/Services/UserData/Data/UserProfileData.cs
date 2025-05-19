using System;
using UnityEngine;

[Serializable]
public class UserProfileData
{
    public string UserName = String.Empty;
    public int Age = 18;
    public Gender Gender = Gender.Male;

    public string UserAvatarAssetName = String.Empty;
}

public enum Gender
{
    Male,
    Female
}
