using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeVM;

namespace BeeCompiler.ExpressionTree
{
    class TreeConverter
    {
        private Stack<ExpressionNode> nodes;
        private RegisterMap globals, constants , locals;
        private Bytecode.ByteCodeWriter writer;

        public TreeConverter(RegisterMap globals, RegisterMap constants,RegisterMap locals, Bytecode.ByteCodeWriter writer)
        {
            this.globals = globals;
            this.constants = constants;
            this.writer = writer;
            this.locals = locals;
        }

        private void addNode(Action generator)
        {
            nodes.Push(new ExpressionNode(generator));
        }

        private void addNativeCallNode(byte registerLocation, string funcName, params byte[] argsRegisters)
        {
            addNode(
                ()
                    =>
                {
                    for (byte i = 0; i < argsRegisters.Length; i++)
                    {
                        writer.AddInstruction(Opcodes.SETCOMMON,argsRegisters[i],i);
                    }
                    writer.AddLoadConstant(constants[funcName].RegisterLocation, 0);
                    writer.AddInstruction(Opcodes.NATIVE_CALL, 0 , (byte)argsRegisters.Length);
                    writer.AddInstruction(Opcodes.MOVE, 0 , registerLocation);
                });
        }

        private void addCallNode(byte registerLocation, string funcName, params byte[] argsRegisters)
        {
            addNode(
                ()
                    =>
                {
                    for (byte i = 0; i < argsRegisters.Length; i++)
                    {
                        writer.AddInstruction(Opcodes.SETCOMMON, argsRegisters[i], i);
                    }
                    writer.AddInstruction(Opcodes.CALL, registerLocation, (byte)argsRegisters.Length);
                    writer.AddInstruction(Opcodes.MOVE, 0 , registerLocation);
                });
        }

        private void addBinaryExpression(Opcodes opcode, int firstChild, int secondChild , bool invert)
        {
            addNode(() => 
            {
                if(!invert)
                    writer.AddInstruction(opcode, (byte)firstChild, (byte)secondChild, (byte)firstChild);
                else
                    writer.AddInstruction(opcode, (byte)secondChild, (byte)firstChild, (byte)firstChild);
            });
        }

        private void addUnaryExpression(Opcodes opcode, byte registerLocation)
        {
            addNode(() => { writer.AddInstruction(opcode, registerLocation, registerLocation); });
        }

        private void addIfStatement(byte parentNumber)
        {
            addNode(
                () =>
                {
                    writer.AddJumpIfInstruction(parentNumber);
                    writer.AddJumpInstruction();
                    writer.FinalizeIfJump();
                });
        }

        private void addIfCloseStatement()
        {
            addNode(() => { writer.FinalizeJump(); });
        }

        private void addLoadConstant(VariableInfo constant, byte registerLoc)
        {
            addNode(() => { writer.AddLoadConstant(constant.RegisterLocation, registerLoc); });
        }

        private void addSetGlobalStatement(byte registerLoc, string globalName)
        {
            addNode(() =>
            {
                writer.AddInstruction(BeeVM.Opcodes.SETGLOBAL, registerLoc, (byte)globals[globalName].RegisterLocation);
            });
        }

        private void addSetLocalStatement(byte registerLoc , string localName)
        {
            addNode(() =>
            {
                writer.AddInstruction(BeeVM.Opcodes.MOVE, registerLoc, (byte)locals[localName].RegisterLocation);
            });
        }

        public Stack<ExpressionNode> ConvertTree(BeeNode expressionRoot, byte startingPoint)
        {
            nodes = new Stack<ExpressionNode>();
            nodeConvert(expressionRoot, startingPoint);
            return nodes;
        }

        private BeeVM.Opcodes GetUnaryOperationOpcode(BeeNode node)
        {
            switch (node.Children[0].Token.ValueString)
            {
                case "!": return BeeVM.Opcodes.NEGATE;
                case "-": return BeeVM.Opcodes.SUBTRACT;
            }
            BeeCompileException.Throw(CompileErrorType.GenerateError, node, "Can't evaluate opcode for operation {0}", node.Token.ValueString);
            return BeeVM.Opcodes.RETURN;
        }

