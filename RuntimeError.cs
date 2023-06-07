namespace Lox;

public class RuntimeError : Exception
{
    public readonly Token token;

    public RuntimeError(Token token, String message) : base(message)
    {
        this.token = token;
    }
}
