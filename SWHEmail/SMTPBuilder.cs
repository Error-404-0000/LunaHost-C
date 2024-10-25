using SWHEmail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SWHSMTP
{
    public class SMTPBuilder
    {
        public IPAddress IPAddress { get; set; } = IPAddress.Any;
        public int Port { get; } = 4095;
        public async Task Build()
        {
            Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress, Port));
            socket.Listen(200 << 5);
            Console.WriteLine("SMTP Started");
            await Task.Run(() =>
            {
                while (true)
                {
                    _ = Task.Run(async () => await HandleRequest(await socket.AcceptAsync()));
                }
            });

        }
        public async Task HandleRequest(Socket client)
        {
           EmailData Mail = new EmailData();
           ArraySegment<byte> buffer = new(Encoding.ASCII.GetBytes("250-smtp.example.com Hello localhost\r\n250-STARTTLS\r\n250 AUTH LOGIN PLAIN\r\n"));
            await client.SendAsync(buffer, SocketFlags.None);
            await client.ReceiveAsync(buffer);
           foreach (var Object in Mail.GetType().GetProperties().Where(x => x.GetCustomAttributes<SMTPCommand>(false).Any()).Select(x=>(prop:x,Attribute:x.GetCustomAttribute<SMTPCommand>())))
           {
                ArraySegment<byte> request = new(Encoding.ASCII.GetBytes("250 OK"+"\r\n"));
                await client.SendAsync(request);
                byte[] mainBuffer = new byte[1028];
                await client.ReceiveAsync(mainBuffer);
                //for string only else:error
                try
                {
                    Object.prop.SetValue(Mail, Encoding.ASCII.GetString(mainBuffer.ToArray(),0,client.ReceiveBufferSize));
                }
                catch
                {
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
                request = new(Encoding.ASCII.GetBytes("250 OK\r\n"));
                await client.SendAsync(request);
           }
           
        }
    }
}
