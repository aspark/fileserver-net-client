using Aspark.FileServer.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace Aspark.FileServer.Client.Common
{
    class Connector : IDisposable
    {
        string _ip, _user, _pwd, _select;
        int _port;

        public Connector(string ip, int port, string user = null, string pwd = null, string pathName = null)
        {
            _ip = ip;
            _port = port;
            _user = user;
            _pwd = pwd;
            _select = pathName;
        }

        public bool IsBusy { get; private set; }

        TcpClient __tcp = null;
        private TcpClient GetConn()
        {
            if (__tcp != null && __tcp.Connected)
                return __tcp;

            CloseConn();

            __tcp = new TcpClient();
            __tcp.Connect(_ip, _port);

            if (_user != null && _pwd != null)
            {
                var time = ((long)(DateTime.UtcNow - DateTime.Parse("1970-01-01")).TotalSeconds).ToString();
                var bytes = Encoding.ASCII.GetBytes(_pwd + time);
                var md5 = string.Join("", MD5.Create().ComputeHash(bytes, 0, bytes.Length).Select(x => x.ToString("x2").ToLower()));
                var result = Send(Parser.ConvertToArrayBytes("AUTH", _user, md5, time));

                if (result is ErrorBlock)
                {
                    throw new Exception("登录失败：" + (result as ErrorBlock).Error);
                }

                if (!string.IsNullOrWhiteSpace(_select))
                {
                    result = Send(Parser.ConvertToArrayBytes("SELECT", _select));

                    if (result is ErrorBlock)
                    {
                        throw new Exception("目录切换失败：" + (result as ErrorBlock).Error);
                    }
                }
            }

            return __tcp;
        }

        private void CloseConn()
        {
            try
            {
                if (__tcp != null)
                {
                    __tcp.Close();
                    __tcp = null;
                }
            }
            catch { }
        }

        public BlockBase Send(byte[] bytes)
        {
            lock(this)
            {
                try
                {
                    IsBusy = true;

                    var ns = GetConn().GetStream();
                    ns.Write(bytes, 0, bytes.Length);
                    ns.Flush();

                    return Parser.ParseBlock(ns);
                }
                catch (IOException ex)
                {
                    CloseConn();

                    throw new FileServerDisconnectException(ex.Message);
                    //throw;
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }


        public void Dispose()
        {
            CloseConn();
        }
    }
}
