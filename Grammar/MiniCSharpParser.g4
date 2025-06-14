parser grammar MiniCSharpParser;

@header {
    using Antlr4.Runtime;
}

options { 
    tokenVocab=MiniCSharpLexer;
    language=CSharp;
    
}

program
    : classDecl+ EOF #programRule
    ;

classDecl
    : CLASS ID LBRACE classBody RBRACE #classDeclaration
    ;

classBody
    : (varDecl | methodDecl | innerClassDecl)* #classBodyRule
    ;

varDecl
    : type ID (ASSIGN expr)? SEMICOLON #variableDeclaration
    ;

methodDecl
    : type ID LPAREN paramList? RPAREN block #methodDeclaration
    ;

paramList
    : param (COMMA param)* #parameterList
    ;

param
    : type ID #parameter
    ;

block
    : LBRACE stmt* RBRACE #blockStmt
    ;

stmt
    : varDecl #variableStatement
    | exprStmt #expressionStmt
    | block #blockStatement
    | IF LPAREN expr RPAREN stmt (ELSE stmt)? #ifStatement
    | WHILE LPAREN expr RPAREN stmt #whileStatement
    | RETURN expr? SEMICOLON #returnStatement
    ;

exprStmt
    : expr SEMICOLON #expressionStatement
    ;

expr
    : expr MULOP expr #multiplicativeExpression
    | expr ADDOP expr #additiveExpression
    | expr RELOP expr #relationalExpression
    | expr LOGOP expr #logicalExpression
    | ID LPAREN argList? RPAREN #methodCall
    | ID #identifier
    | literal #literalExpression
    | LPAREN expr RPAREN #parenthesizedExpression
    ;

argList
    : expr (COMMA expr)* #argumentList
    ;

literal
    : INT #integerLiteral
    | DOUBLE #doubleLiteral
    | CHAR #charLiteral
    | STRING #stringLiteral
    | BOOL #booleanLiteral
    ;

type
    : INT_TYPE #intType
    | CHAR_TYPE #charType
    | DOUBLE_TYPE #doubleType
    | BOOL_TYPE #boolType
    | STRING_TYPE #stringType
    | ID #classType
    | type LBRACK RBRACK #arrayType
    ;

innerClassDecl
    : CLASS ID LBRACE (varDecl)* RBRACE #innerClassDeclaration
    ;