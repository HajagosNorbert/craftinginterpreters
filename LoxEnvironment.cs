namespace Lox;

class LoxEnvironment
{
    private Dictionary<String, Object> _values = new();
    public LoxEnvironment? Enclosing { get; init; } = null;

    public LoxEnvironment(LoxEnvironment enclosing)
    {
        Enclosing = enclosing;
    }

    public LoxEnvironment()
    {
        Enclosing = null;
    }

    public void Define(string name, Object value)
    {
        if (!_values.TryAdd(name, value)){
            _values[name] = value;
        }
    }

    public Object Get(Token name)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            return _values.GetValueOrDefault(name.Lexeme)!;
        }
        if (Enclosing != null)
        {
            return Enclosing.Get(name);
        }
        throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public void Assign(Token name, object value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }
        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);
            return;
        }
        throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
    }
}
