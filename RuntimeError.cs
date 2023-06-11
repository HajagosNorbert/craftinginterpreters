namespace Lox;

public class RuntimeError : Exception
{
    public Token Token {get; init;}

    public RuntimeError(Token token, String message) : base(message)
    {
        this.Token = token;
    }
}
