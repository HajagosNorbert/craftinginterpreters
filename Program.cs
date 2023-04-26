
bool hadError = false;

if (args.Length > 1) {
    Console.WriteLine("Usage: shaprlox [script]");
    Environment.Exit(64);
} else if (args.Length == 1) {
    runFile(args[1]);
} else {
    runPrompt();
}

return 0;

void runPrompt()
{
    while(true){
        Console.Write("> ");
        string line = Console.ReadLine(); 
        if (line == null)
            break;
        run(line);
        hadError = false;
    }
}

void runFile(string file){
    try {
        string content = File.ReadAllText(file);
        run(content);
        if (hadError)
            Environment.Exit(65);
    } catch (Exception _) {
        Console.WriteLine("Error while reading file: '" + file + "'");
    }
}

void run(string source)
{
    Scanner scanner = new Scanner(source);
    List<Tokens> tokens = scanner.scanTokens();

    foreach (token in tokens) {
        Console.Write(token);
    }
}


void error(int line, string message) {
    report(line, "", message);
  }

  void report(int line, string where, string message) {
      Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
      hadError = true;
  }

