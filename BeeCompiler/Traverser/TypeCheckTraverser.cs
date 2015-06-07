using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    public class InputOutputTypes
    {
        public string[] InputTypes { get; set; }
        public string OutputType { get; set; }
    }

    class TypeCheckTraverser : BeeTreeTraverser
    {
        public Dictionary<string, string> IdTypeMap { get; set; }
        public Dictionary<string, InputOutputTypes> FunctionIdTypeMap { get; set; }
        public Dictionary<string, InputOutputTypes> NativeFunctionIdTypeMap { get; set; }

        private Dictionary<string, InputOutputTypes> operatorTypes { get; set; }

        public TypeCheckTraverser(Dictionary<string, string> identifiers, Dictionary<string, InputOutputTypes> functionsTypes , NativeCallInfo nativeCalls)
        {
            FunctionIdTypeMap = functionsTypes;
            IdTypeMap = identifiers;

            #region Operator types initialization
            operatorTypes = new Dictionary<string, InputOutputTypes>()
            {
                {"=="  ,  new InputOutputTypes() { InputTypes = new string[] {"Boolean", "Number", "String"},   OutputType ="Boolean"}}, 
                {">="   ,  new InputOutputTypes() { InputTypes = new string[] {"Number"} ,   OutputType = "Boolean"}},
                {"<="   , new InputOutputTypes() { InputTypes = new string[] { "Number"} ,   OutputType = "Boolean"}},
                {">"    , new InputOutputTypes() { InputTypes =  new string[] {"Number"} ,   OutputType = "Boolean"}},
                {"<"    , new InputOutputTypes() { InputTypes =new string[] {  "Number"} ,   OutputType = "Boolean"}},
                {"+"    , new InputOutputTypes() { InputTypes =new string[] {  "Number"} ,   OutputType = "Number"}},
                { "-"   ,new InputOutputTypes() { InputTypes = new string[] {  "Number"} ,   OutputType = "Number"}},
                {"*"    , new InputOutputTypes() { InputTypes = new string[] { "Number"} ,   OutputType = "Number"}},
                {"/"    ,  new InputOutputTypes() { InputTypes =new string[] { "Number"} ,   OutputType = "Number"}},
                {"&&"   , new InputOutputTypes() { InputTypes = new string[] { "Boolean"} ,   OutputType = "Boolean"}},
                {"||"   ,  new InputOutputTypes() { InputTypes =new string[] { "Boolean"} ,   OutputType = "Boolean"}},
                {"!"    , new InputOutputTypes() { InputTypes = new string[] { "Boolean"} ,   OutputType = "Boolean"}},
            };

            #endregion

            #region Native Function type initialization
            NativeFunctionIdTypeMap = new Dictionary<string, InputOutputTypes>();

            foreach (var nativeCall in nativeCalls.NativeCalls)
            {
                NativeFunctionIdTypeMap.Add(nativeCall.Key, nativeCall.Value);
            }
            NativeFunctionIdTypeMap.Add("getObjectStringProperty", new InputOutputTypes() { OutputType = "String", InputTypes = new string[] { "String", "String" } });
            NativeFunctionIdTypeMap.Add("getObjectNumberProperty", new InputOutputTypes() { OutputType = "Number", InputTypes = new string[] { "String", "String" } });
            NativeFunctionIdTypeMap.Add("getObjectBooleanProperty", new InputOutputTypes() { OutputType = "Boolean", InputTypes = new string[] { "String", "String" } });

            NativeFunctionIdTypeMap.Add("getObjectStringScriptProperty", new InputOutputTypes() { OutputType = "String", InputTypes = new string[] { "String", "String" } });
            NativeFunctionIdTypeMap.Add("getObjectNumberScriptProperty", new InputOutputTypes() { OutputType = "Number", InputTypes = new string[] { "String", "String" } });
            NativeFunctionIdTypeMap.Add("getObjectBooleanScriptProperty", new InputOutputTypes() { OutputType = "Boolean", InputTypes = new string[] { "String", "String" } });

            NativeFunctionIdTypeMap.Add("setObjectStringProperty", new InputOutputTypes() { OutputType = "Void", InputTypes = new string[] { "String", "String", "String" } });
            NativeFunctionIdTypeMap.Add("setObjectNumberProperty", new InputOutputTypes() { OutputType = "Void", InputTypes = new string[] { "String", "String", "Number" } });
            NativeFunctionIdTypeMap.Add("setObjectBooleanProperty", new InputOutputTypes() { OutputType = "Void", InputTypes = new string[] { "String", "String", "Boolean" } });

            NativeFunctionIdTypeMap.Add("setObjectStringScriptProperty", new InputOutputTypes() { OutputType = "Void", InputTypes = new string[] { "String", "String", "String" } });
            NativeFunctionIdTypeMap.Add("setObjectNumberScriptProperty", new InputOutputTypes() { OutputType = "Void", InputTypes = new string[] { "String", "String", "Number" } });
            NativeFunctionIdTypeMap.Add("setObjectBooleanScriptProperty", new InputOutputTypes() { OutputType = "Void", InputTypes = new string[] { "String", "String", "Boolean" } });

            NativeFunctionIdTypeMap.Add("sendMessage", new InputOutputTypes() { OutputType = "Void", InputTypes = new string[] { "String", "String" } });

            #endregion
        }

        protected override void TraverseNodeCore(BeeNode node)
        {
            switch (node.NodeType)
            {
                case BeeNodeType.IncludeStatement:
                    if (EvaluateNodeType(node.Children[0]) != "String")
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Error in include, parameter is not a string");
                    break;

                case BeeNodeType.PropertyStatement:
                case BeeNodeType.VariableStatement:
                case BeeNodeType.LocalStatement:
                    string idName = node.Children.First(n => n.NodeType == BeeNodeType.Identifier).Token.Value as String;
                    string idType = IdTypeMap[idName];
                    var expression = node.Children.First(n => n.NodeType == BeeNodeType.Expression);
                    string expressionType = EvaluateNodeType(expression);
                    if (idType == "Undefined")
                    {
                        if (expressionType == "Undefined" || expressionType == "Void")
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Cannot evaluate type for {0}", idName);
                        else
                            IdTypeMap[idName] = expressionType;
                    }
                    else
                    {
                        if (expressionType != idType)
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Cannot assign a {0} expression to {1}", expressionType, idType);
                    }
                    break;

                case BeeNodeType.FunctionCall:
                    string functionIdentifier = node.Children[0].Token.Value as String;
                    if (!FunctionIdTypeMap.ContainsKey(functionIdentifier))
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Cannot find function {0}", functionIdentifier);
                    var functionType = FunctionIdTypeMap[functionIdentifier];
                    if (functionType.InputTypes.Length != node.Children[1].Children.Count)
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Function {0} required {1} parameters, given {2}", functionIdentifier, functionType.InputTypes.Length, node.Children[1].Children.Count);
                    bool parameterTypeMatch = true;
                    for (int i = 0; i < functionType.InputTypes.Length && parameterTypeMatch; i++)
                    {
                        var argumentType = EvaluateNodeType(node.Children[1].Children[i]);
                        if (functionType.InputTypes[i] != argumentType)
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Given parameter {0} for function {1} is {2} instead of {3}",
                                i + 1, functionIdentifier, argumentType, functionType.InputTypes[i]);
                    }
                    break;

                case BeeNodeType.FunctionDefinition:
                    var statements = node.Children.Find(n => n.NodeType == BeeNodeType.InstructionStatements);
                    functionIdentifier = node.Children[1].Token.Value as String;
                    if (!FunctionIdTypeMap.ContainsKey(functionIdentifier))
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Cannot find function {0}", functionIdentifier);
                    functionType = FunctionIdTypeMap[functionIdentifier];
                    if( statements.Children.Count == 0)
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "A function must at least have one return statement!", functionIdentifier);
                    if(!statements.Children.Last().Children.Any(n => n.NodeType == BeeNodeType.ReturnStatement))
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Last statement of a function must be a return statement", functionIdentifier);
                    CheckFunctionReturnType(statements, functionType.OutputType);
                    break;

                case BeeNodeType.NativeFunctionCall:
                    functionIdentifier = node.Children[0].Token.Value as String;
                    if (!NativeFunctionIdTypeMap.ContainsKey(functionIdentifier))
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Cannot find native function {0}", functionIdentifier);
                    functionType = NativeFunctionIdTypeMap[functionIdentifier];
                    if (functionIdentifier != "sendMessage")
                    {
                        if (functionType.InputTypes.Length != node.Children[1].Children.Count)
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Native function {0} required {1} parameters, given {2}", functionIdentifier, functionType.InputTypes.Length, node.Children[1].Children.Count);
                    }
                    else
                        if (node.Children[1].Children.Count < functionType.InputTypes.Length)
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Native function {0} required at least {1} parameters, given {2}", functionIdentifier, functionType.InputTypes.Length, node.Children[1].Children.Count);
                    parameterTypeMatch = true;
                    
                    for (int i = 0; i < functionType.InputTypes.Length && parameterTypeMatch; i++)
                    {
                        var argumentType = EvaluateNodeType(node.Children[1].Children[i]);
                        if (functionType.InputTypes[i] != argumentType)
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Given parameter {0} for function {1} is {2} instead of {3}",
                                i + 1, functionIdentifier, argumentType, functionType.InputTypes[i]);
                    }
                    break;

                case BeeNodeType.AssignmentStatement:
                    var identifierNode = node.Children[0];
                    string identifierType = EvaluateNodeType(identifierNode);
                    var expressionNode = node.Children[2];
                    string exprType = EvaluateNodeType(expressionNode);
                    if (identifierType != exprType)
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Cannot assign {0} to {1}", exprType, identifierType);
                    break;

                case BeeNodeType.IfStatement:
                    if (EvaluateNodeType(node.Children[0]) != "Boolean")
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Invalid expression type for if. Only Boolean accepted");
                    break;

                case BeeNodeType.IfElseStatement:
                    if (EvaluateNodeType(node.Children[0]) != "Boolean")
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Invalid expression type for if. Only Boolean accepted");
                    break;

                case BeeNodeType.WhileStatement:
                    if (EvaluateNodeType(node.Children[0]) != "Boolean")
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Invalid expression type for while. Only Boolean accepted");
                    break;

                case BeeNodeType.ForStatement:
                    var conditionExpression = node.Children[1];
                    if (EvaluateNodeType(conditionExpression) != "Boolean")
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Invalid condition expression type for for statement. Only Boolean accepted");
                    break;

                case BeeNodeType.YieldStatement:
                     expression = node.Children[0];
                     if (EvaluateNodeType(expression) == "String")
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Invalid expression type for yield argument. Only Boolean or Number accepted");
                    break;
                default:
                    break;
            }
        }

        protected string EvaluateNodeType(BeeNode expressionNode)
        {
            //tipo restituito dalla nativa
            switch (expressionNode.NodeType)
            {

                #region Expression
                case BeeNodeType.Expression:
                    {
                        if (expressionNode.Children.Count == 1)
                            return EvaluateNodeType(expressionNode.Children[0]);
                        else
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, expressionNode, "This expression has more than one child. Ma chi è scemu!?");
                        break;
                    }
                #endregion

                #region String
                case BeeNodeType.String:
                    {
                        return "String";
                    }
                #endregion

                #region Term
                case BeeNodeType.Term:
                    {
                        return EvaluateTermType(expressionNode.Children[0]);
                    }
                #endregion

                #region BinaryExpression
                case BeeNodeType.BinaryExpression:
                    {
                        var firstExprType = EvaluateNodeType(expressionNode.Children[0]);
                        var operandString = expressionNode.Children[1].Children[0].Token.Value as String;
                        var secondExprType = EvaluateNodeType(expressionNode.Children[2]);
                        if (firstExprType != secondExprType)
                        {
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, expressionNode, "Operator '{0}' cannot be applied to {1} and {2}", operandString, firstExprType, secondExprType);
                        }
                        var acceptedInputTypes = operatorTypes[operandString].InputTypes;
                        if (!acceptedInputTypes.Contains(firstExprType))
                        {
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, expressionNode, "Operator '{0}' cannot be applied to {1} and {2}", expressionNode.Node.Span.Location.Line + 1);
                        }
                        var outputType = operatorTypes[operandString].OutputType;
                        return outputType;
                    }
                #endregion

                #region UnaryExpression
                case BeeNodeType.UnaryExpression:
                    {
                        var operandString = expressionNode.Children[0].Children[0].Token.Value as String;
                        var expressionType = EvaluateNodeType(expressionNode.Children[1]);

                        var acceptedInputTypes = operatorTypes[operandString].InputTypes;
                        if (!acceptedInputTypes.Contains(expressionType))
                        {
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, expressionNode, "Operator '{0}' cannot be applied to {1}", operandString, expressionType);
                        }
                        var outputType = operatorTypes[operandString].OutputType;
                        return outputType;
                    }
                #endregion

                #region Identifier
                case BeeNodeType.Identifier:
                    {
                        if (IdTypeMap.ContainsKey(expressionNode.Token.Value as String))
                        {
                            return IdTypeMap[expressionNode.Token.Value as String];
                        }
                        else
                        {
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, expressionNode, "Cannot resolve Identifier {0} at compile time", expressionNode.Token.Value as String);
                        }
                        break;
                    }
                #endregion

                #region FunctionCall
                case BeeNodeType.FunctionCall:
                    if (FunctionIdTypeMap.ContainsKey(expressionNode.Children[0].Token.Value as String))
                    {
                        return FunctionIdTypeMap[expressionNode.Children[0].Token.Value as String].OutputType;
                    }
                    else
                    {
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, expressionNode, "Cannot resolve function {0} at compile time", expressionNode.Children[0].Token.Value as String);
                    }
                    break;
                #endregion

                #region NativeFunctionCall
                case BeeNodeType.NativeFunctionCall:
                    if (NativeFunctionIdTypeMap.ContainsKey(expressionNode.Children[0].Token.Value as String))
                    {
                        return NativeFunctionIdTypeMap[expressionNode.Children[0].Token.Value as String].OutputType;
                    }
                    else
                    {
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, expressionNode, "Cannot resolve native function {0} at compile time", expressionNode.Children[0].Token.ValueString);
                    }
                    break;
                #endregion

            }
            return "Undefined";
        }

        private string EvaluateTermType(BeeNode termNode)
        {
            switch (termNode.NodeType)
            {
                case BeeNodeType.Number: return "Number";
                case BeeNodeType.String: return "String";
                case BeeNodeType.Constant: return "Boolean";
                case BeeNodeType.Identifier:
                    {
                        if (IdTypeMap.ContainsKey(termNode.Token.Value as String))
                        {
                            return IdTypeMap[termNode.Token.Value as String];
                        }
                        else
                        {
                            BeeCompileException.Throw(CompileErrorType.TypeCheckError, termNode, "Cannot resolve Identifier {0} at compile time", termNode.Token.Value as String);
                        }
                        break;
                    }
                case BeeNodeType.ParExpression:
                    {
                        return EvaluateNodeType(termNode.Children[0]);
                    }
            }
            BeeCompileException.Throw(CompileErrorType.TypeCheckError, termNode, "Unable to resolve term type");
            return "";
        }

        private void CheckFunctionReturnType(BeeNode node, string returnType)
        {
            if (node.NodeType == BeeNodeType.VoidConstant && returnType != "Void")
                BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Invalid return type void. Return type must be {0}", returnType);
            if (node.NodeType == BeeNodeType.ReturnExpression)
            {
                var exprNode = node.Children.Find(n => n.NodeType == BeeNodeType.Expression);
                if (exprNode != null)
                {
                    var exprType = EvaluateNodeType(exprNode);
                    if (exprType != returnType)
                        BeeCompileException.Throw(CompileErrorType.TypeCheckError, node, "Invalid return type {0}. Return type must be {1}", exprType, returnType);
                }
            }

            foreach (var child in node.Children)
                CheckFunctionReturnType(child, returnType);
        }

    }
}
