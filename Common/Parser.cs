using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Aspark.FileServer.Client.Common
{
    class Parser
    {
        public static byte[] ConvertToSimpleStringBytes(string content)
        {
            return Encoding.UTF8.GetBytes("+" + content + "\r\n");
        }

        public static byte[] ConvertToBulkStringBytes(string content)
        {
            if (content == null)
                return Encoding.UTF8.GetBytes("$-1\r\n");

            var bytes = Encoding.UTF8.GetBytes(content);

            return Encoding.UTF8.GetBytes("$" + bytes.Length.ToString() + "\r\n").Concat(bytes).Concat(new byte[] { (byte)'\r', (byte)'\n' }).ToArray();
        }

        public static byte[] ConvertToBulkBytes(byte[] content)
        {
            if (content != null)
            {
                var bs = new List<byte>();

                bs.AddRange(Encoding.UTF8.GetBytes("!" + content.Length.ToString() + "\r\n"));

                bs.AddRange(content);

                bs.AddRange(new byte[] { (byte)'\r', (byte)'\n' });

                return bs.ToArray();
            }
            else
            {
                return Encoding.UTF8.GetBytes("!-1\r\n");
            }
        }

        public static byte[] ConvertToArrayBytes(string cmd, params string[] args)
        {
            List<byte[]> bs = new List<byte[]>() { ConvertToBulkStringBytes(cmd) };

            if (args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    bs.Add(ConvertToBulkStringBytes(arg));
                }
            }

            return Encoding.UTF8.GetBytes("*" + bs.Count.ToString() + "\r\n").Concat(bs.SelectMany(b => b)).ToArray();
        }

        public static byte[] ConvertToArrayBytes(string cmd, string[] args, byte[] content)
        {
            List<byte[]> bs = new List<byte[]>() { ConvertToBulkStringBytes(cmd) };

            if (args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    bs.Add(ConvertToBulkStringBytes(arg));
                }
            }

            bs.Add(ConvertToBulkBytes(content));

            return Encoding.UTF8.GetBytes("*" + bs.Count.ToString() + "\r\n").Concat(bs.SelectMany(b => b)).ToArray();
        }


        public static BlockBase ParseBlock(NetworkStream ns)
        {
            var type = (char)ns.ReadByte();
            switch (type)
            {
                case '-':
                    return new ErrorBlock(Encoding.UTF8.GetString(ReadLine(ns)));
                case '+':
                    return new StringBlock(Encoding.UTF8.GetString(ReadLine(ns)));
                case '$':
                    return new StringBlock(Encoding.UTF8.GetString(ReadByLength(ns)));
                case '!':
                    return new BytesBlock(ReadByLength(ns));
                case '*':
                    var count = int.Parse(Encoding.UTF8.GetString(ReadLine(ns)));
                    var blks = new List<BlockBase>();
                    for (var i = 0; i < count; i++)
                    {
                        blks.Add(ParseBlock(ns));
                    }

                    return new ArrayBlock(blks.ToArray());
                case ':':
                    return new Int64Block(long.Parse(Encoding.UTF8.GetString(ReadLine(ns))));
            }

            return null;
        }


        public static byte[] ReadLine(NetworkStream ns)
        {
            var line = new List<byte>();
            int read;
            while ((read = ns.ReadByte()) > -1)
            {
                if (read == '\r')
                {
                    ns.ReadByte();//pop a byte \n
                    break;
                }
                else
                {
                    line.Add((byte)read);
                }
            }

            return line.ToArray();
        }

        public static byte[] ReadByLength(NetworkStream ns)
        {
            var line = ReadLine(ns);
            var length = int.Parse(Encoding.UTF8.GetString(line));//.Skip(1).ToArray()
            var bytes = new byte[length];
            var index = 0;
            var buff = new byte[1024];
            while (index < length)
            {
                var len = ns.Read(buff, 0, length - index > buff.Length ? buff.Length : length - index);
                Array.Copy(buff, 0, bytes, index, len);
                index += len;
            }
            
            //ns.Read(bytes, 0, length);

            ns.ReadByte();//\r
            ns.ReadByte();//\n

            return bytes;
        }
    }
}
