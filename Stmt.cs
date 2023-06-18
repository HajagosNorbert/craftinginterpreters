namespace Lox;

abstract class Stmt {

public abstract T Accept<T>(Visitor<T> visitor);

public interface Visitor<T> {
      public T VisitBlockStmt(BlockStmt block);
      public T VisitIf_Stmt(If_Stmt if_);
      public T VisitWhile_Stmt(While_Stmt while_);
      public T VisitReturn_Stmt(Return_Stmt return_);
      public T VisitFunctionStmt(FunctionStmt function);
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
  public class If_Stmt: Stmt{
      public readonly Expr condition;
      public readonly Stmt thenBranch;
      public readonly Stmt elseBranch;
      public If_Stmt(Expr condition, Stmt thenBranch, Stmt elseBranch) {
          this.condition = condition;
          this.thenBranch = thenBranch;
          this.elseBranch = elseBranch;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitIf_Stmt(this);
      }
  }
  public class While_Stmt: Stmt{
      public readonly Expr condition;
      public readonly Stmt body;
      public While_Stmt(Expr condition, Stmt body) {
          this.condition = condition;
          this.body = body;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitWhile_Stmt(this);
      }
  }
  public class Return_Stmt: Stmt{
      public readonly Token keyword;
      public readonly Expr value;
      public Return_Stmt(Token keyword, Expr value) {
          this.keyword = keyword;
          this.value = value;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitReturn_Stmt(this);
      }
  }
  public class FunctionStmt: Stmt{
      public readonly Token name;
      public readonly List<Token> parameters;
      public readonly List<Stmt> body;
      public FunctionStmt(Token name, List<Token> parameters, List<Stmt> body) {
          this.name = name;
          this.parameters = parameters;
          this.body = body;
      }
      public override T Accept<T>(Visitor<T> visitor) {
          return visitor.VisitFunctionStmt(this);
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
