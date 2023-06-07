namespace Lox;
//TODO: Implement the ternary operator
public class Lox
{
    static bool hadError = false;
    static bool hadRuntimeError = false;
    static readonly AstPrinter astPrinter = new AstPrinter();
    static readonly Interpreter interpreter = new Interpreter();

    private static int Main(string[] args)
    {
        // run("(4/2)");
        // return 0;

        if (args.Length > 1)
        {
            Console.WriteLine("Usage: shaprlox [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            runFile(args[0]);
        }
        else
        {
            runPrompt();
        }

        return 0;

    }

    private static void runPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null)
                break;
            run(line);
            hadError = false;
        }
    }

    private static void runFile(string file)
    {
        Console.WriteLine("Reading file: " + file);
        try
        {
            string content = File.ReadAllText(file);
            run(content);
            if (hadError) Environment.Exit(65);
            if (hadRuntimeError) Environment.Exit(70);
        }
        catch (Exception _)
        {
            Console.WriteLine("Error while reading file: '" + file + "'");
        }
    }

    private static void run(string source)
    {
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.scanTokens();
        Parser parser = new Parser(tokens);
        Expr expr = parser.parse();
        Console.WriteLine(astPrinter.print(expr));
        interpreter.interpret(expr);
    }


    public static void error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            report(token.Line, " at end of file", message);
        }
        else
        {
            report(token.Line, $" at `{token.Lexeme}`", message);
        }
    }

    public static void error(int line, string message)
    {
        report(line, "", message);
    }

    private static void report(int line, string where, string message)
    {
        Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
        hadError = true;
    }

    public static void runtimeError(RuntimeError e)
    {
        Console.WriteLine(e.Message + "\n[line " + e.token.Line + "]");
        hadRuntimeError = true;
    }
}
