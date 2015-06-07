using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    public class SymbolDictionary
    {
        public Variable[] Symbols = new Variable[0];

        public void AddSymbol()
        {
            Array.Resize<Variable>(ref Symbols, Symbols.Length + 1);
            Symbols[Symbols.Length - 1] = new Variable();
        }

        public void AddSymbol(Variable variable) 
        {
            Array.Resize<Variable>(ref Symbols, Symbols.Length + 1);
            Symbols[Symbols.Length - 1] = variable;
        }
    }
}
