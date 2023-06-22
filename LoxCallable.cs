namespace Lox;

interface ILoxCallable
{
    public int arity();
    public object Call(Interpreter interpreter, List<object> args);
}

class LoxClock : ILoxCallable
{
    public int arity() => 0;

    public object Call(Interpreter interpreter, List<object> args)
    {
        return DateTime.Now.Ticks;
    }
}

class LoxFunction : ILoxCallable
{
    private readonly Stmt.FunctionStmt _declaration;
    private readonly LoxEnvironment _closure;
    private readonly bool _isInitializer;

    public LoxFunction(Stmt.FunctionStmt decl, LoxEnvironment closure, bool isInitializer)
    {
        _declaration = decl;
        _closure = closure;
        _isInitializer = isInitializer;
    }

    public int arity() => _declaration.parameters.Count();

    public object Call(Interpreter interpreter, List<object> args)
    {
        LoxEnvironment env = new LoxEnvironment(_closure);
        for (int i = 0; i < _declaration.parameters.Count(); ++i)
        {
            env.Define(_declaration.parameters[i].Lexeme, args[i]);
        }
        try
        {
            interpreter.ExecuteBlock(_declaration.body, env);
        }
        catch (Return retVal)
        {
            if (_isInitializer) return _closure.GetAt(0, "this");
            return retVal.Value;
        }

        if (_isInitializer) return _closure.GetAt(0, "this");
        return null;
    }

    override public String ToString()
    {
        return "<fn " + _declaration.name.Lexeme + ">";
    }

    internal LoxFunction bind(LoxInstance loxInstance)
    {
        LoxEnvironment env = new(_closure);
        env.Define("this", loxInstance);
        return new LoxFunction(this._declaration, env, _isInitializer);
    }
}
