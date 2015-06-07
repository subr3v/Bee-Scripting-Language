using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Irony.Parsing;
using BeeVM;

namespace BeeCompiler
{
    public class BeeCompiler
    {
        static public IDataProvider DataProvider { get; set; }

        static BeeCompiler()
        {
            DataProvider = new FileDataProvider();
        }

        public static bool Compile(string name , NativeCallInfo nativeCalls , out BeeScript script)
        {
            Irony.Parsing.Parser parser = new Irony.Parsing.Parser(new BeeGrammar());
            var tree = parser.Parse(DataProvider.GetScript(name));
            if (tree != null && tree.Root != null)
            {
                BeeNode Root = new BeeNode(tree.Root,null);
                BeeTreeProcessor treeProcessor = new BeeTreeProcessor(nativeCalls);
                StringBuilder builder = new StringBuilder();
                script = treeProcessor.ProcessTree(Root,true);
                return true;
            }
            else
            {
                foreach (var message in tree.ParserMessages)
                {
                    BeeCompileException.Throw(CompileErrorType.ParseError, null,
                        string.Format("Message Error {0} \nSource Location : L {1} , C {2} , P {3} \nLevel {4} File : {5}", message.Message.Replace("{","{{").Replace("}","}}"),
                        message.Location.Line, message.Location.Column, message.Location.Position, message.Level, name));
                }
            }
            throw new NotImplementedException();
        }

        static public void WriteTreeNode(ParseTreeNode node, int level , StringBuilder builder)
        {
            for (int i = 0; i < level; i++)
            {
                builder.Append("\t");
            }

            builder.AppendLine(String.Format("{0} {1} [{2}]", node.Term.Name, node.Token == null ? "" : "(" + node.Token.Value + ")", node.ChildNodes.Count));

            foreach (var child in node.ChildNodes)
            {
                WriteTreeNode(child, level + 1 , builder);
            }
        }
    }
}
