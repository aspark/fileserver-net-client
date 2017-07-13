using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aspark.FileServer.Client.Exceptions
{
    public class FileServerError : Exception
    {
        public FileServerError(string msg)
            : base(msg)
        {

        }
    }
}
