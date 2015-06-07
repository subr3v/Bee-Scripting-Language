using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    public struct Instruction
    {
        public Opcodes Opcode;
        public byte Op1;
        public byte Op2;
        public byte Op3;
    }
    /* AddConstantSymbol 0 "ciao"
     * AddConstantSymbol 1 500
     * AddGlobalSymbol 0 "a"
     * AddGlobalSymbol 1 "b"
     * 
     * LABEL BOH
     * var a;
     * var a = "ciao"
     * var b = 500
     * var c = @HasCollided()
     * jump BOH
     * 
     * var a = 5
     * var b = 5
     * var c = a + b
     * 
     * LoadKG 0 0
     * LoadKG 1 0
     * ADD 0 1 2
     * 
     * CallNative A , B
     * CallNative 0 , 0
     * 
     * lillo( licco() )
     * 
     * function licco()
     * {
     *      ADD_LOCAL()
     *      ALLOCK_STACK()
     *      ALLOCK_STACK()
     *      
     *      local d = 5
     *      local c = "ciao"
     * }
     * 
     * licco ()
     * 
     * 
     */
}
