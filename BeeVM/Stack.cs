using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    class Stack
    {
        public int ProgramCounter;
        public Variable[] Variables;

        public Stack()
        {
            Variables = new Variable[1];
        }

        public void AddVariable()
        {
            Array.Resize<Variable>(ref Variables , Variables.Length + 1);   
        }
    }
}
