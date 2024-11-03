using Newtonsoft.Json;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LunaHost
{
    public class LunaHostBuilder
    {
        private List<PageContent> pageContents;
        public ushort port = 80;
        public IPAddress IP = IPAddress.Any;
        public PageContent DefaultPage = null!;
        public LunaHostBuilder()
        {
            pageContents = new List<PageContent>();
            pageContents.Add(new HealthCheckPage());
        }
        public void AddPage(PageContent content)
        {
            if (content == null) 
                throw new ArgumentNullException("content");
            pageContents.Add(content); 
        }
        public async Task Build()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IP, port));
            socket.Listen(200<<5);
            Console.Write($"SRV -ST\nHost : {IP}\nPort : {port}\nUrl : ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"http://{IP} , http://{IP}:{port}");
            Console.ResetColor();


            await Task.Run( async() =>
            {
                while (true)
                {
                    _ =  HandleRequest(await socket.AcceptAsync());
                }
            });
            
        }
        public class ErrorPage : PageContent
        {
            public ErrorPage() : base("/error")
            {
                
            }
            public  IHttpResponse Get()
            {
                return HttpResponse.NotFound("ERROR");
            }
        }
        public async Task HandleRequest(Socket client)
        {
            using (client)
            {
                
                byte[] data = new byte[32000];
                ArraySegment<byte> buffer = new ArraySegment<byte>(data);
                await client.ReceiveAsync(buffer);
                string request = string.Join("", data.Take(client.ReceiveBufferSize).Select(x => (char)x));
                Console.WriteLine(request);
                HttpRequest httpRequest = new HttpRequest(request);
                PageContent? PageContent = null!;
                var response = HttpResponse.NotFound().GetFullResponse();
                if (httpRequest.Path == "/")
                {
                    if (DefaultPage != null)
                    {
                        DefaultPage.Match(httpRequest);
                        PageContent = DefaultPage;
                    }
                    else PageContent = new ErrorPage();
                }
                else
                {
                     PageContent = pageContents.FirstOrDefault(x => x.Match(httpRequest));
                }
                if (PageContent == null)
                {
                    if (DefaultPage != null)
                    {
                        DefaultPage.Match(httpRequest);
                        PageContent = DefaultPage;
                    }
                 
                }
                if (PageContent != null)
                {
                    using (PageContent)
                    {
                        response = PageContent.HandleRequest().GetFullResponse();
                        data = Encoding.UTF8.GetBytes(response);
                        await client.SendAsync(data);

                    }

                }
                else
                {
                    data = Encoding.UTF8.GetBytes(HttpResponse.NotFound().GetFullResponse());
                  
                    await client.SendAsync(data);
                }
                
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
         
          

        }
    }
}
