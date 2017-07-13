using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aspark.FileServer.Client
{
    public interface IFSClient : IDisposable
    {
        bool Save(string fileName, byte[] content);

        bool Del(string fileName);

        byte[] Get(string fileName);

        string GetAllText(string fileName);

        long Len(string fileName);

        bool Exists(string fileName);

    }
}
