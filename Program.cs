using System.Threading.Tasks;

namespace DummyMailServer
{
    internal class Program
    {
        static async Task Main()
        {
            await TcpServer.StartAsync("127.0.0.1", 25);
        }
    }
}
