namespace Lox;

public class Scanner
{
    private static readonly Dictionary<string, TokenType> _keywords = new() {
        { "and", TokenType.AND },
        { "class", TokenType.CLASS },
        { "else", TokenType.ELSE },
        { "false", TokenType.FALSE },
        { "for", TokenType.FOR },
        { "fun", TokenType.FUN },
        { "if", TokenType.IF },
        { "nil", TokenType.NIL },
        { "or", TokenType.OR },
        { "print", TokenType.PRINT },
        { "return", TokenType.RETURN },
        { "super", TokenType.SUPER },
        { "this", TokenType.THIS },
        { "true", TokenType.TRUE },
        { "var", TokenType.VAR },
        { "while", TokenType.WHILE }
    };
    private readonly string _source;
    private readonly List<Token> _tokens = new List<Token>();

    private int _start = 0;
    private int _current = 0;
    private int _line = 1;


    public Scanner(string source)
    {
        this._source = source;
    }


    internal List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EOF, "", null, _line));
        return _tokens;

    }

    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '+': AddToken(TokenType.PLUS); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '*': AddToken(TokenType.STAR); break;

            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;

            case '/':
                if (Match('/'))
                    while (!IsAtEnd() && Peek() != '\n') Advance();
                else
                    AddToken(TokenType.SLASH);
                break;

            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;

            case '\n':
                _line++;
                break;

            case '"':
                ScanString();
                break;

            default:
                if (IsDigit(c))
                {
                    ScanNumber();
                } else if(IsAlpha(c)) {
                    scanIdentifier();
                }
                else
                {
                    Lox.Error(_line, "Unexpected character: " + c);
                }
                break;
        }
    }

    private void scanIdentifier()
    {
        while(IsAlphaNumeric(Peek())) {Advance();}

        string text = _source.Substring(_start, _current - _start);
        TokenType type = _keywords.GetValueOrDefault(text, TokenType.IDENTIFIER);
        AddToken(type);
    }

    private bool IsAlpha(char c)
    {
        return c == '_' || ('a' <= c && c <= 'z') || (('A' <= c && c <= 'Z'));
    }

    private bool IsAlphaNumeric(char c) => IsDigit(c) || IsAlpha(c);

    private void ScanNumber()
    {
        while (IsDigit(Peek()))
        {
            Advance();
        }


        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            while (IsDigit(Peek()))
            {
                Advance();
            }
        }

        double numberLiteral = Convert.ToDouble(_source.Substring(_start, _current - _start));
        AddToken(TokenType.NUMBER, numberLiteral);
    }

    private char PeekNext()
    {
        if (_current + 1 >= _source.Length) return '\0';
        return _source[_current + 1];
    }

    private bool IsDigit(char c)
    {
        return '0' <= c && c <= '9';
    }

    private void ScanString()
    {
        while (!IsAtEnd() && Peek() != '"')
        {
            if (_source[_current] == '\n') ++_line;
            Advance();
        }

        if (_source[_current] != '"')
        {
            Lox.Error(_line, "String litteral doesn't have a closing quote");
            return;
        }
        int beginningOfStrLiteral = _start + 1;
        string strLiteral = _source.Substring(beginningOfStrLiteral, _current - beginningOfStrLiteral);
        if (!IsAtEnd()) Advance();
        AddToken(TokenType.STRING, strLiteral);
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return _source[_current];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        ++_current;
        return true;
    }

    private void AddToken(TokenType type) => AddToken(type, null);

    private void AddToken(TokenType type, Object literal)
    {
        string text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, literal, _line));
    }


    private char Advance() => _source[_current++];

    private bool IsAtEnd() => _current >= _source.Length;

    public List<Token> Scan()
    {
        throw new NotImplementedException();
    }
}
