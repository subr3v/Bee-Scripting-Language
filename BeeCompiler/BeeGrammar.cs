using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace BeeCompiler
{
    /// <summary>
    /// Backus-Naur definition of Bee Syntax.
    ///
    /// Program :== IncludeStatements VariableStatements FunctionDefinitions
    /// IncludeStatements :== IncludeStatement*
    /// VariableStatements :== (VariableStatement | PropertyStatement)*
    /// FunctionDefinitions :== (FunctionDefinition | CallbackDefinition)+
    /// IncludeStatement :== "include" Filename
    /// Filename :== String
    /// VariableStatement :== "var" Identifier AssignmentOp Expression
    /// PropertyStatement :== "property" "var" Identifier AssignmentOp Expression
    /// Term :== Number | String | Identifier | ParExpr | Constant
    /// ParExpr :== "(" Expression ")"
    /// AssignmentOp :== "="
    /// Expression :== Term | FunctionCall | NativeFunctionCall | BinaryExpression | UnaryExpression
    /// BinaryExpression :== Expression BinaryOp Expression
    /// UnaryExpression :== UnaryOperator Expression
    /// FunctionCall :== Identifier "(" ArgumentList ")
    /// ArgumentList :== Expression * separator ","
    /// NativeFunctionCall :== "@" Identifier + "(" ArgumentList ")"
    /// BinaryOp :== "+" | "-" | "*" | "/"
    /// UnaryOp :== "!" | "-"
    /// FunctionDefinition :== "function" Identifier "(" FunctionSignature ")" "{" LocalStatements InstructionStatements "}"
    /// CallbackDefinition :== "callback" Identifier "{" LocalStatements InstructionStatements "}"
    /// LocalStatements :== LocalStatement*
    /// LocalStatement :== "local" Identifier AssignmentOp Expression
    /// FunctionSignature :== Identifier* separator ","
    /// InstructionStatements :== IntructionStatement * separator ";"   
    /// InstructionStatement :== AssignmentStatement | FunctionCall | NativeFunctionCall | IfElseStatement | WhileStatement | ForStatement
    /// AssignmentStatement :== Identifier AssignmentOp Expression
    /// IfStatement :== "if" "(" Expression ")" "{" InstructionStatements "}"
    /// IfElseStatement :== "if" "(" Expression ")" "{" InstructionStatements "}" "else" "{" InstructionStatements "}"
    /// WhileStatement :== "while" "(" Expression ")" "{" InstructionStatements "}"
    /// ForStatement :== "for" "(" AssignmentStatement ";" Expression ";" AssignmentStatement ")" "{" InstructionStatements "}"
    /// ReturnStatement :== "return" Expression
    /// </summary>

    public class BeeGrammar : Grammar
    {
        public BeeGrammar()
            : base(true)
        {
            var Program = new NonTerminal("Program");
            var IncludeStatements = new NonTerminal("IncludeStatements");
            var VariableStatements = new NonTerminal("VariableStatements");
            var FunctionDefinitions = new NonTerminal("FunctionDefinitions");
            var IncludeStatement = new NonTerminal("IncludeStatement");
            var VariableStatement = new NonTerminal("VariableStatement");
            var PropertyStatement = new NonTerminal("PropertyStatement");
            var Term = new NonTerminal("Term");
            var ParExpression = new NonTerminal("ParExpression");
            var Number = new NumberLiteral("Number", NumberOptions.AllowSign);
            var String = new StringLiteral("String", "\"");
            var Constant = new ConstantTerminal("Constant");
            var VoidConstant = new ConstantTerminal("VoidConstant");
            var Identifier = new IdentifierTerminal("Identifier");
            var Type = new NonTerminal("Type");
            var FunctionType = new NonTerminal("FunctionType");
            var TypedIdentifier = new NonTerminal("TypedIdentifier");
            var AssignmentOp = new NonTerminal("AssignmentOp");
            var Expression = new NonTerminal("Expression");
            var BinaryExpression = new NonTerminal("BinaryExpression");
            var UnaryExpression = new NonTerminal("UnaryExpression");
            var FunctionCall = new NonTerminal("FunctionCall");
            var ArgumentList = new NonTerminal("ArgumentList");
            var NativeFunctionCall = new NonTerminal("NativeFunctionCall");
            var BinaryOp = new NonTerminal("BinaryOp");
            var UnaryOp = new NonTerminal("UnaryOp");
            var FunctionDefinition = new NonTerminal("FunctionDefinition");
            var CallbackDefinition = new NonTerminal("CallbackDefinition");
            var LocalStatements = new NonTerminal("LocalStatements");
            var LocalStatement = new NonTerminal("LocalStatement");
            var FunctionSignature = new NonTerminal("FunctionSignature");
            var InstructionStatements = new NonTerminal("InstructionStatements");
            var InstructionStatement = new NonTerminal("InstructionStatement");
            var AssignmentStatement = new NonTerminal("AssignmentStatement");
            var IfStatement = new NonTerminal("IfStatement");
            var IfElseStatement = new NonTerminal("IfElseStatement");
            var WhileStatement = new NonTerminal("WhileStatement");
            var ForStatement = new NonTerminal("ForStatement");
            var ReturnStatement = new NonTerminal("ReturnStatement");
            var ReturnExpression = new NonTerminal("ReturnExpression");
            var YieldStatement = new NonTerminal("YieldStatement");
            var GlobalVariableStatement = new NonTerminal("GlobalVariable");
            var GlobalFunctionDefinition = new NonTerminal("GlobalFunctionDefinition");
            
            var LineComment = new CommentTerminal("Line_Comment", "//", "\n", "\r\n");

            Constant.Priority = 10;
            VoidConstant.Priority = 10;

            NonGrammarTerminals.Add(LineComment);

            Program.Rule = IncludeStatements + VariableStatements + FunctionDefinitions;
            IncludeStatements.Rule = MakeStarRule(IncludeStatements, ToTerm(";"), IncludeStatement, TermListOptions.AllowTrailingDelimiter);
            VariableStatements.Rule = MakeStarRule(VariableStatements, ToTerm(";"), GlobalVariableStatement, TermListOptions.AllowTrailingDelimiter);
            FunctionDefinitions.Rule = MakePlusRule(FunctionDefinitions, GlobalFunctionDefinition);
            IncludeStatement.Rule = ToTerm("include") + String;
            VariableStatement.Rule = ToTerm("var") + Identifier + AssignmentOp + Expression;
            PropertyStatement.Rule = ToTerm("property") + Identifier + AssignmentOp + Expression;
            Term.Rule = Number | String | Constant | Identifier | ParExpression;
            GlobalFunctionDefinition.Rule = FunctionDefinition | CallbackDefinition;
            GlobalVariableStatement.Rule = VariableStatement | PropertyStatement;
            ParExpression.Rule = ToTerm("(") + Expression + ")";
            AssignmentOp.Rule = ToTerm("=");
            Expression.Rule = Term | FunctionCall | NativeFunctionCall | BinaryExpression | UnaryExpression;
            BinaryExpression.Rule = Expression + BinaryOp + Expression;
            UnaryExpression.Rule = UnaryOp + Expression;
            FunctionCall.Rule = Identifier + "(" + ArgumentList + ")";
            ArgumentList.Rule = MakeStarRule(ArgumentList, ToTerm(","), Expression);
            NativeFunctionCall.Rule = ToTerm("@") + Identifier + "(" + ArgumentList + ")";
            BinaryOp.Rule = ToTerm("+") | "-" | "*" | "/" | ">" | ">=" | "<" | ">=" | "==" | "&&" | "||";
            UnaryOp.Rule = ToTerm("!") | "-";
            Type.Rule = ToTerm("num") | "string" | "bool";
            TypedIdentifier.Rule = Type + Identifier;
            FunctionType.Rule = ToTerm("num") | "string" | "bool" | VoidConstant;
            FunctionDefinition.Rule = ToTerm("function") + FunctionType + Identifier + "(" + FunctionSignature + ")" + "{" + LocalStatements + InstructionStatements + "}";
            CallbackDefinition.Rule = ToTerm("callback") + Identifier + "{" + LocalStatements + InstructionStatements + "}";
            LocalStatements.Rule = MakeStarRule(LocalStatements, ToTerm(";"), LocalStatement, TermListOptions.AllowTrailingDelimiter);
            LocalStatement.Rule = ToTerm("local") + Identifier + AssignmentOp + Expression;
            FunctionSignature.Rule = MakeStarRule(FunctionSignature, ToTerm(","), TypedIdentifier);
            InstructionStatements.Rule = MakeStarRule(InstructionStatements, ToTerm(";"), InstructionStatement, TermListOptions.AllowTrailingDelimiter);
            InstructionStatement.Rule = AssignmentStatement | FunctionCall | NativeFunctionCall | IfStatement | IfElseStatement | WhileStatement | ForStatement | ReturnStatement | YieldStatement;
            AssignmentStatement.Rule = Identifier + AssignmentOp + Expression;
            IfStatement.Rule = ToTerm("if") + "(" + Expression + ")" + "{" + InstructionStatements + "}";
            IfElseStatement.Rule = ToTerm("if") + "(" + Expression + ")" + "{" + InstructionStatements + "}" + "else" + "{" + InstructionStatements + "}";
            WhileStatement.Rule = ToTerm("while") + "(" + Expression + ")" + "{" + InstructionStatements + "}";
            ForStatement.Rule = ToTerm("for") + "(" + AssignmentStatement + ";" + Expression + ";" + AssignmentStatement + ")" + "{" + InstructionStatements + "}";
            ReturnStatement.Rule = ToTerm("return") + ReturnExpression;
            ReturnExpression.Rule = (Expression | VoidConstant);
            YieldStatement.Rule = ToTerm("yield") + Expression;
            
            Constant.Add("true", true);
            Constant.Add("false", false);

            VoidConstant.Add("void", "void");

            this.Root = Program;

            MarkPunctuation("=", "(", ")", "{", "}", ",", "@", ";", "if", "else", "for", "while", "function", "include", "var" , "property" , "callback" , "local", "return", "yield");
            //MarkPunctuation("=", "==", ">=", "<=", ">", "<", "+", "-", "*", "/", "&&", "||", "!", "(", ")", "{", "}", ",", "@", "if", "else", "for", "function", "include", "var", "local");
        }
    }
}
