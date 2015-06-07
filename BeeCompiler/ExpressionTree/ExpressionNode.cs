using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BeeCompiler.Bytecode;

namespace BeeCompiler.ExpressionTree
{
    class ExpressionNode
    {
        private Action generator;

        public ExpressionNode(Action generator) 
        {
            this.generator = generator;
        }

        public void GenerateCode()
        {
            this.generator();
        }
    }
}
