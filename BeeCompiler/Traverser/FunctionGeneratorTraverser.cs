using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BeeCompiler.ExpressionTree;
using BeeVM;

namespace BeeCompiler
{
    class FunctionGeneratorTraverser
    {
        private RegisterMap GlobalMap;
        private RegisterMap ConstantMap;

        public FunctionGeneratorTraverser(RegisterMap GlobalMap, RegisterMap ConstantMap)
        {
            this.GlobalMap = GlobalMap;
            this.ConstantMap = ConstantMap;
        }

        public Instruction[] GenerateFunction(BeeNode node)
        {
            VariableInfo.ResetRegisterCounter(1);
            RegisterMap localVariables = new RegisterMap();
            IdentifierTraverser localTraverser = new IdentifierTraverser();
            localTraverser.TraverseNode(node);
            int localCount = 0;
            int argCount = 0;

            LambdaBeeTreeTraverser argCounter = new LambdaBeeTreeTraverser(
                (n) =>
                {
                    if (n.NodeType == BeeNodeType.FunctionSignature)
                    {
                        argCount = n.Children.Count;
                    }
                });

            argCounter.TraverseNode(node);

            foreach (var local in localTraverser.VariablesIdentifiers)
            {
                if (local.Key.Contains('['))
                {
                    localVariables.Add(local.Key, new VariableInfo(null));
                    localCount++;
                }
            }

            localCount -= argCount;
            byte neededTempLocals = 0;
            byte reservedRegistersStart = (byte)(localCount + argCount + 1);

            LeftInnermostSearch(node,0,ref neededTempLocals);
            neededTempLocals += 1;
            Bytecode.ByteCodeWriter writer = new Bytecode.ByteCodeWriter();

            writer.AddInstruction(BeeVM.Opcodes.MAKELOCAL, (byte)(localCount + neededTempLocals + 1));

            ExpressionTree.TreeConverter converter = new ExpressionTree.TreeConverter(GlobalMap, ConstantMap,localVariables, writer);
            var stackTree = converter.ConvertTree(node, reservedRegistersStart);
            while (stackTree.Count > 0) { stackTree.Pop().GenerateCode(); }
            return writer.Script.Instructions;
        }

        private bool isLeafBinary(BeeNode beeNode)
        {
            return beeNode.Children[0].Children[0].NodeType == BeeNodeType.Term && beeNode.Children[2].Children[0].NodeType == BeeNodeType.Term
                && beeNode.Children[0].Children[0].Children[0].NodeType != BeeNodeType.ParExpression && beeNode.Children[2].Children[0].Children[0].NodeType != BeeNodeType.ParExpression;
        }

        private bool isExpressionNode(BeeNode beeNode)
        {
            BeeNode child = beeNode;
            while (child != null)
            {
                if (child.NodeType == BeeNodeType.Number || child.NodeType == BeeNodeType.String || child.NodeType == BeeNodeType.Constant)
                    return false;
                if (child.NodeType == BeeNodeType.BinaryExpression || child.NodeType == BeeNodeType.UnaryExpression)
                    return true;
                child = child.Children[0];
            }
            return true;
        }

