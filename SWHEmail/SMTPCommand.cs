namespace SWHEmail
{
    public class SMTPCommand(string cmd) : Attribute
    {
        public string Command { get; set; } = cmd;
    }
}
