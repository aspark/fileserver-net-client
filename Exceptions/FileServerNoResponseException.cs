using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aspark.FileServer.Client.Exceptions
{
    class FileServerNoResponseException: Exception
    {
        public FileServerNoResponseException(string msg)
            : base(msg)
        {

        }
    }
}
