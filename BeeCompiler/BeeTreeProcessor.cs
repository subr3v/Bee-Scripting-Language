using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeVM;
using System.IO;

namespace BeeCompiler
{
    public class BeeTreeProcessor
    {

        public IdentifierTraverser globals;

        private List<string> includedFileName;

        private NativeCallInfo nativeCalls;
        public BeeTreeProcessor(NativeCallInfo nativeCalls)
        {
            this.nativeCalls = nativeCalls;
            includedFileName = new List<string>();
        }
        
        public BeeScript ProcessTree(BeeNode Root, bool isRoot)
        {

            #region typechecking
            // Renames local variables of functions in order to avoid duplicate identifier problems with global variables 
            LocalVarRenamerTraverser renamer = new LocalVarRenamerTraverser();
            renamer.TraverseNode(Root);
            ParseIncludedFiles(Root.Children.Find(n => n.NodeType == BeeNodeType.IncludeStatements), Root);
            var typeCheckResult = TypeCheck(Root, isRoot);

            #endregion

            #region Bytecode generation
            VariableInfo.ResetRegisterCounter(0);
            ConstantTraverser constantTraverser = new ConstantTraverser(typeCheckResult.NativeFunctionIdTypeMap);
            constantTraverser.TraverseNode(Root);

            IdentifierTraverser globalTraverser = new IdentifierTraverser(true);
            globalTraverser.TraverseNode(Root);

            RegisterMap GlobalMap = new RegisterMap();
            VariableInfo.ResetRegisterCounter();
            foreach (var identifierAndType in globalTraverser.VariablesIdentifiers)
            {
                GlobalMap.Add(identifierAndType.Key, new VariableInfo(null));
            }
            foreach (var identifierAndType in globalTraverser.FunctionIdentifiers)
            {
                GlobalMap.Add(identifierAndType.Key, new VariableInfo(null));
            }

            FunctionTraverser funcTraverser = new FunctionTraverser(GlobalMap, constantTraverser.ConstantMap);
            funcTraverser.TraverseNode(Root);

            Bytecode.ByteCodeWriter finalCodeWriter = new Bytecode.ByteCodeWriter();
            for (int i = 0; i < GlobalMap.Count; i++)
            {
                if (globalTraverser.PropertyTable.ContainsKey(GlobalMap.Keys.ElementAt(i)))
                {
                    bool isProperty = globalTraverser.PropertyTable[GlobalMap.Keys.ElementAt(i)];
                    finalCodeWriter.AddGlobal(GlobalMap.Keys.ElementAt(i), isProperty);
                }
                else
                    finalCodeWriter.AddGlobal(GlobalMap.Keys.ElementAt(i), false);
            }

            //Merge GlobalInitializer into main and add placeholders for Func Initialization
            var initializers = funcTraverser.functionsCode["GlobalInitializer"];
            var mainInstructions = funcTraverser.functionsCode["Main"];

            foreach (var func in funcTraverser.functionsCode.ToList())
            {
                if (!constantTraverser.ConstantMap.ContainsKey(func.Key) && func.Key != "Main")
                    funcTraverser.functionsCode.Remove(func.Key);
            }

            Array.Resize(ref initializers, mainInstructions.Length + initializers.Length + ((funcTraverser.functionsCode.Count - 1) * 2));

            int funcNumber = (funcTraverser.functionsCode.Count - 1);

            for (int i = (initializers.Length - (mainInstructions.Length)); i < initializers.Length; i++)
            {
                initializers[i] = mainInstructions[i - (initializers.Length - mainInstructions.Length)];
            }

            funcTraverser.functionsCode["Main"] = initializers;

            finalCodeWriter.InjectInstructions(funcTraverser.functionsCode["Main"]);
            foreach (var funcCode in funcTraverser.functionsCode)
            {
                if (funcCode.Key != "Main" && funcCode.Key != "GlobalInitializer")
                {
                    var res = finalCodeWriter.InjectInstructions(funcCode.Value);
                    constantTraverser.ConstantMap[funcCode.Key].Value = res.Item1;
                    GlobalMap[funcCode.Key].Value = res.Item1;
                }
            }

            foreach (var constant in constantTraverser.ConstantMap)
            {
                finalCodeWriter.AddConstant(constant.Value.Value);
            }

            int funcCounter = 0;

            funcTraverser.functionsCode.Remove("Main");

            for (int i = (initializers.Length - (mainInstructions.Length + ((funcTraverser.functionsCode.Count) * 2))); i < (initializers.Length - (mainInstructions.Length)); i += 2)
            {
                var funcKey = funcTraverser.functionsCode.Keys.ElementAt(funcCounter);
                Instruction loadConst = new Instruction() { Opcode = Opcodes.LOADCONST };
                BeeUtils.ConvertToBytes((short)(constantTraverser.ConstantMap[funcKey].RegisterLocation ), out loadConst.Op1, out loadConst.Op2);
                loadConst.Op3 = 0;
                Instruction setGlobal = new Instruction() { Opcode = Opcodes.SETGLOBAL, Op1 = 0, Op2 = (byte)GlobalMap[funcKey].RegisterLocation };
                finalCodeWriter.InjectInstruction(i, loadConst);
                finalCodeWriter.InjectInstruction(i + 1, setGlobal);
                funcCounter++;
            }

            foreach (var function in globalTraverser.FunctionIdentifiers)
            {
                if (globalTraverser.CallbackTable[function.Key])
                {
                    finalCodeWriter.RegisterCallback(function.Key, constantTraverser.ConstantMap[function.Key].RegisterLocation);
                }
            }

            return finalCodeWriter.Script;
            #endregion
        }

