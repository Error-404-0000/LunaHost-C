using LunaHost_Test.Routes;
using LunaHost_Test.Users;
using LunaHost_Test.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost_Test.Db
{
    public static class AppDb
    {
        public static readonly List<Configuration> Configurations=new List<Configuration>();
        public static readonly List<User> Users  = new();
        public static bool UserisAuth(string token,string auth)
        {
            if(string.IsNullOrEmpty(token)) return false;
            if(string.IsNullOrEmpty(auth)) return false;
            if (Users.Any(x => x.Tokens.Any(t => t.UserToken == token && t.IsTokenAlive))) return true;
            return false;
        }

        public static List<Visitor> GetVisitors(string configId,string token, string auth)
        {
            if (UserisAuth(token, auth))
                return Configurations.FirstOrDefault(x =>x.Id.ToString().Replace("-","") == configId && x.UserId == (Users.FirstOrDefault(u => u.Tokens.Any(t => t.UserToken == token && t.IsTokenAlive))?.Id ))?.Visitors??[];
            return [];
        }
    }
}
