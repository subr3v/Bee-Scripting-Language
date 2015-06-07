using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    public class IdentifierTraverser : BeeTreeTraverser
    {
        private bool excludeLocals;
        private List<string> forbiddenIdentifiers;
        private List<string> reservedGlobalIdentifiers;

        public IdentifierTraverser(bool excludeLocals,List<string> reservedGlobalIdentifiers)
        {
            this.forbiddenIdentifiers = new List<string>() { "@", "if", "else", "for", "function", "include", "var", "local", "return", "false", "true", "void", "while", "num", "bool", "string", "yield" };
            this.reservedGlobalIdentifiers = reservedGlobalIdentifiers;
            this.excludeLocals = excludeLocals;
        }

        public IdentifierTraverser(List<string> reservedGlobalIdentifiers) : this(true,reservedGlobalIdentifiers) { }

        public IdentifierTraverser(bool excludeLocals) : this(excludeLocals, new List<string>()) { }

        public IdentifierTraverser() : this(false,new List<string>()) { }

        public Dictionary<string, string> VariablesIdentifiers = new Dictionary<string,string>();
        public Dictionary<string, InputOutputTypes> FunctionIdentifiers = new Dictionary<string, InputOutputTypes>();
        public Dictionary<string, bool> PropertyTable = new Dictionary<string, bool>();
        public Dictionary<string, bool> CallbackTable = new Dictionary<string, bool>();
        //Add Identifiers from Variable and Localstatement

        protected override void TraverseNodeCore(BeeNode node)
        {
            if (node.NodeType == BeeNodeType.VariableStatement || node.NodeType == BeeNodeType.PropertyStatement || (node.NodeType == BeeNodeType.LocalStatement && !excludeLocals))
            {
                string identifier = node.Children[0].Token.Value as String;
                ProcessIdentifier(identifier, node, "Undefined", false);
            }
            else if (node.NodeType == BeeNodeType.FunctionDefinition)
            {
                string functionType = node.Children[0].Children[0].Token.Value as String;
                functionType = ConvertGrammarTypeToCompilerType(functionType);
                string identifier = node.Children[1].Token.Value as String;
                ProcessIdentifier(identifier, node, functionType, true);
                var signatureNode = node.Children.First(n => n.NodeType == BeeNodeType.FunctionSignature);
                var funcType = FunctionIdentifiers[identifier];
                funcType.InputTypes = new string[signatureNode.Children.Count];
                int childNumber = 0;
                foreach (var typedIdentifierNode in signatureNode.Children)
                {
                    string paramIdentifier = typedIdentifierNode.Children[1].Token.Value as String;
                    string paramType = typedIdentifierNode.Children[0].Children[0].Token.Value as String;
                    paramType = ConvertGrammarTypeToCompilerType(paramType);
                    if(!excludeLocals)
                        ProcessIdentifier(paramIdentifier, typedIdentifierNode, paramType, false);
                    funcType.InputTypes[childNumber++] = paramType;
                }
            }
            else if (node.NodeType == BeeNodeType.CallbackDefinition)
            {
                string functionType = ConvertGrammarTypeToCompilerType("void");
                string identifier = node.Children[0].Token.Value as String;
                ProcessIdentifier(identifier, node, functionType, true);
                var funcType = FunctionIdentifiers[identifier];
                funcType.InputTypes = new string[0];
            }
        }

        private void ProcessIdentifier(string identifier, BeeNode node, string type, bool isFunction)
        {
            if (char.IsDigit(identifier[0]))
                BeeCompileException.Throw(CompileErrorType.IdentifierError, node, "Invalid identifier '{0}'. Identifiers can't start with a digit !", identifier);
            if (forbiddenIdentifiers.Contains(identifier))
                BeeCompileException.Throw(CompileErrorType.IdentifierError, node, "Can't use identifier '{0}'. It is a reserved keyword!", identifier);
            if (reservedGlobalIdentifiers.Contains(identifier))
                BeeCompileException.Throw(CompileErrorType.IdentifierError, node, "Duplicated identifier '{0}'", identifier);
            if (isFunction)
            {
                bool exists = FunctionIdentifiers.ContainsKey(identifier);
                if (exists)
                    BeeCompileException.Throw(CompileErrorType.IdentifierError, node, "Duplicated function '{0}'", identifier);
                else
                {
                    FunctionIdentifiers.Add(identifier, new InputOutputTypes() { OutputType = type, InputTypes = null });
                    CallbackTable.Add(identifier, node.NodeType == BeeNodeType.CallbackDefinition);
                }
            }
            else
            {
                bool exists = VariablesIdentifiers.ContainsKey(identifier);
                if (exists)
                    BeeCompileException.Throw(CompileErrorType.IdentifierError, node, "Duplicated identifier '{0}'", identifier);
                else
                {
                    VariablesIdentifiers.Add(identifier, type);
                    PropertyTable.Add(identifier, node.NodeType == BeeNodeType.PropertyStatement);
                }
            }
        }

        private string ConvertGrammarTypeToCompilerType(string type)
        {
            switch (type)
            {
                case "num": return "Number"; 
                case "string": return "String"; 
                case "bool": return "Boolean"; 
                case "void": return "Void";
            }
            throw new ArgumentException("Type not found");
        }
    }
}
