using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    class ConstantTraverser : BeeTreeTraverser
    {
        public RegisterMap ConstantMap { get; set; }

        public ConstantTraverser(Dictionary<string,  InputOutputTypes> nativeCalls)
        {
            ConstantMap = new RegisterMap();
            ConstantMap.Add("True", new VariableInfo(true));
            ConstantMap.Add("False", new VariableInfo(false));
            foreach (var nativeCall in nativeCalls)
            {
                ConstantMap.Add(nativeCall.Key, new VariableInfo( nativeCall.Key ) );
            }
        }

        protected override void TraverseNodeCore(BeeNode node)
        {
            switch (node.NodeType)
            {
                case BeeNodeType.Number :
                    if (!ConstantMap.ContainsKey(node.Token.ValueString))
                        ConstantMap.Add(node.Token.ValueString, new VariableInfo(double.Parse(node.Token.ValueString)));
                    break;
                case BeeNodeType.String:
                    if (!ConstantMap.ContainsKey(node.Token.ValueString))
                        ConstantMap.Add(node.Token.ValueString, new VariableInfo((string)node.Token.Value));
                    break;
                case BeeNodeType.FunctionCall:
                    if (!ConstantMap.ContainsKey(node.Children[0].Token.ValueString))
                    {
                        ConstantMap.Add(node.Children[0].Token.ValueString, new VariableInfo(0));
                    }
                    break;
            }
        }
    }
}
