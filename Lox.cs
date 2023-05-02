namespace Lox;

public class Lox
{
    static bool hadError = false;

    private static int Main(string[] args)
    {
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
            //string? line = "\"hi\" () \"there\"";
            if (line == null)
                break;
            run(line);
            hadError = false;
        }
    }

    private static void runFile(string file)
    {
        Console.WriteLine("Reading file: "+ file);
        try
        {
            string content = File.ReadAllText(file);
            run(content);
            if (hadError)
                Environment.Exit(65);
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

        Console.WriteLine(string.Join(" # ", tokens));
        Console.WriteLine();
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
}
