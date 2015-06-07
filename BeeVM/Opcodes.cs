using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    /// <summary>
    /// All  opcodes follow the convention SourceLoc , DestLoc for binary opcodes ( LOADCONST , GETGLOBAL , SETGLOBAL , NEGATE , MOVE )
    /// All  opcodes follow the convention FirstOperand , SecondOperand , Destination for ternary opcodes ( ADD , SUBTRACT , MULTIPLY ,
    /// DIVIDE , GREATER , GREATEREQUAL , EQUAL , AND_OPERATOR , OR_OPERATOR )
    /// </summary>
    public enum Opcodes : byte
    {
        /// <summary>
        /// Adds ( op1 ) locals in the stack
        /// </summary>
        MAKELOCAL,
        /// <summary>
        /// Loads a constant in the local register ( K1 , K2 , L )
        /// </summary>
        LOADCONST,
        /// <summary>
        /// Loads a global in the local register ( G , L )
        /// </summary>
        GETGLOBAL,
        /// <summary>
        /// Sets a global from the local register ( L , G )
        /// </summary>
        SETGLOBAL,
        /// <summary>
        /// Stop script if stack depth is 1 else puts program counter to the popped stack and puts in the 0 common stack the value returned if any.
        /// </summary>
        RETURN,
        /// <summary>
        /// Shifts the Program Counter by a short value by a short value encoded with op1 ( high ) and op2 ( low ) values 
        /// </summary>
        JUMP,
        /// <summary>
        /// Copy from the stack local pointed by op1 to stack local pointed by op2
        /// </summary>
        MOVE,
        /// <summary>
        /// If the value in the stack location pointed by op1 is true shifts the Program Counter by a short value encoded with op2 ( high ) and op3 ( low ) values 
        /// </summary>
        JUMPIF,
        /// <summary>
        /// Puts in the negated value of stack location pointed by op1 in the stack location pointed by op2
        /// </summary>
        NEGATE,
        /// <summary>
        /// Puts in the stack location pointed by op3 the result of stack location pointed by op1 > stack location pointed by op 2
        /// </summary>
        GREATER,
        /// <summary>
        /// Puts in the stack location pointed by op3 the result of stack location pointed by op1 >= stack location pointed by op 2
        /// </summary>
        GREATEROREQUAL,
        /// <summary>
        ///Puts in the stack location pointed by op3 the result of stack location pointed by op1 == stack location pointed by op 2
        /// </summary>
        EQUAL,
        /// <summary>
        /// Puts in the stack location pointed by op3 the result of stack location pointed by op1 + stack location pointed by op 2
        /// </summary>
        ADD,
        /// <summary>
        /// Puts in the stack location pointed by op3 the result of stack location pointed by op1 - stack location pointed by op 2
        /// </summary>
        SUBTRACT,
        /// <summary>
        /// Puts in the stack location pointed by op3 the result of stack location pointed by op1 * stack location pointed by op 2
        /// </summary>
        MULTIPLY,
        /// <summary>
        /// Puts in the stack location pointed by op3 the result of stack location pointed by op1 / stack location pointed by op 2
        /// </summary>
        DIVIDE,
        /// <summary>
        /// Puts in the stack location pointed by op3 the result of stack location pointed by op1 && stack location pointed by op 2
        /// </summary>
        AND_OPERATOR,
        /// <summary>
        /// Puts in the stack location pointed by op3 the result of stack location pointed by op1 || stack location pointed by op 2
        /// </summary>
        OR_OPERATOR,
        /// <summary>
        /// Calls a function getting the offset from op1 local stack and with Op2 representing the the number of arguments
        /// </summary>
        CALL,
        /// <summary>
        /// Sets in the op2 location of common stack the value from op1 location of current stack
        /// </summary>
        SETCOMMON,
        /// <summary>
        /// Pauses execution of script until the time pointed by the op1 in the local stack has passed
        /// </summary>
        YIELD,
        /// <summary>
        /// Performs a native call function , which is called by the hoster.
        /// Op1 is the location from the constant register that indicates the name of the function
        /// Op2 is the number of arguments
        /// </summary>
        NATIVE_CALL,
    }
}
