using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    public enum CompileErrorType
    {
        ParseError,
        IdentifierError,
        TypeCheckError,
        FileError,
        GenerateError,
        IncludeError,
    }
}
