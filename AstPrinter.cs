namespace Lox;

class AstPrinter : Expr.Visitor<string>
{

    public string print(Expr expr)
    {
        return expr.accept(this);
    }

    private string parenthesize(string name, params Expr[] expresssions)
    {
        string exprStr = string.Join(" ", expresssions.Select(expr => print(expr)));
        return "(" + name +" "+ exprStr + ")";
    }

    public string visitBinaryExpr(Expr.BinaryExpr binary)
    {
        return parenthesize(binary.operatr.Lexeme, binary.left, binary.right);
    }

    public string visitGroupingExpr(Expr.GroupingExpr grouping)
    {
        return parenthesize("group", grouping.expression);
    }

    public string visitLiteralExpr(Expr.LiteralExpr literal)
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

    public string visitUnaryExpr(Expr.UnaryExpr unary)
    {
        return parenthesize(unary.operatr.Lexeme, unary.right);
    }
}

