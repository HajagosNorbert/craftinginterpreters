public readonly struct Token {
    
    public readonly TokenType Type { get; init; } 
    public readonly string Lexeme { get; init; }
    public readonly Object Literal { get; init; }
    public readonly int Line { get; init; } 

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
