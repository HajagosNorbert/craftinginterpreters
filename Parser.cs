using System.Runtime.Serialization;

namespace Lox;

class Parser
{
    private class ParseError : Exception
    {
        // public ParseError()
        // {
        // }
        //
        // public ParseError(string? message) : base(message)
        // {
        // }
        //
        // public ParseError(string? message, Exception? innerException) : base(message, innerException)
        // {
        // }
        //
        // protected ParseError(SerializationInfo info, StreamingContext context) : base(info, context)
        // {
        // }
    }
    private readonly Token[] tokens;
    private int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens.ToArray();
    }

    public Expr expression()
    {
        return equality();
    }

    /**
     * 2 == 2 == 3
     *
     *
     *      == 
     *   ==    3
     * 2   2
     * ----------------
     * Eq -> Cmp (( "==" | "!=" ) Cmp)*
     * */
    private Expr equality()
    {
        Expr expr = comparison();

        while (match(TokenType.EQUAL_EQUAL, TokenType.BANG_EQUAL))
        {
            Token oper = previous();
            Expr second = comparison();
            expr = new Expr.BinaryExpr(expr, oper, second);
        }
        return expr;
    }

    /*
     * Cmp -> Term ((< | > | <= | >=) Term)* 
     */
    private Expr comparison()
    {

        Expr expr = term();

        while (match(TokenType.LESS, TokenType.GREATER, TokenType.LESS_EQUAL, TokenType.GREATER_EQUAL))
        {
            Token oper = previous();
            Expr second = term();
            expr = new Expr.BinaryExpr(expr, oper, second);
        }
        return expr;
    }

    private Expr term()
    {

        Expr expr = factor();

        while (match(TokenType.MINUS, TokenType.PLUS))
        {
            Token oper = previous();
            Expr second = factor();
            expr = new Expr.BinaryExpr(expr, oper, second);
        }
        return expr;
    }

    private Expr factor()
    {

        Expr expr = unary();

        while (match(TokenType.SLASH, TokenType.STAR))
        {
            Token oper = previous();
            Expr second = unary();
            expr = new Expr.BinaryExpr(expr, oper, second);
        }
        return expr;
    }

    /*
     *( "!" | "-" ) unary | primary
     */
    private Expr unary()
    {
        if (match(TokenType.MINUS, TokenType.BANG))
        {
            Token token = previous();
            Expr right = unary();
            return new Expr.UnaryExpr(token, right);
        }
        return primary();
    }


    /**
     * primary → NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" ;
     */
    private Expr primary()
    {
        if (match(TokenType.FALSE)) return new Expr.LiteralExpr(false);
        if (match(TokenType.TRUE)) return new Expr.LiteralExpr(true);
        if (match(TokenType.NIL)) return new Expr.LiteralExpr(null);

        if (match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Expr.LiteralExpr(previous().Literal);
        }

        if (match(TokenType.LEFT_PAREN))
        {
            Expr expr = expression();
            consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.GroupingExpr(expr);
        }
        throw error(peek(), "Expect expression.");
    }

    private Token consume(TokenType tokenType, string message)
    {
        if (check(tokenType))
        {
            return advance();
        }
        throw error(peek(), message);
    }

    private Exception error(Token token, string message)
    {
        Lox.error(token, message);
        return new ParseError();
    }

    private void syncronise()
    {
        advance();

        while (!isAtEnd())
        {
            switch (peek().Type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }
            advance();
        }
    }

    private bool match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (check(type))
            {
                advance();
                return true;
            }
        }
        return false;
    }

    private bool check(TokenType type)
    {
        if (isAtEnd()) return false;
        return peek().Type == type;
    }

    private bool isAtEnd()
    {
        return peek().Type == TokenType.EOF;
    }

    private Token advance()
    {
        if (!isAtEnd()) ++current;
        return previous();
    }

    private Token peek()
    {
        return tokens[current];
    }
    private Token previous()
    {
        return tokens[current - 1];
    }
    private Token peekNext()
    {
        return tokens[current + 1];
    }

    internal Expr? parse()
    {
        try
        {
            return expression();
        }
        catch (ParseError)
        {
            return null;
        }
    }
}

