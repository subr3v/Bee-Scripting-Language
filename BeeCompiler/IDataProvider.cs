using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    public interface IDataProvider
    {
        string GetScript(string address);
    }
}
