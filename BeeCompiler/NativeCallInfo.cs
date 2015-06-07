using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    public class NativeCallInfo
    {
        private Dictionary<string, InputOutputTypes> registeredNativeCalls = new Dictionary<string,InputOutputTypes>();

        public Dictionary<string, InputOutputTypes> NativeCalls { get { return registeredNativeCalls; } }

        public NativeCallInfo()
        {

        }

        public void addNativeCall(string funcName, string outputType, params string[] inputTypes)
        {
            registeredNativeCalls.Add(funcName, new InputOutputTypes()
            {
                InputTypes = inputTypes,
                OutputType = outputType,
            });
        }
    }
}
