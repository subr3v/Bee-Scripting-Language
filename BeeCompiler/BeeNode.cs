using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace BeeCompiler
{
    public class BeeNode
    {

        public BeeNode Parent;
        public Irony.Parsing.ParseTreeNode Node;
        public BeeNodeType NodeType;
        public List<BeeNode> Children;
        public Token Token { get { return Node.Token; } }

        public BeeNode(ParseTreeNode node , BeeNode parent)
        {
            this.Node = node;
            this.Parent = parent;
            Children = new List<BeeNode>();

            if (!Enum.TryParse<BeeNodeType>(node.Term.Name, out NodeType))
                NodeType = BeeNodeType.Invalid;
            else
            {
                foreach (var child in node.ChildNodes)
                {
                    Children.Add(new BeeNode(child,this));
                }
            }
        }
    }
}
