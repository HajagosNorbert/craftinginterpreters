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

        args = args.Append("sample.lox").ToArray();
        if (args.Length > 1)
        {
            Console.WriteLine("Usage: shaprlox [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }

        return 0;

    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            string? line = Console.ReadLine();
            if (line == null)
                break;
            Run(line);
            hadError = false;
        }
    }

    private static void RunFile(string file)
    {
        Console.WriteLine("Reading file: " + file);
        try
        {
            string content = File.ReadAllText(file);
            Run(content);
            if (hadError) Environment.Exit(65);
            if (hadRuntimeError) Environment.Exit(70);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error while reading file: '" + file + "'. This is a bug. Here is the error:\n");
            Console.WriteLine(e.ToString());
        }
    }

    private static void Run(string source)
    {
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();
        Parser parser = new Parser(tokens);
        List<Stmt> statements = parser.Parse();
        interpreter.Interpret(statements);
    }


    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            Report(token.Line, " at end of file", message);
        }
        else
        {
            Report(token.Line, $" at `{token.Lexeme}`", message);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
        hadError = true;
    }

    public static void RuntimeError(RuntimeError e)
    {
        Console.WriteLine(e.Message + "\n[line " + e.Token.Line + "]");
        hadRuntimeError = true;
    }
}
