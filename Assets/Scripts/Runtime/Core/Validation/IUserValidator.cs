public interface IUserValidator
{
    public bool IsUsernameValid(string name);
    public bool IsAgeValid(int age);  
}