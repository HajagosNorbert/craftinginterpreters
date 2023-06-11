namespace Lox;

abstract class Stmt {

public abstract T Accept<T>(Visitor<T> visitor);

public interface Visitor<T> {
      public T VisitBlockStmt(BlockStmt block);
      public T VisitExpressionStmt(ExpressionStmt expression);
      public T VisitPrintStmt(PrintStmt print);
      public T VisitVarStmt(VarStmt var);
}

  public class BlockStmt: Stmt{
      public readonly List<Stmt> statements;
      public BlockStmt(List<Stmt> statements) {
          this.statements = statements;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitBlockStmt(this);
      }
  }
  public class ExpressionStmt: Stmt{
      public readonly Expr expression;
      public ExpressionStmt(Expr expression) {
          this.expression = expression;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitExpressionStmt(this);
      }
  }
  public class PrintStmt: Stmt{
      public readonly Expr expression;
      public PrintStmt(Expr expression) {
          this.expression = expression;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitPrintStmt(this);
      }
  }
  public class VarStmt: Stmt{
      public readonly Token name;
      public readonly Expr initializer;
      public VarStmt(Token name, Expr initializer) {
          this.name = name;
          this.initializer = initializer;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitVarStmt(this);
      }
  }
}
