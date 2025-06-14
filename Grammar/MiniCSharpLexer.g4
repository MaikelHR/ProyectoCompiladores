lexer grammar MiniCSharpLexer;

@header {
    using Antlr4.Runtime;
}

options { 
    language=CSharp;
}

// Tokens
CLASS: 'class';
IF: 'if';
ELSE: 'else';
WHILE: 'while';
RETURN: 'return';
INT_TYPE: 'int';
CHAR_TYPE: 'char';
DOUBLE_TYPE: 'double';
BOOL_TYPE: 'bool';
STRING_TYPE: 'string';

// Operadores
ASSIGN: '=';
ADDOP: '+' | '-';
MULOP: '*' | '/' | '%';
RELOP: '==' | '!=' | '<' | '<=' | '>' | '>=';
LOGOP: '&&' | '||';

// Delimitadores
LPAREN: '(';
RPAREN: ')';
LBRACE: '{';
RBRACE: '}';
LBRACK: '[';
RBRACK: ']';
SEMICOLON: ';';
COMMA: ',';

// Literales
INT: [1-9][0-9]* | '0';
DOUBLE: [0-9]+ '.' [0-9]+;
CHAR: '\'' (~['\\\r\n] | '\\' [nrt'\\]) '\'';
STRING: '"' (~["\\\r\n] | '\\' [nrt"\\])* '"';
BOOL: 'true' | 'false';

// Identificadores
ID: [a-zA-Z_][a-zA-Z0-9_]*;

// Comentarios
COMMENT: '/*' .*? '*/' -> skip;
LINE_COMMENT: '//' ~[\r\n]* -> skip;

// Espacios en blanco
WS: [ \t\r\n]+ -> skip; 