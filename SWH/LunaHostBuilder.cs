using Newtonsoft.Json;
using LunaHost.HTTP.Interface;
using LunaHost.HTTP.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using LunaHost.HTTP.Helper;
using Swegger;

namespace LunaHost
{
    public partial class LunaHostBuilder : IDisposable
    {
        private readonly List<PageContent> pageContents = new();
        public ushort Port { get; set; } = 80;
        public IPAddress IP { get; set; } = IPAddress.Loopback;
        public PageContent DefaultPage { get; set; } = new ErrorPage();
        public PageContent Errorpage { get; set; } = new ErrorPage();
        public readonly int Capacity;
        private bool _useSwagger = false;
        private string swagger_version = "2.0";
        public List<SwaggerContent.Server> Servers { get; set; }
        private SwaggerContent.OpenApiSpec openApiSpec { get; set; }
        public string Openapi { get; set; } = "3.0.0";
        public bool InDebugMode { get; set; }
        public bool UseSwagger
        {
            get => _useSwagger;
            set
            {
                _useSwagger = value;
                ConfigureSwaggerContent();
            }
        }

        public void ConfigureSwaggerContent()
        {
            if (openApiSpec is null)
            {
                openApiSpec = new SwaggerContent.OpenApiSpec
                (
                    info: new SwaggerContent.Info
                    (
                        title: nameof(SwaggerUIContent),
                        version: Openapi ?? "3.0.0",
                        description: "LunaHost HTTP."
                    ),
                    servers: new()
                    {
                         new SwaggerContent.Server(
                                     url: $"http://{IP}:{Port}",
                                     description: nameof(SwaggerUIContent)
                         )
                    },
                    swagger_version: swagger_version

                );
            }

            if (!pageContents.Any(x => x is SwaggerContent))
            {
                this.Add(new SwaggerContent(pageContents, openApiSpec, Openapi));
            }

            if (!pageContents.Any(y => y is SwaggerUIContent))
            {
                this.Add(new SwaggerUIContent(".\\Swegger\\dist\\"));
            }
        }

        public bool LogRequest { get; set; } = false;
        public event Action<HttpRequest>? OnRequestReceived;
        public event Action<HttpRequest, IHttpResponse>? OnResponseSent;

        /// <summary>
        /// Used primarily for private health checks at /health/{Build_Token}/check
        /// </summary>
        /// 
        public static string Build_Token = Guid.NewGuid().ToString();
        private bool _disposed = false;
        private bool Stop { get; set; } = false;
        private Socket socket;

        public LunaHostBuilder(int Capacity = 10 << 2)
        {
            pageContents.Add(new HealthCheckPage());
            this.Capacity = Capacity;

        }

        public void Add(PageContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));
            pageContents.Add(content);
        }



        public async Task BuildAsync(bool SkipHealthCheck = false)
        {
            socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IP, Port));
            socket.Listen(Capacity);

            Console.WriteLine($"LunaHost\nHost : {IP}\nPort : {Port}\nUrl : http://{IP}:{Port}\n\n");

            var task = Task.Run(async () =>
            {
                while (!Stop)
                {
                    Socket client = await socket.AcceptAsync();
                    _ = HandleRequestAsync(client);
                }
            });

            if (SkipHealthCheck)
                await task;
            else
            {

                if (await HealthCheck())
                    await task;
                else
                {
                    throw new Exception("Health check failed. Set skiphealthcheck to true to avoid this.");
                }
            }

        }
        public async Task<bool> HealthCheck()
        {
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine("<<Checking Health Page>>");

                try
                {
                    var result = await client.GetAsync($"http://{IP}:{Port}/health/{Build_Token}/check");
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Health Page returned OK 200 " + await result.Content.ReadAsStringAsync());
                        Console.ResetColor();
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Health Check failed with status: {result.StatusCode}");
                        Console.ResetColor();
                        return false;
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Health Check encountered an error: {ex.Message}");
                    Console.ResetColor();
                    return false;
                }
            }
        }

        public void StopServer() => Stop = true;
        public delegate IHttpResponse HandleRequest(HttpRequest request,bool reset);

        //this try to handle the Request and if it failed it will return error or 404
        private IHttpResponse TryHandleRequest(HandleRequest page,HttpRequest request,bool reset, bool ReturnError)
        {
            try
            {
               return page.Invoke(request, reset);
            }
            catch(Exception ex)
            {
                if (ReturnError)
                {
                    return HttpResponse.InternalServerError(ex.ToString());
                }
                else
                {
                    return HttpResponse.BadRequest();
                }
            }

        }
        private async Task HandleRequestAsync(Socket client)
        {
            try
            {
                using (client)
                {
                    byte[] data = new byte[32000];
                    int bytesReceived = await client.ReceiveAsync(data, SocketFlags.None);
                    string requestString = Encoding.UTF8.GetString(data, 0, bytesReceived);

                    if (LogRequest)
                        Console.WriteLine(requestString);

                    HttpRequest httpRequest = new(requestString);
                    OnRequestReceived?.Invoke(httpRequest);

                    PageContent? pageContent = GetPageContent(httpRequest);
                    IHttpResponse httpResponse = pageContent != null
                        ? TryHandleRequest(pageContent.HandleRequest, httpRequest, false, InDebugMode)
                        : HttpResponse.NotFound();

                    OnResponseSent?.Invoke(httpRequest, httpResponse);

                    string response = httpResponse.GetFullResponse();
                    byte[] responseData = Encoding.UTF8.GetBytes(response);

                    await client.SendAsync(responseData, SocketFlags.None);
                    client.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine("Error handling request: " + ex.Message);
            }
        }

        private PageContent? GetPageContent(HttpRequest httpRequest)
        {
            if (httpRequest.Path == "/")
                return DefaultPage;
            return pageContents.FirstOrDefault(x => x.Match(httpRequest)) ?? Errorpage;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Dispose managed resources
                if (socket != null)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    socket.Dispose();
                }
            }

            // Free unmanaged resources if any here
            _disposed = true;
        }

        // Finalizer
        ~LunaHostBuilder()
        {
            Dispose(false);
        }

    }
}
