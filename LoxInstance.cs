namespace Lox;

class LoxInstance
{
    private LoxClass _klass;
    private readonly Dictionary<string, object> _fields = new();

    public LoxInstance(LoxClass klass)
    {
        this._klass = klass;
    }

    override public String ToString()
    {
        return _klass.Name + " instance";
    }

    internal object Get(Token name)
    {
        if (_fields.TryGetValue(name.Lexeme, out object value))
        {
            return value;
        }
        var method = _klass.FindMethod(name.Lexeme);
        if (method != null)
        {
            return method.bind(this);
        }

        throw new RuntimeError(name, "Undefined property '" + name.Lexeme + "'.");
    }

    internal void Set(Token name, object value)
    {
        _fields[name.Lexeme] = value;
    }
}
