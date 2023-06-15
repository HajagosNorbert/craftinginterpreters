namespace Lox;

class AstPrinter : Expr.Visitor<string>
{

    public string print(Expr expr)
    {
        return expr.Accept(this);
    }

    private string parenthesize(string name, params Expr[] expresssions)
    {
        string exprStr = string.Join(" ", expresssions.Select(expr => print(expr)));
        return "(" + name +" "+ exprStr + ")";
    }

    public string VisitBinaryExpr(Expr.BinaryExpr binary)
    {
        return parenthesize(binary.operatr.Lexeme, binary.left, binary.right);
    }

    public string VisitGroupingExpr(Expr.GroupingExpr grouping)
    {
        return parenthesize("group", grouping.expression);
    }

    public string VisitLiteralExpr(Expr.LiteralExpr literal)
    {
        string strValueOfLiteral;
        if (literal?.value is null){
            strValueOfLiteral =  "nil";
        } else if(literal.value is string) {
            strValueOfLiteral = "\"" + literal.value.ToString() + "\""; 
        } else {
            strValueOfLiteral = literal.value.ToString();
        }

        return strValueOfLiteral;
    }

    public string VisitUnaryExpr(Expr.UnaryExpr unary)
    {
        return parenthesize(unary.operatr.Lexeme, unary.right);
    }

    public string VisitVariableExpr(Expr.VariableExpr variable)
    {
        return "( var_access: " + variable.name + " )";
    }

    public string VisitAssignExpr(Expr.AssignExpr assign)
    {
        return parenthesize("assign " + assign.name + " to ", assign.value);
    }

    public string VisitLogicalExpr(Expr.LogicalExpr logical)
    {
        return VisitBinaryExpr(new Expr.BinaryExpr(logical.left, logical.operatr, logical.right));
    }
}

