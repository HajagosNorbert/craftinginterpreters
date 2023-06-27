namespace Lox;
enum FunctionType
{
    NONE,
    METHOD,
    INITIALIZER,
    FUNCTION
}
enum ClassType
{
    NONE,
    SUBCLASS,
    CLASS
}

class Resolver : Expr.Visitor<object?>, Stmt.Visitor<object?>
{
    public Interpreter Interpreter { get; init; }
    private readonly Stack<Dictionary<string, bool>> _scopes = new();
    private FunctionType _enclosingFunction = FunctionType.NONE;
    private ClassType _currentClass = ClassType.NONE;

    public Resolver(Interpreter interpreter)
    {
        Interpreter = interpreter;
    }

    public object? VisitBlockStmt(Stmt.BlockStmt block)
    {
        BeginScope();
        Resolve(block.statements);
        EndScope();
        return null;
    }

    public object? VisitIf_Stmt(Stmt.If_Stmt if_)
    {
        Resolve(if_.condition);
        Resolve(if_.thenBranch);
        if (if_.elseBranch != null) Resolve(if_.elseBranch);
        return null;
    }

    public object? VisitWhile_Stmt(Stmt.While_Stmt while_)
    {
        Resolve(while_.condition);
        Resolve(while_.body);
        return null;
    }

    public object? VisitReturn_Stmt(Stmt.Return_Stmt return_)
    {
        if (_enclosingFunction == FunctionType.NONE)
        {
            Lox.Error(return_.keyword, "Can't return from top-level code.");
        }
        if (return_.value != null)
        {
            if (_enclosingFunction == FunctionType.INITIALIZER)
            {
                Lox.Error(return_.keyword,
                        "Can't return a value from an initializer.");
            }
            Resolve(return_.value);
        }
        return null;
    }

    public object? VisitFunctionStmt(Stmt.FunctionStmt function)
    {
        Declare(function.name);
        Define(function.name);

        ResolveFunction(function, FunctionType.FUNCTION);
        return null;
    }

    private void ResolveFunction(Stmt.FunctionStmt function, FunctionType funcType)
    {
        var prevFunctionType = _enclosingFunction;
        _enclosingFunction = funcType;

        BeginScope();
        foreach (var param in function.parameters)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.body);
        EndScope();
        _enclosingFunction = prevFunctionType;
    }

    public object? VisitExpressionStmt(Stmt.ExpressionStmt expression)
    {
        Resolve(expression.expression);
        return null;
    }

    public object? VisitPrintStmt(Stmt.PrintStmt print)
    {
        Resolve(print.expression);
        return null;
    }

    public object? VisitVarStmt(Stmt.VarStmt var)
    {
        Declare(var.name);
        if (var.initializer != null)
        {
            Resolve(var.initializer);
        }
        Define(var.name);
        return null;
    }

    private void Declare(Token name)
    {
        if (_scopes.Count() == 0)
        {
            return;
        }
        if (_scopes.Peek().ContainsKey(name.Lexeme))
        {
            Lox.Error(name, "Already a variable with this name in this scope.");
        }
        _scopes.Peek().Add(name.Lexeme, false);
    }

    private void Define(Token name)
    {
        if (_scopes.Count() == 0)
        {
            return;
        }
        _scopes.Peek()[name.Lexeme] = true;
    }

    public object? VisitAssignExpr(Expr.AssignExpr assign)
    {
        Resolve(assign.value);
        ResolveLocal(assign, assign.name);
        return null;
    }

    public object? VisitBinaryExpr(Expr.BinaryExpr binary)
    {
        Resolve(binary.left);
        Resolve(binary.right);
        return null;
    }

    public object? VisitGroupingExpr(Expr.GroupingExpr grouping)
    {
        Resolve(grouping.expression);
        return null;
    }

    public object? VisitLogicalExpr(Expr.LogicalExpr logical)
    {
        Resolve(logical.left);
        Resolve(logical.right);
        return null;
    }

    public object? VisitLiteralExpr(Expr.LiteralExpr literal)
    {
        return null;
    }

    public object? VisitUnaryExpr(Expr.UnaryExpr unary)
    {
        Resolve(unary.right);
        return null;
    }

    public object? VisitCallExpr(Expr.CallExpr call)
    {
        Resolve(call.callee);

        foreach (var arg in call.args)
        {
            Resolve(arg);
        }

        return null;
    }

    public object? VisitVariableExpr(Expr.VariableExpr variable)
    {
        if (_scopes.Count() != 0 && _scopes.Peek().GetValueOrDefault(variable.name.Lexeme, true) == false)
        {
            Lox.Error(variable.name, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(variable, variable.name);
        return null;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (Stmt statement in statements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Stmt statement)
    {
        statement.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void BeginScope()
    {
        _scopes.Push(new());
    }

    private void EndScope()
    {
        _scopes.Pop();
    }

    // 3 elem
    // i = 2; 
    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = _scopes.Count() - 1; i >= 0; --i)
        {
            if (_scopes.ElementAt(_scopes.Count() - i - 1).ContainsKey(name.Lexeme))
            {
                Interpreter.Resolve(expr, _scopes.Count() - i - 1);
                return;
            };
        }
    }

    public object? VisitClass_Stmt(Stmt.Class_Stmt stmt)
    {
        ClassType enclosingClass = _currentClass;
        _currentClass = ClassType.CLASS;

        Declare(stmt.name);
        Define(stmt.name);
        if (stmt.superclass != null)
        {
            _currentClass = ClassType.SUBCLASS;
            if (stmt.superclass.name.Lexeme == stmt.name.Lexeme)
            {
                Lox.Error(stmt.superclass.name, "A class can't inherit from itself.");
            }
            Resolve(stmt.superclass);
            BeginScope();
            _scopes.Peek()["super"] = true;
        }

        BeginScope();
        _scopes.Peek().Add("this", true);
        foreach (var method in stmt.methods)
        {
            FunctionType declaration = FunctionType.METHOD;
            if (method.name.Lexeme == "init")
            {
                declaration = FunctionType.INITIALIZER;
            }

            ResolveFunction(method, declaration);
        }
        EndScope();

        if (stmt.superclass != null) EndScope();

        _currentClass = enclosingClass;
        return null;
    }

    public object? VisitGet_Expr(Expr.Get_Expr get_)
    {
        Resolve(get_.object_);
        return null;
    }

    public object? VisitSet_Expr(Expr.Set_Expr expr)
    {
        Resolve(expr.value);
        Resolve(expr.object_);
        return null;
    }

    public object? VisitThis_Expr(Expr.This_Expr expr)
    {
        if (_currentClass == ClassType.NONE)
        {
            Lox.Error(expr.keyword, "Can't use 'this' outside of a class.");
            return null;
        }
        ResolveLocal(expr, expr.keyword);
        return null;
    }

    public object? VisitSuperExpr(Expr.SuperExpr expr)
    {
        if (_currentClass == ClassType.NONE)
        {
            Lox.Error(expr.keyword, "Can't use 'super' outside of a class.");
        }
        else if (_currentClass != ClassType.SUBCLASS)
        {
            Lox.Error(expr.keyword, "Can't use 'super' in a class with no superclass.");
        }
        ResolveLocal(expr, expr.keyword);

        return null;
    }
}
