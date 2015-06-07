using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace BeeCompiler
{
    [Serializable]
    public class BeeCompileException : Exception
    {
        static public void Throw(CompileErrorType Type, BeeNode Node, string Message, params Object[] Args)
        {
            if (Node != null)
                Message = Message + string.Format("\nAt line {0}.", Node.Node.Span.Location.Line + 1);

            throw new BeeCompileException(Type, String.Format(Message, Args)) { SourceSpan = (Node == null ? new SourceSpan() : Node.Node.Span) };
        }

        public CompileErrorType ErrorType { get; private set; }
        public SourceSpan SourceSpan { get; private set; }

        protected BeeCompileException(CompileErrorType ErrorType) { this.ErrorType = ErrorType; }
        protected BeeCompileException(CompileErrorType ErrorType, string message) : base(message) { this.ErrorType = ErrorType; }
        protected BeeCompileException(CompileErrorType ErrorType, string message, Exception inner) : base(message, inner) { this.ErrorType = ErrorType; }
        protected BeeCompileException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