        private void LeftInnermostSearch(BeeNode node , byte parentNumber , ref byte maxparentNumber)
        {
            if (parentNumber > maxparentNumber)
                maxparentNumber = parentNumber;
            switch (node.NodeType)
            {
                case BeeNodeType.ForStatement:
                    {
                        LeftInnermostSearch(node.Children[2], parentNumber, ref maxparentNumber);
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[3].Children).Reverse())
                        {
                            LeftInnermostSearch(child, parentNumber, ref maxparentNumber);
                        }
                        LeftInnermostSearch(node.Children[1], parentNumber, ref maxparentNumber);
                        LeftInnermostSearch(node.Children[0], parentNumber, ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.YieldStatement:
                    {
                        if (node.Children[0].Children[0].NodeType != BeeNodeType.Term)
                        {
                            LeftInnermostSearch(node.Children[0], parentNumber, ref maxparentNumber);
                            //Unrolled while
                        }
                        else
                        {
                            LeftInnermostSearch(node.Children[0].Children[0], parentNumber, ref maxparentNumber);
                            //Simple yield.
                        }
                        break;
                    }
                case BeeNodeType.WhileStatement:
                    {
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[1].Children).Reverse())
                        {
                            LeftInnermostSearch(child, parentNumber, ref maxparentNumber);
                        }
                        LeftInnermostSearch(node.Children[0], parentNumber, ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.AssignmentStatement:
                    {
                        LeftInnermostSearch(node.Children[2], parentNumber, ref maxparentNumber);
                        LeftInnermostSearch(node.Children[0], parentNumber, ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.ReturnStatement:
                    {
                        LeftInnermostSearch(node.Children[0], parentNumber, ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.VariableStatement:
                    {
                        LeftInnermostSearch(node.Children[2], parentNumber, ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.LocalStatement:
                    {
                        LeftInnermostSearch(node.Children[2], parentNumber, ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.IfStatement:
                    {
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[1].Children).Reverse())
                        {
                            LeftInnermostSearch(child, parentNumber, ref maxparentNumber);
                        }
                        LeftInnermostSearch(node.Children[0], parentNumber, ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.IfElseStatement:
                    {
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[2].Children).Reverse())
                        {
                            LeftInnermostSearch(child, parentNumber, ref maxparentNumber);
                        }
                        foreach (var child in ((IEnumerable<BeeNode>)node.Children[1].Children).Reverse())
                        {
                            LeftInnermostSearch(child, parentNumber, ref maxparentNumber);
                        }
                        LeftInnermostSearch(node.Children[0], parentNumber, ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.BinaryExpression:
                    {
                        LeftInnermostSearch(node.Children[2], (byte)(parentNumber + 1), ref maxparentNumber);
                        LeftInnermostSearch(node.Children[0], parentNumber, ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.UnaryExpression:
                    {
                        break;
                    }
                case BeeNodeType.NativeFunctionCall:
                    {
                        for (int i = node.Children[1].Children.Count - 1; i >= 0; i--)
                            LeftInnermostSearch(node.Children[1].Children[i].Children[0], (byte)(parentNumber + i + 1), ref maxparentNumber);
                        break;
                    }
                case BeeNodeType.FunctionCall:
                    {
                        for (int i = node.Children[1].Children.Count -1; i >= 0; i--)
                            LeftInnermostSearch(node.Children[1].Children[i].Children[0], (byte)(parentNumber + i + 1), ref maxparentNumber);
                        break;
                    }
                default:
                    {
                        foreach (var child in (((IEnumerable<BeeNode>)node.Children).Reverse()))
                        {
                            LeftInnermostSearch(child, (byte)(parentNumber), ref maxparentNumber);
                        }
                        break;
                    }
            }
        }

        private int BinaryNodeDepth(BeeNode node)
        {
            if (node.NodeType == BeeNodeType.BinaryExpression)
            {
                if (isLeafBinary(node))
                    return 2;
                else
                    return 1 + Math.Max(BinaryNodeDepth(node.Children[0]), BinaryNodeDepth(node.Children[2]));
            }
            else
            {
                int depth = 0;
                foreach (var child in node.Children)
                {
                    depth = Math.Max(BinaryNodeDepth(child), depth);
                }
                return depth;
            }
        }

        private bool isParentComplexExpression(BeeNode node, out BeeNode expressionNode)
        {
            BeeNode parent = node;
            expressionNode = null;
            while (parent != null)
            {
                if (parent.NodeType == BeeNodeType.BinaryExpression)
                {
                    if (isExpressionNode(parent.Children[0]) || isExpressionNode(parent.Children[2]))
                    {
                        expressionNode = parent;
                        return true;
                    }
                }
                parent = parent.Parent;
            }
            return false;
        }

        private BeeNode getFirstBinaryExpression(BeeNode node)
        {
            if (node.NodeType == BeeNodeType.BinaryExpression)
                return node;
            else if ((node.NodeType != BeeNodeType.String || node.NodeType != BeeNodeType.Number || node.NodeType != BeeNodeType.Constant)
                && node.Children.Count > 0)
            {
                return getFirstBinaryExpression(node.Children[0]);
            }
            else
                return null;
        }

        private BeeNode getFirstBinaryExpression(bool excludeFirst, BeeNode node)
        {
            if (node.NodeType == BeeNodeType.BinaryExpression && !excludeFirst)
                return node;
            else if ((node.NodeType != BeeNodeType.String || node.NodeType != BeeNodeType.Number || node.NodeType != BeeNodeType.Constant)
                && node.Children.Count > 0)
            {
                return getFirstBinaryExpression(false, node.Children[0]);
            }
            else
                return null;
        }
    }
}
