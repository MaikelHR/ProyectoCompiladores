@parser::header {
using Antlr4.Runtime;
}

parser grammar MiniCSharpParser;

options { tokenVocab=MiniCSharpLexer; }

program
    : classDecl+ EOF
    ;

classDecl
    : CLASS ID LBRACE classBody RBRACE 
    ;

classBody
    : (varDecl | methodDecl | innerClassDecl)*
    ;

varDecl
    : type ID (ASSIGN expr)? SEMICOLON
    ;

methodDecl
    : type ID LPAREN paramList? RPAREN block
    ;

paramList
    : param (COMMA param)*
    ;

param
    : type ID
    ;

block
    : LBRACE stmt* RBRACE
    ;

stmt
    : varDecl
    | exprStmt
    | block
    | IF LPAREN expr RPAREN stmt (ELSE stmt)?
    | WHILE LPAREN expr RPAREN stmt
    | RETURN expr? SEMICOLON
    ;

exprStmt
    : expr SEMICOLON
    ;

expr
    : expr MULOP expr
    | expr ADDOP expr
    | expr RELOP expr
    | expr LOGOP expr
    | ID LPAREN argList? RPAREN
    | ID
    | literal
    | LPAREN expr RPAREN
    ;

argList
    : expr (COMMA expr)*
    ;

literal
    : INT
    | DOUBLE
    | CHAR
    | STRING
    | BOOL
    ;

type
    : INT_TYPE
    | CHAR_TYPE
    | DOUBLE_TYPE
    | BOOL_TYPE
    | STRING_TYPE
    | ID
    | type LBRACK RBRACK // lista unidimensional
    ;

innerClassDecl
    : CLASS ID LBRACE (varDecl)* RBRACE
    ; 