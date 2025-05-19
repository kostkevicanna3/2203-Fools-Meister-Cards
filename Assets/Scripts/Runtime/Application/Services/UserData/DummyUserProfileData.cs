using UnityEngine;

public class DummyUserProfileData
{
    private UserProfileData _profileData;
    public UserProfileData ProfileData => _profileData;

    public void CopyDataFrom(UserProfileData originalData)
    {
        _profileData = new();
        _profileData.UserAvatarAssetName = originalData.UserAvatarAssetName;
        _profileData.Gender = originalData.Gender;
        _profileData.UserName = originalData.UserName;
        _profileData.Age = originalData.Age;
    }

    public void PasteDataTo(UserProfileData originalData)
    {
        originalData.UserAvatarAssetName = _profileData.UserAvatarAssetName;
        originalData.Gender = _profileData.Gender;
        originalData.UserName = _profileData.UserName;
        originalData.Age = _profileData.Age;
    }

    public void ModifyAvatar(string newAvatarAssetName) => _profileData.UserAvatarAssetName = newAvatarAssetName;
    public void ModifyGender(Gender newGender) => _profileData.Gender = newGender;
    public void ModifyUserName(string newName) => _profileData.UserName = newName;
    public void ModifyAge(int newAge) => _profileData.Age = newAge;
}
