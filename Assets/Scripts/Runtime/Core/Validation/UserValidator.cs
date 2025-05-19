using Core;
using Core.Services;
using System.Text.RegularExpressions;

public class UserValidator : IUserValidator
{
    private ISettingProvider _settingProvider;

    public UserValidator(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    public bool IsAgeValid(int age)
    {
        UserValidationConfig config = _settingProvider.Get<UserValidationConfig>(); 
        return age >= config.MinAge && age <= config.MaxAge;
    }

    public bool IsUsernameValid(string name)
    {
        UserValidationConfig config = _settingProvider.Get<UserValidationConfig>();
        return Regex.IsMatch(name, config.UsernameRegex);
    }
}