        private BeeVM.Opcodes GetBinaryOperationOpcode(BeeNode node , out bool reverse)
        {
            reverse = false;
            switch (node.Children[0].Token.ValueString)
            {
                case "+": return BeeVM.Opcodes.ADD;
                case "-": return BeeVM.Opcodes.SUBTRACT;
                case "*": return BeeVM.Opcodes.MULTIPLY;
                case "/": return BeeVM.Opcodes.DIVIDE;
                case ">": return Opcodes.GREATER;
                case ">=": return Opcodes.GREATEROREQUAL;
                case "<": reverse = true; return Opcodes.GREATER;
                case "<=": reverse = true; return Opcodes.GREATEROREQUAL;
                case "&&": return Opcodes.AND_OPERATOR;
                case "||": return Opcodes.OR_OPERATOR;
                case "==": return Opcodes.EQUAL;
            }
            BeeCompileException.Throw(CompileErrorType.GenerateError, node, "Can't evaluate opcode for operation {0}", node.Children[0].Token.ValueString);
            return BeeVM.Opcodes.RETURN;
        }

        private void nodeConvert(BeeNode node, byte parentNumber)
        {
            switch (node.NodeType)
            {
                    // TODO
                    // SWITCH?!!?
                case BeeNodeType.ForStatement:
                    {
                        addIfCloseStatement();
                        addNode(() => { writer.FinalizeInverseJump(); });
                        nodeConvert(node.Children[2], parentNumber);
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[3].Children).Reverse())
                        {
                            nodeConvert(child, parentNumber);
                        }
                        addIfStatement(parentNumber);
                        nodeConvert(node.Children[1], parentNumber);
                        addNode(() => { writer.RegisterInverseJumpInstruction(); });
                        nodeConvert(node.Children[0], parentNumber);
                        break;
                    }
                case BeeNodeType.YieldStatement:
                    {
                        if (node.Children[0].Children[0].NodeType != BeeNodeType.Term)
                        {
                            addIfCloseStatement();
                            addNode(() => { writer.FinalizeInverseJump(); });
                            addNode(() =>
                            {
                                writer.AddLoadConstant(constants["0"].RegisterLocation, parentNumber);
                                writer.AddInstruction(Opcodes.YIELD, parentNumber);
                            });
                            addIfStatement(parentNumber);
                            nodeConvert(node.Children[0], parentNumber);
                            addNode(() => { writer.RegisterInverseJumpInstruction(); });
                            //Unrolled while
                        }
                        else
                        {
                            addNode(() =>
                                {
                                    writer.AddInstruction(Opcodes.YIELD, parentNumber);
                                });
                            nodeConvert(node.Children[0].Children[0], parentNumber);
                            //Simple yield.
                        }
                        break;
                    }
                case BeeNodeType.WhileStatement:
                    {
                        addIfCloseStatement();
                        addNode(() => { writer.FinalizeInverseJump(); });
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[1].Children).Reverse())
                        {
                            nodeConvert(child, parentNumber);
                        }
                        addIfStatement(parentNumber);
                        nodeConvert(node.Children[0], parentNumber);
                        addNode(() => { writer.RegisterInverseJumpInstruction(); });
                        break;
                    }
                case BeeNodeType.AssignmentStatement:
                    {
                        addNode(() =>
                            {
                                if (locals.ContainsKey(node.Children[0].Token.ValueString))
                                {
                                    writer.AddInstruction(BeeVM.Opcodes.MOVE, parentNumber, (byte)locals[node.Children[0].Token.ValueString].RegisterLocation);
                                }
                                else
                                {
                                    writer.AddInstruction(BeeVM.Opcodes.SETGLOBAL, parentNumber, (byte)globals[node.Children[0].Token.ValueString].RegisterLocation);
                                }
                            });
                        nodeConvert(node.Children[2], parentNumber);
                        nodeConvert(node.Children[0], parentNumber);
                        break;
                    }
                case BeeNodeType.ReturnStatement:
                    {
                        addNode(() => { writer.AddInstruction(Opcodes.RETURN); });
                        addNode(() => { writer.AddInstruction(Opcodes.MOVE, parentNumber, 0); });
                        nodeConvert(node.Children[0], parentNumber);
                        break;
                    }
                case BeeNodeType.VariableStatement:
                case BeeNodeType.PropertyStatement:
                    {
                        addSetGlobalStatement(parentNumber, node.Children[0].Token.ValueString);
                        nodeConvert(node.Children[2], parentNumber);
                        break;
                    }
                case BeeNodeType.LocalStatement:
                    {
                        addSetLocalStatement(parentNumber, node.Children[0].Token.ValueString);
                        nodeConvert(node.Children[2], parentNumber);
                        break;
                    }
                case BeeNodeType.IfStatement:
                    {
                        addIfCloseStatement();
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[1].Children).Reverse())
                        {
                            nodeConvert(child, parentNumber);
                        }
                        addIfStatement(parentNumber);
                        nodeConvert(node.Children[0], parentNumber);
                        break;
                    }
                case BeeNodeType.IfElseStatement:
                    {
                        addNode(() => { writer.FinalizeJump(); });
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[2].Children).Reverse())
                        {
                            nodeConvert(child, parentNumber);
                        }
                        addIfCloseStatement();
                        addNode(() => { writer.AddJumpInstruction(); });
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[1].Children).Reverse())
                        {
                            nodeConvert(child, parentNumber);
                        }
                        addIfStatement(parentNumber);
                        nodeConvert(node.Children[0], parentNumber);
                        break;
                    }
                case BeeNodeType.BinaryExpression:
                    {
                        bool invert;
                        addBinaryExpression(GetBinaryOperationOpcode(node.Children[1],out invert), parentNumber , parentNumber + 1 , invert);
                        nodeConvert(node.Children[2], (byte)(parentNumber + 1));
                        nodeConvert(node.Children[0], parentNumber);
                        break;
                    }
                case BeeNodeType.UnaryExpression:
                    {
                        addUnaryExpression(GetUnaryOperationOpcode(node.Children[0]), parentNumber);
                        nodeConvert(node.Children[0], parentNumber);
                        break;
                    }
                case BeeNodeType.NativeFunctionCall:
                    {
                        byte[] args = new byte[node.Children[1].Children.Count];
                        for (byte i = 0; i < args.Length; i++)
                            args[i] = (byte)(parentNumber + i);
                        addNativeCallNode(parentNumber, node.Children[0].Token.ValueString, args);
                        for (int i = node.Children[1].Children.Count - 1; i >= 0; i--)
                            nodeConvert(node.Children[1].Children[i].Children[0], (byte)(parentNumber + i));
                        break;
                    }
                case BeeNodeType.FunctionCall:
                    {
                        byte[] args = new byte[node.Children[1].Children.Count];
                        for(byte i = 0; i < args.Length ; i ++)
                            args[i] = (byte)(parentNumber + i + 1);
                        addCallNode(parentNumber, node.Children[0].Token.ValueString, args);
                        for (int i = node.Children[1].Children.Count -1; i >= 0; i--)
                            nodeConvert(node.Children[1].Children[i].Children[0], (byte)(parentNumber + i + 1));
                        addNode(() =>
                        {
                            if (locals.ContainsKey(node.Children[0].Token.ValueString))
                            {
                                writer.AddInstruction(BeeVM.Opcodes.MOVE, (byte)locals[node.Children[0].Token.ValueString].RegisterLocation, parentNumber);
                            }
                            else
                            {
                                writer.AddInstruction(BeeVM.Opcodes.GETGLOBAL, (byte)globals[node.Children[0].Token.ValueString].RegisterLocation, parentNumber);
                            }
                        });
                        break;
                    }
                case BeeNodeType.Term:
                    {
                        if (node.Children[0].NodeType == BeeNodeType.ParExpression)
                        {
                            nodeConvert(node.Children[0], parentNumber);
                        }
                        else if (node.Children[0].NodeType != BeeNodeType.Identifier)
                        {
                            addLoadConstant(getConstantVariable(node), parentNumber);
                        }
                        else
                        {
                            addNode(() =>
                                {
                                    if (locals.ContainsKey(node.Children[0].Token.ValueString))
                                    {
                                        writer.AddInstruction(BeeVM.Opcodes.MOVE, (byte)locals[node.Children[0].Token.ValueString].RegisterLocation, parentNumber);
                                    }
                                    else
                                    {
                                        writer.AddInstruction(BeeVM.Opcodes.GETGLOBAL, (byte)globals[node.Children[0].Token.ValueString].RegisterLocation, parentNumber);
                                    }
                                });
                        }
                        break;
                    }
                default:
                    {
                        foreach (var child in (((IEnumerable<BeeNode>)node.Children).Reverse()))
                        {
                            nodeConvert(child, (byte)(parentNumber));
                        }
                        break;
                    }
            }
        }

        private VariableInfo getConstantVariable(BeeNode node)
        {
            return constants[node.Children[0].Token.ValueString];
        }
    }
}
