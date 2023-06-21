namespace Lox;
enum FunctionType {
    NONE,
    FUNCTION
}

class Resolver : Expr.Visitor<object?>, Stmt.Visitor<object?>
{
    public Interpreter Interpreter { get; init; }
    private Stack<Dictionary<string, bool>> Scopes { get; } = new();
     private FunctionType enclosingFunction = FunctionType.NONE;

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
        if (enclosingFunction == FunctionType.NONE){
            Lox.Error(return_.keyword, "Can't return from top-level code.");
        }
        if (return_ != null) Resolve(return_.value);
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
        var prevFunctionType = enclosingFunction;
        enclosingFunction = funcType;

        BeginScope();
        foreach (var param in function.parameters)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.body);
        EndScope();
        enclosingFunction = prevFunctionType;
    }

    public object? VisitExpressionStmt(Stmt.ExpressionStmt expression)
    {
        Resolve(expression);
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
        if (Scopes.Count() == 0)
        {
            return;
        }
        if(Scopes.Peek().ContainsKey(name.Lexeme)){
            Lox.Error(name, "Already a variable with this name in this scope.");
        }
        Scopes.Peek().Add(name.Lexeme, false);
    }

    private void Define(Token name)
    {
        if (Scopes.Count() == 0)
        {
            return;
        }
        Scopes.Peek()[name.Lexeme] = true;
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
        if (Scopes.Count() != 0 && Scopes.Peek().GetValueOrDefault(variable.name.Lexeme, true) == false)
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
        Scopes.Push(new());
    }

    private void EndScope()
    {
        Scopes.Pop();
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = Scopes.Count() - 1; i >= 0; --i)
        {
            if (Scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                Interpreter.Resolve(expr, Scopes.Count() - i - 1);
                return;
            };
        }
    }

    public object? VisitClass_Stmt(Stmt.Class_Stmt class_)
    {
        Declare(class_.name);
        Define(class_.name);

        // BeginScope();
        // 
        //
        // EndScope();
        return null;
    }
}