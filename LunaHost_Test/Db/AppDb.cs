using LunaHost_Test.Routes;
using LunaHost_Test.Users;
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
    }
}
