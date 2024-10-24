using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DummyMailServer
{
    public class ReadCompletedArgs : EventArgs
    {
        public string command;
    }

    public class Request : IDisposable
    {
        public NetworkStream stream;
        protected IBuffer buff = new CBuffer();
#if FROMASYNC
        private readonly SemaphoreSlim receiver= new SemaphoreSlim(1, 1);
#endif
#if BEGINREAD
        public void Read(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            var chars = new byte[1024];
            stream.BeginRead(chars, 0, chars.Length, 
                (ar)=>
                {
                    int len = stream.EndRead(ar);
                    Processing(chars, len);
                }, null);
        }
#endif
#if FROMASYNC
        public async Task ReadAsync(CancellationToken ct)
        {
            if (ct.IsCancellationRequested) return;

            await receiver.WaitAsync();

            var chars = new byte[1024];
            int len = await Task.Factory.FromAsync(stream.BeginRead, stream.EndRead, chars, 0, chars.Length, null);
            Processing(chars, len);

            receiver.Release();
        }
#endif
#if NORMAL
        public async Task ReadAsync(CancellationToken ct) => await InternalReadAsync(ct);

        private async Task InternalReadAsync(CancellationToken ct)
        {
            int len;
            var chars = new byte[1024];

            while (stream.DataAvailable && (len = await stream.ReadAsync(chars, 0, chars.Length, ct)) > 0)
            {
                Processing(chars, len);
            }
        }
#endif
        private void Processing(byte[] chars, int len)
        {
            if (len <= 0) return;

            var sb = Enumerable.Range(0, len).Aggregate(new StringBuilder(), (data, i) =>
            {
                if (chars[i] != '\r' && chars[i] != '\n')
                    buff.Append(chars, i, 1);
                else
                {
                    if (buff.Length > 0) data.AppendLine(Encoding.ASCII.GetString(buff.Get()));
                    buff.Reset();
                }
                return data;
            });
            if (sb.Length > 0)
                ReadCompleted?.Invoke(this, new ReadCompletedArgs() { command = sb.ToString() });
        }

        public void Dispose() => stream?.Close();

        public event EventHandler<ReadCompletedArgs> ReadCompleted;
    }

    public class Response : IDisposable
    {
        public NetworkStream stream;
        public async Task WriteAsync(string s)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            await stream.WriteAsync(bytes, 0, bytes.Length);
            await stream.FlushAsync();
        }

        public void Dispose() => stream?.Close();
    }
}
