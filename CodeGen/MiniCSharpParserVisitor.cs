//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.13.2
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from D:/TEC/Compiladores3/ProyectoCompi/ProyectoCompi/Grammar/MiniCSharpParser.g4 by ANTLR 4.13.2

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419


    using Antlr4.Runtime;

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="MiniCSharpParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.13.2")]
[System.CLSCompliant(false)]
public interface IMiniCSharpParserVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by the <c>programRule</c>
	/// labeled alternative in <see cref="MiniCSharpParser.program"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitProgramRule([NotNull] MiniCSharpParser.ProgramRuleContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>classDeclaration</c>
	/// labeled alternative in <see cref="MiniCSharpParser.classDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassDeclaration([NotNull] MiniCSharpParser.ClassDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>classBodyRule</c>
	/// labeled alternative in <see cref="MiniCSharpParser.classBody"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassBodyRule([NotNull] MiniCSharpParser.ClassBodyRuleContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>variableDeclaration</c>
	/// labeled alternative in <see cref="MiniCSharpParser.varDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariableDeclaration([NotNull] MiniCSharpParser.VariableDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>methodDeclaration</c>
	/// labeled alternative in <see cref="MiniCSharpParser.methodDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodDeclaration([NotNull] MiniCSharpParser.MethodDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>parameterList</c>
	/// labeled alternative in <see cref="MiniCSharpParser.paramList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameterList([NotNull] MiniCSharpParser.ParameterListContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>parameter</c>
	/// labeled alternative in <see cref="MiniCSharpParser.param"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameter([NotNull] MiniCSharpParser.ParameterContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>blockStmt</c>
	/// labeled alternative in <see cref="MiniCSharpParser.block"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlockStmt([NotNull] MiniCSharpParser.BlockStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>variableStatement</c>
	/// labeled alternative in <see cref="MiniCSharpParser.stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitVariableStatement([NotNull] MiniCSharpParser.VariableStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expressionStmt</c>
	/// labeled alternative in <see cref="MiniCSharpParser.stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionStmt([NotNull] MiniCSharpParser.ExpressionStmtContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>blockStatement</c>
	/// labeled alternative in <see cref="MiniCSharpParser.stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlockStatement([NotNull] MiniCSharpParser.BlockStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>ifStatement</c>
	/// labeled alternative in <see cref="MiniCSharpParser.stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIfStatement([NotNull] MiniCSharpParser.IfStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>whileStatement</c>
	/// labeled alternative in <see cref="MiniCSharpParser.stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitWhileStatement([NotNull] MiniCSharpParser.WhileStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>returnStatement</c>
	/// labeled alternative in <see cref="MiniCSharpParser.stmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitReturnStatement([NotNull] MiniCSharpParser.ReturnStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>expressionStatement</c>
	/// labeled alternative in <see cref="MiniCSharpParser.exprStmt"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionStatement([NotNull] MiniCSharpParser.ExpressionStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>identifier</c>
	/// labeled alternative in <see cref="MiniCSharpParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentifier([NotNull] MiniCSharpParser.IdentifierContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>parenthesizedExpression</c>
	/// labeled alternative in <see cref="MiniCSharpParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParenthesizedExpression([NotNull] MiniCSharpParser.ParenthesizedExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>additiveExpression</c>
	/// labeled alternative in <see cref="MiniCSharpParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAdditiveExpression([NotNull] MiniCSharpParser.AdditiveExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>relationalExpression</c>
	/// labeled alternative in <see cref="MiniCSharpParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRelationalExpression([NotNull] MiniCSharpParser.RelationalExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>multiplicativeExpression</c>
	/// labeled alternative in <see cref="MiniCSharpParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultiplicativeExpression([NotNull] MiniCSharpParser.MultiplicativeExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>literalExpression</c>
	/// labeled alternative in <see cref="MiniCSharpParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLiteralExpression([NotNull] MiniCSharpParser.LiteralExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>logicalExpression</c>
	/// labeled alternative in <see cref="MiniCSharpParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicalExpression([NotNull] MiniCSharpParser.LogicalExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>methodCall</c>
	/// labeled alternative in <see cref="MiniCSharpParser.expr"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMethodCall([NotNull] MiniCSharpParser.MethodCallContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>argumentList</c>
	/// labeled alternative in <see cref="MiniCSharpParser.argList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArgumentList([NotNull] MiniCSharpParser.ArgumentListContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>integerLiteral</c>
	/// labeled alternative in <see cref="MiniCSharpParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIntegerLiteral([NotNull] MiniCSharpParser.IntegerLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>doubleLiteral</c>
	/// labeled alternative in <see cref="MiniCSharpParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDoubleLiteral([NotNull] MiniCSharpParser.DoubleLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>charLiteral</c>
	/// labeled alternative in <see cref="MiniCSharpParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCharLiteral([NotNull] MiniCSharpParser.CharLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>stringLiteral</c>
	/// labeled alternative in <see cref="MiniCSharpParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStringLiteral([NotNull] MiniCSharpParser.StringLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>booleanLiteral</c>
	/// labeled alternative in <see cref="MiniCSharpParser.literal"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBooleanLiteral([NotNull] MiniCSharpParser.BooleanLiteralContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>charType</c>
	/// labeled alternative in <see cref="MiniCSharpParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCharType([NotNull] MiniCSharpParser.CharTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>doubleType</c>
	/// labeled alternative in <see cref="MiniCSharpParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDoubleType([NotNull] MiniCSharpParser.DoubleTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>arrayType</c>
	/// labeled alternative in <see cref="MiniCSharpParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayType([NotNull] MiniCSharpParser.ArrayTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>intType</c>
	/// labeled alternative in <see cref="MiniCSharpParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIntType([NotNull] MiniCSharpParser.IntTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>stringType</c>
	/// labeled alternative in <see cref="MiniCSharpParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStringType([NotNull] MiniCSharpParser.StringTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>boolType</c>
	/// labeled alternative in <see cref="MiniCSharpParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBoolType([NotNull] MiniCSharpParser.BoolTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>classType</c>
	/// labeled alternative in <see cref="MiniCSharpParser.type"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitClassType([NotNull] MiniCSharpParser.ClassTypeContext context);
	/// <summary>
	/// Visit a parse tree produced by the <c>innerClassDeclaration</c>
	/// labeled alternative in <see cref="MiniCSharpParser.innerClassDecl"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInnerClassDeclaration([NotNull] MiniCSharpParser.InnerClassDeclarationContext context);
}
