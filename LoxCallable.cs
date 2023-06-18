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
    public LoxFunction(Stmt.FunctionStmt decl, LoxEnvironment closure)
    {
        _declaration = decl;
        _closure = closure;
    }

    public int arity() => _declaration.parameters.Count();

    public object Call(Interpreter interpreter, List<object> args)
    {
        LoxEnvironment env = new LoxEnvironment(_closure);
        for (int i = 0; i < _declaration.parameters.Count(); ++i)
        {
            env.Define(_declaration.parameters[i].Lexeme, args[i]);
        }
        try {
            interpreter.ExecuteBlock(_declaration.body, env);
        } catch(Return retVal){
            return retVal.Value;
        }
        return null;
    }

    override public String ToString()
    {
        return "<fn " + _declaration.name.Lexeme + ">";
    }

}
