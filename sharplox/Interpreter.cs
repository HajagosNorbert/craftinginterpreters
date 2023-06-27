namespace Lox;

class Interpreter : Expr.Visitor<Object>, Stmt.Visitor<object>
{
    public LoxEnvironment Globals { get; init; } = new LoxEnvironment();
    public Dictionary<Expr, int> Locals { get; init; } = new();

    private LoxEnvironment _environment;

    public Interpreter()
    {
        Globals.Define("clock", new LoxClock());
        _environment = Globals;
    }
    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt stmt in statements)
            {
                Execute(stmt);
            }
        }
        catch (RuntimeError e)
        {
            Lox.RuntimeError(e);
        }
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public object Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    public object VisitBinaryExpr(Expr.BinaryExpr binary)
    {
        object left = Evaluate(binary.left);
        object right = Evaluate(binary.right);

        switch (binary.operatr.Type)
        {
            case TokenType.GREATER:
                return ParseDouble(binary.operatr, left) > ParseDouble(binary.operatr, right);
            case TokenType.GREATER_EQUAL:
                return ParseDouble(binary.operatr, left) >= ParseDouble(binary.operatr, right);
            case TokenType.LESS:
                return ParseDouble(binary.operatr, left) < ParseDouble(binary.operatr, right);
            case TokenType.LESS_EQUAL:
                return ParseDouble(binary.operatr, left) <= ParseDouble(binary.operatr, right);
            case TokenType.BANG_EQUAL: return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL: return IsEqual(left, right);
            case TokenType.MINUS:
                return ParseDouble(binary.operatr, left) - ParseDouble(binary.operatr, right);
            case TokenType.SLASH:
                return ParseDouble(binary.operatr, left) / ParseNonZero(binary.operatr, ParseDouble(binary.operatr, right));
            case TokenType.STAR:
                return ParseDouble(binary.operatr, left) * ParseDouble(binary.operatr, right);
            case TokenType.PLUS:
                if (left is string leftStr && right is string rightStr)
                {
                    return leftStr + rightStr;
                }
                else if (left is string onlyLeftStr)
                {
                    return onlyLeftStr + Stringify(right);
                }
                else if (right is string onlyRightStr)
                {
                    return Stringify(left) + onlyRightStr;
                }
                else if (left is Double leftDouble && right is Double rightDouble)
                {
                    return leftDouble + rightDouble;
                }
                throw new RuntimeError(binary.operatr, "Operands must be two numbers or two strings.");
        }
        return null;
    }

    public object VisitGroupingExpr(Expr.GroupingExpr grouping)
    {
        return Evaluate(grouping.expression);
    }


    public object VisitLiteralExpr(Expr.LiteralExpr literal)
    {
        return literal.value;
    }

    public object VisitUnaryExpr(Expr.UnaryExpr unary)
    {
        object right = Evaluate(unary.right);
        switch (unary.operatr.Type)
        {
            case TokenType.MINUS:
                return -ParseDouble(unary.operatr, right);

            case TokenType.BANG:
                return !IsTruthy(right);
        }

        return null;
    }

    private bool IsTruthy(object obj)
    {
        return !(obj == null || (obj is bool val && val == false));
    }

    private bool IsEqual(object left, object right)
    {
        if (left == null && right == null)
        {
            return true;
        }
        if (left == null)
        {
            return false;
        }
        return left.Equals(right);
    }

    private double ParseDouble(Token operatr, object operand)
    {
        if (operand is Double num)
        {
            return num;
        }
        throw new RuntimeError(operatr, "Operand must be a number.");
    }

    private double ParseNonZero(Token operatr, double num)
    {
        if (num != 0)
        {
            return num;
        }
        throw new RuntimeError(operatr, "Cannont divide by zero!");
    }

    private string Stringify(object obj)
    {
        if (obj == null) return "nil";
        if (obj is Double num)
        {
            string numericText = num.ToString();
            if (numericText.EndsWith(".0"))
            {
                numericText = numericText.Substring(0, numericText.Length - 2);
            }
            return numericText;
        }
        return obj.ToString();
    }

    public object VisitExpressionStmt(Stmt.ExpressionStmt stmt)
    {
        Evaluate(stmt.expression);
        return null;
    }

    public object VisitPrintStmt(Stmt.PrintStmt stmt)
    {
        var expr = Evaluate(stmt.expression);
        Console.WriteLine(Stringify(expr));
        return null;
    }

    public object VisitVarStmt(Stmt.VarStmt var)
    {
        object initValue = null;
        if (var.initializer is not null)
        {
            initValue = Evaluate(var.initializer);
        }
        _environment.Define(var.name.Lexeme, initValue);
        return null;
    }

    public object VisitVariableExpr(Expr.VariableExpr variable)
    {
        return LookUpVariable(variable.name, variable);
    }

    private object LookUpVariable(Token name, Expr variable)
    {
        if (Locals.TryGetValue(variable, out int dist))
        {
            var currEnv = _environment;
            for (int i = 0; i < dist; ++i)
            {
                currEnv = currEnv.Enclosing;
            }
            return currEnv.Get(name);
        }
        else
        {
            return Globals.Get(name);
        }
    }
    public object VisitAssignExpr(Expr.AssignExpr assign)
    {
        var value = Evaluate(assign.value);
        if (Locals.TryGetValue(assign, out int dist))
        {
            var currEnv = _environment;
            for (int i = 0; i < dist; ++i)
            {
                currEnv = currEnv.Enclosing;
            }
            currEnv.Assign(assign.name, value);
        }
        else
        {
            Globals.Assign(assign.name, value);
        }
        return assign.value;
    }

    public object VisitBlockStmt(Stmt.BlockStmt stmt)
    {
        ExecuteBlock(stmt.statements, new LoxEnvironment(_environment));
        return null;
    }

    public void ExecuteBlock(List<Stmt> statements, LoxEnvironment newEnvironment)
    {
        LoxEnvironment previousEnv = _environment;

        try
        {
            _environment = newEnvironment;
            foreach (Stmt stmt in statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            _environment = previousEnv;
        }
    }

    public object VisitIf_Stmt(Stmt.If_Stmt if_)
    {
        if (IsTruthy(Evaluate(if_.condition)))
        {
            Execute(if_.thenBranch);
        }
        else if (if_.elseBranch != null)
        {
            Execute(if_.elseBranch);
        }
        return null;
    }

    public object VisitLogicalExpr(Expr.LogicalExpr logical)
    {
        bool isLeftTruthy = IsTruthy(Evaluate(logical.left));
        if (logical.operatr.Type == TokenType.AND && !isLeftTruthy || logical.operatr.Type == TokenType.OR && isLeftTruthy)
        {
            return Evaluate(logical.left);
        }
        return Evaluate(logical.right);

    }

    public object VisitWhile_Stmt(Stmt.While_Stmt while_)
    {
        while (IsTruthy(Evaluate(while_.condition)))
        {
            Execute(while_.body);
        }
        return null;
    }

    public object VisitCallExpr(Expr.CallExpr call)
    {

        ILoxCallable callee;
        try
        {
            callee = (ILoxCallable)Evaluate(call.callee);
        }
        catch (InvalidCastException e)
        {
            throw new RuntimeError(call.paren, "identifier is not callable.");
        }
        if (callee.arity() != call.args.Count())
        {
            throw new RuntimeError(call.paren, "Expected " +
                    callee.arity() + " arguments but got " +
                    call.args.Count() + ".");
        }
        return callee.Call(this, call.args.Select(x => Evaluate(x)).ToList());
    }

    public object VisitFunctionStmt(Stmt.FunctionStmt functionStmt)
    {
        LoxFunction functionObj = new LoxFunction(functionStmt, _environment, false);
        _environment.Define(functionStmt.name.Lexeme, functionObj);
        return null;
    }

    public object VisitReturn_Stmt(Stmt.Return_Stmt return_)
    {
        object value = null;
        if (return_.value != null) { value = Evaluate(return_.value); }
        throw new Return(value);
    }

    internal void Resolve(Expr expr, int depth)
    {
        Locals[expr] = depth;
    }

    public object VisitClass_Stmt(Stmt.Class_Stmt stmt)
    {
        LoxClass superClass = null;
        if (stmt.superclass != null)
        {
            try
            {
                superClass = (LoxClass)Evaluate(stmt.superclass);
            }
            catch (InvalidCastException e)
            {
                throw new RuntimeError(stmt.superclass.name, "Superclass must be a class.");
            }
        }
        _environment.Define(stmt.name.Lexeme, null);

        if (superClass != null)
        {
            _environment = new LoxEnvironment(_environment);
            _environment.Define("super", superClass);
        }

        Dictionary<string, LoxFunction> methods = new();
        foreach (var methodStmt in stmt.methods)
        {
            var methodName = methodStmt.name.Lexeme;
            LoxFunction method = new(methodStmt, _environment, methodName == "init");
            methods.Add(methodName, method);
        }

        LoxClass klass = new(stmt.name.Lexeme, superClass, methods);
        if (superClass != null)
        {
            _environment = _environment.Enclosing;
        }
        _environment.Assign(stmt.name, klass);
        return klass;
    }

    public object VisitGet_Expr(Expr.Get_Expr expr)
    {
        object obj = Evaluate(expr.object_);
        if (obj is LoxInstance loxInstance)
        {
            return loxInstance.Get(expr.name);
        }
        throw new RuntimeError(expr.name, "Only instances have properties.");
    }

    public object VisitSet_Expr(Expr.Set_Expr expr)
    {
        object obj = Evaluate(expr.object_);

        if (obj is LoxInstance loxInstance)
        {
            object value = Evaluate(expr.value);
            loxInstance.Set(expr.name, value);
            return value;
        }
        throw new RuntimeError(expr.name, "Only instances have fields.");
    }

    public object VisitThis_Expr(Expr.This_Expr expr)
    {
        return LookUpVariable(expr.keyword, expr);
    }

    public object VisitSuperExpr(Expr.SuperExpr expr)
    {
        int distance = Locals[expr];
        LoxClass superclass = (LoxClass)_environment.GetAt(distance, "super");

        LoxInstance obj = (LoxInstance)_environment.GetAt(distance -1, "this");
        LoxFunction method = superclass.FindMethod(expr.method.Lexeme);

        if (method == null)
        {
            throw new RuntimeError(expr.method, "Undefined property '" + expr.method.Lexeme + "'.");
        }
        return method.bind(obj);
    }
}
