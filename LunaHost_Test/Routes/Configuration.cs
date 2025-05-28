using LunaHost_Test.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunaHost_Test.Routes
{
    public class Configuration
    {
        public Guid UserId { get; set; }
        public readonly Guid Id = Guid.NewGuid();
        //for admin to view mul names
        public string ConfigurationName {  get; set; }
        //redirct:exmple.com
        public string Domain {  get; set; }
        //hoster.com/{guid}/StartPath
        public string StartPath {  get; set; }
        public bool HasStartPath =>StartPath != null;
        public List<TargetPathConfiguration> TargetPathConfiguration { get; set; } = new();
        public List<TargetPathLog> TargetPathLogs { get; set; } = new();
        public bool EnableLogging {  get; set; }
        public List<Visitor> Visitors { get; set; }
        public bool Disabled { get; set; }
        public bool isDisabled => Disabled;
        public int TotalRequest { get; set; }
    }
}
