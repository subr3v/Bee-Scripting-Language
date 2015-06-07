using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeVM;

namespace BeeCompiler
{
    class FunctionTraverser : BeeTreeTraverser
    {
        public RegisterMap GlobalMap;
        public RegisterMap ConstantMap;

        public Dictionary<string, Instruction[]> functionsCode;

        public FunctionTraverser(RegisterMap GlobalMap, RegisterMap ConstantMap)
        {
            this.GlobalMap = GlobalMap;
            this.ConstantMap = ConstantMap;
            functionsCode = new Dictionary<string, Instruction[]>();
        }

        protected override void TraverseNodeCore(BeeNode node)
        {
            if (node.NodeType == BeeNodeType.FunctionDefinition)
            {
                FunctionGeneratorTraverser generator = new FunctionGeneratorTraverser(GlobalMap , ConstantMap);
                functionsCode.Add(node.Children[1].Token.ValueString , generator.GenerateFunction(node) );
            }
            else if (node.NodeType == BeeNodeType.CallbackDefinition)
            {
                FunctionGeneratorTraverser generator = new FunctionGeneratorTraverser(GlobalMap, ConstantMap);
                functionsCode.Add(node.Children[0].Token.ValueString, generator.GenerateFunction(node));
            }
            if (node.NodeType == BeeNodeType.VariableStatements)
            {
                FunctionGeneratorTraverser generator = new FunctionGeneratorTraverser(GlobalMap, ConstantMap);
                functionsCode.Add("GlobalInitializer", generator.GenerateFunction(node));
            }
        }
    }
}
