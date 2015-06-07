using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    public class MemberFunctionPointer
    {
        private string funcName;
        private int argNum;
        private object obj;

        public MemberFunctionPointer(Object obj, string FuncName, int argNum)
        {
            this.obj = obj;
            this.funcName = FuncName;
            this.argNum = argNum;
        }

        public Object Call(Variable[] Args)
        {
            var args = new Object[Args.Length];
            for (int i = 0; i < args.Length; i++)
                args[i] = Args[i].Value;
            if (!(obj is Type))
                return obj.GetType().GetMethod(funcName).Invoke(obj, args);
            else
                return (obj as Type).GetMethod(funcName).Invoke(null, args);
        }
    }
}
