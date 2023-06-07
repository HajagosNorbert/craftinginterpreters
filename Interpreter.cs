namespace Lox;

class Interpreter : Expr.Visitor<Object>
{
    public void interpret(Expr expr)
    {
        try {
            object res = evaluate(expr);
            Console.WriteLine(stringify(res));
        } catch(RuntimeError e){
            Lox.runtimeError(e);
        }
    }

    public object evaluate(Expr expr)
    {
        return expr.accept(this);
    }

    public object visitBinaryExpr(Expr.BinaryExpr binary)
    {
        object left = evaluate(binary.left);
        object right = evaluate(binary.right);

        switch (binary.operatr.Type)
        {
            case TokenType.GREATER:
                return parseDouble(binary.operatr,left) > parseDouble(binary.operatr,right);
            case TokenType.GREATER_EQUAL:
                return parseDouble(binary.operatr,left) >= parseDouble(binary.operatr,right);
            case TokenType.LESS:
                return parseDouble(binary.operatr,left) < parseDouble(binary.operatr,right);
            case TokenType.LESS_EQUAL:
                return parseDouble(binary.operatr,left) <= parseDouble(binary.operatr,right);
            case TokenType.BANG_EQUAL: return !isEqual(left, right);
            case TokenType.EQUAL_EQUAL: return isEqual(left, right);
            case TokenType.MINUS:
                return parseDouble(binary.operatr,left) - parseDouble(binary.operatr,right);
            case TokenType.SLASH:
                return parseDouble(binary.operatr,left) / parseNonZero(binary.operatr, parseDouble(binary.operatr,right));
            case TokenType.STAR:
                return parseDouble(binary.operatr,left) * parseDouble(binary.operatr,right);
            case TokenType.PLUS:
                if (left is string leftStr && right is string rightStr)
                {
                    return leftStr + rightStr;
                }
                else if (left is string str)
                {
                    return str + stringify(right);
                }
                else if (left is Double leftDouble && right is Double rightDouble)
                {
                    return leftDouble + rightDouble;
                }
                throw new RuntimeError(binary.operatr, "Operands must be two numbers or two strings.");
        }
        return null;
    }

    public object visitGroupingExpr(Expr.GroupingExpr grouping)
    {
        return evaluate(grouping.expression);
    }


    public object visitLiteralExpr(Expr.LiteralExpr literal)
    {
        return literal.value;
    }

    public object visitUnaryExpr(Expr.UnaryExpr unary)
    {
        object right = evaluate(unary.right);
        switch (unary.operatr.Type)
        {
            case TokenType.MINUS:
                return - parseDouble(unary.operatr, right);

            case TokenType.BANG:
                return !isTruthy(right);
        }

        return null;
    }

    private bool isTruthy(object obj)
    {
        return !(obj == null || (obj is bool val && val == false));
    }

    private bool isEqual(object left, object right)
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

    private double parseDouble(Token operatr, object operand) {
        if (operand is Double num){
            return num;
        }
        throw new RuntimeError(operatr, "Operand must be a number.");
    }

    private double parseNonZero(Token operatr, double num)
    {
        if (num != 0){
            return num;
        }
        throw new RuntimeError(operatr, "Cannont divide by zero!");
    }

    private string stringify(object obj)
    {
        if(obj == null) return "nil";
        if (obj is Double num) {
            string numericText = num.ToString();
            if(numericText.EndsWith(".0")) {
                numericText = numericText.Substring(0, numericText.Length - 2);
            }
            return numericText;
        }
        return obj.ToString();
    }
}
