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
                if (isDigit(c))
                {
                    scanNumber();
                }
                else
                {
                    Lox.error(line, "Unexpected character: " + c);
                }
                break;
        }
    }

    private void scanNumber()
    {
        while (isDigit(peek()))
        {
            advance();
        }


        if (peek() == '.' && isDigit(peekNext()))
        {
            advance();
            while (isDigit(peek()))
            {
                advance();
            }
        }

        double numberLiteral = Convert.ToDouble(source.Substring(start, current - start));
        addToken(TokenType.NUMBER, numberLiteral);
    }

    private char peekNext()
    {
        if (current + 1 >= source.Length) return '\0';
        return source[current + 1];
    }

    private bool isDigit(char c)
    {
        return '0' <= c && c <= '9';
    }

    private void scanString()
    {
        while (!isAtEnd() && peek() != '"')
        {
            if (source[current] == '\n') ++line;
            advance();
        }

        if (source[current] != '"')
        {
            Lox.error(line, "String litteral doesn't have a closing quote");
            return;
        }
        int beginningOfStrLiteral = start + 1;
        string strLiteral = source.Substring(beginningOfStrLiteral, current - beginningOfStrLiteral);
        if (!isAtEnd()) advance();
        addToken(TokenType.STRING, strLiteral);
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
