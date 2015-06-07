using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeVM;
using System.IO;

namespace BeeCompiler.Bytecode
{
    public class ByteCodeReader
    {
        // 4 bytes : version number
        // 4 bytes : global size Array
        // for every constant
        // 1 byte : type encoded as ( int = 0 , bool = 1 , string = 2 , objRef = 3 )
        // 2 byte for lenght
        // x bytes for content
        // 4 bytes : instruction size
        // for every instruction
        // 4 bytes for OpCode , Op1 , Op2 , Op3

        static public BeeScript LoadScript(Stream stream)
        {
            BeeScript script = new BeeScript();
            byte[] fixed_buffer = new byte[8];
            byte[] string_buffer = null;
            stream.Read(fixed_buffer, 0, 4);
            script.VersionNumber = BitConverter.ToInt32(fixed_buffer, 0);
            stream.Read(fixed_buffer, 0, 4);
            script.Literals = new string[BitConverter.ToInt32(fixed_buffer, 0)];
            for (int i = 0; i < script.Literals.Length; i++)
            {
                stream.Read(fixed_buffer, 0, 2);
                ushort len = BitConverter.ToUInt16(fixed_buffer, 0);
                string_buffer = new byte[len];
                stream.Read(string_buffer, 0, len);
                script.Literals[i] = ASCIIEncoding.ASCII.GetString(string_buffer);
            }
            stream.Read(fixed_buffer, 0, 4);
            script.Callbacks = new int[BitConverter.ToInt32(fixed_buffer, 0)];
            for (int i = 0; i < script.Callbacks.Length; i++)
            {
                stream.Read(fixed_buffer, 0, 4);
                script.Callbacks[i] = BitConverter.ToInt32(fixed_buffer, 0);
            }
            stream.Read(fixed_buffer, 0, 4);
            script.Properties = new int[BitConverter.ToInt32(fixed_buffer, 0)];
            for (int i = 0; i < script.Properties.Length; i++)
            {
                stream.Read(fixed_buffer, 0, 4);
                script.Properties[i] = BitConverter.ToInt32(fixed_buffer, 0);
            }
            stream.Read(fixed_buffer, 0, 4);
            script.Globals = new Variable[BitConverter.ToInt32(fixed_buffer, 0)];
            stream.Read(fixed_buffer, 0, 4);
            script.Constants = new Variable[BitConverter.ToInt32(fixed_buffer, 0)];
            for (int i = 0; i < script.Constants.Length; i++)
            {
                stream.Read(fixed_buffer, 0, 3);
                VariableType type = (VariableType)fixed_buffer[0];
                ushort len = BitConverter.ToUInt16(fixed_buffer, 1);
                switch (type)
                {
                    case VariableType.Double:
                        stream.Read(fixed_buffer, 0, len);
                        script.Constants[i].Value = BitConverter.ToDouble(fixed_buffer, 0);
                        break;
                    case VariableType.Boolean:
                        stream.Read(fixed_buffer, 0, len);
                        script.Constants[i].Value = BitConverter.ToBoolean(fixed_buffer, 0);
                        break;
                    case VariableType.String:
                        string_buffer = new byte[len];
                        stream.Read(string_buffer, 0, len);
                        script.Constants[i].Value = ASCIIEncoding.ASCII.GetString(string_buffer);
                        break;
                }
            }
            stream.Read(fixed_buffer, 0, 4);
            int instructionLen = BitConverter.ToInt32(fixed_buffer, 0);
            script.Instructions = new Instruction[instructionLen];
            for (int i = 0; i < script.Instructions.Length; i++)
            {
                stream.Read(fixed_buffer, 0, 4);
                script.Instructions[i].Opcode = (Opcodes)fixed_buffer[0];
                script.Instructions[i].Op1 = fixed_buffer[1];
                script.Instructions[i].Op2 = fixed_buffer[2];
                script.Instructions[i].Op3 = fixed_buffer[3];
            }
            return script;
        }

    }
}
