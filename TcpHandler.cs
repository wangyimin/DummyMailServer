using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DummyMailServer
{
    public class TcpHandler
    {
        private readonly Commands commands = new Commands();
        protected TcpClient client;
        protected Request req;
        protected Response resp;
        protected CancellationTokenSource cts = new CancellationTokenSource();

        public TcpHandler(TcpClient client)
        {
            this.client = client;
            var stream = client.GetStream();
            req = new Request() { stream = stream };
            resp = new Response() { stream = stream };
            req.ReadCompleted += OnReadCompleted;
        }
        
        public async Task HandleAsync()
        {
            try
            {
                await resp.WriteAsync("220 Hello from server\r\n");
                while (!cts.IsCancellationRequested)
                {
                    await req.ReadAsync(cts.Token);
                }
            }
            finally { cts?.Dispose(); req?.Dispose(); resp?.Dispose(); client?.Dispose(); }
        }

        private async void OnReadCompleted(object s, ReadCompletedArgs e)
        {
            string data = e.command;
#if DEBUG
            Console.Write(data);
#endif
            (bool rst, string cmd, string msg) = commands.TryExecute(data);
            if (rst)                     await resp.WriteAsync(msg);
            if (data.Equals("QUIT\r\n")) cts.Cancel();
        }
    }
}
