using Aspark.FileServer.Client.FS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Aspark.FileServer.Client
{
    public static class FSClient
    {
        //使用文件服务器
        public static IFSClient CreateFSClient(string ip, int port, string user = null, string pwd = null, string pathName = null)
        {
            return new ServerFSClient(ip, port, user, pwd, pathName);
        }

        //使用本地文件
        public static IFSClient CreateFSClient(string baseDir)
        {
            return new LocalFSClient(baseDir);
        }

        /// <summary>
        /// <para>本地地址：aaa/bbb</para>
        /// <para>远程地址：user:pwd@127.0.0.1:19860[pathName]</para>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IFSClient CreateFromPath(string path)
        {
            //todo:改为正则匹配
            if ((path.Contains(':') && !path.Contains("//")) || path.StartsWith("fileserver://", StringComparison.InvariantCultureIgnoreCase))//去除对现有协议的影响，如：ftp://
            {
                path = Regex.Replace(path, "^fileserver://", "", RegexOptions.IgnoreCase);

                string ip = null, user = null, pwd = null, pathName = null;
                int port = 19860;

                string[] arr = null;
                var url = path;
                if (path.Contains('@'))
                {
                    arr = path.Split('@');
                    if (arr[0].Contains(':'))
                    {
                        var arrUser = arr[0].Split(':');
                        user = arrUser[0];
                        pwd = arrUser[1];
                    }
                    else
                    {
                        user = arr[0];
                    }

                    url = arr[1];
                }

                var index = url.IndexOf('[');
                if (index > -1)
                {
                    pathName = url.Substring(index + 1).TrimEnd(']');
                    url = url.Substring(0, index);
                }

                index = url.IndexOf(':');
                if (index > -1)
                {
                    ip = url.Substring(0, index);
                    port = int.Parse(url.Substring(index + 1));
                }
                else
                    ip = url;


                return CreateFSClient(ip, port, user, pwd, pathName);
            }

            return CreateFSClient(path);
        }

        public static bool SaveToLocal(this IFSClient client, string remoteFileName, string fileFullName)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fileFullName));
            
            if (client is LocalFSClient)
            {
                var srcFileFullName = remoteFileName;

                if (!Path.IsPathRooted(remoteFileName))
                {
                    srcFileFullName = (client as LocalFSClient).GetFileFullName(remoteFileName);
                }

                if (!File.Exists(srcFileFullName))
                {
                    return false;
                }

                if (File.Exists(fileFullName) && new FileInfo(fileFullName).FullName == new FileInfo(srcFileFullName).FullName)//skip the same file
                    return true;

                File.Copy(srcFileFullName, fileFullName);

                return true;
            }

            if (client.Exists(remoteFileName))
            {
                var bytes = client.Get(remoteFileName);
                using(var fs = File.Open(fileFullName, FileMode.Create))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }

                return true;
            }

            return false;
        }
    }
}
