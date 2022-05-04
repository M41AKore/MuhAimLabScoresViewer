using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuhAimLabScoresViewer
{
    public class BufferSecondsQueue
    {
        private ConcurrentQueue<byte[]> buffers;
        private int cutOff;

        public BufferSecondsQueue(int maxLength)
        {
            buffers = new ConcurrentQueue<byte[]>();
            cutOff = maxLength;
        }

        public void push(byte[] secondOfBytes)
        {
            buffers.Enqueue(secondOfBytes);
            if (buffers.Count > cutOff)
            {
                if (buffers.TryDequeue(out byte[] removed)) Console.WriteLine("dequeued");
            }
        }

        public byte[] getFullBuffer()
        {
            var ee = buffers.SelectMany(a => a).ToArray();
            return ee;
        }

        private byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }
    }
}
