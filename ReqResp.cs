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

        public async Task<string> ReadAsync(CancellationToken ct) => await InternalReadAsync(ct);

        private async Task<string> InternalReadAsync(CancellationToken ct)
        {
            int len = 0;
            var chars = new byte[1024];
            var sb = new StringBuilder();

            while (stream.DataAvailable && (len = await stream.ReadAsync(chars, 0, chars.Length, ct)) != 0)
            {
                sb = Enumerable.Range(0, len).Aggregate(sb, (data, i) => {
                    if (chars[i] != '\r' && chars[i] != '\n')
                        buff.Append(chars, i, 1);
                    else
                    {
                        if (buff.Length > 0) data.AppendLine(Encoding.ASCII.GetString(buff.Get()));
                        buff.Reset();
                    }
                    return data;});
            }
            if (sb.Length > 0)
                ReadCompleted?.Invoke(this, new ReadCompletedArgs() { command = sb.ToString() });
            return sb.ToString();
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
