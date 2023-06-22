namespace Lox;

class LoxClass : ILoxCallable
{
    public String Name { get; init; }
    public Dictionary<string, LoxFunction> Methods { get; init; }

    public LoxClass(String name, Dictionary<string, LoxFunction> methods)
    {
        this.Name = name;
        this.Methods = methods;
    }

    public LoxFunction FindMethod(string name)
    {
        return Methods.GetValueOrDefault(name, null);
    }

    override public String ToString()
    {
        return Name;
    }

    public int arity()
    {
        var initializer = FindMethod("init");
        if (initializer != null)
        {
            return initializer.arity();
        }
        return 0;

    }

    public object Call(Interpreter interpreter, List<object> args)
    {
        LoxInstance instance = new(this);

        var initializer = FindMethod("init");
        if (initializer != null)
        {
            initializer.bind(instance).Call(interpreter, args);
        }
        return instance;
    }
}
