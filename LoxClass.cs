namespace Lox;

class LoxClass : ILoxCallable
{
    public String Name { get; init; }
    public Dictionary<string, LoxFunction> Methods {get; init;}

    public LoxClass(String name, Dictionary<string, LoxFunction> methods)
    {
        this.Name = name;
        this.Methods = methods;
    }

    public LoxFunction FindMethod(string name){
        return Methods.GetValueOrDefault(name, null);
    }

    override public String ToString()
    {
        return Name;
    }

    public int arity()
    {
        return 0;
    }

    public object Call(Interpreter interpreter, List<object> args)
    {
        LoxInstance instance = new(this);
        return instance;
    }
}
