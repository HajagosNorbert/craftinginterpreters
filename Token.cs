public readonly struct Token {
    
    readonly TokenType Type { get; init; } 
    readonly string Lexeme { get; init; }
    readonly Object Literal { get; init; }
    readonly int Line { get; init; } 

    public Token (TokenType type, string lexeme, Object literal, int line) {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
    }

    public override string ToString(){
        return Type + " " + Lexeme + " " + Literal;
    }
}
