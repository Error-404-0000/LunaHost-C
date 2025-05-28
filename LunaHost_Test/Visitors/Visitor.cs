using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost_Test.Visitors
{
    public class Visitor
    {
        public readonly Guid Id = Guid.NewGuid();
        public string IP { get; set; }
        public string NickName => IP;
        public DateTime FirstViste { get; set; }
        public DateTime LastViste { get; set; }
  
        public string LastUrl {  get; set; }
        public List<RequestInformation> RequestInformation { get; set; } = new();
        public bool DontLog { get; set; }=false;
    }
}
