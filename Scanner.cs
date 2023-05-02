namespace Lox;

public class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = new List<Token>();

    private int start = 0;
    private int current = 0;
    private int line = 1;


    public Scanner(string source)
    {
        this.source = source;
    }


    internal List<Token> scanTokens()
    {
        while (!isAtEnd())
        {
            start = current;
            scanToken();
        }

        tokens.Add(new Token(TokenType.EOF, "", null, current));
        return tokens;

    }

    private void scanToken()
    {
        char c = advance();
        switch (c)
        {
            case '(': addToken(TokenType.LEFT_PAREN); break;
            case ')': addToken(TokenType.RIGHT_PAREN); break;
            case '{': addToken(TokenType.LEFT_BRACE); break;
            case '}': addToken(TokenType.RIGHT_BRACE); break;
            case ',': addToken(TokenType.COMMA); break;
            case '.': addToken(TokenType.DOT); break;
            case '-': addToken(TokenType.MINUS); break;
            case '+': addToken(TokenType.PLUS); break;
            case ';': addToken(TokenType.SEMICOLON); break;
            case '*': addToken(TokenType.STAR); break;

            case '!':
                addToken(match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;

            case '/':
                if (match('/'))
                    while (!isAtEnd() && peek() != '\n') advance();
                else
                    addToken(TokenType.SLASH);
                break;

            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                line++;
                break;

            case '"': 
                scanString();
                break;

            default:
                Lox.error(line, "Unexpected character: " + c);
                break;
        }
    }

    private void scanString()
    {
        char peekRes = peek();

        while(!isAtEnd() && peekRes != '"' && peekRes != '\n') {
            peekRes = advance();
        }

        if(peekRes != '"' ) {
            Lox.error(line, "String litteral not closed on the same line");
            return;
        }

        // current is the closing quote
        string strLiteral = source.Substring(start+1, current - start -2);
        addToken(TokenType.STRING, strLiteral);
        if(!isAtEnd()) advance();

    }

    private char peek()
    {
        if (isAtEnd()) return '\0';
        return source[current];
    }

    private bool match(char expected)
    {
        if (isAtEnd()) return false;
        if (source[current] != expected) return false;

        ++current;
        return true;
    }

    private void addToken(TokenType type) => addToken(type, null);

    private void addToken(TokenType type, Object literal)
    {
        string text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }


    private char advance() => source[current++];

    private bool isAtEnd() => current >= source.Length;

    public List<Token> scan()
    {
        throw new NotImplementedException();
    }
}
