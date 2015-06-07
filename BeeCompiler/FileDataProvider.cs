using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BeeCompiler
{
    class FileDataProvider : IDataProvider
    {
        public string GetScript(string address)
        {
            return File.ReadAllText(address);
        }
    }
}
