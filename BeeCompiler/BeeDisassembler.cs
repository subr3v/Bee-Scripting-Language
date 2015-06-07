using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BeeVM;

namespace BeeCompiler
{
    public class BeeDisassembler
    {
        public static String Decompile(Stream stream)
        {
            StringBuilder builder = new StringBuilder();
            var script = Bytecode.ByteCodeReader.LoadScript(stream);
            builder.AppendLine("Bee Decompiler");
            builder.AppendLine("Decompiling Bee Script Version : " + script.VersionNumber);
            builder.AppendLine("Globals : ");
            int i = 0;
            foreach (var global in script.Globals)
            {
                builder.AppendLine(String.Format("Global {0} : {1}" , i , i));
                i++;
            }
            builder.AppendLine("Constants : ");
            i = 0;
            foreach (var constant in script.Constants)
            {
                builder.AppendLine(String.Format("Constant {0} : {1}", i, constant.Value));
                i++;
            }
            builder.AppendLine("Instructions : ");
            foreach (var instruction in script.Instructions)
            {
                if (instruction.Opcode != BeeVM.Opcodes.JUMP && instruction.Opcode != BeeVM.Opcodes.JUMPIF && instruction.Opcode != BeeVM.Opcodes.LOADCONST)
                    builder.AppendLine(String.Format("{0} {1} {2} {3}", instruction.Opcode.ToString().PadRight(20,' ') , instruction.Op1 , instruction.Op2, instruction.Op3));
                else
                {
                    switch (instruction.Opcode)
                    {
                        case BeeVM.Opcodes.JUMP :
                            builder.AppendLine(String.Format("{0} {1}", instruction.Opcode.ToString().PadRight(20, ' '), BeeUtils.ConvertFromBytes(instruction.Op1, instruction.Op2)));
                            break;
                        case Opcodes.LOADCONST:
                            builder.AppendLine(String.Format("{0} {1} {2}\t;\"{3}\"", instruction.Opcode.ToString().PadRight(20, ' '), BeeUtils.ConvertFromBytes(instruction.Op1, instruction.Op2), instruction.Op3
                                ,script.Constants[BeeUtils.ConvertFromBytes(instruction.Op1,instruction.Op2)].Value));
                            break;
                        case Opcodes.JUMPIF:
                            builder.AppendLine(String.Format("{0} {1} {2}", instruction.Opcode.ToString().PadRight(20, ' ') , instruction.Op1, BeeUtils.ConvertFromBytes(instruction.Op2, instruction.Op3)));
                            break;
                    }
                }
            }

            return builder.ToString();
        }
    }
}
