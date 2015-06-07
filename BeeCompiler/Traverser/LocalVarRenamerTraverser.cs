using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    class LocalVarRenamerTraverser : BeeTreeTraverser
    {
        protected override void TraverseNodeCore(BeeNode node)
        {
            if (node.NodeType == BeeNodeType.FunctionDefinition)
            {
                List<string> renamedIdentifiers = new List<string>();

                var functionIdentifierNode = node.Children.First(n => n.NodeType == BeeNodeType.Identifier);
                string functionName = functionIdentifierNode.Token.Value as String;

                var functionSignatureNode = node.Children.First(n => n.NodeType == BeeNodeType.FunctionSignature);
                foreach (var typedIdentifierNode in functionSignatureNode.Children)
                {
                    renamedIdentifiers.Add(typedIdentifierNode.Children[1].Token.Value as String);
                    typedIdentifierNode.Children[1].Token.Value = GetRenamedIdentifier((typedIdentifierNode.Children[1].Token.Value as String), functionName);
                }

                var localStatemens = node.Children.First(n => n.NodeType == BeeNodeType.LocalStatements);
                foreach (var localStatement in localStatemens.Children)
                {
                    renamedIdentifiers.Add(localStatement.Children[0].Token.Value as String);
                    localStatement.Children[0].Token.Value = GetRenamedIdentifier((localStatement.Children[0].Token.Value as String), functionName);
                    RenameIdentifiers(localStatement.Children[2], functionName, renamedIdentifiers);
                }

                var instructionStatemens = node.Children.First(n => n.NodeType == BeeNodeType.InstructionStatements);
                RenameIdentifiers(instructionStatemens, functionName, renamedIdentifiers);
            }
        }

        private void RenameIdentifiers(BeeNode node, string functionName, List<string> renamedIdentifiers)
        {
            if (node.NodeType == BeeNodeType.Identifier)
            {
                string identifier = node.Token.Value as String;
                if (renamedIdentifiers.Contains(identifier))
                {
                    node.Token.Value = GetRenamedIdentifier(identifier, functionName);
                }
            }
            foreach (var child in node.Children)
                RenameIdentifiers(child, functionName, renamedIdentifiers);
        }

        private string GetRenamedIdentifier(string identifier, string functionName)
        {
            return "[" + functionName + "]" + identifier;
        }
    }
}