        private TypeCheckTraverser TypeCheck(BeeNode Root, bool isRoot)
        {
            // Fetches every identifier of the program, including function identifiers 
            IdentifierTraverser idTraverser = new IdentifierTraverser();
            idTraverser.TraverseNode(Root);

            var variablesIdentifiers = idTraverser.VariablesIdentifiers;
            var functionIndentifiers = idTraverser.FunctionIdentifiers;

            if (isRoot)
            {
                if (!functionIndentifiers.ContainsKey("Main"))
                    BeeCompileException.Throw(CompileErrorType.FileError, null, "You need to define a function called Main ( main function ) in the starting script");
                else if (functionIndentifiers["Main"].OutputType != "Void" || functionIndentifiers["Main"].InputTypes.Length != 0)
                    BeeCompileException.Throw(CompileErrorType.FileError, null, "Main function must return void and have 0 arguments");
            }
            TypeCheckTraverser typeCheckTraverser = new TypeCheckTraverser(variablesIdentifiers, functionIndentifiers,nativeCalls);
            typeCheckTraverser.TraverseNode(Root);
            return typeCheckTraverser;
        }

        private void ParseIncludedFiles(BeeNode IncludeStatementsNode, BeeNode IncluderNode)
        {
            foreach (var includeStatement in IncludeStatementsNode.Children)
            {
                string fileName = includeStatement.Children[0].Token.Value as String;
                if (includedFileName.Contains(fileName))
                {
                    BeeCompileException.Throw(CompileErrorType.IncludeError, IncluderNode, "Cannot include file more than once. File name {0}", fileName);
                }
                includedFileName.Add(fileName);
                Irony.Parsing.Parser parser = new Irony.Parsing.Parser(new BeeGrammar());
                var tree = parser.Parse( BeeCompiler.DataProvider.GetScript(fileName) );
                if (tree != null && tree.Root != null)
                {
                    BeeNode IncludedRootNode = new BeeNode(tree.Root, null);

                    LocalVarRenamerTraverser renamer = new LocalVarRenamerTraverser();
                    renamer.TraverseNode(IncludedRootNode);
                    TypeCheck(IncludedRootNode, false);

                    ParseIncludedFiles(IncludedRootNode.Children.Find(n => n.NodeType == BeeNodeType.IncludeStatements), IncludedRootNode);

                    //add included global variables statements before the includers ones
                    IncluderNode.Children.Find(n => n.NodeType == BeeNodeType.VariableStatements).Children.InsertRange(0, IncludedRootNode.Children.Find(n => n.NodeType == BeeNodeType.VariableStatements).Children);
                    //Merge included function definitions statements before the includers ones
                    IncluderNode.Children.Find(n => n.NodeType == BeeNodeType.FunctionDefinitions).Children.InsertRange(0,IncludedRootNode.Children.Find(n => n.NodeType == BeeNodeType.FunctionDefinitions).Children);

                }
                else
                {
                    foreach (var message in tree.ParserMessages)
                    {
                        BeeCompileException.Throw(CompileErrorType.ParseError, null,
                            String.Format("Message Error {0}\nSource Location : L {1} , C {2} , P {3}\nLevel {4} [File:{5}]", message.Message,
                            message.Location.Line, message.Location.Column, message.Location.Position, message.Level, fileName));
                    }
                }
            }
        }
    }
}
