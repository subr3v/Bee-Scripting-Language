using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeeCompiler
{
    public enum BeeNodeType
    {
        Program,
        GlobalVariable,
        GlobalFunctionDefinition,
        IncludeStatements,
        VariableStatements,
        FunctionDefinitions,
        IncludeStatement,
        VariableStatement,
        Term,
        ParExpression,
        Number,
        String,
        Constant,
        Identifier,
        AssignmentOp,
        Expression,
        BinaryExpression,
        UnaryExpression,
        FunctionCall,
        ArgumentList,
        NativeFunctionCall,
        BinaryOp,
        UnaryOp,
        FunctionDefinition,
        LocalStatements,
        LocalStatement,
        FunctionSignature,
        InstructionStatements,
        InstructionStatement,
        AssignmentStatement,
        IfStatement,
        IfElseStatement,
        WhileStatement,
        ForStatement,
        LineComment,
        ReturnStatement,
        ReturnExpression,
        VoidConstant,
        Invalid,
        Type,
        TypedIdentifier,
        FunctionType,
        YieldStatement,
        PropertyStatement,
        CallbackDefinition
    }
}
