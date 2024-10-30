using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using SWH;
using webHosting.Endpoints;
namespace webHosting
{
    internal partial class Program
    {
        public static async Task Main()
        {
            SWHBuilder webbuilder = new SWHBuilder();
       
            webbuilder.AddPage(new Register());
            webbuilder.DefaultPage = new DynamicPage();
            webbuilder.AddPage(new Jobs());
            webbuilder.AddPage(new Replies());
            //var builder = new SWHSMTP.SMTPBuilder();
            //await builder.Build();
            await webbuilder.Build();
        }
       
        
    }

 
    
}
