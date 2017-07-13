using Aspark.FileServer.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aspark.FileServer.Client.FS
{
    class LocalFSClient : IFSClient
    {
        private string _dir;

        public LocalFSClient(string dir)
        {
            if (Path.IsPathRooted(dir) == false)
                _dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir);
            else
                _dir = dir;
        }

        public string GetFileFullName(string fileName)
        {
            return Path.Combine(_dir, fileName);
        }

        public bool Save(string fileName, byte[] content)
        {
            var fileFullName = Path.Combine(_dir, fileName);

            var dir = Path.GetDirectoryName(fileFullName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using(var fs = File.Create(fileFullName))
            {

                fs.Write(content, 0, content.Length);
            }

            return true;
        }

        public bool Del(string fileName)
        {
            var fileFullName = Path.Combine(_dir, fileName);
            if (File.Exists(fileFullName))
                File.Delete(fileFullName);

            return true;
        }

        public byte[] Get(string fileName)
        {
            var fileFullName = Path.Combine(_dir, fileName);
            if (File.Exists(fileFullName))
            {
                return File.ReadAllBytes(fileFullName);
            }

            throw new FileServerError("file is not exists");
        }

        public string GetAllText(string fileName)
        {
            var fileFullName = Path.Combine(_dir, fileName);
            if (File.Exists(fileFullName))
            {
                return File.ReadAllText(fileFullName);
            }

            throw new FileServerError("file is not exists");
        }

        public long Len(string fileName)
        {
            var fileFullName = Path.Combine(_dir, fileName);
            if (File.Exists(fileFullName))
            {
                var fi = new FileInfo(fileFullName);
                
                return fi.Length;
            }

            return -1;
        }

        public bool Exists(string fileName)
        {
            var fileFullName = Path.Combine(_dir, fileName);

            return File.Exists(fileFullName);
        }

        public void Dispose()
        {
            
        }


    }
}
