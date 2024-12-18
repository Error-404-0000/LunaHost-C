using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost_Test.Users
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserAuth = Guid.NewGuid().ToString().Replace("-", "");
        public List<Token> _tokens { get; set; } = new();
        public List<Token> Tokens
        {
            
            get
            {
                LastActive = DateTime.Now;
                return _tokens;
            }
        }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int TotalDomain;
        public DateTime JoinedDate { get; set; }
        public DateTime LastActive { get; set; }
    }
}
