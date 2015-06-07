using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeVM;
using System.IO;

namespace BeeCompiler.Bytecode
{
    public class ByteCodeWriter
    {
        private SymbolDictionary Constants = new SymbolDictionary();
        private SymbolDictionary Globals = new SymbolDictionary();
        private List<Instruction> Instructions = new List<Instruction>();
        private Dictionary<string, int> Properties = new Dictionary<string, int>();
        private Dictionary<string, int> Callbacks = new Dictionary<string, int>();

        private Stack<int> queuedJumps = new Stack<int>();
        private Stack<int> queuedInverseJumps = new Stack<int>();
        private Stack<int> queuedIfJumps = new Stack<int>();

        public BeeScript Script
        {
            get
            {
                return generateScript();
            }
        }

        // 4 bytes : version number
        // 4 bytes : global size Array
        // for every constant
        // 1 byte : type encoded as ( int = 0 , bool = 1 , string = 2 , objRef = 3 )
        // 2 byte for lenght
        // x bytes for content
        // 4 bytes : instruction size
        // for every instruction
        // 4 bytes for OpCode , Op1 , Op2 , Op3

        public static void WriteScript(BeeScript script , Stream stream)
        {
            //Version Number
            byte[] buffer = BitConverter.GetBytes(script.VersionNumber);
            stream.Write(buffer, 0, buffer.Length);
            //Literals Array
            buffer = BitConverter.GetBytes(script.Literals.Length);
            stream.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < script.Literals.Length; i++)
            {
                ushort size = (ushort)(script.Literals[i].Length);
                buffer = BitConverter.GetBytes(size);
                stream.Write(buffer, 0, buffer.Length);
                string str = (string)script.Literals[i];
                buffer = ASCIIEncoding.ASCII.GetBytes(str);
                stream.Write(buffer, 0, buffer.Length);
            }
            //Callbacks Array
            buffer = BitConverter.GetBytes(script.Callbacks.Length);
            stream.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < script.Callbacks.Length;  i++)
            {
                buffer = BitConverter.GetBytes(script.Callbacks[i]);
                stream.Write(buffer, 0, buffer.Length);
            }
            //Properties Array
            buffer = BitConverter.GetBytes(script.Properties.Length);
            stream.Write(buffer, 0, buffer.Length);
            for (int i = 0; i < script.Properties.Length; i++ )
            {
                buffer = BitConverter.GetBytes(script.Properties[i]);
                stream.Write(buffer, 0, buffer.Length);
            }
            //Global Size Array
            buffer = BitConverter.GetBytes(script.Globals.Length);
            stream.Write(buffer, 0, buffer.Length);
            //Constant Size Array
            buffer = BitConverter.GetBytes(script.Constants.Length);
            stream.Write(buffer, 0, buffer.Length);
            // loop constants
            foreach (var constant in script.Constants)
            {
                VariableType type = 0;
                if (constant.Value is double) type = VariableType.Double;
                if (constant.Value is bool) type = VariableType.Boolean;
                if (constant.Value is string) type = VariableType.String;
                ushort size = 0;
                stream.WriteByte((byte)type);
                switch (type)
                {
                    case VariableType.Double:
                        {
                            size = 8;
                            buffer = BitConverter.GetBytes(size);
                            stream.Write(buffer, 0, buffer.Length);
                            buffer = BitConverter.GetBytes((double)constant.Value);
                            stream.Write(buffer, 0, buffer.Length);
                            break;
                        }
                    case VariableType.Boolean:
                        {
                            size = 1;
                            buffer = BitConverter.GetBytes(size);
                            stream.Write(buffer, 0, buffer.Length);
                            stream.WriteByte((bool)constant.Value == true ? (byte)1 : (byte)0);
                            break;
                        }
                    case VariableType.String:
                        {
                            size = (ushort)(((string)(constant.Value)).Length);
                            buffer = BitConverter.GetBytes(size);
                            stream.Write(buffer, 0, buffer.Length);
                            string str = (string)constant.Value;
                            buffer = ASCIIEncoding.ASCII.GetBytes(str);
                            stream.Write(buffer, 0, buffer.Length);
                            break;
                        }
                }
            }
            //Instruction Array
            buffer = BitConverter.GetBytes(script.Instructions.Length);
            stream.Write(buffer, 0, buffer.Length);
            foreach (var instruction in script.Instructions)
            {
                stream.WriteByte((byte)instruction.Opcode);
                stream.WriteByte(instruction.Op1);
                stream.WriteByte(instruction.Op2);
                stream.WriteByte(instruction.Op3);
            }
        }

        public void WriteToStream(Stream stream)
        {
            WriteScript(this.Script, stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instructions"></param>
        /// <returns>Returns as value 1 the starting instruction and as value 2 the ending instruction</returns>
        public Tuple<int,int> InjectInstructions(Instruction[] instructions)
        {
            Tuple<int, int> injectInfo = new Tuple<int, int>( this.Instructions.Count , this.Instructions.Count + instructions.Length );
            this.Instructions.AddRange(instructions);
            return injectInfo;
        }

        public void InjectInstruction(int index, Instruction newInstruction)
        {
            Instructions[index] = newInstruction;
        }

        public void AddConstant(Object value)
        {
            Variable var = new Variable();
            var.Value = value;
            Constants.AddSymbol( var );
        }

        public void AddGlobal(string Name , bool exportAsProperty)
        {
            Variable var = new Variable();
            var.Value = null;
            Globals.AddSymbol(var);
            if(exportAsProperty)
                Properties.Add( Name , Globals.Symbols.Length -1);
        }

        public void AddLoadConstant(short constantIndex, byte op3)
        {
            Instruction instruction = new Instruction();
            instruction.Opcode = Opcodes.LOADCONST;
            BeeUtils.ConvertToBytes(constantIndex, out instruction.Op1, out instruction.Op2);
            instruction.Op3 = op3;
            Instructions.Add(instruction);
        }

        public void AddInstruction(Opcodes opcode, byte op1 = 0, byte op2 = 0, byte op3 = 0)
        {
            Instruction instruction = new Instruction();
            instruction.Opcode = opcode;
            instruction.Op1 = op1;
            instruction.Op2 = op2;
            instruction.Op3 = op3;
            Instructions.Add(instruction);
        }

        //Points last jump to next instruction
        public void FinalizeJump()
        {
            var instructionIndex = queuedJumps.Pop();
            var instruction = Instructions[instructionIndex];
            BeeUtils.ConvertToBytes((short)( (Instructions.Count) - instructionIndex ), out instruction.Op1, out instruction.Op2);
            Instructions[instructionIndex] = instruction;
        }

        //Points last Ifjump to next instruction
        public void FinalizeIfJump()
        {
            var instructionIndex = queuedIfJumps.Pop();
            var instruction = Instructions[instructionIndex];
            BeeUtils.ConvertToBytes((short)((Instructions.Count) - instructionIndex), out instruction.Op2, out instruction.Op3  );
            Instructions[instructionIndex] = instruction;
        }

        public void RegisterInverseJumpInstruction()
        {
            queuedInverseJumps.Push(Instructions.Count - 1);
        }

        public void FinalizeInverseJump()
        {
            var registeredJumpIndex = queuedInverseJumps.Pop();
            Instruction instruction = new Instruction();
            instruction.Opcode = Opcodes.JUMP;
            BeeUtils.ConvertToBytes((short)(registeredJumpIndex - (Instructions.Count - 1)), out instruction.Op1, out instruction.Op2);
            Instructions.Add(instruction);
        }

        public void AddJumpInstruction()
        {
            Instruction instruction = new Instruction();
            instruction.Opcode = Opcodes.JUMP;
            Instructions.Add(instruction);
            queuedJumps.Push(Instructions.Count - 1);
        }

        public void AddJumpIfInstruction(byte boolRegister)
        {
            Instruction instruction = new Instruction();
            instruction.Opcode = Opcodes.JUMPIF;
            instruction.Op1 = boolRegister;
            //BeeUtils.ConvertToBytes((short)(jumpOffset), out instruction.Op2, out instruction.Op3);
            Instructions.Add(instruction);
            queuedIfJumps.Push(Instructions.Count - 1);
        }

        private BeeScript generateScript()
        {
            BeeScript script = new BeeScript();
            script.Constants = Constants.Symbols;
            script.Globals = Globals.Symbols;
            script.Instructions = Instructions.ToArray();
            script.VersionNumber = 0;
            script.Callbacks = new int[Callbacks.Count * 2];
            script.Properties = new int[Properties.Count * 2];
            script.Literals = new string[Callbacks.Count + Properties.Count];
            int literalI = 0;
            int callbackI = 0;
            int propertyI = 0;
            foreach (var callback in Callbacks)
            {
                script.Literals[literalI ++ ] = callback.Key;
                script.Callbacks[ callbackI ++ ] = callback.Value;
                script.Callbacks[ callbackI ++ ] = literalI - 1;
            }
            foreach (var property in Properties)
            {
                script.Literals[literalI++] = property.Key;
                script.Properties[propertyI++] = property.Value;
                script.Properties[propertyI++] = literalI - 1;
            }
            return script;
        }


        internal void Reverse()
        {
            Instructions.Reverse();
        }

        internal void RegisterCallback(string functionName , int index)
        {
            Callbacks.Add(functionName, index);
        }
    }
}
