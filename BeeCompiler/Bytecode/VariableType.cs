using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler.Bytecode
{
    enum VariableType : byte
    {
        Double = 0 ,
        Boolean = 1 ,
        String = 2 ,
        ObjRef = 3,
    }
}
