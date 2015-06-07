using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    class VariableInfo
    {
        public Object Value;
        public short RegisterLocation;

        static private short registerCounter = 0;

        public VariableInfo(Object value)
        {
            this.Value = value;
            this.RegisterLocation = registerCounter++;
        }

        static public void ResetRegisterCounter(short startCount = 0)
        {
            registerCounter = startCount;
        }
    }

    class RegisterMap : Dictionary<string, VariableInfo> { }
}
