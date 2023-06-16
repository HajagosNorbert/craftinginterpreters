namespace Lox;

abstract class Expr {

public abstract T Accept<T>(Visitor<T> visitor);

public interface Visitor<T> {
      public T VisitAssignExpr(AssignExpr assign);
      public T VisitBinaryExpr(BinaryExpr binary);
      public T VisitGroupingExpr(GroupingExpr grouping);
      public T VisitLogicalExpr(LogicalExpr logical);
      public T VisitLiteralExpr(LiteralExpr literal);
      public T VisitUnaryExpr(UnaryExpr unary);
      public T VisitCallExpr(CallExpr call);
      public T VisitVariableExpr(VariableExpr variable);
}

  public class AssignExpr: Expr{
      public readonly Token name;
      public readonly Expr value;
      public AssignExpr(Token name, Expr value) {
          this.name = name;
          this.value = value;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitAssignExpr(this);
      }
  }
  public class BinaryExpr: Expr{
      public readonly Expr left;
      public readonly Token operatr;
      public readonly Expr right;
      public BinaryExpr(Expr left, Token operatr, Expr right) {
          this.left = left;
          this.operatr = operatr;
          this.right = right;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitBinaryExpr(this);
      }
  }
  public class GroupingExpr: Expr{
      public readonly Expr expression;
      public GroupingExpr(Expr expression) {
          this.expression = expression;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitGroupingExpr(this);
      }
  }
  public class LogicalExpr: Expr{
      public readonly Expr left;
      public readonly Token operatr;
      public readonly Expr right;
      public LogicalExpr(Expr left, Token operatr, Expr right) {
          this.left = left;
          this.operatr = operatr;
          this.right = right;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitLogicalExpr(this);
      }
  }
  public class LiteralExpr: Expr{
      public readonly Object value;
      public LiteralExpr(Object value) {
          this.value = value;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitLiteralExpr(this);
      }
  }
  public class UnaryExpr: Expr{
      public readonly Token operatr;
      public readonly Expr right;
      public UnaryExpr(Token operatr, Expr right) {
          this.operatr = operatr;
          this.right = right;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitUnaryExpr(this);
      }
  }
  public class CallExpr: Expr{
      public readonly Expr callee;
      public readonly Token paren;
      public readonly List<Expr> args;
      public CallExpr(Expr callee, Token paren, List<Expr> args) {
          this.callee = callee;
          this.paren = paren;
          this.args = args;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitCallExpr(this);
      }
  }
  public class VariableExpr: Expr{
      public readonly Token name;
      public VariableExpr(Token name) {
          this.name = name;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitVariableExpr(this);
      }
  }
}
