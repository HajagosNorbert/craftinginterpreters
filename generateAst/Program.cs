internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: generate_ast: <output directory>");
            Environment.Exit(64);
        }

        string outputDir = args[0];
        defineAst(outputDir, "Expr", new List<string>(){
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token operatr, Expr right",
                "Grouping : Expr expression",
                "Logical  : Expr left, Token operatr, Expr right",
                "Literal  : Object value",
                "Set_     : Expr object_, Token name, Expr value",
                "This_     : Token keyword",
                "Unary    : Token operatr, Expr right",
                "Get_     : Expr object_, Token name",
                "Super    : Token keyword, Token method",
                "Call     : Expr callee, Token paren, List<Expr> args",
                "Variable : Token name"
                });

        defineAst(outputDir, "Stmt", new List<string> {
                "Block      : List<Stmt> statements",
                "If_        : Expr condition, Stmt thenBranch, Stmt elseBranch",
                "While_     : Expr condition, Stmt body",
                "Class_     : Token name, Expr.VariableExpr superclass, List<Stmt.FunctionStmt> methods",
                "Return_    : Token keyword, Expr value",
                "Function   : Token name, List<Token> parameters, List<Stmt> body",
                "Expression : Expr expression",
                "Print      : Expr expression",
                "Var        : Token name, Expr initializer"
          });
    }
    static void defineAst(string outputDir, string baseName, List<string> types)
    {
        using (StreamWriter sw = new StreamWriter(Path.Combine(outputDir, baseName + ".cs"), false))
        {
            sw.WriteLine("namespace Lox;");
            sw.WriteLine();

            sw.WriteLine("abstract class " + baseName + " {");
            sw.WriteLine();

            sw.WriteLine("public abstract T Accept<T>(Visitor<T> visitor);");
            sw.WriteLine();

            sw.WriteLine("public interface Visitor<T> {");
            foreach (string type in types)
            {
                string className = type.Split(":")[0].Trim();
                defineVisitorInterfaceMethod(sw, baseName, className);
            }
            sw.WriteLine("}");
            sw.WriteLine();

            //todo make this one function
            foreach (string type in types)
            {
                string className = type.Split(":")[0].Trim();
                string fields = type.Split(":")[1].Trim();
                defineType(sw, baseName, className, fields);
            }

            sw.WriteLine("}");

        }
    }

    private static void defineVisitorInterfaceMethod(StreamWriter sw, string baseName, string className)
    {
        sw.WriteLine($"      public T Visit{className + baseName}({className + baseName} {className.ToLower()});");
    }

    private static void defineType(StreamWriter sw, string baseName, string className, string fields)
    {
        sw.WriteLine("  public class " + className + baseName + ": " + baseName + "{");

        //fields
        foreach (string field in fields.Split(", "))
        {
            sw.WriteLine("      public readonly " + field + ";");
        }

        //constructor
        sw.WriteLine("      public " + className + baseName + "(" + fields + ") {");
        foreach (string field in fields.Split(", "))
        {
            string f = field.Split(" ")[1];
            sw.WriteLine("          this." + f + " = " + f + ";");
        }
        sw.WriteLine("      }");

        //accept visitors
        sw.WriteLine("      public override T Accept<T>(Visitor<T> visitor) {");
        sw.WriteLine("          return visitor.Visit" + className + baseName + "(this);");
        sw.WriteLine("      }");

        sw.WriteLine("  }");
    }
}
