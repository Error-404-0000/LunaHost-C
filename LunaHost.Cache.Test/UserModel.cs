namespace LunaHost.Test
{
    [_LoggerAttribute]
    public class UserModel
    {
        public UserModel(string email, string password, int age)
        {
            Email = email;
            Password = password;
            Age = age;
        }

        public string Email { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }
    }   

}
