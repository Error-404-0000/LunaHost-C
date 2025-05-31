using Attributes;
using Interfaces;
using LunaHost.Attributes;
using LunaHost.Attributes.HttpMethodAttributes;
using LunaHost.Dependency;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;

namespace LunaHost.Test
{
    public class MembersCount
    {
        public int ActiveMembers { get; set; }
        public int TotalMembers { get; set; }
        public int MemberCount { get; set; }
    }
    [Route("/members/active")]
    public class GetActiveMembers : PageContent
    {
      
        [GetMethod("/")]
        public IHttpResponse GetActiveMember([DInject] MembersCount members)
        {
            return HttpResponse.OK($"Active Members: {members.TotalMembers}");
        }
    }
    [Route("/hello")]
    public class HelloWorld_ : PageContent
    {
        [DInject]
        public  MembersCount members { get; set; }
        [GetMethod]
        public IHttpResponse GetHelloWorld()
        {
            members.TotalMembers++;
            return HttpResponse.OK("Hello World!");
        }
    }
    public class Program
    {
        static void Main(string[] args)
        {
            IDependencyContainer container = new DependencyContainer();
            using (LunaHostBuilder host = new LunaHostBuilder(container))
            {
                container.AddSingleton<MembersCount>();
                container.AddTransient < ...> ();
                container.AddTransient<IInterface,...> ();
                container.AddTransient<IInterface,...> (builder => object);




                host.Errorpage = (HTMLContent)"<h1>Error Page</h1>";
                host.Add(new UserRoute());
                host.Add(new GetActiveMembers());
                host.Add(new HelloWorld_());
                host.Port = 80;
                host.UseSwagger = true;
                ////
                //host.onRequestReceived += Host_onRequestReceived;
                //host.onResponseSent += Host_onResponseSent;
                host.BuildAsync().Wait();
            }
        }

        private static void Host_onResponseSent(object sender, HttpRequest request, IHttpResponse respond)
        {
            Console.WriteLine($"[RESPOND] :{respond}");
        }

        private static void Host_onRequestReceived(object sender, HttpRequest request)
        {
            Console.WriteLine($"[REC] : {request}");
        }
    }

}
