namespace LunaHost_Test.Users
{
    public record Token(string UserToken,DateTime ExpirDate)
    {
        public bool IsTokenAlive => ExpirDate > DateTime.Now;
    }
}
