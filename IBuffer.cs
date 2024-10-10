using System;

namespace DummyMailServer
{
    public interface IBuffer
    {
        byte[] Get();
        int Length { get; }
        void Append(byte[] buffer, int start, int length);
        void Reset();
    }

    public class CBuffer : IBuffer
    {
        private readonly byte[] buff = new byte[1024];
        private int pos = 0;

        public int Length => pos;

        public byte[] Get()
        {
            var r = new byte[pos];
            Array.Copy(buff, r, pos);
            return r;
        }
        public void Append(byte[] buffer, int start, int length)
        {
            Array.Copy(buffer, start, buff, pos, length);
            pos += length;
        }

        public void Reset() => pos = 0;
    }
}
