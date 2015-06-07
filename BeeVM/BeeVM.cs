using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeVM
{
    public class BeeVM
    {
        public static bool ExecuteScript(BeeScriptInstance scriptInstance , int timePassed)
        {
            if (scriptInstance.currentStacks.Count == 0)
            {
                StartupScript(scriptInstance);
            }
            return InterpretBytecode(scriptInstance , timePassed);
        }

        private static bool InterpretBytecode(BeeScriptInstance scriptInstance , int timePassed)
        {
            bool execute = true;
            scriptInstance.timeCounter += timePassed;
            bool isYielding = scriptInstance.timeCounter < scriptInstance.yieldTime;
            bool jumped = false;
            while (execute && !isYielding)
            {
                jumped = false;
                Instruction instruction = scriptInstance.script.Instructions[scriptInstance.ProgramCounter];
                Stack currentStack = scriptInstance.currentStacks.Peek();
                switch (instruction.Opcode)
                {
                    case Opcodes.MAKELOCAL:
                        for(int i = 0 ; i < instruction.Op1 ; i++)
                            currentStack.AddVariable();
                        break;
                    case Opcodes.LOADCONST:
                        currentStack.Variables[instruction.Op3] = scriptInstance.Constants.Symbols[BeeUtils.ConvertFromBytes(instruction.Op1, instruction.Op2)];
                        break;
                    case Opcodes.GETGLOBAL:
                        currentStack.Variables[instruction.Op2] = scriptInstance.Globals.Symbols[instruction.Op1];
                        break;
                    case Opcodes.SETGLOBAL:
                        scriptInstance.Globals.Symbols[instruction.Op2] = currentStack.Variables[instruction.Op1];
                        break;
                    case Opcodes.MOVE:
                        currentStack.Variables[instruction.Op2] = currentStack.Variables[instruction.Op1];
                        break;
                    case Opcodes.JUMPIF:
                        var variable = currentStack.Variables[instruction.Op1];
                        if (variable.Value is Boolean)
                        {
                            if ( (Boolean)variable.Value == true)
                            {
                                short offsetJumpIf = BeeUtils.ConvertFromBytes(instruction.Op2, instruction.Op3);
                                scriptInstance.ProgramCounter += offsetJumpIf;
                                jumped = true;
                            }
                        }
                        else
                        {
                            throw new BeeVMException("Can't JumpIf on a local variable different than boolean");
                        }
                        break;
                    case Opcodes.RETURN:
                        if (scriptInstance.currentStacks.Count == 1)
                            execute = false;
                        else
                        {
                            var returnVal = currentStack.Variables[0];
                            scriptInstance.currentStacks.Pop();
                            scriptInstance.ProgramCounter = scriptInstance.currentStacks.Peek().ProgramCounter;
                            scriptInstance.currentStacks.Peek().Variables[0] = returnVal;
                            jumped = true;
                        }
                        break;
                    case Opcodes.JUMP:
                        short offsetJump = BeeUtils.ConvertFromBytes(instruction.Op1, instruction.Op2);
                        scriptInstance.ProgramCounter += offsetJump;
                        jumped = true;
                        break;
                    case Opcodes.NEGATE:
                        currentStack.Variables[instruction.Op2] = currentStack.Variables[instruction.Op1].Negated();
                        break;
                    case Opcodes.GREATER:
                        currentStack.Variables[instruction.Op3] = currentStack.Variables[instruction.Op1].Greater(currentStack.Variables[instruction.Op2]);
                        break;
                    case Opcodes.GREATEROREQUAL:
                        currentStack.Variables[instruction.Op3] = currentStack.Variables[instruction.Op1].GreaterEqual(currentStack.Variables[instruction.Op2]);
                        break;
                    case Opcodes.EQUAL:
                        currentStack.Variables[instruction.Op3] = currentStack.Variables[instruction.Op1].Equality(currentStack.Variables[instruction.Op2]);
                        break;
                    case Opcodes.ADD:
                        currentStack.Variables[instruction.Op3] = currentStack.Variables[instruction.Op1].Add(currentStack.Variables[instruction.Op2]);
                        break;
                    case Opcodes.SUBTRACT:
                        currentStack.Variables[instruction.Op3] = currentStack.Variables[instruction.Op1].Subtract(currentStack.Variables[instruction.Op2]);
                        break;
                    case Opcodes.MULTIPLY:
                        currentStack.Variables[instruction.Op3] = currentStack.Variables[instruction.Op1].Multiply(currentStack.Variables[instruction.Op2]);
                        break;
                    case Opcodes.DIVIDE:
                        currentStack.Variables[instruction.Op3] = currentStack.Variables[instruction.Op1].Divide(currentStack.Variables[instruction.Op2]);
                        break;
                    case Opcodes.OR_OPERATOR:
                        currentStack.Variables[instruction.Op3] = currentStack.Variables[instruction.Op1].Or(currentStack.Variables[instruction.Op2]);
                        break;
                    case Opcodes.AND_OPERATOR:
                        currentStack.Variables[instruction.Op3] = currentStack.Variables[instruction.Op1].And(currentStack.Variables[instruction.Op2]);
                        break;
                    case Opcodes.CALL:
                        currentStack.ProgramCounter = scriptInstance.ProgramCounter + 1;
                        var offsetAbsolute = ((double)currentStack.Variables[instruction.Op1].Value);
                        offsetJump = (short)(offsetAbsolute - scriptInstance.ProgramCounter);
                        scriptInstance.currentStacks.Push(new Stack());
                        currentStack = scriptInstance.currentStacks.Peek();
                        scriptInstance.ProgramCounter += offsetJump;
                        jumped = true;
                        for (int i = 0; i < instruction.Op2; i++)
                        {
                            currentStack.AddVariable();
                            currentStack.Variables[i + 1] = scriptInstance.CommonStack.Symbols[i];
                        }
                        break;
                    case Opcodes.NATIVE_CALL:
                        Variable retVal = new Variable();
                        string methodName = currentStack.Variables[instruction.Op1].Value as String;
                        Variable[] args = new Variable[instruction.Op2];
                        for (int i = 0; i < args.Length; i++)
                        {
                            args[i] = scriptInstance.CommonStack.Symbols[i];
                        }
                        retVal.Value = scriptInstance.InvokeMethod(methodName, args);
                        currentStack.Variables[0] = retVal;
                        break;
                    case Opcodes.SETCOMMON:
                        scriptInstance.CommonStack.Symbols[instruction.Op2] = currentStack.Variables[instruction.Op1];
                        break;
                    case Opcodes.YIELD:
                        scriptInstance.timeCounter = 0;
                        scriptInstance.yieldTime = (double)currentStack.Variables[instruction.Op1].Value;
                        isYielding = true;
                        break;
                    default:
                        break;
                }
                if(!jumped)
                    scriptInstance.ProgramCounter++;
                if (scriptInstance.ProgramCounter >= scriptInstance.script.Instructions.Length || execute == false)
                    return false;
                else if (isYielding)
                    return true;
            }
            return isYielding;
        }

        private static void StartupScript(BeeScriptInstance scriptInstance)
        {
            scriptInstance.currentStacks.Push(new Stack());
            for ( int i = 0 ; i < scriptInstance.script.Globals.Length ; i++)
                scriptInstance.Globals.AddSymbol();
            for ( int i = 0 ; i < scriptInstance.script.Constants.Length ; i++)
                scriptInstance.Constants.AddSymbol( scriptInstance.script.Constants[i] );
        }
    }
}
