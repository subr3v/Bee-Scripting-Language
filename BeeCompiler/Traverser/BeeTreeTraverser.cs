using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    public abstract class BeeTreeTraverser
    {
        public void TraverseNode(BeeNode node)
        {
            if (node.NodeType != BeeNodeType.Invalid)
            {
                TraverseNodeCore(node);
                foreach (var child in node.Children)
                {
                    TraverseNode(child);
                }
            }
        }

        protected abstract void TraverseNodeCore(BeeNode node);
    }

    class LambdaBeeTreeTraverser : BeeTreeTraverser
    {
        private Action<BeeNode> traverser;

        public LambdaBeeTreeTraverser(Action<BeeNode> traverser)
        {
            this.traverser = traverser;
        }

        protected override void TraverseNodeCore(BeeNode node)
        {
            traverser(node);
        }
    }
}
