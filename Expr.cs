namespace Lox;

abstract class Expr
{
    public abstract T accept<T>(Visitor<T> visitor);

    public interface Visitor<T>
    {
        public T visitBinaryExpr(BinaryExpr binary);
        public T visitGroupingExpr(GroupingExpr grouping);
        public T visitLiteralExpr(LiteralExpr literal);
        public T visitUnaryExpr(UnaryExpr unary);
    }

    public class BinaryExpr : Expr
    {
        public readonly Expr left;
        public readonly Token operatr;
        public readonly Expr right;
        public BinaryExpr(Expr left, Token operatr, Expr right)
        {
            this.left = left;
            this.operatr = operatr;
            this.right = right;
        }
        public override T accept<T>(Visitor<T> visitor)
        {
            return visitor.visitBinaryExpr(this);
        }
    }
    public class GroupingExpr : Expr
    {
        public readonly Expr expression;
        public GroupingExpr(Expr expression)
        {
            this.expression = expression;
        }
        public override T accept<T>(Visitor<T> visitor)
        {
            return visitor.visitGroupingExpr(this);
        }
    }
    public class LiteralExpr : Expr
    {
        public readonly Object? value;
        public LiteralExpr(Object? value)
        {
            this.value = value;
        }
        public override T accept<T>(Visitor<T> visitor)
        {
            return visitor.visitLiteralExpr(this);
        }
    }
    public class UnaryExpr : Expr
    {
        public readonly Token operatr;
        public readonly Expr right;
        public UnaryExpr(Token operatr, Expr right)
        {
            this.operatr = operatr;
            this.right = right;
        }
        public override T accept<T>(Visitor<T> visitor)
        {
            return visitor.visitUnaryExpr(this);
        }
    }
}
