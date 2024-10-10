using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DummyMailServer
{
    public class TcpServer
    {
        public static async Task StartAsync(string ip, int port)
        {
            TcpListener server = new TcpListener(IPAddress.Parse(ip), port);

            server.Start();

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                await new TcpHandler(client).HandleAsync();
            }
        }
    }
}