using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace HCDU.Web.Server
{
    public static class HttpUtils
    {
        public static byte[] ReadBlock(NetworkStream stream, int blockLength)
        {
            const int maxInitialSize = 4096;
            MemoryStream mem = new MemoryStream(blockLength < maxInitialSize ? blockLength : maxInitialSize);

            byte[] buffer = new byte[4096];

            int restLength = blockLength;
            while (restLength > 0)
            {
                int bytesRead = stream.Read(buffer, 0, Math.Min(restLength, buffer.Length));
                //todo: can bytesRead be zero ?
                restLength -= bytesRead;
                mem.Write(buffer, 0, bytesRead);
            }

            return mem.ToArray();
        }

        public static void WriteLine(NetworkStream stream, string line)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(line + "\r\n");
            stream.Write(buffer, 0, buffer.Length);
        }

        public static void WriteHeader(NetworkStream stream, string headerName, string headerValue)
        {
            WriteLine(stream, string.Format("{0}: {1}", headerName, headerValue));
        }
    }
}