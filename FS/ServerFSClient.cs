using Aspark.FileServer.Client.Common;
using Aspark.FileServer.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Aspark.FileServer.Client.FS
{
    class ServerFSClient : IFSClient, IDisposable
    {
        private Connector _conn = null;
        private Thread _aliveThread = null;

        public ServerFSClient(string ip, int port, string user, string pwd, string pathName)
        {
            _conn = new Connector(ip, port, user, pwd, pathName);
            _aliveThread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(60 * 1000);//60s一次
                    if (!_conn.IsBusy)
                        _conn.Send(Parser.ConvertToArrayBytes("PING"));
                }
            });
            _aliveThread.IsBackground = true;
            _aliveThread.Start();
        }

        private void HandleError(BlockBase block)
        {
            if (block == null)
            {
                throw new FileServerNoResponseException("无返回值");
            }

            if (block is ErrorBlock)
            {
                throw new FileServerError((block as ErrorBlock).Error);
            }
        }

        public bool Save(string fileName, byte[] content)
        {
            var result = _conn.Send(Parser.ConvertToArrayBytes("SAVE", new string[] { fileName }, content));

            HandleError(result);

            return (result as StringBlock).Value == "OK";
        }

        public byte[] Get(string fileName)
        {
            var result = _conn.Send(Parser.ConvertToArrayBytes("GET", fileName));

            HandleError(result);

            return (result as BytesBlock).Value;
        }

        public string GetAllText(string fileName)
        {
            using (var ms = new MemoryStream(this.Get(fileName)))
            {
                using (var sr = new StreamReader(ms))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public bool Del(string fileName)
        {
            var result = _conn.Send(Parser.ConvertToArrayBytes("DEL", fileName));

            HandleError(result);

            return (result as StringBlock).Value == "OK";
        }

        public long Len(string fileName)
        {
            var result = _conn.Send(Parser.ConvertToArrayBytes("LEN", fileName));

            HandleError(result);

            return (result as Int64Block).Value;
        }

        public bool Exists(string fileName)
        {
            var content = Parser.ConvertToArrayBytes("EXISTS", fileName);

            var result = _conn.Send(Parser.ConvertToArrayBytes("EXISTS", fileName));

            HandleError(result);

            return (result as Int64Block).Value == 1;
        }

        public void Dispose()
        {
            if (_aliveThread != null)
            {
                _aliveThread.Abort();
            }

            if (_conn != null)
                _conn.Dispose();
        }


    }
}
