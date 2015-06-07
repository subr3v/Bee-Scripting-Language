using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    public class BeeScriptInstance
    {
        public Object ObjectContext;

        private Dictionary<string, MemberFunctionPointer> RegisteredMemberFunctionPointer;
        internal BeeScript script;
        internal Stack<Stack> currentStacks;
        internal SymbolDictionary Globals;
        internal SymbolDictionary Constants;
        internal SymbolDictionary CommonStack;
        internal int timeCounter = 0;
        internal double yieldTime = 0;
        internal int ProgramCounter = 0;

        public BeeScriptInstance(BeeScript script , object Context = null)
        {
            this.script = script;
            currentStacks = new Stack<Stack>();
            Globals = new SymbolDictionary();
            Constants = new SymbolDictionary();
            CommonStack = new SymbolDictionary();
            for(int i = 0 ; i < 16 ; i++)
            {
                CommonStack.AddSymbol();
            }
            RegisteredMemberFunctionPointer = new Dictionary<string, MemberFunctionPointer>();
            this.ObjectContext = Context;
        }

        public void RegisterMemberFunctionPointer(string Name , MemberFunctionPointer memberFunctionPointer)
        {
            this.RegisteredMemberFunctionPointer.Add(Name, memberFunctionPointer);
        }

        public Object InvokeMethod(string methodName , Variable[] Args)
        {
            if (RegisteredMemberFunctionPointer.ContainsKey(methodName))
            {
                return RegisteredMemberFunctionPointer[methodName].Call(Args);
            }
            else
            {
                var args = new Object[Args.Length];
                for (int i = 0; i < args.Length; i++)
                    args[i] = Args[i].Value;
                return ObjectContext.GetType().GetMethod(methodName).Invoke(ObjectContext, args);
            }
        }
    }
}
