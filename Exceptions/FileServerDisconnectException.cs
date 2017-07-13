using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Aspark.FileServer.Client.Exceptions
{
    class FileServerDisconnectException : IOException
    {
        public FileServerDisconnectException(string msg)
            : base(msg)
        {

        }
    }
}
