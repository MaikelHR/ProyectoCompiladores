@lexer::header {
using Antlr4.Runtime;
}

lexer grammar MiniCSharpLexer;

// Comentarios
COMMENT_LINE    : '//' ~[\r\n]* -> skip ;
COMMENT_BLOCK   : '/*' .*? '*/' -> skip ;

// Espacios en blanco
WS              : [ \t\r\n]+ -> skip ;

// Identificadores
fragment LETTER : [a-zA-Z_] ;
fragment DIGIT  : [0-9] ;
ID              : LETTER (LETTER | DIGIT)* ;

// Constantes numéricas
INT             : '0' | [1-9] DIGIT* ;
DOUBLE          : INT '.' DIGIT+ ;

// Constantes carácter
CHAR            : '\'' (ESC | ~['\\\r\n]) '\'' ;
fragment ESC    : '\\' [btnfr"'\\] ;

// Constantes string
STRING          : '"' (ESC | ~["\\\r\n])* '"' ;

// Operadores
ASSIGN          : '=' ;
RELOP           : '==' | '!=' | '<' | '<=' | '>' | '>=' ;
ADDOP           : '+' | '-' ;
MULOP           : '*' | '/' | '%' ;
LOGOP           : '||' | '&&' ;

// Palabras reservadas
CLASS           : 'class' ;
IF              : 'if' ;
ELSE            : 'else' ;
WHILE           : 'while' ;
RETURN          : 'return' ;

// Tipos
INT_TYPE        : 'int' ;
CHAR_TYPE       : 'char' ;
DOUBLE_TYPE     : 'double' ;
BOOL_TYPE       : 'bool' ;
STRING_TYPE     : 'string' ;

// Valores booleanos
BOOL            : 'true' | 'false' ;

// Símbolos literales
LBRACE          : '{' ;
RBRACE          : '}' ;
LPAREN          : '(' ;
RPAREN          : ')' ;
LBRACK          : '[' ;
RBRACK          : ']' ;
SEMICOLON       : ';' ;
COMMA           : ',' ; 